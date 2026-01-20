using System.Collections.Generic;
using Arch.Core;
using Arch.Core.Extensions;
using Microsoft.Xna.Framework;
using Game3.ECS.Components;

namespace Game3.ECS.Systems
{
    /// <summary>
    /// Handles delayed removal of entities marked with ToBeDestroyed.
    /// Should run at the end of the update loop to ensure all systems have processed.
    /// </summary>
    public class RemovalSystem
    {
        private readonly World world;
        private readonly QueryDescription removalQuery;

        public RemovalSystem(World world)
        {
            this.world = world;
            this.removalQuery = new QueryDescription()
                .WithAll<ToBeDestroyed>();
        }

        public void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            var entitiesToRemove = new List<Entity>();

            world.Query(in removalQuery, (Entity entity, ref ToBeDestroyed toBeDestroyed) =>
            {
                toBeDestroyed.Timer -= deltaTime;
                if (toBeDestroyed.Timer <= 0)
                {
                    entitiesToRemove.Add(entity);
                }
            });

            // Destroy entities outside of query
            foreach (var entity in entitiesToRemove)
            {
                if (entity.IsAlive())
                {
                    // Clean up any audio sources before destroying
                    CleanupEntity(entity);
                    world.Destroy(entity);
                }
            }

            if (entitiesToRemove.Count > 0)
            {
                Program.Log($"RemovalSystem: Removed {entitiesToRemove.Count} entities");
            }
        }

        private void CleanupEntity(Entity entity)
        {
            // Stop any ambient sounds
            if (entity.Has<AmbientSound>())
            {
                var ambient = entity.Get<AmbientSound>();
                ambient.Source?.Stop();
                ambient.Source?.Dispose();
            }

            // Stop any audio source refs
            if (entity.Has<AudioSourceRef>())
            {
                var audioRef = entity.Get<AudioSourceRef>();
                audioRef.Source?.Stop();
                audioRef.Source?.Dispose();
            }
        }
    }
}
