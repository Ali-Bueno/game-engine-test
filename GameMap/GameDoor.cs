using System;
using Microsoft.Xna.Framework;
using Game3.Audio;
using vaudio;

namespace Game3.GameMap
{
    /// <summary>
    /// Puerta interactiva que se coloca en una apertura de habitación.
    /// Gestiona sus propias primitivas de raytracing (añade/quita al abrir/cerrar).
    /// </summary>
    public class GameDoor
    {
        private GameMap map;
        private GameRoom room;
        private RoomOpening opening;

        private Vector3 position;
        private Vector3 size;
        private bool isOpen;

        // Primitiva de raytracing (para añadir/quitar dinámicamente)
        private PrismPrimitive doorPrimitive;
        private BoxCollider doorCollider;

        // Sonidos
        private string openSoundPath;
        private string closeSoundPath;
        private string loopSoundPath;
        private AudioSource loopSound;

        private bool isBuilt = false;

        public Vector3 Position => position;
        public bool IsOpen => isOpen;
        public GameRoom Room => room;

        public GameDoor(GameMap map, GameRoom room, RoomOpening opening, string soundFolder)
        {
            this.map = map;
            this.room = room;
            this.opening = opening;
            this.isOpen = false;

            // Calcular posición de la puerta
            CalculatePositionAndSize();

            // Rutas de sonido
            openSoundPath = $"{soundFolder}/open.mp3";
            closeSoundPath = $"{soundFolder}/close.mp3";
            loopSoundPath = $"{soundFolder}/loop.mp3";
        }

        private void CalculatePositionAndSize()
        {
            float thickness = 0.1f;
            float doorHeight = opening.Height;
            float doorWidth = opening.Width;

            // Posición base desde la habitación
            float x, y;
            bool isNorthSouth = (opening.Side == WallSide.North || opening.Side == WallSide.South);
            float wallLength = isNorthSouth ? room.Size.X : room.Size.Y;
            float offset = (opening.PositionAlongWall - 0.5f) * wallLength;

            switch (opening.Side)
            {
                case WallSide.North:
                    x = room.Center.X + offset;
                    y = room.Top;
                    size = new Vector3(doorWidth, thickness, doorHeight);
                    break;
                case WallSide.South:
                    x = room.Center.X + offset;
                    y = room.Bottom;
                    size = new Vector3(doorWidth, thickness, doorHeight);
                    break;
                case WallSide.East:
                    x = room.Right;
                    y = room.Center.Y + offset;
                    size = new Vector3(thickness, doorWidth, doorHeight);
                    break;
                case WallSide.West:
                    x = room.Left;
                    y = room.Center.Y + offset;
                    size = new Vector3(thickness, doorWidth, doorHeight);
                    break;
                default:
                    x = room.Center.X;
                    y = room.Center.Y;
                    size = new Vector3(doorWidth, thickness, doorHeight);
                    break;
            }

            // Z = altura del suelo + bottomOffset + mitad de la altura de la puerta
            float z = room.FloorZ + opening.BottomOffset + doorHeight / 2;
            position = new Vector3(x, y, z);
        }

        public void Build()
        {
            if (isBuilt) return;
            isBuilt = true;

            // Crear primitiva de la puerta (metal)
            doorPrimitive = map.AddPrimitiveTracked(position, size, MaterialType.Metal);

            // Crear collider
            doorCollider = map.AddCollider(position, size);

            // Iniciar sonido de loop (audible cuando está cerrada)
            loopSound = map.AudioManager.Play3D(loopSoundPath, position.X, position.Y, position.Z, true);
            if (loopSound != null)
            {
                loopSound.SetVolume(0.4f);
            }

            Program.Log($"GameDoor built at ({position.X:F1}, {position.Y:F1}, {position.Z:F1}) in room '{room.Name}'");
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

            // Quitar primitiva del raytracing (sonido pasa libremente)
            if (doorPrimitive != null)
            {
                map.RemovePrimitive(doorPrimitive);
            }

            // Quitar collider
            if (doorCollider != null)
            {
                map.RemoveCollider(doorCollider);
            }

            // Reproducir sonido de apertura
            map.AudioManager.Play3D(openSoundPath, position.X, position.Y, position.Z, false);

            // Silenciar loop
            loopSound?.SetVolume(0f);

            Program.Log($"Door opened at ({position.X:F1}, {position.Y:F1})");
        }

        public void Close()
        {
            if (!isOpen) return;
            isOpen = false;

            // Añadir primitiva de vuelta al raytracing
            if (doorPrimitive != null)
            {
                map.AudioManager.AddPrimitive(doorPrimitive);
            }

            // Añadir collider de vuelta
            if (doorCollider != null && !map.Colliders.Contains(doorCollider))
            {
                map.Colliders.Add(doorCollider);
            }

            // Reproducir sonido de cierre
            map.AudioManager.Play3D(closeSoundPath, position.X, position.Y, position.Z, false);

            // Restaurar loop
            loopSound?.SetVolume(0.4f);

            Program.Log($"Door closed at ({position.X:F1}, {position.Y:F1})");
        }

        public bool IsPlayerNear(Vector3 playerPos, float interactionDistance = 1.5f)
        {
            float dx = playerPos.X - position.X;
            float dy = playerPos.Y - position.Y;
            float distance = MathF.Sqrt(dx * dx + dy * dy);
            return distance <= interactionDistance;
        }

        public void Update(float deltaTime)
        {
            // Animaciones o lógica adicional podrían ir aquí
        }
    }
}
