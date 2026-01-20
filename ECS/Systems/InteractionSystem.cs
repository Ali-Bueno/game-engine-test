using System;
using System.Collections.Generic;
using Arch.Core;
using Arch.Core.Extensions;
using Microsoft.Xna.Framework;
using Game3.ECS.Components;

namespace Game3.ECS.Systems
{
    /// <summary>
    /// Generalized interaction system that detects nearby interactables
    /// and raises InteractionEvent when player presses the interact key.
    /// </summary>
    public class InteractionSystem
    {
        private readonly World world;
        private readonly SharedResources resources;
        private readonly QueryDescription playerQuery;
        private readonly QueryDescription interactableQuery;

        // Track the closest interactable for UI feedback
        private Entity? closestInteractable;
        private float closestDistance;

        public InteractionSystem(World world, SharedResources resources)
        {
            this.world = world;
            this.resources = resources;
            this.playerQuery = new QueryDescription()
                .WithAll<Position, PlayerControlled>();
            this.interactableQuery = new QueryDescription()
                .WithAll<Position, Interactable>()
                .WithNone<Disabled>();
        }

        /// <summary>
        /// Gets the closest interactable entity to the player (for UI hints).
        /// </summary>
        public Entity? ClosestInteractable => closestInteractable;

        /// <summary>
        /// Gets the distance to the closest interactable.
        /// </summary>
        public float ClosestDistance => closestDistance;

        public void Update(GameTime gameTime)
        {
            closestInteractable = null;
            closestDistance = float.MaxValue;

            // Get player position
            Vector3 playerPos = Vector3.Zero;
            bool hasPlayer = false;

            world.Query(in playerQuery, (ref Position pos) =>
            {
                playerPos = pos.ToVector3();
                hasPlayer = true;
            });

            if (!hasPlayer) return;

            // Find closest interactable within range
            Entity? bestTarget = null;
            float bestDistance = float.MaxValue;
            InteractionType bestType = InteractionType.Trigger;

            world.Query(in interactableQuery, (Entity entity, ref Position pos, ref Interactable interactable) =>
            {
                float distance = Vector3.Distance(playerPos, pos.ToVector3());
                if (distance <= interactable.Radius && distance < bestDistance)
                {
                    bestTarget = entity;
                    bestDistance = distance;
                    bestType = interactable.Type;
                }
            });

            closestInteractable = bestTarget;
            closestDistance = bestDistance;

            // Check for interaction input (E key)
            if (bestTarget.HasValue && Input.WasKeyPressed(Microsoft.Xna.Framework.Input.Keys.E))
            {
                RaiseInteractionEvent(bestTarget.Value, bestType);
            }
        }

        private void RaiseInteractionEvent(Entity target, InteractionType type)
        {
            // Add interaction event to player (systems can query for this)
            // For now, handle specific types directly

            switch (type)
            {
                case InteractionType.Door:
                    if (target.Has<DoorState>())
                    {
                        ref var door = ref target.Get<DoorState>();
                        door.ToggleRequested = true;
                    }
                    break;

                case InteractionType.Switch:
                    if (target.Has<Switch>())
                    {
                        ref var sw = ref target.Get<Switch>();
                        // Toggle or activate
                        if (sw.IsToggle)
                        {
                            sw.IsActivated = !sw.IsActivated;
                        }
                        else if (!sw.IsActivated)
                        {
                            sw.IsActivated = true;
                        }
                    }
                    break;

                case InteractionType.Pickup:
                case InteractionType.Item:
                    if (target.Has<ItemPickup>())
                    {
                        ref var pickup = ref target.Get<ItemPickup>();
                        if (!pickup.IsCollected)
                        {
                            pickup.IsCollected = true;
                        }
                    }
                    break;

                case InteractionType.NPC:
                    // Future: dialogue system
                    Program.Log($"Interacted with NPC");
                    break;

                case InteractionType.Trigger:
                    // Future: trigger system
                    Program.Log($"Triggered");
                    break;
            }
        }
    }
}
