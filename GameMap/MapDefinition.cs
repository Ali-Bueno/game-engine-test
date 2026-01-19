using System.Collections.Generic;
using Microsoft.Xna.Framework;
using vaudio;

namespace Game3.GameMap
{
    /// <summary>
    /// Pure data definition of a map.
    /// Contains all the information needed to build a map but no behavior.
    /// Can be serialized to/from JSON for map loading.
    /// </summary>
    public class MapDefinition
    {
        /// <summary>
        /// Map name/identifier.
        /// </summary>
        public string Name { get; set; } = "Unnamed Map";

        /// <summary>
        /// Room definitions.
        /// </summary>
        public List<RoomDefinition> Rooms { get; set; } = new List<RoomDefinition>();

        /// <summary>
        /// Stair definitions.
        /// </summary>
        public List<StairDefinition> Stairs { get; set; } = new List<StairDefinition>();

        /// <summary>
        /// Sound source definitions.
        /// </summary>
        public List<SoundSourceDefinition> SoundSources { get; set; } = new List<SoundSourceDefinition>();

        /// <summary>
        /// Player spawn point.
        /// </summary>
        public PlayerSpawnDefinition PlayerSpawn { get; set; }

        /// <summary>
        /// Default wall settings.
        /// </summary>
        public WallSettings DefaultWallSettings { get; set; } = new WallSettings();
    }

    /// <summary>
    /// Definition of a room.
    /// </summary>
    public class RoomDefinition
    {
        public string Name { get; set; }
        public Vector3 Center { get; set; }
        public Vector3 Size { get; set; }
        public float FloorZ { get; set; } = 0f;
        public List<DoorOpeningDefinition> DoorOpenings { get; set; } = new List<DoorOpeningDefinition>();
        public MaterialType WallMaterial { get; set; } = MaterialType.Brick;
        public MaterialType FloorMaterial { get; set; } = MaterialType.Concrete;
        public MaterialType CeilingMaterial { get; set; } = MaterialType.Concrete;
    }

    /// <summary>
    /// Definition of a door opening in a wall.
    /// </summary>
    public class DoorOpeningDefinition
    {
        public string Id { get; set; }
        public WallSide Side { get; set; }
        public float PositionAlongWall { get; set; } = 0.5f;
        public float Width { get; set; } = 1.5f;
        public float Height { get; set; } = 2.2f;
        public float BottomOffset { get; set; } = 0f;
        public bool HasDoor { get; set; } = true;
        public string DoorSoundFolder { get; set; } = "sounds/doors/door1";
    }

    /// <summary>
    /// Definition of a stair.
    /// </summary>
    public class StairDefinition
    {
        public Vector3 StartPosition { get; set; }
        public float Length { get; set; }
        public float Width { get; set; }
        public float HeightChange { get; set; }
        public StairDirection Direction { get; set; }
        public bool HasSideWalls { get; set; } = false;
        public float SideWallHeight { get; set; } = 1.5f;
        public MaterialType Material { get; set; } = MaterialType.Brick;
    }

    /// <summary>
    /// Definition of a sound source.
    /// </summary>
    public class SoundSourceDefinition
    {
        public Vector3 Position { get; set; }
        public string SoundPath { get; set; }
        public bool Looping { get; set; } = true;
        public float Volume { get; set; } = 1f;
    }

    /// <summary>
    /// Definition of player spawn point.
    /// </summary>
    public class PlayerSpawnDefinition
    {
        public Vector3 Position { get; set; }
        public float Angle { get; set; } = 0f;
    }

    /// <summary>
    /// Default wall settings for the map.
    /// </summary>
    public class WallSettings
    {
        public float Height { get; set; } = 5f;
        public float Thickness { get; set; } = 0.3f;
        public MaterialType Material { get; set; } = MaterialType.Brick;
        public MaterialType FloorMaterial { get; set; } = MaterialType.Concrete;
        public MaterialType CeilingMaterial { get; set; } = MaterialType.Concrete;
    }
}
