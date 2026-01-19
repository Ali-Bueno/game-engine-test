using Arch.Core;
using Microsoft.Xna.Framework;
using Game3.ECS.Components;

namespace Game3.ECS.Systems
{
    /// <summary>
    /// Applies gravity to entities with Gravity component.
    /// Handles jumping, falling, and landing on floors/stairs/platforms.
    /// Pure ECS system - uses SharedResources for floor height queries.
    /// </summary>
    public class GravitySystem
    {
        private readonly World world;
        private readonly SharedResources resources;
        private readonly QueryDescription gravityQuery;

        private const float GravityForce = 15f;

        public GravitySystem(World world, SharedResources resources)
        {
            this.world = world;
            this.resources = resources;
            this.gravityQuery = new QueryDescription()
                .WithAll<Position, Gravity>();
        }

        public void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            world.Query(in gravityQuery, (ref Position pos, ref Gravity gravity) =>
            {
                // Calculate floor height at current position using SharedResources (ECS queries)
                var position = pos.ToVector3();
                float floorHeight = resources.GetFloorHeight(position);

                if (!gravity.IsGrounded)
                {
                    // Apply gravity
                    gravity.VerticalVelocity -= GravityForce * deltaTime;
                    pos.Z += gravity.VerticalVelocity * deltaTime;

                    // Check if we hit the ground
                    if (pos.Z <= floorHeight)
                    {
                        pos.Z = floorHeight;
                        gravity.VerticalVelocity = 0f;
                        gravity.IsGrounded = true;
                        gravity.CurrentFloorHeight = floorHeight;
                    }
                }
                else
                {
                    // On the ground, follow terrain height
                    gravity.CurrentFloorHeight = floorHeight;
                    pos.Z = gravity.CurrentFloorHeight;
                }
            });
        }
    }
}
