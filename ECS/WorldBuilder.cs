using System.Collections.Generic;
using Arch.Core;
using Microsoft.Xna.Framework;
using Game3.ECS.Components;
using Game3.GameMap;

namespace Game3.ECS
{
    /// <summary>
    /// Creates pure ECS entities from GameMap data.
    /// No legacy object references - all data is stored in components.
    /// </summary>
    public class WorldBuilder
    {
        private readonly World world;
        private readonly SharedResources resources;

        // Constants (formerly in GamePlayer)
        public const float PlayerHeight = 1.75f;
        public const float EyeHeight = 1.65f;

        public WorldBuilder(World world, SharedResources resources)
        {
            this.world = world;
            this.resources = resources;
        }

        /// <summary>
        /// Creates the player entity with all necessary components.
        /// </summary>
        public Entity CreatePlayer(Vector3 position, float angle)
        {
            var entity = world.Create(
                new Position(position.X, position.Y, position.Z),
                new Rotation(angle),
                new Velocity(0, 0, 0),
                new PlayerControlled(),
                new PlayerMovement
                {
                    MoveSpeed = 5f,
                    RotateSpeed = 120f,
                    JumpForce = 6f,
                    IsMoving = false,
                    CollisionCooldown = 0f
                },
                new Gravity(true),
                new CollisionShape(0.3f, PlayerHeight),
                new FootstepSounds
                {
                    NormalSoundFolder = "sounds/steps/womanstep",
                    StairSoundFolder = "sounds/steps/wood",
                    NormalSoundCount = 13,
                    StairSoundCount = 4,
                    Interval = 0.4f,
                    Volume = 0.5f,
                    Timer = 0.4f,
                    IsOnStair = false
                },
                new AudioListener(EyeHeight)
            );

            Program.Log($"ECS: Created player entity at ({position.X:F1}, {position.Y:F1}, {position.Z:F1})");
            return entity;
        }

        /// <summary>
        /// Creates a pure door entity from door data.
        /// </summary>
        public Entity CreateDoor(Vector3 position, Vector3 size, WallSide side, string roomName, string soundFolder)
        {
            var entity = world.Create(
                new Position(position.X, position.Y, position.Z),
                new DoorState
                {
                    IsOpen = false,
                    ToggleRequested = false,
                    Size = size,
                    OpenSound = $"{soundFolder}/open.mp3",
                    CloseSound = $"{soundFolder}/close.mp3",
                    LoopSound = $"{soundFolder}/loop.mp3",
                    DoorPrimitive = null,
                    ColliderIndex = -1,
                    IsBuilt = false,
                    RoomName = roomName,
                    Side = side
                },
                new DoorAudio
                {
                    LoopSource = null,
                    LoopVolume = 0.4f
                },
                new Interactable
                {
                    Radius = 2f,
                    Type = InteractionType.Door
                }
            );

            Program.Log($"ECS: Created door entity at ({position.X:F1}, {position.Y:F1}, {position.Z:F1}) in {roomName}");
            return entity;
        }

        /// <summary>
        /// Creates a pure stair entity from stair data.
        /// </summary>
        public Entity CreateStair(Vector3 startPosition, float length, float width, float heightChange, StairDirection direction)
        {
            var entity = world.Create(
                new Position(startPosition.X, startPosition.Y, startPosition.Z),
                new StairData
                {
                    StartPosition = startPosition,
                    HeightChange = heightChange,
                    Length = length,
                    Width = width,
                    Direction = direction
                }
            );

            Program.Log($"ECS: Created stair entity at ({startPosition.X:F1}, {startPosition.Y:F1}, {startPosition.Z:F1})");
            return entity;
        }

        /// <summary>
        /// Creates a pure platform entity.
        /// </summary>
        public Entity CreatePlatform(float minX, float minY, float maxX, float maxY, float height)
        {
            var centerX = (minX + maxX) / 2f;
            var centerY = (minY + maxY) / 2f;

            var entity = world.Create(
                new Position(centerX, centerY, height),
                new PlatformData
                {
                    MinX = minX,
                    MinY = minY,
                    MaxX = maxX,
                    MaxY = maxY,
                    Height = height
                }
            );

            Program.Log($"ECS: Created platform entity at ({centerX:F1}, {centerY:F1}, {height:F1})");
            return entity;
        }

        /// <summary>
        /// Creates a pure ambient sound entity.
        /// </summary>
        public Entity CreateAmbientSound(Vector3 position, string soundPath, bool looping, float volume)
        {
            var entity = world.Create(
                new Position(position.X, position.Y, position.Z),
                new AmbientSound
                {
                    SoundPath = soundPath,
                    Volume = volume,
                    Looping = looping,
                    Source = null,
                    IsInitialized = false
                }
            );

            Program.Log($"ECS: Created ambient sound entity at ({position.X:F1}, {position.Y:F1}, {position.Z:F1})");
            return entity;
        }

        /// <summary>
        /// Creates all ECS entities from a GameMap.
        /// Extracts pure data from legacy objects.
        /// </summary>
        public (Entity player, List<Entity> doors, List<Entity> soundSources, List<Entity> stairs, List<Entity> platforms) BuildFromGameMap(GameMap.GameMap gameMap)
        {
            // Create player entity from spawn data
            Entity playerEntity = CreatePlayer(gameMap.SpawnPosition, gameMap.SpawnAngle);

            // Create door entities from door data
            var doorEntities = new List<Entity>();
            foreach (var door in gameMap.Doors)
            {
                var doorEntity = CreateDoor(
                    door.Position,
                    door.Size,
                    door.Side,
                    door.RoomName,
                    door.SoundFolder
                );
                doorEntities.Add(doorEntity);
            }

            // Create stair entities from legacy stairs (extract pure data)
            var stairEntities = new List<Entity>();
            foreach (var legacyStair in gameMap.Stairs)
            {
                var stairEntity = CreateStair(
                    legacyStair.StartPosition,
                    legacyStair.Length,
                    legacyStair.Width,
                    legacyStair.HeightChange,
                    legacyStair.Direction
                );
                stairEntities.Add(stairEntity);
            }

            // Create platform entities from legacy platforms (extract pure data)
            var platformEntities = new List<Entity>();
            foreach (var legacyPlatform in gameMap.Platforms)
            {
                var platformEntity = CreatePlatform(
                    legacyPlatform.MinX,
                    legacyPlatform.MinY,
                    legacyPlatform.MaxX,
                    legacyPlatform.MaxY,
                    legacyPlatform.Height
                );
                platformEntities.Add(platformEntity);
            }

            // Create sound source entities from SoundSource data
            var soundEntities = new List<Entity>();
            foreach (var source in gameMap.SoundSources)
            {
                var soundEntity = CreateAmbientSound(
                    source.Position,
                    source.SoundPath,
                    source.Looping,
                    source.Volume
                );
                soundEntities.Add(soundEntity);
            }

            return (playerEntity, doorEntities, soundEntities, stairEntities, platformEntities);
        }
    }
}
