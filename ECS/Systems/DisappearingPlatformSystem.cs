using Arch.Core;
using Arch.Core.Extensions;
using Microsoft.Xna.Framework;
using Game3.ECS.Components;

namespace Game3.ECS.Systems
{
    /// <summary>
    /// Handles platforms that appear and disappear on a timer.
    /// </summary>
    public class DisappearingPlatformSystem
    {
        private readonly World world;
        private readonly SharedResources resources;
        private readonly QueryDescription platformQuery;
        private readonly QueryDescription playerQuery;

        public DisappearingPlatformSystem(World world, SharedResources resources)
        {
            this.world = world;
            this.resources = resources;
            this.platformQuery = new QueryDescription()
                .WithAll<Position, DisappearingPlatform, PlatformData>();
            this.playerQuery = new QueryDescription()
                .WithAll<Position, PlayerControlled>();
        }

        public void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Get player position
            Vector3 playerPos = Vector3.Zero;
            bool hasPlayer = false;

            world.Query(in playerQuery, (ref Position pos) =>
            {
                playerPos = pos.ToVector3();
                hasPlayer = true;
            });

            world.Query(in platformQuery, (Entity entity, ref Position pos, ref DisappearingPlatform platform, ref PlatformData platformData) =>
            {
                // Check if player is on this platform
                if (hasPlayer)
                {
                    bool wasOnPlatform = platform.PlayerOnPlatform;
                    platform.PlayerOnPlatform = platformData.Contains(playerPos) &&
                        System.Math.Abs(playerPos.Z - platformData.Height) < 0.5f;

                    // Trigger collapse if player just stepped on
                    if (platform.PlayerOnPlatform && !wasOnPlatform && platform.CollapseDelay > 0 && !platform.IsCollapsing)
                    {
                        platform.IsCollapsing = true;
                        platform.Timer = platform.CollapseDelay;
                    }
                }

                // Update timer
                platform.Timer -= deltaTime;

                if (platform.Timer <= 0)
                {
                    if (platform.IsVisible)
                    {
                        // Disappear
                        platform.IsVisible = false;
                        platform.IsCollapsing = false;
                        platform.Timer = platform.InvisibleDuration;

                        PlayDisappearSound(ref platform, pos);

                        // Add Disabled tag to make platform non-solid
                        if (!entity.Has<Disabled>())
                        {
                            entity.Add(new Disabled());
                        }
                    }
                    else
                    {
                        // Reappear
                        platform.IsVisible = true;
                        platform.Timer = platform.VisibleDuration;

                        PlayAppearSound(ref platform, pos);

                        // Remove Disabled tag to make platform solid again
                        if (entity.Has<Disabled>())
                        {
                            entity.Remove<Disabled>();
                        }
                    }
                }
            });
        }

        private void PlayDisappearSound(ref DisappearingPlatform platform, Position pos)
        {
            if (!string.IsNullOrEmpty(platform.DisappearSound))
            {
                resources.AudioManager.Play3D(platform.DisappearSound, pos.X, pos.Y, pos.Z, false, 0.7f);
            }
        }

        private void PlayAppearSound(ref DisappearingPlatform platform, Position pos)
        {
            if (!string.IsNullOrEmpty(platform.AppearSound))
            {
                resources.AudioManager.Play3D(platform.AppearSound, pos.X, pos.Y, pos.Z, false, 0.7f);
            }
        }
    }
}
