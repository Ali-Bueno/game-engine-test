using System.Collections.Generic;
using Arch.Core;
using Arch.Core.Extensions;
using Microsoft.Xna.Framework;
using Game3.ECS.Components;

namespace Game3.ECS.Systems
{
    /// <summary>
    /// Handles item pickup processing after InteractionSystem marks items as collected.
    /// </summary>
    public class PickupSystem
    {
        private readonly World world;
        private readonly SharedResources resources;
        private readonly QueryDescription pickupQuery;

        // Simple inventory - could be expanded to a proper inventory component
        private Dictionary<string, int> playerInventory = new Dictionary<string, int>();

        /// <summary>
        /// Event raised when an item is picked up.
        /// </summary>
        public event System.Action<string, int> OnItemPickedUp;

        public PickupSystem(World world, SharedResources resources)
        {
            this.world = world;
            this.resources = resources;
            this.pickupQuery = new QueryDescription()
                .WithAll<Position, ItemPickup>()
                .WithNone<Disabled, ToBeDestroyed>();
        }

        /// <summary>
        /// Gets the current player inventory.
        /// </summary>
        public IReadOnlyDictionary<string, int> Inventory => playerInventory;

        /// <summary>
        /// Checks if player has a certain quantity of an item.
        /// </summary>
        public bool HasItem(string itemType, int quantity = 1)
        {
            return playerInventory.TryGetValue(itemType, out int count) && count >= quantity;
        }

        /// <summary>
        /// Removes items from inventory (for using keys, etc.).
        /// </summary>
        public bool UseItem(string itemType, int quantity = 1)
        {
            if (!HasItem(itemType, quantity))
                return false;

            playerInventory[itemType] -= quantity;
            if (playerInventory[itemType] <= 0)
                playerInventory.Remove(itemType);

            return true;
        }

        public void Update(GameTime gameTime)
        {
            var entitiesToRemove = new List<Entity>();

            world.Query(in pickupQuery, (Entity entity, ref Position pos, ref ItemPickup pickup) =>
            {
                if (pickup.IsCollected)
                {
                    // Add to inventory
                    if (!playerInventory.ContainsKey(pickup.ItemType))
                        playerInventory[pickup.ItemType] = 0;

                    playerInventory[pickup.ItemType] += pickup.Quantity;

                    // Play pickup sound
                    if (!string.IsNullOrEmpty(pickup.PickupSound))
                    {
                        resources.AudioManager.Play3D(
                            pickup.PickupSound,
                            pos.X, pos.Y, pos.Z,
                            false, 0.8f);
                    }

                    // Notify listeners
                    OnItemPickedUp?.Invoke(pickup.ItemType, pickup.Quantity);

                    Program.Log($"Picked up {pickup.Quantity}x {pickup.ItemType}. Inventory: {playerInventory[pickup.ItemType]}");

                    // Mark for removal
                    entitiesToRemove.Add(entity);
                }
            });

            // Add ToBeDestroyed to collected items
            foreach (var entity in entitiesToRemove)
            {
                if (entity.IsAlive() && !entity.Has<ToBeDestroyed>())
                {
                    entity.Add(new ToBeDestroyed(0f));
                }
            }
        }
    }
}
