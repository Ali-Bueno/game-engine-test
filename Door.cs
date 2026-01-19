using System;
using Microsoft.Xna.Framework;
using Game3.Audio;

namespace Game3
{
    public class Door
    {
        private Map map;
        private Vector3 position;
        private Vector3 closedSize;
        private float rotation; // 0 = faces Y axis (north-south door), 90 = faces X axis (east-west door)
        private bool isOpen;
        private BoxCollider collider;
        private AudioSource loopSound;

        private string openSoundPath;
        private string closeSoundPath;
        private string loopSoundPath;

        public Vector3 Position => position;
        public bool IsOpen => isOpen;
        public BoxCollider Collider => collider;

        public Door(Map map, Vector3 position, float rotation, string soundFolder = "sounds/doors/door1")
        {
            this.map = map;
            this.position = position;
            this.rotation = rotation;
            this.isOpen = false;

            // Door size: 1.2m wide, 0.1m thick, 2.2m tall
            if (rotation == 0) // North-south door (blocks Y movement)
            {
                closedSize = new Vector3(1.2f, 0.1f, 2.2f);
            }
            else // East-west door (blocks X movement)
            {
                closedSize = new Vector3(0.1f, 1.2f, 2.2f);
            }

            // Sound paths
            openSoundPath = $"{soundFolder}/open.mp3";
            closeSoundPath = $"{soundFolder}/close.mp3";
            loopSoundPath = $"{soundFolder}/loop.mp3";

            // Create collider (only when closed)
            collider = new BoxCollider(position, closedSize);
            map.Colliders.Add(collider);

            // Add door as primitive for raytracing
            AddDoorPrimitive();

            // Start loop sound (audible when door is closed)
            loopSound = map.AudioManager.Play3D(loopSoundPath, position.X, position.Y, position.Z, true);
            if (loopSound != null)
            {
                loopSound.SetVolume(0.4f);
            }

            Program.Log($"Door created at ({position.X}, {position.Y}, {position.Z}), rotation={rotation}");
        }

        private void AddDoorPrimitive()
        {
            var primitive = new vaudio.PrismPrimitive()
            {
                material = vaudio.MaterialType.Metal,
                size = new vaudio.Vector3F(closedSize.X, closedSize.Y, closedSize.Z),
                transform = vaudio.Matrix4F.CreateTranslation(position.X, position.Y, position.Z)
            };
            map.AudioManager.AddPrimitive(primitive);
        }

        public void Toggle()
        {
            if (isOpen)
                Close();
            else
                Open();
        }

        public void Open()
        {
            if (isOpen) return;

            isOpen = true;

            // Remove collider from map
            map.Colliders.Remove(collider);

            // Play open sound
            map.AudioManager.Play3D(openSoundPath, position.X, position.Y, position.Z, false);

            // Stop loop sound when open
            if (loopSound != null)
            {
                loopSound.SetVolume(0f);
            }

            Program.Log($"Door opened at ({position.X}, {position.Y})");
        }

        public void Close()
        {
            if (!isOpen) return;

            isOpen = false;

            // Add collider back to map
            map.Colliders.Add(collider);

            // Play close sound
            map.AudioManager.Play3D(closeSoundPath, position.X, position.Y, position.Z, false);

            // Resume loop sound when closed
            if (loopSound != null)
            {
                loopSound.SetVolume(0.4f);
            }

            Program.Log($"Door closed at ({position.X}, {position.Y})");
        }

        public bool IsPlayerNear(Vector3 playerPos, float interactionDistance = 1.5f)
        {
            float dx = playerPos.X - position.X;
            float dy = playerPos.Y - position.Y;
            float distance = MathF.Sqrt(dx * dx + dy * dy);
            return distance <= interactionDistance;
        }

        public void Update()
        {
            // Could add animation logic here in the future
        }
    }
}
