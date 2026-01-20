using System;
using Arch.Core;
using Microsoft.Xna.Framework;
using Game3.ECS.Components;

namespace Game3.ECS.Systems
{
    /// <summary>
    /// Updates moving platforms, interpolating between start and end positions.
    /// </summary>
    public class MovingPlatformSystem
    {
        private readonly World world;
        private readonly SharedResources resources;
        private readonly QueryDescription platformQuery;

        public MovingPlatformSystem(World world, SharedResources resources)
        {
            this.world = world;
            this.resources = resources;
            this.platformQuery = new QueryDescription()
                .WithAll<Position, MovingPlatform, PlatformData>()
                .WithNone<Disabled>();
        }

        public void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            world.Query(in platformQuery, (ref Position pos, ref MovingPlatform platform, ref PlatformData platformData) =>
            {
                // Handle reversal request from switches
                if (platform.IsReversing)
                {
                    platform.MovingForward = !platform.MovingForward;
                    platform.IsReversing = false;
                    platform.IsMoving = true;
                }

                if (!platform.IsMoving)
                    return;

                // Handle wait time at endpoints
                if (platform.WaitTimer > 0)
                {
                    platform.WaitTimer -= deltaTime;
                    return;
                }

                // Calculate movement
                float totalDistance = Vector3.Distance(platform.Start, platform.End);
                float progressDelta = (platform.Speed * deltaTime) / totalDistance;

                if (platform.MovingForward)
                {
                    platform.Progress += progressDelta;
                    if (platform.Progress >= 1f)
                    {
                        platform.Progress = 1f;
                        OnReachedEnd(ref platform);
                    }
                }
                else
                {
                    platform.Progress -= progressDelta;
                    if (platform.Progress <= 0f)
                    {
                        platform.Progress = 0f;
                        OnReachedStart(ref platform);
                    }
                }

                // Update position
                Vector3 newPos = platform.CurrentPosition;
                pos.X = newPos.X;
                pos.Y = newPos.Y;
                pos.Z = newPos.Z;

                // Update platform bounds (keep size, move position)
                float halfWidth = (platformData.MaxX - platformData.MinX) / 2f;
                float halfLength = (platformData.MaxY - platformData.MinY) / 2f;
                platformData.MinX = newPos.X - halfWidth;
                platformData.MaxX = newPos.X + halfWidth;
                platformData.MinY = newPos.Y - halfLength;
                platformData.MaxY = newPos.Y + halfLength;
                platformData.Height = newPos.Z;
            });
        }

        private void OnReachedEnd(ref MovingPlatform platform)
        {
            if (platform.PingPong)
            {
                platform.MovingForward = false;
                platform.WaitTimer = platform.WaitTime;

                if (!string.IsNullOrEmpty(platform.StopSound))
                {
                    var pos = platform.CurrentPosition;
                    resources.AudioManager.Play3D(platform.StopSound, pos.X, pos.Y, pos.Z, false, 0.6f);
                }
            }
            else
            {
                // Loop: jump back to start
                platform.Progress = 0f;
                platform.WaitTimer = platform.WaitTime;
            }
        }

        private void OnReachedStart(ref MovingPlatform platform)
        {
            if (platform.PingPong)
            {
                platform.MovingForward = true;
                platform.WaitTimer = platform.WaitTime;

                if (!string.IsNullOrEmpty(platform.StopSound))
                {
                    var pos = platform.CurrentPosition;
                    resources.AudioManager.Play3D(platform.StopSound, pos.X, pos.Y, pos.Z, false, 0.6f);
                }
            }
        }
    }
}
