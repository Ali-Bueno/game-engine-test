using System;
using Microsoft.Xna.Framework;
using vaudio;

namespace Game3.GameMap
{
    /// <summary>
    /// Pasillo que conecta dos habitaciones.
    /// Crea automáticamente las paredes laterales, suelo y techo del pasillo.
    /// </summary>
    public class Corridor
    {
        private GameMap map;
        private GameRoom fromRoom;
        private WallSide fromSide;
        private GameRoom toRoom;
        private WallSide toSide;
        private float width;
        private float height;

        // Materiales (opcionales, hereda del mapa si no se especifican)
        private MaterialType? wallMaterial;
        private MaterialType? floorMaterial;
        private MaterialType? ceilingMaterial;

        private bool isBuilt = false;

        public GameRoom FromRoom => fromRoom;
        public GameRoom ToRoom => toRoom;
        public float Width => width;
        public float Height => height;

        public MaterialType WallMaterial => wallMaterial ?? map.DefaultWallMaterial;
        public MaterialType FloorMaterial => floorMaterial ?? map.DefaultFloorMaterial;
        public MaterialType CeilingMaterial => ceilingMaterial ?? map.DefaultCeilingMaterial;

        public Corridor(GameMap map, GameRoom from, WallSide fromSide, GameRoom to, WallSide toSide, float width)
        {
            this.map = map;
            this.fromRoom = from;
            this.fromSide = fromSide;
            this.toRoom = to;
            this.toSide = toSide;
            this.width = width;
            this.height = Math.Min(from.Size.Z, to.Size.Z); // Altura = menor de las dos habitaciones
        }

        public Corridor SetWallMaterial(MaterialType material)
        {
            this.wallMaterial = material;
            return this;
        }

        public Corridor SetFloorMaterial(MaterialType material)
        {
            this.floorMaterial = material;
            return this;
        }

        public Corridor SetCeilingMaterial(MaterialType material)
        {
            this.ceilingMaterial = material;
            return this;
        }

        public Corridor SetHeight(float h)
        {
            this.height = h;
            return this;
        }

        public void Build()
        {
            if (isBuilt) return;
            isBuilt = true;

            // Obtener puntos de conexión
            Vector3 startPoint = GetConnectionPoint(fromRoom, fromSide);
            Vector3 endPoint = GetConnectionPoint(toRoom, toSide);

            // Calcular dirección y longitud del pasillo
            Vector3 direction = endPoint - startPoint;
            float length = direction.Length();

            if (length < 0.1f)
            {
                Program.Log($"Corridor: Rooms are too close, skipping ({fromRoom.Name} to {toRoom.Name})");
                return;
            }

            direction.Normalize();

            // Determinar si el pasillo es horizontal (X) o vertical (Y)
            bool isHorizontal = Math.Abs(direction.X) > Math.Abs(direction.Y);

            // Altura del suelo (usar la más baja de las dos habitaciones)
            float floorZ = Math.Min(fromRoom.FloorZ, toRoom.FloorZ);
            float thickness = map.DefaultWallThickness;

            // Centro del pasillo
            Vector3 center = (startPoint + endPoint) / 2;
            center.Z = floorZ + height / 2;

            Program.Log($"Building corridor from '{fromRoom.Name}' to '{toRoom.Name}': length={length:F1}, width={width:F1}");

            if (isHorizontal)
            {
                // Pasillo horizontal (va de este a oeste o viceversa)
                BuildHorizontalCorridor(startPoint, endPoint, floorZ, length, thickness);
            }
            else
            {
                // Pasillo vertical (va de norte a sur o viceversa)
                BuildVerticalCorridor(startPoint, endPoint, floorZ, length, thickness);
            }
        }

        private void BuildHorizontalCorridor(Vector3 start, Vector3 end, float floorZ, float length, float thickness)
        {
            float minX = Math.Min(start.X, end.X);
            float maxX = Math.Max(start.X, end.X);
            float centerX = (minX + maxX) / 2;
            float centerY = (start.Y + end.Y) / 2;

            // Suelo
            map.AddPrimitive(
                new Vector3(centerX, centerY, floorZ - thickness / 2),
                new Vector3(length, width, thickness),
                FloorMaterial, false
            );

            // Techo
            map.AddPrimitive(
                new Vector3(centerX, centerY, floorZ + height + thickness / 2),
                new Vector3(length, width, thickness),
                CeilingMaterial, false
            );

            // Paredes laterales (norte y sur)
            float wallY_North = centerY + width / 2;
            float wallY_South = centerY - width / 2;

            map.AddPrimitive(
                new Vector3(centerX, wallY_North, floorZ + height / 2),
                new Vector3(length, thickness, height),
                WallMaterial, true
            );

            map.AddPrimitive(
                new Vector3(centerX, wallY_South, floorZ + height / 2),
                new Vector3(length, thickness, height),
                WallMaterial, true
            );
        }

        private void BuildVerticalCorridor(Vector3 start, Vector3 end, float floorZ, float length, float thickness)
        {
            float minY = Math.Min(start.Y, end.Y);
            float maxY = Math.Max(start.Y, end.Y);
            float centerY = (minY + maxY) / 2;
            float centerX = (start.X + end.X) / 2;

            // Suelo
            map.AddPrimitive(
                new Vector3(centerX, centerY, floorZ - thickness / 2),
                new Vector3(width, length, thickness),
                FloorMaterial, false
            );

            // Techo
            map.AddPrimitive(
                new Vector3(centerX, centerY, floorZ + height + thickness / 2),
                new Vector3(width, length, thickness),
                CeilingMaterial, false
            );

            // Paredes laterales (este y oeste)
            float wallX_East = centerX + width / 2;
            float wallX_West = centerX - width / 2;

            map.AddPrimitive(
                new Vector3(wallX_East, centerY, floorZ + height / 2),
                new Vector3(thickness, length, height),
                WallMaterial, true
            );

            map.AddPrimitive(
                new Vector3(wallX_West, centerY, floorZ + height / 2),
                new Vector3(thickness, length, height),
                WallMaterial, true
            );
        }

        private Vector3 GetConnectionPoint(GameRoom room, WallSide side)
        {
            float x, y;
            float z = room.FloorZ;

            switch (side)
            {
                case WallSide.North:
                    x = room.Center.X;
                    y = room.Top;
                    break;
                case WallSide.South:
                    x = room.Center.X;
                    y = room.Bottom;
                    break;
                case WallSide.East:
                    x = room.Right;
                    y = room.Center.Y;
                    break;
                case WallSide.West:
                    x = room.Left;
                    y = room.Center.Y;
                    break;
                default:
                    return room.Center;
            }

            return new Vector3(x, y, z);
        }
    }
}
