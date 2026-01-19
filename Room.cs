using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Game3.Audio;
using vaudio;

namespace Game3
{
    /// <summary>
    /// Define una habitación rectangular completa con todas las paredes, suelo y techo
    /// para que vaudio pueda calcular correctamente el reverb.
    /// </summary>
    public class Room
    {
        private Map map;
        private string name;
        private Vector3 position;      // Centro de la habitación (X, Y) y altura del suelo (Z)
        private Vector3 size;          // Ancho (X), Largo (Y), Alto (Z)
        private MaterialType material;
        private float wallThickness;

        // Aperturas (para puertas, pasillos, etc.)
        private List<RoomOpening> openings = new List<RoomOpening>();

        public string Name => name;
        public Vector3 Position => position;
        public Vector3 Size => size;
        public Vector3 Min => new Vector3(position.X - size.X / 2, position.Y - size.Y / 2, position.Z);
        public Vector3 Max => new Vector3(position.X + size.X / 2, position.Y + size.Y / 2, position.Z + size.Z);

        public Room(Map map, string name, Vector3 center, Vector3 size, MaterialType material = MaterialType.Brick, float wallThickness = 0.3f)
        {
            this.map = map;
            this.name = name;
            this.position = center;
            this.size = size;
            this.material = material;
            this.wallThickness = wallThickness;
        }

        /// <summary>
        /// Añade una apertura en una pared (para puertas o conexiones con otras habitaciones)
        /// </summary>
        public void AddOpening(WallSide side, float position, float width, float height, float bottomOffset = 0f)
        {
            openings.Add(new RoomOpening
            {
                Side = side,
                Position = position,      // Posición a lo largo de la pared (0 = inicio, 1 = fin)
                Width = width,
                Height = height,
                BottomOffset = bottomOffset
            });
        }

        /// <summary>
        /// Construye toda la geometría de la habitación
        /// </summary>
        public void Build()
        {
            float halfX = size.X / 2;
            float halfY = size.Y / 2;
            float floorZ = position.Z;
            float ceilingZ = position.Z + size.Z;

            // Suelo
            AddPrimitive(
                new Vector3(position.X, position.Y, floorZ - wallThickness / 2),
                new Vector3(size.X + wallThickness * 2, size.Y + wallThickness * 2, wallThickness),
                false
            );

            // Techo
            AddPrimitive(
                new Vector3(position.X, position.Y, ceilingZ + wallThickness / 2),
                new Vector3(size.X + wallThickness * 2, size.Y + wallThickness * 2, wallThickness),
                false
            );

            // Paredes con aperturas
            BuildWallWithOpenings(WallSide.North,
                new Vector3(position.X, position.Y + halfY, floorZ + size.Z / 2),
                new Vector3(size.X, wallThickness, size.Z));

            BuildWallWithOpenings(WallSide.South,
                new Vector3(position.X, position.Y - halfY, floorZ + size.Z / 2),
                new Vector3(size.X, wallThickness, size.Z));

            BuildWallWithOpenings(WallSide.East,
                new Vector3(position.X + halfX, position.Y, floorZ + size.Z / 2),
                new Vector3(wallThickness, size.Y, size.Z));

            BuildWallWithOpenings(WallSide.West,
                new Vector3(position.X - halfX, position.Y, floorZ + size.Z / 2),
                new Vector3(wallThickness, size.Y, size.Z));

            Program.Log($"Room '{name}' built: center=({position.X}, {position.Y}, {position.Z}), size=({size.X}, {size.Y}, {size.Z})");
        }

        private void BuildWallWithOpenings(WallSide side, Vector3 wallCenter, Vector3 wallSize)
        {
            var sideOpenings = openings.FindAll(o => o.Side == side);

            if (sideOpenings.Count == 0)
            {
                // Pared completa sin aperturas
                AddWall(wallCenter, wallSize);
                return;
            }

            // Construir pared con aperturas
            // Por simplicidad, solo soportamos una apertura por pared por ahora
            var opening = sideOpenings[0];

            bool isNorthSouth = (side == WallSide.North || side == WallSide.South);
            float wallLength = isNorthSouth ? wallSize.X : wallSize.Y;
            float openingCenter = wallLength * (opening.Position - 0.5f); // Convertir 0-1 a posición real

            // Segmento izquierdo/inferior de la pared
            float leftLength = (wallLength / 2) - (opening.Width / 2) + openingCenter;
            if (leftLength > 0.1f)
            {
                Vector3 leftSize, leftPos;
                if (isNorthSouth)
                {
                    leftSize = new Vector3(leftLength, wallSize.Y, wallSize.Z);
                    leftPos = new Vector3(wallCenter.X - wallLength / 2 + leftLength / 2, wallCenter.Y, wallCenter.Z);
                }
                else
                {
                    leftSize = new Vector3(wallSize.X, leftLength, wallSize.Z);
                    leftPos = new Vector3(wallCenter.X, wallCenter.Y - wallLength / 2 + leftLength / 2, wallCenter.Z);
                }
                AddWall(leftPos, leftSize);
            }

            // Segmento derecho/superior de la pared
            float rightLength = (wallLength / 2) - (opening.Width / 2) - openingCenter;
            if (rightLength > 0.1f)
            {
                Vector3 rightSize, rightPos;
                if (isNorthSouth)
                {
                    rightSize = new Vector3(rightLength, wallSize.Y, wallSize.Z);
                    rightPos = new Vector3(wallCenter.X + wallLength / 2 - rightLength / 2, wallCenter.Y, wallCenter.Z);
                }
                else
                {
                    rightSize = new Vector3(wallSize.X, rightLength, wallSize.Z);
                    rightPos = new Vector3(wallCenter.X, wallCenter.Y + wallLength / 2 - rightLength / 2, wallCenter.Z);
                }
                AddWall(rightPos, rightSize);
            }

            // Dintel sobre la apertura (si la apertura no llega al techo)
            float topHeight = wallSize.Z - opening.Height - opening.BottomOffset;
            if (topHeight > 0.1f)
            {
                Vector3 topSize, topPos;
                float topZ = wallCenter.Z + wallSize.Z / 2 - topHeight / 2;
                if (isNorthSouth)
                {
                    topSize = new Vector3(opening.Width, wallSize.Y, topHeight);
                    topPos = new Vector3(wallCenter.X + openingCenter, wallCenter.Y, topZ);
                }
                else
                {
                    topSize = new Vector3(wallSize.X, opening.Width, topHeight);
                    topPos = new Vector3(wallCenter.X, wallCenter.Y + openingCenter, topZ);
                }
                AddPrimitive(topPos, topSize, true);
            }
        }

        private void AddWall(Vector3 pos, Vector3 size)
        {
            // Añadir primitiva para raytracing
            var primitive = new PrismPrimitive()
            {
                material = material,
                size = new Vector3F(size.X, size.Y, size.Z),
                transform = Matrix4F.CreateTranslation(pos.X, pos.Y, pos.Z)
            };
            map.AudioManager.AddPrimitive(primitive);

            // Añadir collider
            map.Colliders.Add(new BoxCollider(pos, size));
        }

        private void AddPrimitive(Vector3 pos, Vector3 size, bool hasCollision)
        {
            var primitive = new PrismPrimitive()
            {
                material = material,
                size = new Vector3F(size.X, size.Y, size.Z),
                transform = Matrix4F.CreateTranslation(pos.X, pos.Y, pos.Z)
            };
            map.AudioManager.AddPrimitive(primitive);

            if (hasCollision)
            {
                map.Colliders.Add(new BoxCollider(pos, size));
            }
        }

        /// <summary>
        /// Verifica si un punto está dentro de la habitación
        /// </summary>
        public bool Contains(Vector3 point)
        {
            return point.X >= Min.X && point.X <= Max.X &&
                   point.Y >= Min.Y && point.Y <= Max.Y &&
                   point.Z >= Min.Z && point.Z <= Max.Z;
        }
    }

    public enum WallSide
    {
        North,  // +Y
        South,  // -Y
        East,   // +X
        West    // -X
    }

    public struct RoomOpening
    {
        public WallSide Side;
        public float Position;      // 0-1 a lo largo de la pared
        public float Width;
        public float Height;
        public float BottomOffset;  // Distancia desde el suelo
    }
}
