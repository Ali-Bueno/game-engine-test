using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Game3.Audio;
using vaudio;

namespace Game3
{
    public struct Platform
    {
        public Vector3 Min;
        public Vector3 Max;
        public float Height;

        public Platform(float minX, float minY, float maxX, float maxY, float height)
        {
            Min = new Vector3(minX, minY, 0);
            Max = new Vector3(maxX, maxY, 0);
            Height = height;
        }
    }

    public class Map
    {
        private AudioManager audioManager;
        private Player player;
        private List<BoxCollider> colliders = new List<BoxCollider>();
        private List<SoundObject> soundObjects = new List<SoundObject>();
        private List<Door> doors = new List<Door>();
        private List<Stair> stairs = new List<Stair>();
        private List<Platform> platforms = new List<Platform>();

        public Player Player => player;
        public AudioManager AudioManager => audioManager;
        public List<BoxCollider> Colliders => colliders;
        public List<Door> Doors => doors;
        public List<Stair> Stairs => stairs;
        public List<Platform> Platforms => platforms;

        public Map(AudioManager audioManager)
        {
            this.audioManager = audioManager;
        }

        public void BuildMap()
        {
            // Layout with larger rooms for varied reverb:
            //
            //  +------------------------------------------+
            //  |                                          |
            //  |        ROOM 4: HALL (50x30m)             |  Huge reverb
            //  |        with upper balcony (Z=4)          |
            //  |                                          |
            //  +----+                                +----+
            //       |           DOOR 3               |
            //  +----+                                +----+
            //  |                                          |
            //  |        ROOM 3: LARGE (30x20m)            |  Long reverb
            //  |                                          |
            //  +----+                                +----+
            //       |           DOOR 2               |
            //  +----+                                +----+
            //  |                                          |
            //  |        ROOM 2: MEDIUM (20x15m)           |  Medium reverb
            //  |                                          |
            //  +----+                                +----+
            //       |           DOOR 1               |
            //  +----+                                +----+
            //  |                                          |
            //  |        ROOM 1: SMALL (10x10m)            |  Short reverb
            //  |        (Starting room)                   |
            //  |                                          |
            //  +------------------------------------------+

            float wallHeight = 5f;
            float wallThickness = 0.3f;
            float upperFloorHeight = 4f;
            float doorWidth = 2f;

            // ============================================
            // ROOM 1: SMALL (10x10m) - Tight reverb
            // Position: (0,0) to (10,10)
            // ============================================
            AddWall(new Vector3(5f, 0f, wallHeight / 2), new Vector3(10f, wallThickness, wallHeight));  // South
            AddWall(new Vector3(0f, 5f, wallHeight / 2), new Vector3(wallThickness, 10f, wallHeight)); // West
            AddWall(new Vector3(10f, 5f, wallHeight / 2), new Vector3(wallThickness, 10f, wallHeight)); // East
            // North wall with door opening
            AddWall(new Vector3(2f, 10f, wallHeight / 2), new Vector3(4f - doorWidth / 2, wallThickness, wallHeight));
            AddWall(new Vector3(8f, 10f, wallHeight / 2), new Vector3(4f - doorWidth / 2, wallThickness, wallHeight));

            // Door 1 between Room 1 and Room 2
            var door1 = new Door(this, new Vector3(5f, 10f, 1.1f), 0);
            doors.Add(door1);

            // ============================================
            // ROOM 2: MEDIUM (20x15m) - Medium reverb
            // Position: (-5,10) to (15,25)
            // ============================================
            AddWall(new Vector3(-5f, 17.5f, wallHeight / 2), new Vector3(wallThickness, 15f, wallHeight)); // West
            AddWall(new Vector3(15f, 17.5f, wallHeight / 2), new Vector3(wallThickness, 15f, wallHeight)); // East
            // South wall segments (door at 5)
            AddWall(new Vector3(-2.5f, 10f, wallHeight / 2), new Vector3(5f, wallThickness, wallHeight));
            AddWall(new Vector3(12.5f, 10f, wallHeight / 2), new Vector3(5f, wallThickness, wallHeight));
            // North wall with door opening
            AddWall(new Vector3(-0.5f, 25f, wallHeight / 2), new Vector3(9f - doorWidth / 2, wallThickness, wallHeight));
            AddWall(new Vector3(10.5f, 25f, wallHeight / 2), new Vector3(9f - doorWidth / 2, wallThickness, wallHeight));

            // Door 2 between Room 2 and Room 3
            var door2 = new Door(this, new Vector3(5f, 25f, 1.1f), 0);
            doors.Add(door2);

            // ============================================
            // ROOM 3: LARGE (30x20m) - Long reverb
            // Position: (-10,25) to (20,45)
            // ============================================
            AddWall(new Vector3(-10f, 35f, wallHeight / 2), new Vector3(wallThickness, 20f, wallHeight)); // West
            AddWall(new Vector3(20f, 35f, wallHeight / 2), new Vector3(wallThickness, 20f, wallHeight)); // East
            // South wall segments
            AddWall(new Vector3(-5f, 25f, wallHeight / 2), new Vector3(10f, wallThickness, wallHeight));
            AddWall(new Vector3(15f, 25f, wallHeight / 2), new Vector3(10f, wallThickness, wallHeight));
            // North wall with door opening
            AddWall(new Vector3(-3f, 45f, wallHeight / 2), new Vector3(14f - doorWidth / 2, wallThickness, wallHeight));
            AddWall(new Vector3(13f, 45f, wallHeight / 2), new Vector3(14f - doorWidth / 2, wallThickness, wallHeight));

            // Door 3 between Room 3 and Hall
            var door3 = new Door(this, new Vector3(5f, 45f, 1.1f), 0);
            doors.Add(door3);

            // ============================================
            // ROOM 4: HUGE HALL (50x30m) - Cathedral reverb
            // Position: (-20,45) to (30,75)
            // With stairs and upper balcony
            // ============================================
            AddWall(new Vector3(-20f, 60f, wallHeight / 2), new Vector3(wallThickness, 30f, wallHeight)); // West
            AddWall(new Vector3(30f, 60f, wallHeight / 2), new Vector3(wallThickness, 30f, wallHeight)); // East
            // South wall segments
            AddWall(new Vector3(-10f, 45f, wallHeight / 2), new Vector3(20f, wallThickness, wallHeight));
            AddWall(new Vector3(20f, 45f, wallHeight / 2), new Vector3(20f, wallThickness, wallHeight));
            // North wall
            AddWall(new Vector3(5f, 75f, wallHeight / 2), new Vector3(50f, wallThickness, wallHeight));

            // Stairs in the hall (going up along west wall)
            // Longer stairs: 14m length, wider (4m), closer to door for easier access
            float stairLength = 14f;
            float stairWidth = 4f;
            float stairStartY = 47f;  // Closer to door 3
            float stairX = -10f;      // More centered, easier to find

            var stair = new Stair(
                this,
                new Vector3(stairX, stairStartY, 0f),
                length: stairLength,
                width: stairWidth,
                heightChange: upperFloorHeight,
                StairDirection.North
            );
            stairs.Add(stair);

            // Stair side walls (so player doesn't fall off)
            float stairWallHeight = upperFloorHeight + 1.5f;  // Taller than the max stair height
            float halfStairWidth = stairWidth / 2f;
            // Left wall (west side of stairs)
            AddWall(
                new Vector3(stairX - halfStairWidth - wallThickness / 2, stairStartY + stairLength / 2, stairWallHeight / 2),
                new Vector3(wallThickness, stairLength, stairWallHeight)
            );
            // Right wall (east side of stairs)
            AddWall(
                new Vector3(stairX + halfStairWidth + wallThickness / 2, stairStartY + stairLength / 2, stairWallHeight / 2),
                new Vector3(wallThickness, stairLength, stairWallHeight)
            );

            // Sound source at the bottom of stairs to help locate them
            AddSoundSource(new Vector3(stairX, stairStartY, 1.0f), "sounds/UI/menumove.mp3", true);

            // Upper balcony platform (starts where stairs end at Y=60)
            float balconyStartY = stairStartY + stairLength;  // Y=60
            AddPrimitive(
                new Vector3(-13f, (balconyStartY + 75f) / 2f, upperFloorHeight),
                new Vector3(14f, 75f - balconyStartY, 0.3f),
                MaterialType.Brick,
                false
            );
            platforms.Add(new Platform(-20f, balconyStartY, -6f, 75f, upperFloorHeight));

            // Balcony railing
            float railHeight = 1.2f;
            // East edge railing
            AddWall(new Vector3(-6f, (balconyStartY + 75f) / 2f, upperFloorHeight + railHeight / 2), new Vector3(wallThickness, 75f - balconyStartY, railHeight));

            // South edge railing - split in two parts to leave opening for stairs
            // Stairs go from X=-12 to X=-8, so leave that gap open
            float stairLeftEdge = stairX - halfStairWidth;   // -12
            float stairRightEdge = stairX + halfStairWidth;  // -8
            // West part of south railing (from X=-20 to stair left edge)
            float westRailWidth = stairLeftEdge - (-20f);  // 8m
            AddWall(new Vector3(-20f + westRailWidth / 2f, balconyStartY, upperFloorHeight + railHeight / 2), new Vector3(westRailWidth, wallThickness, railHeight));
            // East part of south railing (from stair right edge to X=-6)
            float eastRailWidth = -6f - stairRightEdge;  // 2m
            AddWall(new Vector3(stairRightEdge + eastRailWidth / 2f, balconyStartY, upperFloorHeight + railHeight / 2), new Vector3(eastRailWidth, wallThickness, railHeight));

            // ============================================
            // FLOORS & CEILINGS
            // ============================================
            // Ground floor
            AddPrimitive(new Vector3(5f, 40f, -0.15f), new Vector3(60f, 80f, 0.3f), MaterialType.Concrete, false);
            // Ceiling
            AddPrimitive(new Vector3(5f, 40f, wallHeight + 0.15f), new Vector3(60f, 80f, 0.3f), MaterialType.Concrete, false);

            // ============================================
            // SOUND SOURCES
            // ============================================
            // Room 1 - small room ambience
            AddSoundSource(new Vector3(5f, 5f, 1.5f), "sounds/rooms/window.mp3", true);

            // Room 2 - medium room rain
            AddSoundSource(new Vector3(5f, 17f, 1.5f), "sounds/rooms/rain house.mp3", true);

            // Room 3 - large room ambience
            AddSoundSource(new Vector3(5f, 35f, 1.5f), "sounds/rooms/window.mp3", true);

            // Hall - ground floor ambience
            AddSoundSource(new Vector3(10f, 60f, 1.5f), "sounds/rooms/rain house.mp3", true);

            // Hall - upper balcony (tests HRTF vertical!)
            AddSoundSource(new Vector3(-15f, 65f, upperFloorHeight + 1.5f), "sounds/rooms/window.mp3", true);

            // ============================================
            // PLAYER SPAWN
            // ============================================
            player = new Player(this);
            player.Position = new Vector3(5f, 5f, 0);
            player.Angle = 0;

            Program.Log($"Map built with {doors.Count} doors, {stairs.Count} stairs, {platforms.Count} platforms");
            Program.Log($"Room 1: 10x10m, Room 2: 20x15m, Room 3: 30x20m, Hall: 50x30m");
            Program.Log($"Press E near doors to open/close them");
        }

        private void AddPrimitive(Vector3 position, Vector3 size, MaterialType material, bool hasCollision)
        {
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

        private void AddWall(Vector3 position, Vector3 size, MaterialType material = MaterialType.Brick)
        {
            AddPrimitive(position, size, material, true);
        }

        private void AddSoundSource(Vector3 position, string soundPath, bool looping)
        {
            var soundObj = new SoundObject(this, position, soundPath, looping);
            soundObjects.Add(soundObj);
        }

        public void Update(KeyboardState keystate, GameTime gameTime)
        {
            player.Update(keystate, gameTime);

            // Update audio listener (player ear height is 1.7m above feet)
            float yaw = player.Angle * MathF.PI / 180f;
            audioManager.UpdateListener(
                player.Position.X,
                player.Position.Y,
                player.Position.Z + 1.7f,  // Ear height
                yaw
            );

            // Update doors
            foreach (var door in doors)
            {
                door.Update();
            }

            // Update sound objects
            foreach (var obj in soundObjects)
            {
                obj.Update(gameTime);
            }
        }
    }

    public class BoxCollider
    {
        public Vector3 Position;
        public Vector3 Size;

        public Vector3 Min => Position - Size / 2;
        public Vector3 Max => Position + Size / 2;

        public BoxCollider(Vector3 position, Vector3 size)
        {
            Position = position;
            Size = size;
        }

        public bool Intersects(Vector3 point, float radius)
        {
            float closestX = Math.Clamp(point.X, Min.X, Max.X);
            float closestY = Math.Clamp(point.Y, Min.Y, Max.Y);
            float closestZ = Math.Clamp(point.Z, Min.Z, Max.Z);

            float distanceX = point.X - closestX;
            float distanceY = point.Y - closestY;
            float distanceZ = point.Z - closestZ;

            float distanceSquared = distanceX * distanceX + distanceY * distanceY + distanceZ * distanceZ;
            return distanceSquared < (radius * radius);
        }

        public Vector3 ResolveCollision(Vector3 point, float radius)
        {
            float closestX = Math.Clamp(point.X, Min.X, Max.X);
            float closestY = Math.Clamp(point.Y, Min.Y, Max.Y);
            float closestZ = Math.Clamp(point.Z, Min.Z, Max.Z);

            Vector3 closest = new Vector3(closestX, closestY, closestZ);
            Vector3 direction = point - closest;
            float distance = direction.Length();

            if (distance < radius && distance > 0.0001f)
            {
                direction.Normalize();
                return closest + direction * radius;
            }

            return point;
        }
    }

    public class SoundObject
    {
        private Map map;
        private Vector3 position;
        private string soundPath;
        private bool looping;
        private AudioSource audioSource;

        public SoundObject(Map map, Vector3 position, string soundPath, bool looping)
        {
            this.map = map;
            this.position = position;
            this.soundPath = soundPath;
            this.looping = looping;

            audioSource = map.AudioManager.Play3D(soundPath, position.X, position.Y, position.Z, looping);

            if (audioSource == null)
            {
                Program.Log($"SoundObject: Could not load sound {soundPath}");
            }
            else
            {
                Program.Log($"SoundObject: Playing {soundPath} at ({position.X}, {position.Y}, {position.Z})");
            }
        }

        public void Update(GameTime gameTime)
        {
        }
    }
}
