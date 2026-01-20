using Arch.Core;
using Game3.ECS.Components;
using Game3.GameMap;
using vaudio;

namespace Game3.ECS.Systems
{
    /// <summary>
    /// Static utility to create SurfaceMaterial entities from GameMap data.
    /// Called once during initialization.
    /// </summary>
    public static class SurfaceInitSystem
    {
        private const float PlayerHeightTolerance = 2.5f;  // How high above floor to detect

        /// <summary>
        /// Creates SurfaceMaterial entities for all rooms and stairs in the GameMap.
        /// </summary>
        public static void InitializeFromGameMap(World world, GameMap.GameMap gameMap)
        {
            int surfaceCount = 0;

            // Create surfaces from rooms
            foreach (var room in gameMap.Rooms)
            {
                CreateRoomSurface(world, room);
                surfaceCount++;
            }

            // Create surfaces from stairs (higher priority to override room floors)
            foreach (var stair in gameMap.Stairs)
            {
                CreateStairSurface(world, stair);
                surfaceCount++;
            }

            Program.Log($"SurfaceInitSystem: Created {surfaceCount} surface material entities");
        }

        private static void CreateRoomSurface(World world, GameRoom room)
        {
            // Create a SurfaceMaterial entity for the room's floor
            world.Create(
                new SurfaceMaterial
                {
                    Material = room.FloorMaterial,
                    MinX = room.Left,
                    MinY = room.Bottom,
                    MaxX = room.Right,
                    MaxY = room.Top,
                    MinZ = room.FloorZ - 0.5f,  // Slightly below floor
                    MaxZ = room.FloorZ + PlayerHeightTolerance,
                    Priority = 0  // Base priority for rooms
                }
            );

            Program.Log($"SurfaceInitSystem: Room '{room.Name}' - material={room.FloorMaterial}, bounds=({room.Left:F1},{room.Bottom:F1}) to ({room.Right:F1},{room.Top:F1})");
        }

        private static void CreateStairSurface(World world, GameStair stair)
        {
            // Calculate stair bounds based on direction
            float minX, maxX, minY, maxY;
            float minZ = stair.StartPosition.Z - 0.5f;
            float maxZ = stair.EndPosition.Z + PlayerHeightTolerance;

            switch (stair.Direction)
            {
                case StairDirection.North:
                    minX = stair.StartPosition.X - stair.Width / 2;
                    maxX = stair.StartPosition.X + stair.Width / 2;
                    minY = stair.StartPosition.Y;
                    maxY = stair.StartPosition.Y + stair.Length;
                    break;
                case StairDirection.South:
                    minX = stair.StartPosition.X - stair.Width / 2;
                    maxX = stair.StartPosition.X + stair.Width / 2;
                    minY = stair.StartPosition.Y - stair.Length;
                    maxY = stair.StartPosition.Y;
                    break;
                case StairDirection.East:
                    minX = stair.StartPosition.X;
                    maxX = stair.StartPosition.X + stair.Length;
                    minY = stair.StartPosition.Y - stair.Width / 2;
                    maxY = stair.StartPosition.Y + stair.Width / 2;
                    break;
                case StairDirection.West:
                    minX = stair.StartPosition.X - stair.Length;
                    maxX = stair.StartPosition.X;
                    minY = stair.StartPosition.Y - stair.Width / 2;
                    maxY = stair.StartPosition.Y + stair.Width / 2;
                    break;
                default:
                    return;
            }

            // Create surface for the stair with higher priority
            world.Create(
                new SurfaceMaterial
                {
                    Material = stair.Material,
                    MinX = minX,
                    MinY = minY,
                    MaxX = maxX,
                    MaxY = maxY,
                    MinZ = minZ,
                    MaxZ = maxZ,
                    Priority = 10  // Higher priority to override room floors
                }
            );

            Program.Log($"SurfaceInitSystem: Stair - material={stair.Material}, bounds=({minX:F1},{minY:F1}) to ({maxX:F1},{maxY:F1})");
        }

        /// <summary>
        /// Creates a custom SurfaceMaterial entity.
        /// </summary>
        public static Entity CreateSurface(World world, MaterialType material,
            float minX, float minY, float maxX, float maxY,
            float minZ, float maxZ, int priority = 0)
        {
            return world.Create(
                new SurfaceMaterial
                {
                    Material = material,
                    MinX = minX,
                    MinY = minY,
                    MaxX = maxX,
                    MaxY = maxY,
                    MinZ = minZ,
                    MaxZ = maxZ,
                    Priority = priority
                }
            );
        }
    }
}
