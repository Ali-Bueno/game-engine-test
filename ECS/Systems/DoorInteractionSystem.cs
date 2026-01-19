using Arch.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Game3.ECS.Components;

namespace Game3.ECS.Systems
{
    /// <summary>
    /// Handles door interactions when player presses E key.
    /// Pure ECS system - sets ToggleRequested flag on nearest door.
    /// </summary>
    public class DoorInteractionSystem
    {
        private readonly World world;
        private readonly SharedResources resources;
        private readonly QueryDescription playerQuery;
        private readonly QueryDescription doorQuery;
        private KeyboardState previousKeyState;

        private const float InteractionRadius = 2f;

        public DoorInteractionSystem(World world, SharedResources resources)
        {
            this.world = world;
            this.resources = resources;
            this.playerQuery = new QueryDescription()
                .WithAll<PlayerControlled, Position>();
            this.doorQuery = new QueryDescription()
                .WithAll<Position, DoorState, Interactable>();
        }

        public void Update(GameTime gameTime)
        {
            var keystate = Keyboard.GetState();

            // Check for E key press
            if (keystate.IsKeyDown(Keys.E) && !previousKeyState.IsKeyDown(Keys.E))
            {
                // Get player position
                Vector3 playerPos = Vector3.Zero;
                world.Query(in playerQuery, (ref Position pos) =>
                {
                    playerPos = pos.ToVector3();
                });

                // Find nearest door entity and request toggle
                Entity nearestDoorEntity = Entity.Null;
                float nearestDist = InteractionRadius;

                world.Query(in doorQuery, (Entity entity, ref Position doorPos, ref Interactable interactable) =>
                {
                    Vector3 doorPosition = doorPos.ToVector3();
                    float dist = Vector3.Distance(playerPos, doorPosition);
                    if (dist < nearestDist)
                    {
                        nearestDist = dist;
                        nearestDoorEntity = entity;
                    }
                });

                // Request toggle on nearest door
                if (nearestDoorEntity != Entity.Null)
                {
                    ref var doorState = ref world.Get<DoorState>(nearestDoorEntity);
                    doorState.ToggleRequested = true;
                }
            }

            previousKeyState = keystate;
        }
    }
}
