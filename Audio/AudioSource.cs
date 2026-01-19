using System;
using Silk.NET.OpenAL;
using vaudio;

namespace Game3.Audio
{
    public class AudioSource : IDisposable
    {
        private AudioManager audioManager;
        private AL al;
        private uint sourceId;
        private Voice voice;
        private RaytracingContext raytracingContext;
        private bool is3D;
        private bool disposed;
        private float baseVolume = 1.0f;
        private float x, y, z;
        private StaticPositionF positionProvider;
        private uint directFilter;      // Filter for direct sound (occlusion)
        private uint reverbSendFilter;  // Filter for reverb send (wetness control)

        // EFX constants
        private const int AL_DIRECT_FILTER = 0x20005;
        private const int AL_AUXILIARY_SEND_FILTER = 0x20006;

        // Cached delegate
        private Source3iDelegate cachedAlSource3i;

        public bool IsPlaying
        {
            get
            {
                al.GetSourceProperty(sourceId, GetSourceInteger.SourceState, out int state);
                return state == (int)SourceState.Playing;
            }
        }

        public bool IsLooping { get; private set; }
        public Voice Voice => voice;

        public AudioSource(AudioManager manager, AL al, RaytracingContext context, uint buffer, bool is3D)
        {
            this.audioManager = manager;
            this.al = al;
            this.raytracingContext = context;
            this.is3D = is3D;

            sourceId = al.GenSource();
            al.SetSourceProperty(sourceId, SourceInteger.Buffer, buffer);

            // Cache the alSource3i function pointer
            cachedAlSource3i = GetAlSource3i();

            if (is3D)
            {
                al.SetSourceProperty(sourceId, SourceBoolean.SourceRelative, false);
                al.SetSourceProperty(sourceId, SourceFloat.RolloffFactor, 2.0f);      // Faster falloff
                al.SetSourceProperty(sourceId, SourceFloat.ReferenceDistance, 1.0f);  // Full volume at 1m
                al.SetSourceProperty(sourceId, SourceFloat.MaxDistance, 20.0f);       // Silence at 20m

                if (manager.HasEfx)
                {
                    // Create filter for direct path (occlusion)
                    directFilter = manager.CreateFilter();
                    if (directFilter != 0)
                    {
                        al.SetSourceProperty(sourceId, (SourceInteger)AL_DIRECT_FILTER, (int)directFilter);
                    }

                    // Create filter for reverb send (controls wet level based on reverbWetness)
                    reverbSendFilter = manager.CreateFilter();

                    // Connect to reverb with the send filter
                    ConnectToReverbWithFilter(manager.EffectSlot, reverbSendFilter);

                    Program.Log($"Source {sourceId}: Created directFilter={directFilter}, reverbSendFilter={reverbSendFilter}");
                }
            }
            else
            {
                al.SetSourceProperty(sourceId, SourceBoolean.SourceRelative, true);
                al.SetSourceProperty(sourceId, SourceVector3.Position, 0, 0, 0);
            }
        }

        private void ConnectToReverbWithFilter(uint effectSlot, uint sendFilter)
        {
            if (cachedAlSource3i != null)
            {
                // Connect source to auxiliary send 0, with the effect slot and our send filter
                cachedAlSource3i(sourceId, AL_AUXILIARY_SEND_FILTER, (int)effectSlot, 0, (int)sendFilter);
            }
        }

        private delegate void Source3iDelegate(uint source, int param, int value1, int value2, int value3);
        private Source3iDelegate GetAlSource3i()
        {
            try
            {
                var ptr = al.GetProcAddress("alSource3i");
                if (ptr != IntPtr.Zero)
                {
                    return System.Runtime.InteropServices.Marshal.GetDelegateForFunctionPointer<Source3iDelegate>((nint)ptr);
                }
            }
            catch { }
            return null;
        }

        public void SetPosition(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;

            if (is3D)
            {
                // Convert game coordinates (X-right, Y-forward, Z-up) to OpenAL (X-right, Y-up, Z-back)
                al.SetSourceProperty(sourceId, SourceVector3.Position, x, z, -y);

                // Create or update vaudio voice for raytracing
                if (voice == null && raytracingContext != null)
                {
                    positionProvider = new StaticPositionF(x, y, z);
                    voice = raytracingContext.CreateVoice(positionProvider);
                    voice.OnRaytracingComplete = OnVoiceRaytracingComplete;
                }
                else if (positionProvider != null)
                {
                    positionProvider.SetPosition(x, y, z);
                }
            }
        }

        public void SetVolume(float volume)
        {
            baseVolume = volume;
            al.SetSourceProperty(sourceId, SourceFloat.Gain, volume);
        }

        public void SetLoop(bool loop)
        {
            IsLooping = loop;
            al.SetSourceProperty(sourceId, SourceBoolean.Looping, loop);
        }

        public void SetPitch(float pitch)
        {
            al.SetSourceProperty(sourceId, SourceFloat.Pitch, pitch);
        }

        public void Play()
        {
            al.SourcePlay(sourceId);
        }

        public void Pause()
        {
            al.SourcePause(sourceId);
        }

        public void Stop()
        {
            al.SourceStop(sourceId);
        }

        private int updateCounter = 0;
        private static bool loggedOnce = false;

        public void Update()
        {
            updateCounter++;

            if (is3D && voice != null && !voice.initialising)
            {
                ApplyRaytracingFilters();
            }

            // Log detailed raytracing info once after initialization
            if (updateCounter == 120 && !loggedOnce && voice != null && !voice.initialising)
            {
                loggedOnce = true;
                LogDetailedRaytracingInfo();
            }
        }

        private void OnVoiceRaytracingComplete()
        {
            Program.Log($"Voice raytracing complete for source {sourceId}");
            ApplyRaytracingFilters();
        }

        private static int globalLogCounter = 0;

        private void LogDetailedRaytracingInfo()
        {
            if (voice == null) return;

            try
            {
                var filter = voice.filter;
                var processed = voice.processedReverb;

                Program.Log($"=== Raytracing Info for Source {sourceId} ===");
                Program.Log($"  Position: ({x:F1}, {y:F1}, {z:F1})");
                Program.Log($"  Filter: gainLF={filter.gainLF:F3}, gainHF={filter.gainHF:F3}");
                Program.Log($"  ReverbWetness: {voice.reverbWetness:F3}");

                if (processed != null)
                {
                    Program.Log($"  Room: Volume={processed.RoomVolume:F1}m3");
                    Program.Log($"  Returned: {processed.ReturnedPercent:F1}%, Outside: {processed.OutsidePercent:F1}%");
                }
            }
            catch (Exception ex)
            {
                Program.Log($"  Error logging raytracing info: {ex.Message}");
            }
        }

        private void ApplyRaytracingFilters()
        {
            if (voice == null || voice.initialising) return;

            float gainLF = voice.filter.gainLF;
            float gainHF = voice.filter.gainHF;
            float reverbWetness = voice.reverbWetness;

            // Periodic logging
            globalLogCounter++;
            if (globalLogCounter >= 300) // ~5 seconds
            {
                globalLogCounter = 0;
                Program.Log($"Src{sourceId}: gainLF={gainLF:F3}, gainHF={gainHF:F3}, wetness={reverbWetness:F3}");
            }

            // Clamp values
            gainLF = Math.Clamp(gainLF, 0.0f, 1.0f);
            gainHF = Math.Clamp(gainHF, 0.0f, 1.0f);
            reverbWetness = Math.Clamp(reverbWetness, 0.0f, 1.0f);

            if (audioManager != null && audioManager.HasEfx)
            {
                // Apply occlusion to direct path
                if (directFilter != 0)
                {
                    audioManager.UpdateFilter(directFilter, gainLF, gainHF);
                }

                // Apply reverb wetness to the reverb send
                // Higher wetness = more reverb, lower wetness = less reverb
                if (reverbSendFilter != 0)
                {
                    // Use reverbWetness to control how much signal goes to reverb
                    // When wetness is 1.0 = full reverb, when 0.0 = no reverb
                    audioManager.UpdateFilter(reverbSendFilter, reverbWetness, reverbWetness);
                }
            }
            else
            {
                // Fallback without EFX
                float occlusionFactor = (gainLF * 0.3f + gainHF * 0.7f);
                float effectiveVolume = baseVolume * Math.Max(0.05f, occlusionFactor);
                al.SetSourceProperty(sourceId, SourceFloat.Gain, effectiveVolume);
            }
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;

            al.SourceStop(sourceId);

            if (audioManager != null && audioManager.HasEfx)
            {
                // Disconnect from reverb
                if (cachedAlSource3i != null)
                {
                    cachedAlSource3i(sourceId, AL_AUXILIARY_SEND_FILTER, 0, 0, 0);
                }

                // Remove direct filter
                al.SetSourceProperty(sourceId, (SourceInteger)AL_DIRECT_FILTER, 0);

                // Delete filters
                if (directFilter != 0)
                {
                    audioManager.DeleteFilter(directFilter);
                    directFilter = 0;
                }
                if (reverbSendFilter != 0)
                {
                    audioManager.DeleteFilter(reverbSendFilter);
                    reverbSendFilter = 0;
                }
            }

            al.DeleteSource(sourceId);

            if (voice != null && raytracingContext != null)
            {
                raytracingContext.RemoveVoice(voice);
                voice = null;
            }
        }
    }

    public class StaticPositionF : IPositionF
    {
        private Vector3F position;

        public StaticPositionF(float x, float y, float z)
        {
            position = new Vector3F(x, y, z);
        }

        public Vector3F GetPosition()
        {
            return position;
        }

        public void SetPosition(float x, float y, float z)
        {
            position = new Vector3F(x, y, z);
        }
    }
}
