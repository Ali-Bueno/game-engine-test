using System;
using Microsoft.Xna.Framework;
using vaudio;

namespace Game3.GameMap
{
    /// <summary>
    /// Escalera que conecta diferentes alturas.
    /// Calcula automáticamente la altura Z del jugador basándose en su posición.
    /// </summary>
    public class GameStair
    {
        private GameMap map;
        private Vector3 startPosition;  // Esquina inferior (Z más bajo)
        private float length;           // Largo en la dirección de subida
        private float width;            // Ancho perpendicular a la dirección
        private float heightChange;     // Cambio total de altura
        private StairDirection direction;

        // Material de la escalera
        private MaterialType material = MaterialType.Brick; // Madera-like

        // Paredes laterales opcionales
        private bool hasSideWalls = false;
        private float sideWallHeight = 1.5f;

        private bool isBuilt = false;

        public Vector3 StartPosition => startPosition;
        public float Length => length;
        public float Width => width;
        public float HeightChange => heightChange;
        public StairDirection Direction => direction;
        public MaterialType Material => material;

        /// <summary>
        /// End position of the stair (top of the stairs).
        /// </summary>
        public Vector3 EndPosition
        {
            get
            {
                switch (direction)
                {
                    case StairDirection.North:
                        return new Vector3(startPosition.X, startPosition.Y + length, startPosition.Z + heightChange);
                    case StairDirection.South:
                        return new Vector3(startPosition.X, startPosition.Y - length, startPosition.Z + heightChange);
                    case StairDirection.East:
                        return new Vector3(startPosition.X + length, startPosition.Y, startPosition.Z + heightChange);
                    case StairDirection.West:
                        return new Vector3(startPosition.X - length, startPosition.Y, startPosition.Z + heightChange);
                    default:
                        return startPosition + new Vector3(0, 0, heightChange);
                }
            }
        }

        /// <summary>
        /// Esquina mínima del bounding box de la escalera
        /// </summary>
        public Vector3 Min
        {
            get
            {
                float halfWidth = width / 2f;
                switch (direction)
                {
                    case StairDirection.North:
                        return new Vector3(startPosition.X - halfWidth, startPosition.Y, startPosition.Z);
                    case StairDirection.South:
                        return new Vector3(startPosition.X - halfWidth, startPosition.Y - length, startPosition.Z);
                    case StairDirection.East:
                        return new Vector3(startPosition.X, startPosition.Y - halfWidth, startPosition.Z);
                    case StairDirection.West:
                        return new Vector3(startPosition.X - length, startPosition.Y - halfWidth, startPosition.Z);
                    default:
                        return startPosition;
                }
            }
        }

        /// <summary>
        /// Esquina máxima del bounding box de la escalera
        /// </summary>
        public Vector3 Max
        {
            get
            {
                float halfWidth = width / 2f;
                float topZ = startPosition.Z + heightChange + (hasSideWalls ? sideWallHeight : 2f);
                switch (direction)
                {
                    case StairDirection.North:
                        return new Vector3(startPosition.X + halfWidth, startPosition.Y + length, topZ);
                    case StairDirection.South:
                        return new Vector3(startPosition.X + halfWidth, startPosition.Y, topZ);
                    case StairDirection.East:
                        return new Vector3(startPosition.X + length, startPosition.Y + halfWidth, topZ);
                    case StairDirection.West:
                        return new Vector3(startPosition.X, startPosition.Y + halfWidth, topZ);
                    default:
                        return startPosition + new Vector3(width, length, topZ);
                }
            }
        }

        public GameStair(GameMap map, Vector3 startPosition, float length, float width, float heightChange, StairDirection direction)
        {
            this.map = map;
            this.startPosition = startPosition;
            this.length = length;
            this.width = width;
            this.heightChange = heightChange;
            this.direction = direction;
        }

        public GameStair SetMaterial(MaterialType mat)
        {
            this.material = mat;
            return this;
        }

        public GameStair WithSideWalls(float wallHeight = 1.5f)
        {
            this.hasSideWalls = true;
            this.sideWallHeight = wallHeight;
            return this;
        }

        public void Build()
        {
            if (isBuilt) return;
            isBuilt = true;

            // Crear geometría de la escalera para raytracing
            Vector3 center = GetCenterPosition();
            Vector3 stairSize = GetSize();

            // Primitiva principal (el suelo de la escalera)
            map.AddPrimitive(
                new Vector3(center.X, center.Y, startPosition.Z + heightChange / 2f),
                new Vector3(stairSize.X, stairSize.Y, 0.2f),
                material,
                false  // Sin collider (el jugador camina por encima)
            );

            // Paredes laterales opcionales
            if (hasSideWalls)
            {
                BuildSideWalls();
            }

            // Añadir como plataforma para el jugador
            AddStairPlatform();

            Program.Log($"GameStair built: {direction} at ({startPosition.X:F1}, {startPosition.Y:F1}, {startPosition.Z:F1}), " +
                       $"length={length:F1}, width={width:F1}, height={heightChange:F1}");
        }

        private void BuildSideWalls()
        {
            float halfWidth = width / 2f;
            float wallThickness = map.DefaultWallThickness;
            float maxWallHeight = heightChange + sideWallHeight;

            Vector3 leftPos, rightPos, wallSize;

            switch (direction)
            {
                case StairDirection.North:
                    leftPos = new Vector3(startPosition.X - halfWidth - wallThickness / 2, startPosition.Y + length / 2, startPosition.Z + maxWallHeight / 2);
                    rightPos = new Vector3(startPosition.X + halfWidth + wallThickness / 2, startPosition.Y + length / 2, startPosition.Z + maxWallHeight / 2);
                    wallSize = new Vector3(wallThickness, length, maxWallHeight);
                    break;
                case StairDirection.South:
                    leftPos = new Vector3(startPosition.X + halfWidth + wallThickness / 2, startPosition.Y - length / 2, startPosition.Z + maxWallHeight / 2);
                    rightPos = new Vector3(startPosition.X - halfWidth - wallThickness / 2, startPosition.Y - length / 2, startPosition.Z + maxWallHeight / 2);
                    wallSize = new Vector3(wallThickness, length, maxWallHeight);
                    break;
                case StairDirection.East:
                    leftPos = new Vector3(startPosition.X + length / 2, startPosition.Y + halfWidth + wallThickness / 2, startPosition.Z + maxWallHeight / 2);
                    rightPos = new Vector3(startPosition.X + length / 2, startPosition.Y - halfWidth - wallThickness / 2, startPosition.Z + maxWallHeight / 2);
                    wallSize = new Vector3(length, wallThickness, maxWallHeight);
                    break;
                case StairDirection.West:
                    leftPos = new Vector3(startPosition.X - length / 2, startPosition.Y - halfWidth - wallThickness / 2, startPosition.Z + maxWallHeight / 2);
                    rightPos = new Vector3(startPosition.X - length / 2, startPosition.Y + halfWidth + wallThickness / 2, startPosition.Z + maxWallHeight / 2);
                    wallSize = new Vector3(length, wallThickness, maxWallHeight);
                    break;
                default:
                    return;
            }

            // Pared izquierda
            map.AddPrimitive(leftPos, wallSize, MaterialType.Brick, true);
            // Pared derecha
            map.AddPrimitive(rightPos, wallSize, MaterialType.Brick, true);
        }

        private void AddStairPlatform()
        {
            float minX, maxX, minY, maxY;
            float halfWidth = width / 2f;

            switch (direction)
            {
                case StairDirection.North:
                    minX = startPosition.X - halfWidth;
                    maxX = startPosition.X + halfWidth;
                    minY = startPosition.Y;
                    maxY = startPosition.Y + length;
                    break;
                case StairDirection.South:
                    minX = startPosition.X - halfWidth;
                    maxX = startPosition.X + halfWidth;
                    minY = startPosition.Y - length;
                    maxY = startPosition.Y;
                    break;
                case StairDirection.East:
                    minX = startPosition.X;
                    maxX = startPosition.X + length;
                    minY = startPosition.Y - halfWidth;
                    maxY = startPosition.Y + halfWidth;
                    break;
                case StairDirection.West:
                    minX = startPosition.X - length;
                    maxX = startPosition.X;
                    minY = startPosition.Y - halfWidth;
                    maxY = startPosition.Y + halfWidth;
                    break;
                default:
                    return;
            }

            // Se maneja a través de GetHeightAtPosition en lugar de Platform
        }

        private Vector3 GetCenterPosition()
        {
            float halfWidth = width / 2f;
            float halfLength = length / 2f;

            switch (direction)
            {
                case StairDirection.North:
                    return new Vector3(startPosition.X, startPosition.Y + halfLength, startPosition.Z);
                case StairDirection.South:
                    return new Vector3(startPosition.X, startPosition.Y - halfLength, startPosition.Z);
                case StairDirection.East:
                    return new Vector3(startPosition.X + halfLength, startPosition.Y, startPosition.Z);
                case StairDirection.West:
                    return new Vector3(startPosition.X - halfLength, startPosition.Y, startPosition.Z);
                default:
                    return startPosition;
            }
        }

        private Vector3 GetSize()
        {
            switch (direction)
            {
                case StairDirection.North:
                case StairDirection.South:
                    return new Vector3(width, length, 0.2f);
                default:
                    return new Vector3(length, width, 0.2f);
            }
        }
    }
}
