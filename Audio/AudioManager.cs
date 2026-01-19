using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Reflection;
using Silk.NET.OpenAL;
using vaudio;
using NVorbis;
using NAudio.Wave;

namespace Game3.Audio
{
    public class AudioManager : IDisposable
    {
        // Preload OpenAL Soft from local directory
        private static IntPtr openALHandle;

        static AudioManager()
        {
            PreloadOpenALSoft();
        }

        private static void PreloadOpenALSoft()
        {
            try
            {
                // Get the directory where the executable is located
                string exePath = AppContext.BaseDirectory;
                string openALPath = Path.Combine(exePath, "OpenAL32.dll");

                if (File.Exists(openALPath))
                {
                    openALHandle = NativeLibrary.Load(openALPath);
                    Program.Log($"Preloaded OpenAL Soft from: {openALPath}");
                }
                else
                {
                    Program.Log($"OpenAL32.dll not found at: {openALPath}");
                }
            }
            catch (Exception ex)
            {
                Program.Log($"Failed to preload OpenAL Soft: {ex.Message}");
            }
        }

        private AL al;
        private ALContext alContext;
        private unsafe Device* device;
        private unsafe Context* context;

        private RaytracingContext raytracingContext;
        private Dictionary<string, uint> audioBuffers;
        private Dictionary<string, uint> monoAudioBuffers;
        private List<AudioSource> activeSources;
        private bool disposed;

        // EFX function pointers (using ref to avoid unsafe pointer issues)
        private delegate void GenEffectsDelegate(int n, ref uint effects);
        private delegate void DeleteEffectsDelegate(int n, ref uint effects);
        private delegate void EffectiDelegate(uint effect, int param, int value);
        private delegate void EffectfDelegate(uint effect, int param, float value);
        private delegate void GenFiltersDelegate(int n, ref uint filters);
        private delegate void DeleteFiltersDelegate(int n, ref uint filters);
        private delegate void FilteriDelegate(uint filter, int param, int value);
        private delegate void FilterfDelegate(uint filter, int param, float value);
        private delegate void GenAuxiliaryEffectSlotsDelegate(int n, ref uint slots);
        private delegate void DeleteAuxiliaryEffectSlotsDelegate(int n, ref uint slots);
        private delegate void AuxiliaryEffectSlotiDelegate(uint slot, int param, int value);

        private GenEffectsDelegate alGenEffects;
        private DeleteEffectsDelegate alDeleteEffects;
        private EffectiDelegate alEffecti;
        private EffectfDelegate alEffectf;
        private GenFiltersDelegate alGenFilters;
        private DeleteFiltersDelegate alDeleteFilters;
        private FilteriDelegate alFilteri;
        private FilterfDelegate alFilterf;
        private GenAuxiliaryEffectSlotsDelegate alGenAuxiliaryEffectSlots;
        private DeleteAuxiliaryEffectSlotsDelegate alDeleteAuxiliaryEffectSlots;
        private AuxiliaryEffectSlotiDelegate alAuxiliaryEffectSloti;

        private bool hasEfx;
        private uint reverbEffect;
        private uint effectSlot;

        // EFX Constants
        private const int AL_EFFECT_TYPE = 0x8001;
        private const int AL_EFFECT_EAXREVERB = 0x8000;
        private const int AL_FILTER_TYPE = 0x8001;
        private const int AL_FILTER_LOWPASS = 0x0001;
        private const int AL_LOWPASS_GAIN = 0x0001;
        private const int AL_LOWPASS_GAINHF = 0x0002;
        private const int AL_EFFECTSLOT_EFFECT = 0x0001;
        private const int AL_DIRECT_FILTER = 0x20005;
        private const int AL_AUXILIARY_SEND_FILTER = 0x20006;

        // EAX Reverb parameters
        private const int AL_EAXREVERB_DENSITY = 0x0001;
        private const int AL_EAXREVERB_DIFFUSION = 0x0002;
        private const int AL_EAXREVERB_GAIN = 0x0003;
        private const int AL_EAXREVERB_GAINHF = 0x0004;
        private const int AL_EAXREVERB_GAINLF = 0x0005;
        private const int AL_EAXREVERB_DECAY_TIME = 0x0006;
        private const int AL_EAXREVERB_DECAY_HFRATIO = 0x0007;
        private const int AL_EAXREVERB_DECAY_LFRATIO = 0x0008;
        private const int AL_EAXREVERB_REFLECTIONS_GAIN = 0x0009;
        private const int AL_EAXREVERB_REFLECTIONS_DELAY = 0x000A;
        private const int AL_EAXREVERB_LATE_REVERB_GAIN = 0x000C;
        private const int AL_EAXREVERB_LATE_REVERB_DELAY = 0x000D;
        private const int AL_EAXREVERB_ECHO_TIME = 0x000F;
        private const int AL_EAXREVERB_ECHO_DEPTH = 0x0010;
        private const int AL_EAXREVERB_MODULATION_TIME = 0x0011;
        private const int AL_EAXREVERB_MODULATION_DEPTH = 0x0012;
        private const int AL_EAXREVERB_AIR_ABSORPTION_GAINHF = 0x0013;
        private const int AL_EAXREVERB_HFREFERENCE = 0x0014;
        private const int AL_EAXREVERB_LFREFERENCE = 0x0015;
        private const int AL_EAXREVERB_ROOM_ROLLOFF_FACTOR = 0x0016;
        private const int AL_EAXREVERB_DECAY_HFLIMIT = 0x0017;

        public RaytracingContext RaytracingContext => raytracingContext;
        public bool HasEfx => hasEfx;
        public uint EffectSlot => effectSlot;

        public AudioManager(Vector3F worldSize, bool enableDebugWindow = false)
        {
            audioBuffers = new Dictionary<string, uint>();
            monoAudioBuffers = new Dictionary<string, uint>();
            activeSources = new List<AudioSource>();

            InitializeOpenAL();
            InitializeEFX();
            InitializeVaudio(worldSize, enableDebugWindow);
        }

        // HRTF constants
        private const int ALC_HRTF_SOFT = 0x1992;
        private const int ALC_HRTF_STATUS_SOFT = 0x1993;
        private const int ALC_HRTF_DISABLED_SOFT = 0x0000;
        private const int ALC_HRTF_ENABLED_SOFT = 0x0001;
        private const int ALC_HRTF_DENIED_SOFT = 0x0002;
        private const int ALC_HRTF_REQUIRED_SOFT = 0x0003;
        private const int ALC_HRTF_HEADPHONES_DETECTED_SOFT = 0x0004;
        private const int ALC_HRTF_UNSUPPORTED_FORMAT_SOFT = 0x0005;
        private const int ALC_TRUE = 1;

        private unsafe void InitializeOpenAL()
        {
            alContext = ALContext.GetApi();
            al = AL.GetApi();

            device = alContext.OpenDevice(null);
            if (device == null)
            {
                throw new Exception("Could not open OpenAL device");
            }

            // Check if HRTF is supported
            bool hrtfSupported = alContext.IsExtensionPresent(device, "ALC_SOFT_HRTF");
            Program.Log($"HRTF extension present: {hrtfSupported}");

            if (hrtfSupported)
            {
                // Create context with HRTF enabled
                int* contextAttribs = stackalloc int[3];
                contextAttribs[0] = ALC_HRTF_SOFT;
                contextAttribs[1] = ALC_TRUE;
                contextAttribs[2] = 0; // Null terminator

                context = alContext.CreateContext(device, contextAttribs);
            }
            else
            {
                context = alContext.CreateContext(device, null);
            }

            alContext.MakeContextCurrent(context);

            // Check HRTF status
            if (hrtfSupported)
            {
                int hrtfStatus = 0;
                alContext.GetContextProperty(device, (GetContextInteger)ALC_HRTF_STATUS_SOFT, 1, &hrtfStatus);
                string statusStr = hrtfStatus switch
                {
                    ALC_HRTF_DISABLED_SOFT => "Disabled",
                    ALC_HRTF_ENABLED_SOFT => "Enabled",
                    ALC_HRTF_DENIED_SOFT => "Denied",
                    ALC_HRTF_REQUIRED_SOFT => "Required",
                    ALC_HRTF_HEADPHONES_DETECTED_SOFT => "Headphones Detected",
                    ALC_HRTF_UNSUPPORTED_FORMAT_SOFT => "Unsupported Format",
                    _ => $"Unknown ({hrtfStatus})"
                };
                Program.Log($"HRTF status: {statusStr}");
            }

            al.SetListenerProperty(ListenerFloat.Gain, 1.0f);
            al.DistanceModel(DistanceModel.InverseDistanceClamped);

            Program.Log("OpenAL initialized successfully");
        }

        private unsafe void InitializeEFX()
        {
            // Check if EFX is supported
            bool efxSupported = alContext.IsExtensionPresent(device, "ALC_EXT_EFX");
            Program.Log($"EFX extension present: {efxSupported}");

            if (!efxSupported)
            {
                hasEfx = false;
                return;
            }

            try
            {
                // Get EFX function pointers
                alGenEffects = Marshal.GetDelegateForFunctionPointer<GenEffectsDelegate>(
                    (nint)al.GetProcAddress("alGenEffects"));
                alDeleteEffects = Marshal.GetDelegateForFunctionPointer<DeleteEffectsDelegate>(
                    (nint)al.GetProcAddress("alDeleteEffects"));
                alEffecti = Marshal.GetDelegateForFunctionPointer<EffectiDelegate>(
                    (nint)al.GetProcAddress("alEffecti"));
                alEffectf = Marshal.GetDelegateForFunctionPointer<EffectfDelegate>(
                    (nint)al.GetProcAddress("alEffectf"));
                alGenFilters = Marshal.GetDelegateForFunctionPointer<GenFiltersDelegate>(
                    (nint)al.GetProcAddress("alGenFilters"));
                alDeleteFilters = Marshal.GetDelegateForFunctionPointer<DeleteFiltersDelegate>(
                    (nint)al.GetProcAddress("alDeleteFilters"));
                alFilteri = Marshal.GetDelegateForFunctionPointer<FilteriDelegate>(
                    (nint)al.GetProcAddress("alFilteri"));
                alFilterf = Marshal.GetDelegateForFunctionPointer<FilterfDelegate>(
                    (nint)al.GetProcAddress("alFilterf"));
                alGenAuxiliaryEffectSlots = Marshal.GetDelegateForFunctionPointer<GenAuxiliaryEffectSlotsDelegate>(
                    (nint)al.GetProcAddress("alGenAuxiliaryEffectSlots"));
                alDeleteAuxiliaryEffectSlots = Marshal.GetDelegateForFunctionPointer<DeleteAuxiliaryEffectSlotsDelegate>(
                    (nint)al.GetProcAddress("alDeleteAuxiliaryEffectSlots"));
                alAuxiliaryEffectSloti = Marshal.GetDelegateForFunctionPointer<AuxiliaryEffectSlotiDelegate>(
                    (nint)al.GetProcAddress("alAuxiliaryEffectSloti"));

                // Create reverb effect
                uint effect = 0;
                alGenEffects(1, ref effect);
                reverbEffect = effect;

                // Set effect type to EAX Reverb
                alEffecti(reverbEffect, AL_EFFECT_TYPE, AL_EFFECT_EAXREVERB);

                // Create auxiliary effect slot
                uint slot = 0;
                alGenAuxiliaryEffectSlots(1, ref slot);
                effectSlot = slot;

                // Attach reverb effect to slot
                alAuxiliaryEffectSloti(effectSlot, AL_EFFECTSLOT_EFFECT, (int)reverbEffect);

                hasEfx = true;
                Program.Log($"EFX initialized - reverb effect: {reverbEffect}, effect slot: {effectSlot}");
            }
            catch (Exception ex)
            {
                Program.Log($"EFX initialization failed: {ex.Message}");
                hasEfx = false;
            }
        }

        private void InitializeVaudio(Vector3F worldSize, bool enableDebugWindow)
        {
            // worldPosition is the corner of the volume, worldSize extends from there
            // Map goes from X=-20 to X=30, Y=0 to Y=75, Z=0 to Z=10
            // Position at (-25, -5, -1) with size (60, 80, 12) covers everything with margin
            var settings = new RaytracingContextSettings()
            {
                worldPosition = new Vector3F(-25, -5, -1),
                worldSize = worldSize,
                renderingEnabled = enableDebugWindow,
                reverbRayCount = 1024,
                occlusionRayCount = 512,
                permeationRayCount = 256,
                trailBounceCount = 8,
                maxVoices = 32,
                metersPerUnit = 1.0f,
            };

            raytracingContext = new RaytracingContext(settings);

            // Configure materials with proper acoustic properties
            ConfigureMaterials();

            Program.Log("Vaudio raytracing context initialized with materials configured");
        }

        private void ConfigureMaterials()
        {
            // =============================================
            // MATERIALES PARA RAYTRACING DE AUDIO
            // =============================================
            // Absorción: cuánto sonido absorbe (0=refleja todo, 1=absorbe todo)
            // Scattering: cuánto dispersa el sonido (difusión)
            // Transmisión: cuánto sonido pasa A TRAVÉS del material (0=bloquea, 1=pasa todo)
            //   - LF = Low Frequency (graves)
            //   - HF = High Frequency (agudos)
            // Nota: Las altas frecuencias se absorben/bloquean más fácilmente que las bajas

            // Concrete - Hormigón grueso, muy denso, bloquea casi todo
            var concrete = raytracingContext.GetMaterial(MaterialType.Concrete);
            concrete.AbsorptionLF = 0.01f;   // Muy reflectivo en graves
            concrete.AbsorptionHF = 0.02f;   // Muy reflectivo en agudos
            concrete.ScatteringLF = 0.10f;
            concrete.ScatteringHF = 0.20f;
            concrete.TransmissionLF = 0.02f; // Casi nada pasa (graves pasan un poco más)
            concrete.TransmissionHF = 0.01f; // Agudos bloqueados casi completamente

            // Brick - Ladrillo, denso pero permite algo de transmisión
            var brick = raytracingContext.GetMaterial(MaterialType.Brick);
            brick.AbsorptionLF = 0.03f;
            brick.AbsorptionHF = 0.06f;
            brick.ScatteringLF = 0.20f;
            brick.ScatteringHF = 0.40f;
            brick.TransmissionLF = 0.05f;  // Algo de graves pasan (se escucha amortiguado)
            brick.TransmissionHF = 0.02f;  // Agudos muy bloqueados

            // Metal - Puerta metálica, muy reflectivo, bloquea bien
            var metal = raytracingContext.GetMaterial(MaterialType.Metal);
            metal.AbsorptionLF = 0.01f;
            metal.AbsorptionHF = 0.02f;
            metal.ScatteringLF = 0.05f;
            metal.ScatteringHF = 0.10f;
            metal.TransmissionLF = 0.03f;  // Metal deja pasar un poco de graves (vibra)
            metal.TransmissionHF = 0.01f;  // Agudos bloqueados

            // Wood - Madera (para escaleras, puertas de madera)
            var wood = raytracingContext.GetMaterial(MaterialType.Brick); // Usamos Brick como base
            // Nota: Si vaudio tiene MaterialType.Wood, usarlo en su lugar

            // Fabric/Carpet - Tela/alfombra (absorbe mucho, no transmite)
            // Se puede usar para áreas que absorben sonido

            Program.Log("Materials configured with realistic transmission values:");
            Program.Log($"  Concrete: Trans LF={concrete.TransmissionLF}, HF={concrete.TransmissionHF}");
            Program.Log($"  Brick: Trans LF={brick.TransmissionLF}, HF={brick.TransmissionHF}");
            Program.Log($"  Metal: Trans LF={metal.TransmissionLF}, HF={metal.TransmissionHF}");
        }

        public void UpdateListener(float x, float y, float z, float yaw = 0, float pitch = 0)
        {
            // Convert game coordinates to OpenAL (swap Y and Z)
            al.SetListenerProperty(ListenerVector3.Position, x, z, -y);

            float frontX = (float)Math.Sin(yaw);
            float frontY = 0;
            float frontZ = -(float)Math.Cos(yaw);

            unsafe
            {
                float* orientation = stackalloc float[6];
                orientation[0] = frontX;
                orientation[1] = frontY;
                orientation[2] = frontZ;
                orientation[3] = 0;
                orientation[4] = 1;
                orientation[5] = 0;
                al.SetListenerProperty(ListenerFloatArray.Orientation, orientation);
            }

            raytracingContext.UpdateListener(new Vector3F(x, y, z), pitch, yaw);
        }

        private int reverbLogCounter = 0;

        public void UpdateReverb()
        {
            if (!hasEfx || !raytracingContext.ReverbCalculated) return;

            try
            {
                var eax = raytracingContext.ListenerEAX;

                // Log reverb values periodically
                reverbLogCounter++;
                if (reverbLogCounter == 60) // Every ~1 second at 60fps
                {
                    reverbLogCounter = 0;
                    Program.Log($"Reverb: Decay={eax.DecayTime:F2}s, Density={eax.Density:F2}, Diffusion={eax.Diffusion:F2}, Gain={eax.Gain:F3}");
                }

                alEffectf(reverbEffect, AL_EAXREVERB_DENSITY, Math.Clamp(eax.Density, 0f, 1f));
                alEffectf(reverbEffect, AL_EAXREVERB_DIFFUSION, Math.Clamp(eax.Diffusion, 0f, 1f));
                alEffectf(reverbEffect, AL_EAXREVERB_GAIN, Math.Clamp(eax.Gain, 0f, 1f));
                alEffectf(reverbEffect, AL_EAXREVERB_GAINHF, Math.Clamp(eax.GainHF, 0f, 1f));
                alEffectf(reverbEffect, AL_EAXREVERB_GAINLF, Math.Clamp(eax.GainLF, 0f, 1f));
                alEffectf(reverbEffect, AL_EAXREVERB_DECAY_TIME, Math.Clamp(eax.DecayTime, 0.1f, 20f));
                alEffectf(reverbEffect, AL_EAXREVERB_DECAY_HFRATIO, Math.Clamp(eax.DecayHFRatio, 0.1f, 2f));
                alEffectf(reverbEffect, AL_EAXREVERB_DECAY_LFRATIO, Math.Clamp(eax.DecayLFRatio, 0.1f, 2f));
                alEffectf(reverbEffect, AL_EAXREVERB_REFLECTIONS_GAIN, Math.Clamp(eax.ReflectionsGain, 0f, 3.16f));
                alEffectf(reverbEffect, AL_EAXREVERB_REFLECTIONS_DELAY, Math.Clamp(eax.ReflectionsDelay, 0f, 0.3f));
                alEffectf(reverbEffect, AL_EAXREVERB_LATE_REVERB_GAIN, Math.Clamp(eax.LateReverbGain, 0f, 10f));
                alEffectf(reverbEffect, AL_EAXREVERB_LATE_REVERB_DELAY, Math.Clamp(eax.LateReverbDelay, 0f, 0.1f));
                alEffectf(reverbEffect, AL_EAXREVERB_ECHO_TIME, Math.Clamp(eax.EchoTime, 0.075f, 0.25f));
                alEffectf(reverbEffect, AL_EAXREVERB_ECHO_DEPTH, Math.Clamp(eax.EchoDepth, 0f, 1f));
                alEffectf(reverbEffect, AL_EAXREVERB_MODULATION_TIME, Math.Clamp(eax.ModulationTime, 0.04f, 4f));
                alEffectf(reverbEffect, AL_EAXREVERB_MODULATION_DEPTH, Math.Clamp(eax.ModulationDepth, 0f, 1f));
                alEffectf(reverbEffect, AL_EAXREVERB_AIR_ABSORPTION_GAINHF, Math.Clamp(eax.AirAbsorptionGainHF, 0.892f, 1f));
                alEffectf(reverbEffect, AL_EAXREVERB_HFREFERENCE, Math.Clamp(eax.HFReference, 1000f, 20000f));
                alEffectf(reverbEffect, AL_EAXREVERB_LFREFERENCE, Math.Clamp(eax.LFReference, 20f, 1000f));
                alEffectf(reverbEffect, AL_EAXREVERB_ROOM_ROLLOFF_FACTOR, Math.Clamp(eax.RoomRolloffFactor, 0f, 10f));
                alEffecti(reverbEffect, AL_EAXREVERB_DECAY_HFLIMIT, eax.DecayHFLimit);

                // Re-attach effect to slot to apply changes
                alAuxiliaryEffectSloti(effectSlot, AL_EFFECTSLOT_EFFECT, (int)reverbEffect);
            }
            catch (Exception ex)
            {
                Program.Log($"UpdateReverb error: {ex.Message}");
            }
        }

        public uint CreateFilter()
        {
            if (!hasEfx) return 0;

            uint filter = 0;
            alGenFilters(1, ref filter);
            alFilteri(filter, AL_FILTER_TYPE, AL_FILTER_LOWPASS);
            alFilterf(filter, AL_LOWPASS_GAIN, 1.0f);
            alFilterf(filter, AL_LOWPASS_GAINHF, 1.0f);
            return filter;
        }

        public void UpdateFilter(uint filter, float gainLF, float gainHF)
        {
            if (!hasEfx || filter == 0) return;

            alFilterf(filter, AL_LOWPASS_GAIN, Math.Clamp(gainLF, 0f, 1f));
            alFilterf(filter, AL_LOWPASS_GAINHF, Math.Clamp(gainHF, 0f, 1f));
        }

        public void DeleteFilter(uint filter)
        {
            if (!hasEfx || filter == 0) return;
            alDeleteFilters(1, ref filter);
        }

        public uint LoadSound(string filePath)
        {
            if (audioBuffers.TryGetValue(filePath, out uint existingBuffer))
                return existingBuffer;

            string actualPath = FindAudioFile(filePath);
            if (actualPath == null)
            {
                Program.Log($"Audio file not found: {filePath}");
                return 0;
            }

            uint buffer = al.GenBuffer();

            try
            {
                AudioData audioData = LoadAudioFile(actualPath);
                if (audioData.Data == null || audioData.Data.Length == 0)
                {
                    al.DeleteBuffer(buffer);
                    return 0;
                }

                unsafe
                {
                    fixed (byte* data = audioData.Data)
                    {
                        al.BufferData(buffer, audioData.Format, data, audioData.Data.Length, audioData.SampleRate);
                    }
                }

                audioBuffers[filePath] = buffer;
                return buffer;
            }
            catch (Exception ex)
            {
                Program.Log($"Error loading sound {filePath}: {ex.Message}");
                al.DeleteBuffer(buffer);
                return 0;
            }
        }

        public uint LoadSoundMono(string filePath)
        {
            if (monoAudioBuffers.TryGetValue(filePath, out uint existingBuffer))
                return existingBuffer;

            string actualPath = FindAudioFile(filePath);
            if (actualPath == null)
            {
                Program.Log($"Audio file not found: {filePath}");
                return 0;
            }

            uint buffer = al.GenBuffer();

            try
            {
                AudioData audioData = LoadAudioFile(actualPath);
                if (audioData.Data == null || audioData.Data.Length == 0)
                {
                    al.DeleteBuffer(buffer);
                    return 0;
                }

                if (audioData.Format == BufferFormat.Stereo16 || audioData.Format == BufferFormat.Stereo8)
                    audioData = ConvertToMono(audioData);

                unsafe
                {
                    fixed (byte* data = audioData.Data)
                    {
                        al.BufferData(buffer, audioData.Format, data, audioData.Data.Length, audioData.SampleRate);
                    }
                }

                monoAudioBuffers[filePath] = buffer;
                return buffer;
            }
            catch (Exception ex)
            {
                Program.Log($"Error loading sound {filePath}: {ex.Message}");
                al.DeleteBuffer(buffer);
                return 0;
            }
        }

        private string FindAudioFile(string filePath)
        {
            if (File.Exists(filePath)) return filePath;

            string basePath = Path.ChangeExtension(filePath, null);
            string[] extensions = { ".wav", ".ogg", ".mp3" };

            foreach (var ext in extensions)
            {
                string testPath = basePath + ext;
                if (File.Exists(testPath)) return testPath;
            }

            return null;
        }

        private AudioData LoadAudioFile(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLowerInvariant();

            return extension switch
            {
                ".wav" => LoadWavFile(filePath),
                ".ogg" => LoadOggFile(filePath),
                ".mp3" => LoadMp3File(filePath),
                _ => throw new NotSupportedException($"Audio format not supported: {extension}")
            };
        }

        private AudioData LoadWavFile(string filePath)
        {
            using var stream = File.OpenRead(filePath);
            using var reader = new BinaryReader(stream);

            string riff = new string(reader.ReadChars(4));
            if (riff != "RIFF") throw new Exception("Invalid WAV file");

            reader.ReadInt32();
            string wave = new string(reader.ReadChars(4));
            if (wave != "WAVE") throw new Exception("Invalid WAV file");

            reader.ReadChars(4); // fmt
            int fmtSize = reader.ReadInt32();
            reader.ReadInt16(); // audioFormat
            short numChannels = reader.ReadInt16();
            int sampleRate = reader.ReadInt32();
            reader.ReadInt32();
            reader.ReadInt16();
            short bitsPerSample = reader.ReadInt16();

            if (fmtSize > 16) reader.ReadBytes(fmtSize - 16);

            while (true)
            {
                string chunkId = new string(reader.ReadChars(4));
                int chunkSize = reader.ReadInt32();

                if (chunkId == "data")
                {
                    byte[] data = reader.ReadBytes(chunkSize);
                    BufferFormat format = GetBufferFormat(numChannels, bitsPerSample);
                    return new AudioData { Data = data, Format = format, SampleRate = sampleRate };
                }
                else
                {
                    reader.ReadBytes(chunkSize);
                }
            }
        }

        private AudioData LoadOggFile(string filePath)
        {
            using var vorbis = new VorbisReader(filePath);

            int channels = vorbis.Channels;
            int sampleRate = vorbis.SampleRate;
            long totalSamples = vorbis.TotalSamples;

            float[] floatBuffer = new float[totalSamples * channels];
            int samplesRead = vorbis.ReadSamples(floatBuffer, 0, floatBuffer.Length);

            byte[] data = new byte[samplesRead * 2];
            for (int i = 0; i < samplesRead; i++)
            {
                float sample = Math.Clamp(floatBuffer[i], -1f, 1f);
                short shortSample = (short)(sample * 32767);
                data[i * 2] = (byte)(shortSample & 0xFF);
                data[i * 2 + 1] = (byte)((shortSample >> 8) & 0xFF);
            }

            BufferFormat format = GetBufferFormat(channels, 16);
            return new AudioData { Data = data, Format = format, SampleRate = sampleRate };
        }

        private AudioData LoadMp3File(string filePath)
        {
            using var mp3Reader = new Mp3FileReader(filePath);
            using var waveStream = WaveFormatConversionStream.CreatePcmStream(mp3Reader);

            int channels = waveStream.WaveFormat.Channels;
            int sampleRate = waveStream.WaveFormat.SampleRate;
            int bitsPerSample = waveStream.WaveFormat.BitsPerSample;

            using var memoryStream = new MemoryStream();
            waveStream.CopyTo(memoryStream);
            byte[] data = memoryStream.ToArray();

            BufferFormat format = GetBufferFormat(channels, bitsPerSample);
            return new AudioData { Data = data, Format = format, SampleRate = sampleRate };
        }

        private BufferFormat GetBufferFormat(int channels, int bitsPerSample)
        {
            if (channels == 1)
                return bitsPerSample == 8 ? BufferFormat.Mono8 : BufferFormat.Mono16;
            else
                return bitsPerSample == 8 ? BufferFormat.Stereo8 : BufferFormat.Stereo16;
        }

        private AudioData ConvertToMono(AudioData stereoData)
        {
            bool is16Bit = stereoData.Format == BufferFormat.Stereo16;
            int bytesPerSample = is16Bit ? 2 : 1;
            int stereoFrameSize = bytesPerSample * 2;
            int frameCount = stereoData.Data.Length / stereoFrameSize;

            byte[] monoData = new byte[frameCount * bytesPerSample];

            if (is16Bit)
            {
                for (int i = 0; i < frameCount; i++)
                {
                    int stereoOffset = i * 4;
                    short left = (short)(stereoData.Data[stereoOffset] | (stereoData.Data[stereoOffset + 1] << 8));
                    short right = (short)(stereoData.Data[stereoOffset + 2] | (stereoData.Data[stereoOffset + 3] << 8));
                    short mono = (short)((left + right) / 2);

                    int monoOffset = i * 2;
                    monoData[monoOffset] = (byte)(mono & 0xFF);
                    monoData[monoOffset + 1] = (byte)((mono >> 8) & 0xFF);
                }
                return new AudioData { Data = monoData, Format = BufferFormat.Mono16, SampleRate = stereoData.SampleRate };
            }
            else
            {
                for (int i = 0; i < frameCount; i++)
                {
                    int stereoOffset = i * 2;
                    byte left = stereoData.Data[stereoOffset];
                    byte right = stereoData.Data[stereoOffset + 1];
                    monoData[i] = (byte)((left + right) / 2);
                }
                return new AudioData { Data = monoData, Format = BufferFormat.Mono8, SampleRate = stereoData.SampleRate };
            }
        }

        public AudioSource Play2D(string filePath, bool loop = false, float volume = 1.0f)
        {
            uint buffer = LoadSound(filePath);
            if (buffer == 0) return null;

            var source = new AudioSource(this, al, raytracingContext, buffer, false);
            source.SetVolume(volume);
            source.SetLoop(loop);
            source.Play();

            activeSources.Add(source);
            return source;
        }

        public AudioSource Play3D(string filePath, float x, float y, float z, bool loop = false, float volume = 1.0f)
        {
            uint buffer = LoadSoundMono(filePath);
            if (buffer == 0) return null;

            var source = new AudioSource(this, al, raytracingContext, buffer, true);
            source.SetPosition(x, y, z);
            source.SetVolume(volume);
            source.SetLoop(loop);
            source.Play();

            activeSources.Add(source);
            return source;
        }

        private int primitiveCount = 0;

        public void AddPrimitive(Primitive primitive)
        {
            raytracingContext.AddPrimitive(primitive);
            primitiveCount++;

            // Log every 10th primitive to avoid spam
            if (primitiveCount <= 5 || primitiveCount % 10 == 0)
            {
                if (primitive is PrismPrimitive prism)
                {
                    // Extract translation from matrix (M41, M42, M43)
                    var t = prism.transform;
                    Program.Log($"Primitive #{primitiveCount}: Prism at ({t.M41:F1}, {t.M42:F1}, {t.M43:F1}), size=({prism.size.X:F1}, {prism.size.Y:F1}, {prism.size.Z:F1}), material={prism.material}");
                }
            }
        }

        public void LogRaytracingState()
        {
            Program.Log($"=== Raytracing State ===");
            Program.Log($"  Primitives added: {primitiveCount}");
            Program.Log($"  World Position: {raytracingContext.WorldPosition}");
            Program.Log($"  World Size: {raytracingContext.WorldSize}");
            Program.Log($"  World Max: {raytracingContext.WorldMax}");
            Program.Log($"  Active Voices: {raytracingContext.ActiveVoiceCount}");
            Program.Log($"  Reverb Calculated: {raytracingContext.ReverbCalculated}");
            Program.Log($"  Rays Cast This Frame: {raytracingContext.RaysCastThisFrame}");
            Program.Log($"  Occlusion Ray Count: {raytracingContext.GetOcclusionRayCount()}");
            Program.Log($"  Permeation Ray Count: {raytracingContext.GetPermeationRayCount()}");
            Program.Log($"  Trail Bounce Count: {raytracingContext.GetTrailBounceCount()}");
        }

        public void RemovePrimitive(Primitive primitive)
        {
            raytracingContext.RemovePrimitive(primitive);
        }

        public void Update()
        {
            raytracingContext.Update();
            UpdateReverb();

            for (int i = activeSources.Count - 1; i >= 0; i--)
            {
                var source = activeSources[i];
                source.Update();

                if (!source.IsPlaying && !source.IsLooping)
                {
                    source.Dispose();
                    activeSources.RemoveAt(i);
                }
            }
        }

        public void StopAllSounds()
        {
            foreach (var source in activeSources)
            {
                source.Stop();
                source.Dispose();
            }
            activeSources.Clear();
        }

        public unsafe void Dispose()
        {
            if (disposed) return;
            disposed = true;

            StopAllSounds();

            foreach (var buffer in audioBuffers.Values)
                al.DeleteBuffer(buffer);
            audioBuffers.Clear();

            foreach (var buffer in monoAudioBuffers.Values)
                al.DeleteBuffer(buffer);
            monoAudioBuffers.Clear();

            if (hasEfx)
            {
                uint slot = effectSlot;
                alDeleteAuxiliaryEffectSlots(1, ref slot);
                uint effect = reverbEffect;
                alDeleteEffects(1, ref effect);
            }

            raytracingContext?.Dispose();

            if (context != null)
                alContext.DestroyContext(context);
            if (device != null)
                alContext.CloseDevice(device);

            al?.Dispose();
            alContext?.Dispose();
        }

        private struct AudioData
        {
            public byte[] Data;
            public BufferFormat Format;
            public int SampleRate;
        }
    }
}
