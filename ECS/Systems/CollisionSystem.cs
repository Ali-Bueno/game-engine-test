using Arch.Core;
using Microsoft.Xna.Framework;
using Game3.ECS.Components;
using Game3.GameMap;

namespace Game3.ECS.Systems
{
    /// <summary>
    /// Handles collision detection and resolution for entities with CollisionShape.
    /// Resolves collisions with static colliders (walls, doors, etc.)
    /// </summary>
    public class CollisionSystem
    {
        private readonly World world;
        private readonly SharedResources resources;
        private readonly QueryDescription collisionQuery;

        private const float CollisionCooldownTime = 0.3f;

        public CollisionSystem(World world, SharedResources resources)
        {
            this.world = world;
            this.resources = resources;
            this.collisionQuery = new QueryDescription()
                .WithAll<Position, CollisionShape, PlayerMovement>();
        }

        public void Update(GameTime gameTime)
        {
            world.Query(in collisionQuery, (ref Position pos, ref CollisionShape shape, ref PlayerMovement movement) =>
            {
                Vector3 position = pos.ToVector3();
                Vector3 originalPosition = position;

                // Check against all static colliders
                foreach (var collider in resources.StaticColliders)
                {
                    if (collider.Intersects3D(position, shape.Radius, shape.Height))
                    {
                        position = collider.ResolveCollision(position, shape.Radius);
                    }
                }

                // Clamp Z
                if (position.Z < 0)
                    position.Z = 0;

                // Check if we collided
                float collisionDistance = (position - originalPosition).Length();
                if (collisionDistance > 0.01f && movement.CollisionCooldown <= 0)
                {
                    // Play collision sound
                    resources.AudioManager.Play3D("sounds/walls/wall.mp3", position.X, position.Y, position.Z + 1.0f, false, 0.7f);
                    movement.CollisionCooldown = CollisionCooldownTime;
                }

                // Update position
                pos.X = position.X;
                pos.Y = position.Y;
                // Z is handled by gravity system
            });
        }
    }
}
