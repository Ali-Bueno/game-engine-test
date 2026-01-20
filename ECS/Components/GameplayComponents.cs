using vaudio;

namespace Game3.ECS.Components
{
    /// <summary>
    /// Tag component marking an entity as player-controlled.
    /// </summary>
    public struct PlayerControlled
    {
        // Tag component - no data needed
    }

    /// <summary>
    /// Player movement configuration.
    /// </summary>
    public struct PlayerMovement
    {
        /// <summary>
        /// Movement speed in units per second.
        /// </summary>
        public float MoveSpeed;

        /// <summary>
        /// Rotation speed in degrees per second.
        /// </summary>
        public float RotateSpeed;

        /// <summary>
        /// Jump force (initial vertical velocity).
        /// </summary>
        public float JumpForce;

        /// <summary>
        /// Whether the player is currently moving.
        /// </summary>
        public bool IsMoving;

        /// <summary>
        /// Cooldown timer for collision sounds.
        /// </summary>
        public float CollisionCooldown;
    }

    /// <summary>
    /// Door state and configuration - pure ECS component.
    /// </summary>
    public struct DoorState
    {
        /// <summary>
        /// Whether the door is currently open.
        /// </summary>
        public bool IsOpen;

        /// <summary>
        /// Whether a toggle was requested this frame.
        /// </summary>
        public bool ToggleRequested;

        /// <summary>
        /// Door size for collision and rendering.
        /// </summary>
        public Microsoft.Xna.Framework.Vector3 Size;

        /// <summary>
        /// Path to open sound.
        /// </summary>
        public string OpenSound;

        /// <summary>
        /// Path to close sound.
        /// </summary>
        public string CloseSound;

        /// <summary>
        /// Path to loop sound (ambient door sound).
        /// </summary>
        public string LoopSound;

        /// <summary>
        /// Reference to the door primitive for vaudio (managed by DoorSystem).
        /// </summary>
        public PrismPrimitive DoorPrimitive;

        /// <summary>
        /// Index of the door collider in SharedResources.StaticColliders (-1 if not added).
        /// </summary>
        public int ColliderIndex;

        /// <summary>
        /// Whether the door has been built (primitive and collider created).
        /// </summary>
        public bool IsBuilt;

        /// <summary>
        /// Room name this door belongs to.
        /// </summary>
        public string RoomName;

        /// <summary>
        /// Which wall side the door is on.
        /// </summary>
        public Game3.GameMap.WallSide Side;
    }

    /// <summary>
    /// Audio source attached to a door (loop sound).
    /// </summary>
    public struct DoorAudio
    {
        public Game3.Audio.AudioSource LoopSource;
        public float LoopVolume;
    }

    /// <summary>
    /// Marks an entity as interactable.
    /// </summary>
    public struct Interactable
    {
        /// <summary>
        /// Interaction radius (how close player must be).
        /// </summary>
        public float Radius;

        /// <summary>
        /// Type of interaction.
        /// </summary>
        public InteractionType Type;
    }

    /// <summary>
    /// Types of interactions.
    /// </summary>
    public enum InteractionType
    {
        Door,
        Item,
        NPC,
        Trigger,
        Switch,
        Pickup
    }

    /// <summary>
    /// Item pickup component for collectable items.
    /// </summary>
    public struct ItemPickup
    {
        /// <summary>
        /// Type/name of the item (e.g., "key", "health", "ammo").
        /// </summary>
        public string ItemType;

        /// <summary>
        /// Quantity of the item.
        /// </summary>
        public int Quantity;

        /// <summary>
        /// Sound to play when picked up.
        /// </summary>
        public string PickupSound;

        /// <summary>
        /// Whether this pickup has been collected.
        /// </summary>
        public bool IsCollected;

        public ItemPickup(string itemType, int quantity = 1, string pickupSound = null)
        {
            ItemType = itemType;
            Quantity = quantity;
            PickupSound = pickupSound;
            IsCollected = false;
        }
    }

    /// <summary>
    /// Switch/button component that can trigger actions on other entities.
    /// </summary>
    public struct Switch
    {
        /// <summary>
        /// Whether the switch is currently activated.
        /// </summary>
        public bool IsActivated;

        /// <summary>
        /// Target entity to affect when activated.
        /// </summary>
        public Arch.Core.Entity Target;

        /// <summary>
        /// Action to perform on the target.
        /// </summary>
        public SwitchAction Action;

        /// <summary>
        /// Sound to play when activated.
        /// </summary>
        public string ActivateSound;

        /// <summary>
        /// Sound to play when deactivated (for toggle switches).
        /// </summary>
        public string DeactivateSound;

        /// <summary>
        /// Whether this switch can be toggled (true) or is one-time (false).
        /// </summary>
        public bool IsToggle;
    }

    /// <summary>
    /// Actions that a switch can perform on its target.
    /// </summary>
    public enum SwitchAction
    {
        ToggleDoor,
        MovePlatform,
        TriggerEvent,
        EnableEntity,
        DisableEntity,
        PlaySound
    }

    /// <summary>
    /// Tag component to disable an entity temporarily.
    /// </summary>
    public struct Disabled
    {
        // Tag component - entity is inactive but not destroyed
    }

    /// <summary>
    /// Event raised when player interacts with something.
    /// </summary>
    public struct InteractionEvent
    {
        /// <summary>
        /// The entity being interacted with.
        /// </summary>
        public Arch.Core.Entity Target;

        /// <summary>
        /// Type of interaction.
        /// </summary>
        public InteractionType Type;
    }

    /// <summary>
    /// Stair data for height calculation - pure ECS component.
    /// </summary>
    public struct StairData
    {
        /// <summary>
        /// Starting position of the stair.
        /// </summary>
        public Microsoft.Xna.Framework.Vector3 StartPosition;

        /// <summary>
        /// Total height change.
        /// </summary>
        public float HeightChange;

        /// <summary>
        /// Length of the stair along its direction.
        /// </summary>
        public float Length;

        /// <summary>
        /// Width of the stair.
        /// </summary>
        public float Width;

        /// <summary>
        /// Direction the stair faces.
        /// </summary>
        public Game3.GameMap.StairDirection Direction;

        /// <summary>
        /// Checks if a position is on this stair.
        /// </summary>
        public bool IsPositionOnStair(Microsoft.Xna.Framework.Vector3 pos)
        {
            float minX, maxX, minY, maxY;
            float halfWidth = Width / 2f;

            switch (Direction)
            {
                case Game3.GameMap.StairDirection.North:
                    minX = StartPosition.X - halfWidth;
                    maxX = StartPosition.X + halfWidth;
                    minY = StartPosition.Y;
                    maxY = StartPosition.Y + Length;
                    break;
                case Game3.GameMap.StairDirection.South:
                    minX = StartPosition.X - halfWidth;
                    maxX = StartPosition.X + halfWidth;
                    minY = StartPosition.Y - Length;
                    maxY = StartPosition.Y;
                    break;
                case Game3.GameMap.StairDirection.East:
                    minX = StartPosition.X;
                    maxX = StartPosition.X + Length;
                    minY = StartPosition.Y - halfWidth;
                    maxY = StartPosition.Y + halfWidth;
                    break;
                case Game3.GameMap.StairDirection.West:
                    minX = StartPosition.X - Length;
                    maxX = StartPosition.X;
                    minY = StartPosition.Y - halfWidth;
                    maxY = StartPosition.Y + halfWidth;
                    break;
                default:
                    return false;
            }

            return pos.X >= minX && pos.X <= maxX && pos.Y >= minY && pos.Y <= maxY;
        }

        /// <summary>
        /// Gets the height at a position on this stair. Returns -1 if not on stair.
        /// </summary>
        public float GetHeightAtPosition(Microsoft.Xna.Framework.Vector3 pos)
        {
            if (!IsPositionOnStair(pos))
                return -1f;

            float progress = 0f;

            switch (Direction)
            {
                case Game3.GameMap.StairDirection.North:
                    progress = (pos.Y - StartPosition.Y) / Length;
                    break;
                case Game3.GameMap.StairDirection.South:
                    progress = (StartPosition.Y - pos.Y) / Length;
                    break;
                case Game3.GameMap.StairDirection.East:
                    progress = (pos.X - StartPosition.X) / Length;
                    break;
                case Game3.GameMap.StairDirection.West:
                    progress = (StartPosition.X - pos.X) / Length;
                    break;
            }

            progress = System.Math.Clamp(progress, 0f, 1f);
            return StartPosition.Z + progress * HeightChange;
        }
    }

    /// <summary>
    /// Platform data - pure ECS component.
    /// </summary>
    public struct PlatformData
    {
        public float MinX, MinY, MaxX, MaxY;
        public float Height;

        public bool Contains(Microsoft.Xna.Framework.Vector3 pos)
        {
            return pos.X >= MinX && pos.X <= MaxX && pos.Y >= MinY && pos.Y <= MaxY;
        }
    }
}
