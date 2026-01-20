using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Game3.Audio;
using vaudio;

namespace Game3.GameMap
{
    /// <summary>
    /// Pure data for a door (calculated from room opening).
    /// </summary>
    public struct DoorData
    {
        public Vector3 Position;
        public Vector3 Size;
        public WallSide Side;
        public string RoomName;
        public string SoundFolder;
    }

    /// <summary>
    /// Pure data for a sound source.
    /// </summary>
    public struct SoundSourceData
    {
        public Vector3 Position;
        public string SoundPath;
        public bool Looping;
        public float Volume;
    }

    /// <summary>
    /// Map building system for audio games with vaudio raytracing.
    /// Manages rooms, doors, stairs, and sound sources.
    /// </summary>
    public class GameMap
    {
        private AudioManager audioManager;

        // Spawn data
        private Vector3 spawnPosition;
        private float spawnAngle;

        // Main collections
        private List<GameRoom> rooms = new List<GameRoom>();
        private List<DoorData> doors = new List<DoorData>();
        private List<GameStair> stairs = new List<GameStair>();
        private List<BoxCollider> colliders = new List<BoxCollider>();
        private List<Platform> platforms = new List<Platform>();
        private List<SoundSourceData> soundSources = new List<SoundSourceData>();

        // Pending primitives (for adding during Build when vaudio is ready)
        private List<PendingPrimitive> pendingPrimitives = new List<PendingPrimitive>();

        private struct PendingPrimitive
        {
            public Vector3 Position;
            public Vector3 Size;
            public MaterialType Material;
            public bool HasCollision;
        }

        // World configuration
        private Vector3 worldMin;
        private Vector3 worldMax;
        private float defaultWallHeight = 5f;
        private float defaultWallThickness = 0.3f;
        private MaterialType defaultWallMaterial = MaterialType.Brick;
        private MaterialType defaultFloorMaterial = MaterialType.Concrete;
        private MaterialType defaultCeilingMaterial = MaterialType.Concrete;

        // Public properties
        public AudioManager AudioManager => audioManager;
        public Vector3 SpawnPosition => spawnPosition;
        public float SpawnAngle => spawnAngle;
        public List<GameRoom> Rooms => rooms;
        public List<DoorData> Doors => doors;
        public List<GameStair> Stairs => stairs;
        public List<BoxCollider> Colliders => colliders;
        public List<Platform> Platforms => platforms;
        public List<SoundSourceData> SoundSources => soundSources;

        public float DefaultWallHeight { get => defaultWallHeight; set => defaultWallHeight = value; }
        public float DefaultWallThickness { get => defaultWallThickness; set => defaultWallThickness = value; }
        public MaterialType DefaultWallMaterial { get => defaultWallMaterial; set => defaultWallMaterial = value; }
        public MaterialType DefaultFloorMaterial { get => defaultFloorMaterial; set => defaultFloorMaterial = value; }
        public MaterialType DefaultCeilingMaterial { get => defaultCeilingMaterial; set => defaultCeilingMaterial = value; }

        public GameMap(AudioManager audioManager = null)
        {
            this.audioManager = audioManager;
        }

        public void SetAudioManager(AudioManager manager)
        {
            this.audioManager = manager;
        }

        /// <summary>
        /// Calculates map bounds based on rooms, stairs, and platforms.
        /// Call after creating all elements but before Build().
        /// </summary>
        public (Vector3 min, Vector3 max) CalculateBounds()
        {
            Vector3 min = new Vector3(float.MaxValue);
            Vector3 max = new Vector3(float.MinValue);

            foreach (var room in rooms)
            {
                min = Vector3.Min(min, room.Min);
                max = Vector3.Max(max, room.Max);
            }

            foreach (var stair in stairs)
            {
                min = Vector3.Min(min, stair.Min);
                max = Vector3.Max(max, stair.Max);
            }

            foreach (var platform in platforms)
            {
                min = Vector3.Min(min, new Vector3(platform.Min.X, platform.Min.Y, 0));
                max = Vector3.Max(max, new Vector3(platform.Max.X, platform.Max.Y, platform.Height + 5f));
            }

            min -= new Vector3(5f);
            max += new Vector3(5f);

            return (min, max);
        }

        #region Element Creation

        public GameRoom CreateRoom(string name, Vector3 center, Vector3 size)
        {
            var room = new GameRoom(this, name, center, size);
            rooms.Add(room);
            return room;
        }

        /// <summary>
        /// Creates a door at a room opening. Calculates position and size automatically.
        /// </summary>
        public DoorData CreateDoor(GameRoom room, string openingId, string soundFolder = "sounds/doors/door1")
        {
            var opening = room.GetOpening(openingId);
            if (opening == null)
            {
                Program.Log($"GameMap: Opening '{openingId}' not found in room '{room.Name}'");
                return default;
            }

            var doorData = CalculateDoorData(room, opening, soundFolder);
            doors.Add(doorData);
            return doorData;
        }

        /// <summary>
        /// Calculates door position and size from room opening data.
        /// </summary>
        private DoorData CalculateDoorData(GameRoom room, RoomOpening opening, string soundFolder)
        {
            float thickness = 0.1f;
            float doorHeight = opening.Height;
            float doorWidth = opening.Width;

            float x, y;
            Vector3 size;
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

            float z = room.FloorZ + opening.BottomOffset + doorHeight / 2;

            return new DoorData
            {
                Position = new Vector3(x, y, z),
                Size = size,
                Side = opening.Side,
                RoomName = room.Name,
                SoundFolder = soundFolder
            };
        }

        public GameStair CreateStair(Vector3 startPosition, float length, float width, float heightChange, StairDirection direction)
        {
            var stair = new GameStair(this, startPosition, length, width, heightChange, direction);
            stairs.Add(stair);
            return stair;
        }

        public void AddSoundSource(Vector3 position, string soundPath, bool looping = true, float volume = 1f)
        {
            soundSources.Add(new SoundSourceData
            {
                Position = position,
                SoundPath = soundPath,
                Looping = looping,
                Volume = volume
            });
        }

        public void SetSpawnPoint(Vector3 position, float angle = 0f)
        {
            spawnPosition = position;
            spawnAngle = angle;
            Program.Log($"GameMap: Spawn point set at ({position.X:F1}, {position.Y:F1}, {position.Z:F1}), angle {angle:F0}°");
        }

        #endregion

        #region Build

        /// <summary>
        /// Builds the entire map (call after creating all elements).
        /// </summary>
        public void Build()
        {
            Program.Log("=== Building GameMap ===");

            // Add pending primitives
            if (pendingPrimitives.Count > 0)
            {
                Program.Log($"Adding {pendingPrimitives.Count} pending primitives...");
                foreach (var pending in pendingPrimitives)
                {
                    var primitive = new PrismPrimitive()
                    {
                        material = pending.Material,
                        size = new Vector3F(pending.Size.X, pending.Size.Y, pending.Size.Z),
                        transform = Matrix4F.CreateTranslation(pending.Position.X, pending.Position.Y, pending.Position.Z)
                    };
                    audioManager.AddPrimitive(primitive);

                    if (pending.HasCollision)
                    {
                        colliders.Add(new BoxCollider(pending.Position, pending.Size));
                    }
                }
                pendingPrimitives.Clear();
            }

            // Build rooms (walls, floor, ceiling)
            foreach (var room in rooms)
            {
                room.Build();
            }

            // Build stairs
            foreach (var stair in stairs)
            {
                stair.Build();
            }

            // Note: Doors and sound sources are handled by ECS systems

            CalculateWorldBounds();

            Program.Log($"GameMap built: {rooms.Count} rooms, {doors.Count} doors, {stairs.Count} stairs");
            Program.Log($"World bounds: ({worldMin.X}, {worldMin.Y}, {worldMin.Z}) to ({worldMax.X}, {worldMax.Y}, {worldMax.Z})");
        }

        private void CalculateWorldBounds()
        {
            worldMin = new Vector3(float.MaxValue);
            worldMax = new Vector3(float.MinValue);

            foreach (var room in rooms)
            {
                worldMin = Vector3.Min(worldMin, room.Min);
                worldMax = Vector3.Max(worldMax, room.Max);
            }

            worldMin -= new Vector3(5f);
            worldMax += new Vector3(5f);
        }

        #endregion

        #region Internal Helpers

        internal void AddPrimitive(Vector3 position, Vector3 size, MaterialType material, bool hasCollision = true)
        {
            if (audioManager == null || !audioManager.IsVaudioInitialized)
            {
                pendingPrimitives.Add(new PendingPrimitive
                {
                    Position = position,
                    Size = size,
                    Material = material,
                    HasCollision = hasCollision
                });
                return;
            }

            var primitive = new PrismPrimitive()
            {
                material = material,
                size = new Vector3F(size.X, size.Y, size.Z),
                transform = Matrix4F.CreateTranslation(position.X, position.Y, position.Z)
            };
            audioManager.AddPrimitive(primitive);

            if (hasCollision)
            {
                colliders.Add(new BoxCollider(position, size));
            }
        }

        #endregion
    }
}
