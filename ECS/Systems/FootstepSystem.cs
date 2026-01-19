using System;
using Arch.Core;
using Microsoft.Xna.Framework;
using Game3.ECS.Components;

namespace Game3.ECS.Systems
{
    /// <summary>
    /// Plays footstep sounds when entities are moving and grounded.
    /// Pure ECS system - uses SharedResources for stair detection.
    /// </summary>
    public class FootstepSystem
    {
        private readonly World world;
        private readonly SharedResources resources;
        private readonly QueryDescription footstepQuery;
        private readonly Random random = new Random();

        public FootstepSystem(World world, SharedResources resources)
        {
            this.world = world;
            this.resources = resources;
            this.footstepQuery = new QueryDescription()
                .WithAll<Position, PlayerMovement, Gravity, FootstepSounds>();
        }

        public void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            world.Query(in footstepQuery, (ref Position pos, ref PlayerMovement movement,
                ref Gravity gravity, ref FootstepSounds footsteps) =>
            {
                // Update stair status using SharedResources (ECS query)
                Vector3 position = pos.ToVector3();
                footsteps.IsOnStair = resources.IsOnStair(position);

                // Only play footsteps if moving and grounded
                if (movement.IsMoving && gravity.IsGrounded)
                {
                    footsteps.Timer += deltaTime;
                    if (footsteps.Timer >= footsteps.Interval)
                    {
                        footsteps.Timer = 0f;
                        PlayFootstep(pos, footsteps);
                    }
                }
                else
                {
                    // Reset timer to play immediately when starting to move
                    footsteps.Timer = footsteps.Interval;
                }
            });
        }

        private void PlayFootstep(Position pos, FootstepSounds footsteps)
        {
            float footX = pos.X;
            float footY = pos.Y;
            float footZ = pos.Z + 0.1f;

            string soundPath;
            if (footsteps.IsOnStair)
            {
                int stepNumber = random.Next(1, footsteps.StairSoundCount + 1);
                soundPath = $"{footsteps.StairSoundFolder}/{stepNumber}.wav";
            }
            else
            {
                int stepNumber = random.Next(1, footsteps.NormalSoundCount + 1);
                soundPath = $"{footsteps.NormalSoundFolder}/{stepNumber}.ogg";
            }

            resources.AudioManager.Play3D(soundPath, footX, footY, footZ, false, footsteps.Volume);
        }
    }
}
