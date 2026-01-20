using Arch.Core;
using Arch.Core.Extensions;
using Microsoft.Xna.Framework;
using Game3.Audio;
using Game3.ECS.Components;

namespace Game3.ECS.Systems
{
    /// <summary>
    /// Processes damage events and handles entity death/destruction.
    /// </summary>
    public class HealthSystem
    {
        private readonly World world;
        private readonly SharedResources resources;
        private readonly QueryDescription damageQuery;
        private readonly QueryDescription destructibleQuery;

        public HealthSystem(World world, SharedResources resources)
        {
            this.world = world;
            this.resources = resources;
            this.damageQuery = new QueryDescription()
                .WithAll<Health, DamageEvent>();
            this.destructibleQuery = new QueryDescription()
                .WithAll<Health, Destructible, Position>();
        }

        public void Update(GameTime gameTime)
        {
            // Process damage events
            ProcessDamageEvents();

            // Check for deaths and trigger destruction
            CheckDeaths();
        }

        private void ProcessDamageEvents()
        {
            // Collect entities with damage events
            var entitiesToProcess = new System.Collections.Generic.List<Entity>();

            world.Query(in damageQuery, (Entity entity, ref Health health, ref DamageEvent damage) =>
            {
                // Apply damage
                health.TakeDamage(damage.Amount);

                // Play impact sound if entity has Destructible component
                if (entity.Has<Destructible>() && entity.Has<Position>())
                {
                    var destructible = entity.Get<Destructible>();
                    var pos = entity.Get<Position>();
                    PlayImpactSound(destructible.Material, pos.ToVector3(), destructible.SoundVolume);
                }

                entitiesToProcess.Add(entity);
            });

            // Remove DamageEvent components after processing
            foreach (var entity in entitiesToProcess)
            {
                if (entity.IsAlive())
                {
                    entity.Remove<DamageEvent>();
                }
            }
        }

        private void CheckDeaths()
        {
            var entitiesToDestroy = new System.Collections.Generic.List<Entity>();

            world.Query(in destructibleQuery, (Entity entity, ref Health health, ref Destructible destructible, ref Position pos) =>
            {
                if (health.IsDead && !destructible.IsBroken)
                {
                    destructible.IsBroken = true;

                    // Play break sound
                    PlayBreakSound(destructible.Material, pos.ToVector3(), destructible.SoundVolume);

                    // Mark for removal if configured
                    if (destructible.RemoveOnBreak)
                    {
                        entitiesToDestroy.Add(entity);
                    }
                }
            });

            // Add ToBeDestroyed component to broken entities
            foreach (var entity in entitiesToDestroy)
            {
                if (entity.IsAlive() && !entity.Has<ToBeDestroyed>())
                {
                    entity.Add(new ToBeDestroyed(0.1f)); // Small delay for sound to play
                }
            }
        }

        private void PlayImpactSound(vaudio.MaterialType material, Vector3 position, float volume)
        {
            string soundPath = MaterialSoundRegistry.Instance.GetRandomSound(material, SoundCategory.Impact);
            if (soundPath != null)
            {
                resources.AudioManager.Play3D(soundPath, position.X, position.Y, position.Z, false, volume);
            }
        }

        private void PlayBreakSound(vaudio.MaterialType material, Vector3 position, float volume)
        {
            string soundPath = MaterialSoundRegistry.Instance.GetRandomSound(material, SoundCategory.Break);
            if (soundPath != null)
            {
                resources.AudioManager.Play3D(soundPath, position.X, position.Y, position.Z, false, volume);
            }
        }
    }
}
