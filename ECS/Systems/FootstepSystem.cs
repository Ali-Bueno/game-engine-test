using System;
using Arch.Core;
using Microsoft.Xna.Framework;
using Game3.Audio;
using Game3.ECS.Components;
using vaudio;

namespace Game3.ECS.Systems
{
    /// <summary>
    /// Plays footstep sounds when entities are moving and grounded.
    /// Uses MaterialSoundRegistry for material-based sound selection.
    /// </summary>
    public class FootstepSystem
    {
        private readonly World world;
        private readonly SharedResources resources;
        private readonly QueryDescription footstepQuery;
        private readonly QueryDescription surfaceQuery;

        public FootstepSystem(World world, SharedResources resources)
        {
            this.world = world;
            this.resources = resources;
            this.footstepQuery = new QueryDescription()
                .WithAll<Position, PlayerMovement, Gravity, FootstepSounds>();
            this.surfaceQuery = new QueryDescription()
                .WithAll<SurfaceMaterial>();
        }

        public void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            world.Query(in footstepQuery, (ref Position pos, ref PlayerMovement movement,
                ref Gravity gravity, ref FootstepSounds footsteps) =>
            {
                Vector3 position = pos.ToVector3();

                // Update stair status using SharedResources (ECS query)
                footsteps.IsOnStair = resources.IsOnStair(position);

                // Detect current material from SurfaceMaterial entities
                footsteps.CurrentMaterial = DetectMaterial(position, footsteps.CurrentMaterial);

                // Calculate effective interval (slower on stairs)
                float effectiveInterval = footsteps.IsOnStair
                    ? footsteps.Interval * footsteps.StairIntervalMultiplier
                    : footsteps.Interval;

                // Only play footsteps if moving and grounded
                if (movement.IsMoving && gravity.IsGrounded)
                {
                    footsteps.Timer += deltaTime;
                    if (footsteps.Timer >= effectiveInterval)
                    {
                        footsteps.Timer = 0f;
                        PlayFootstep(pos, footsteps);
                    }
                }
                else
                {
                    // Reset timer to play immediately when starting to move
                    footsteps.Timer = effectiveInterval;
                }
            });
        }

        /// <summary>
        /// Detects the material at a given position from SurfaceMaterial entities.
        /// Returns the material with highest priority if multiple surfaces overlap.
        /// </summary>
        private MaterialType DetectMaterial(Vector3 position, MaterialType fallback)
        {
            MaterialType detectedMaterial = fallback;
            int highestPriority = int.MinValue;

            world.Query(in surfaceQuery, (ref SurfaceMaterial surface) =>
            {
                if (surface.Contains(position.X, position.Y, position.Z) &&
                    surface.Priority > highestPriority)
                {
                    detectedMaterial = surface.Material;
                    highestPriority = surface.Priority;
                }
            });

            return detectedMaterial;
        }

        private void PlayFootstep(Position pos, FootstepSounds footsteps)
        {
            float footX = pos.X;
            float footY = pos.Y;
            float footZ = pos.Z + 0.1f;

            // Get random sound from MaterialSoundRegistry
            string soundPath = MaterialSoundRegistry.Instance.GetRandomSound(
                footsteps.CurrentMaterial,
                SoundCategory.Steps);

            if (soundPath != null)
            {
                resources.AudioManager.Play3D(soundPath, footX, footY, footZ, false, footsteps.Volume);
            }
        }
    }
}
