using Arch.Core;
using Microsoft.Xna.Framework;
using Game3.ECS.Components;

namespace Game3.ECS.Systems
{
    /// <summary>
    /// Manages ambient sound sources.
    /// Pure ECS system - initializes and manages 3D sound sources.
    /// </summary>
    public class AmbientSoundSystem
    {
        private readonly World world;
        private readonly SharedResources resources;
        private readonly QueryDescription soundQuery;

        public AmbientSoundSystem(World world, SharedResources resources)
        {
            this.world = world;
            this.resources = resources;
            this.soundQuery = new QueryDescription()
                .WithAll<Position, AmbientSound>();
        }

        public void Update(GameTime gameTime)
        {
            world.Query(in soundQuery, (ref Position pos, ref AmbientSound sound) =>
            {
                // Initialize sound if not done yet
                if (!sound.IsInitialized && !string.IsNullOrEmpty(sound.SoundPath))
                {
                    sound.Source = resources.AudioManager.Play3D(
                        sound.SoundPath,
                        pos.X, pos.Y, pos.Z,
                        sound.Looping,
                        sound.Volume
                    );
                    sound.IsInitialized = true;

                    if (sound.Source != null)
                    {
                        Program.Log($"AmbientSoundSystem: Started {sound.SoundPath} at ({pos.X:F1}, {pos.Y:F1}, {pos.Z:F1})");
                    }
                }
            });
        }

        /// <summary>
        /// Sets the volume of an ambient sound.
        /// </summary>
        public void SetVolume(ref AmbientSound sound, float volume)
        {
            sound.Volume = volume;
            sound.Source?.SetVolume(volume);
        }

        /// <summary>
        /// Stops an ambient sound.
        /// </summary>
        public void Stop(ref AmbientSound sound)
        {
            sound.Source?.Stop();
            sound.IsInitialized = false;
        }
    }
}
