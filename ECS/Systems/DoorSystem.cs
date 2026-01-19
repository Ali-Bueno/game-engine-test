using Arch.Core;
using Microsoft.Xna.Framework;
using vaudio;
using Game3.ECS.Components;
using Game3.GameMap;

namespace Game3.ECS.Systems
{
    /// <summary>
    /// Manages door state, primitives, colliders, and sounds.
    /// Pure ECS system - no legacy dependencies.
    /// </summary>
    public class DoorSystem
    {
        private readonly World world;
        private readonly SharedResources resources;
        private readonly QueryDescription doorQuery;

        public DoorSystem(World world, SharedResources resources)
        {
            this.world = world;
            this.resources = resources;
            this.doorQuery = new QueryDescription()
                .WithAll<Position, DoorState, DoorAudio>();
        }

        /// <summary>
        /// Initializes a door entity - creates primitive, collider, and loop sound.
        /// Call once after creating the door entity.
        /// </summary>
        public void InitializeDoor(ref Position pos, ref DoorState door, ref DoorAudio audio)
        {
            if (door.IsBuilt) return;

            // Create vaudio primitive for sound occlusion
            door.DoorPrimitive = new PrismPrimitive()
            {
                material = MaterialType.Metal,
                size = new Vector3F(door.Size.X, door.Size.Y, door.Size.Z),
                transform = Matrix4F.CreateTranslation(pos.X, pos.Y, pos.Z)
            };
            resources.AudioManager.AddPrimitive(door.DoorPrimitive);

            // Create collider
            var collider = new BoxCollider(
                new Vector3(pos.X, pos.Y, pos.Z),
                door.Size
            );
            resources.StaticColliders.Add(collider);
            door.ColliderIndex = resources.StaticColliders.Count - 1;

            // Start loop sound
            if (!string.IsNullOrEmpty(door.LoopSound))
            {
                audio.LoopSource = resources.AudioManager.Play3D(door.LoopSound, pos.X, pos.Y, pos.Z, true);
                audio.LoopSource?.SetVolume(audio.LoopVolume);
            }

            door.IsBuilt = true;
            Program.Log($"DoorSystem: Initialized door at ({pos.X:F1}, {pos.Y:F1}, {pos.Z:F1})");
        }

        public void Update(GameTime gameTime)
        {
            world.Query(in doorQuery, (ref Position pos, ref DoorState door, ref DoorAudio audio) =>
            {
                // Initialize if needed
                if (!door.IsBuilt)
                {
                    InitializeDoor(ref pos, ref door, ref audio);
                }

                // Handle toggle request
                if (door.ToggleRequested)
                {
                    door.ToggleRequested = false;

                    if (door.IsOpen)
                    {
                        CloseDoor(ref pos, ref door, ref audio);
                    }
                    else
                    {
                        OpenDoor(ref pos, ref door, ref audio);
                    }
                }
            });
        }

        private void OpenDoor(ref Position pos, ref DoorState door, ref DoorAudio audio)
        {
            if (door.IsOpen) return;
            door.IsOpen = true;

            // Remove primitive from raytracing (sound passes freely)
            if (door.DoorPrimitive != null)
            {
                resources.AudioManager.RemovePrimitive(door.DoorPrimitive);
            }

            // Remove collider
            if (door.ColliderIndex >= 0 && door.ColliderIndex < resources.StaticColliders.Count)
            {
                resources.StaticColliders.RemoveAt(door.ColliderIndex);
                door.ColliderIndex = -1;
                // Note: This invalidates other collider indices, but doors are rarely opened/closed rapidly
            }

            // Play open sound
            if (!string.IsNullOrEmpty(door.OpenSound))
            {
                resources.AudioManager.Play3D(door.OpenSound, pos.X, pos.Y, pos.Z, false);
            }

            // Silence loop
            audio.LoopSource?.SetVolume(0f);

            Program.Log($"DoorSystem: Door opened at ({pos.X:F1}, {pos.Y:F1})");
        }

        private void CloseDoor(ref Position pos, ref DoorState door, ref DoorAudio audio)
        {
            if (!door.IsOpen) return;
            door.IsOpen = false;

            // Add primitive back to raytracing
            if (door.DoorPrimitive != null)
            {
                resources.AudioManager.AddPrimitive(door.DoorPrimitive);
            }

            // Re-add collider
            var collider = new BoxCollider(
                new Vector3(pos.X, pos.Y, pos.Z),
                door.Size
            );
            resources.StaticColliders.Add(collider);
            door.ColliderIndex = resources.StaticColliders.Count - 1;

            // Play close sound
            if (!string.IsNullOrEmpty(door.CloseSound))
            {
                resources.AudioManager.Play3D(door.CloseSound, pos.X, pos.Y, pos.Z, false);
            }

            // Restore loop
            audio.LoopSource?.SetVolume(audio.LoopVolume);

            Program.Log($"DoorSystem: Door closed at ({pos.X:F1}, {pos.Y:F1})");
        }
    }
}
