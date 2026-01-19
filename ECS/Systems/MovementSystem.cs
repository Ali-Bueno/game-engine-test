using Arch.Core;
using Microsoft.Xna.Framework;
using Game3.ECS.Components;

namespace Game3.ECS.Systems
{
    /// <summary>
    /// Applies velocity to position for all entities with Position and Velocity.
    /// Only handles horizontal (XY) movement - vertical is handled by GravitySystem.
    /// </summary>
    public class MovementSystem
    {
        private readonly World world;
        private readonly QueryDescription movementQuery;

        public MovementSystem(World world)
        {
            this.world = world;
            this.movementQuery = new QueryDescription()
                .WithAll<Position, Velocity>();
        }

        public void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            world.Query(in movementQuery, (ref Position pos, ref Velocity vel) =>
            {
                // Apply horizontal velocity (Z is handled by gravity)
                pos.X += vel.X * deltaTime;
                pos.Y += vel.Y * deltaTime;
            });
        }
    }
}
