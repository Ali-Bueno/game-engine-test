using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using vaudio;

namespace Game3.GameMap
{
    /// <summary>
    /// Habitación rectangular con paredes, suelo y techo auto-construidos.
    /// Soporta múltiples aperturas en cada pared para puertas y conexiones.
    /// </summary>
    public class GameRoom
    {
        private GameMap map;
        private string name;
        private Vector3 center;     // Centro XY, Z = altura del suelo
        private Vector3 size;       // Ancho (X), Largo (Y), Alto (Z)

        // Materiales (heredados del mapa si no se especifican)
        private MaterialType? wallMaterial;
        private MaterialType? floorMaterial;
        private MaterialType? ceilingMaterial;
        private float? wallThickness;

        // Aperturas en las paredes
        private Dictionary<string, RoomOpening> openings = new Dictionary<string, RoomOpening>();

        // Estado
        private bool isBuilt = false;

        #region Propiedades

        public string Name => name;
        public Vector3 Center => center;
        public Vector3 Size => size;

        public float Left => center.X - size.X / 2;
        public float Right => center.X + size.X / 2;
        public float Bottom => center.Y - size.Y / 2;    // Sur
        public float Top => center.Y + size.Y / 2;       // Norte
        public float FloorZ => center.Z;
        public float CeilingZ => center.Z + size.Z;

        public Vector3 Min => new Vector3(Left, Bottom, FloorZ);
        public Vector3 Max => new Vector3(Right, Top, CeilingZ);

        public MaterialType WallMaterial => wallMaterial ?? map.DefaultWallMaterial;
        public MaterialType FloorMaterial => floorMaterial ?? map.DefaultFloorMaterial;
        public MaterialType CeilingMaterial => ceilingMaterial ?? map.DefaultCeilingMaterial;
        public float WallThickness => wallThickness ?? map.DefaultWallThickness;

        #endregion

        public GameRoom(GameMap map, string name, Vector3 center, Vector3 size)
        {
            this.map = map;
            this.name = name;
            this.center = center;
            this.size = size;
        }

        #region Configuración

        public GameRoom SetWallMaterial(MaterialType material)
        {
            this.wallMaterial = material;
            return this;
        }

        public GameRoom SetFloorMaterial(MaterialType material)
        {
            this.floorMaterial = material;
            return this;
        }

        public GameRoom SetCeilingMaterial(MaterialType material)
        {
            this.ceilingMaterial = material;
            return this;
        }

        public GameRoom SetWallThickness(float thickness)
        {
            this.wallThickness = thickness;
            return this;
        }

        #endregion

        #region Aperturas

        /// <summary>
        /// Añade una apertura en una pared (para puerta, ventana o conexión)
        /// </summary>
        /// <param name="id">ID único de la apertura</param>
        /// <param name="side">Lado de la pared</param>
        /// <param name="positionAlongWall">Posición a lo largo de la pared (0=inicio, 1=fin, 0.5=centro)</param>
        /// <param name="width">Ancho de la apertura</param>
        /// <param name="height">Alto de la apertura</param>
        /// <param name="bottomOffset">Distancia desde el suelo (0 para puertas)</param>
        public GameRoom AddOpening(string id, WallSide side, float positionAlongWall, float width, float height, float bottomOffset = 0f)
        {
            openings[id] = new RoomOpening
            {
                Id = id,
                Side = side,
                PositionAlongWall = positionAlongWall,
                Width = width,
                Height = height,
                BottomOffset = bottomOffset
            };
            return this;
        }

        /// <summary>
        /// Añade una apertura para puerta (altura estándar 2.2m, desde el suelo)
        /// </summary>
        public GameRoom AddDoorOpening(string id, WallSide side, float positionAlongWall = 0.5f, float width = 2f)
        {
            return AddOpening(id, side, positionAlongWall, width, 2.2f, 0f);
        }

        /// <summary>
        /// Añade una apertura para ventana
        /// </summary>
        public GameRoom AddWindowOpening(string id, WallSide side, float positionAlongWall, float width, float height = 1.5f, float bottomOffset = 1f)
        {
            return AddOpening(id, side, positionAlongWall, width, height, bottomOffset);
        }

        /// <summary>
        /// Obtiene una apertura por ID
        /// </summary>
        public RoomOpening GetOpening(string id)
        {
            return openings.TryGetValue(id, out var opening) ? opening : null;
        }

        /// <summary>
        /// Obtiene la posición mundial de una apertura
        /// </summary>
        public Vector3 GetOpeningWorldPosition(string id)
        {
            var opening = GetOpening(id);
            if (opening == null) return Vector3.Zero;

            return GetOpeningWorldPosition(opening);
        }

        private Vector3 GetOpeningWorldPosition(RoomOpening opening)
        {
            float x, y;
            float wallLength = GetWallLength(opening.Side);
            float offset = (opening.PositionAlongWall - 0.5f) * wallLength;

            switch (opening.Side)
            {
                case WallSide.North:
                    x = center.X + offset;
                    y = Top;
                    break;
                case WallSide.South:
                    x = center.X + offset;
                    y = Bottom;
                    break;
                case WallSide.East:
                    x = Right;
                    y = center.Y + offset;
                    break;
                case WallSide.West:
                    x = Left;
                    y = center.Y + offset;
                    break;
                default:
                    return Vector3.Zero;
            }

            float z = FloorZ + opening.BottomOffset + opening.Height / 2;
            return new Vector3(x, y, z);
        }

        private float GetWallLength(WallSide side)
        {
            return (side == WallSide.North || side == WallSide.South) ? size.X : size.Y;
        }

        #endregion

        #region Construcción

        public void Build()
        {
            if (isBuilt) return;
            isBuilt = true;

            Program.Log($"Building room '{name}': center=({center.X:F1}, {center.Y:F1}, {center.Z:F1}), size=({size.X:F1}, {size.Y:F1}, {size.Z:F1})");

            // Construir suelo
            BuildFloor();

            // Construir techo
            BuildCeiling();

            // Construir paredes con aperturas
            BuildWall(WallSide.North);
            BuildWall(WallSide.South);
            BuildWall(WallSide.East);
            BuildWall(WallSide.West);
        }

        private void BuildFloor()
        {
            Vector3 pos = new Vector3(center.X, center.Y, FloorZ - WallThickness / 2);
            Vector3 floorSize = new Vector3(size.X + WallThickness * 2, size.Y + WallThickness * 2, WallThickness);
            map.AddPrimitive(pos, floorSize, FloorMaterial, false);
        }

        private void BuildCeiling()
        {
            Vector3 pos = new Vector3(center.X, center.Y, CeilingZ + WallThickness / 2);
            Vector3 ceilingSize = new Vector3(size.X + WallThickness * 2, size.Y + WallThickness * 2, WallThickness);
            map.AddPrimitive(pos, ceilingSize, CeilingMaterial, false);
        }

        private void BuildWall(WallSide side)
        {
            // Obtener aperturas para este lado
            var sideOpenings = new List<RoomOpening>();
            foreach (var opening in openings.Values)
            {
                if (opening.Side == side)
                    sideOpenings.Add(opening);
            }

            // Ordenar aperturas por posición
            sideOpenings.Sort((a, b) => a.PositionAlongWall.CompareTo(b.PositionAlongWall));

            if (sideOpenings.Count == 0)
            {
                // Pared completa sin aperturas
                BuildFullWall(side);
                return;
            }

            // Construir pared con aperturas
            BuildWallWithOpenings(side, sideOpenings);
        }

        private void BuildFullWall(WallSide side)
        {
            Vector3 pos, wallSize;
            float thickness = WallThickness;
            float height = size.Z;

            switch (side)
            {
                case WallSide.North:
                    pos = new Vector3(center.X, Top, FloorZ + height / 2);
                    wallSize = new Vector3(size.X, thickness, height);
                    break;
                case WallSide.South:
                    pos = new Vector3(center.X, Bottom, FloorZ + height / 2);
                    wallSize = new Vector3(size.X, thickness, height);
                    break;
                case WallSide.East:
                    pos = new Vector3(Right, center.Y, FloorZ + height / 2);
                    wallSize = new Vector3(thickness, size.Y, height);
                    break;
                case WallSide.West:
                    pos = new Vector3(Left, center.Y, FloorZ + height / 2);
                    wallSize = new Vector3(thickness, size.Y, height);
                    break;
                default:
                    return;
            }

            map.AddPrimitive(pos, wallSize, WallMaterial, true);
        }

        private void BuildWallWithOpenings(WallSide side, List<RoomOpening> sideOpenings)
        {
            float thickness = WallThickness;
            float wallLength = GetWallLength(side);
            float wallHeight = size.Z;
            bool isNorthSouth = (side == WallSide.North || side == WallSide.South);

            // Posición base de la pared
            float wallX, wallY;
            switch (side)
            {
                case WallSide.North:
                    wallX = center.X;
                    wallY = Top;
                    break;
                case WallSide.South:
                    wallX = center.X;
                    wallY = Bottom;
                    break;
                case WallSide.East:
                    wallX = Right;
                    wallY = center.Y;
                    break;
                case WallSide.West:
                    wallX = Left;
                    wallY = center.Y;
                    break;
                default:
                    return;
            }

            // Inicio de la pared (extremo izquierdo/inferior)
            float wallStart = isNorthSouth ? Left : Bottom;
            float wallEnd = isNorthSouth ? Right : Top;
            float currentPos = wallStart;

            foreach (var opening in sideOpenings)
            {
                // Calcular posición absoluta de la apertura
                float openingCenter = (isNorthSouth ? center.X : center.Y) +
                                     (opening.PositionAlongWall - 0.5f) * wallLength;
                float openingStart = openingCenter - opening.Width / 2;
                float openingEnd = openingCenter + opening.Width / 2;

                // Segmento de pared ANTES de la apertura
                if (openingStart > currentPos + 0.01f)
                {
                    float segmentLength = openingStart - currentPos;
                    float segmentCenter = currentPos + segmentLength / 2;

                    Vector3 segPos, segSize;
                    if (isNorthSouth)
                    {
                        segPos = new Vector3(segmentCenter, wallY, FloorZ + wallHeight / 2);
                        segSize = new Vector3(segmentLength, thickness, wallHeight);
                    }
                    else
                    {
                        segPos = new Vector3(wallX, segmentCenter, FloorZ + wallHeight / 2);
                        segSize = new Vector3(thickness, segmentLength, wallHeight);
                    }
                    map.AddPrimitive(segPos, segSize, WallMaterial, true);
                }

                // Dintel SOBRE la apertura (si no llega al techo)
                float topOfOpening = opening.BottomOffset + opening.Height;
                if (topOfOpening < wallHeight - 0.01f)
                {
                    float lintelHeight = wallHeight - topOfOpening;
                    float lintelZ = FloorZ + topOfOpening + lintelHeight / 2;

                    Vector3 lintelPos, lintelSize;
                    if (isNorthSouth)
                    {
                        lintelPos = new Vector3(openingCenter, wallY, lintelZ);
                        lintelSize = new Vector3(opening.Width, thickness, lintelHeight);
                    }
                    else
                    {
                        lintelPos = new Vector3(wallX, openingCenter, lintelZ);
                        lintelSize = new Vector3(thickness, opening.Width, lintelHeight);
                    }
                    map.AddPrimitive(lintelPos, lintelSize, WallMaterial, true);
                }

                // Muro bajo la apertura (si hay bottomOffset)
                if (opening.BottomOffset > 0.01f)
                {
                    float sillZ = FloorZ + opening.BottomOffset / 2;

                    Vector3 sillPos, sillSize;
                    if (isNorthSouth)
                    {
                        sillPos = new Vector3(openingCenter, wallY, sillZ);
                        sillSize = new Vector3(opening.Width, thickness, opening.BottomOffset);
                    }
                    else
                    {
                        sillPos = new Vector3(wallX, openingCenter, sillZ);
                        sillSize = new Vector3(thickness, opening.Width, opening.BottomOffset);
                    }
                    map.AddPrimitive(sillPos, sillSize, WallMaterial, true);
                }

                currentPos = openingEnd;
            }

            // Segmento final de pared DESPUÉS de la última apertura
            if (currentPos < wallEnd - 0.01f)
            {
                float segmentLength = wallEnd - currentPos;
                float segmentCenter = currentPos + segmentLength / 2;

                Vector3 segPos, segSize;
                if (isNorthSouth)
                {
                    segPos = new Vector3(segmentCenter, wallY, FloorZ + wallHeight / 2);
                    segSize = new Vector3(segmentLength, thickness, wallHeight);
                }
                else
                {
                    segPos = new Vector3(wallX, segmentCenter, FloorZ + wallHeight / 2);
                    segSize = new Vector3(thickness, segmentLength, wallHeight);
                }
                map.AddPrimitive(segPos, segSize, WallMaterial, true);
            }
        }

        #endregion

        #region Utilidades

        /// <summary>
        /// Verifica si un punto está dentro de la habitación
        /// </summary>
        public bool Contains(Vector3 point)
        {
            return point.X >= Left && point.X <= Right &&
                   point.Y >= Bottom && point.Y <= Top &&
                   point.Z >= FloorZ && point.Z <= CeilingZ;
        }

        /// <summary>
        /// Obtiene el centro de una pared
        /// </summary>
        public Vector3 GetWallCenter(WallSide side)
        {
            switch (side)
            {
                case WallSide.North:
                    return new Vector3(center.X, Top, FloorZ + size.Z / 2);
                case WallSide.South:
                    return new Vector3(center.X, Bottom, FloorZ + size.Z / 2);
                case WallSide.East:
                    return new Vector3(Right, center.Y, FloorZ + size.Z / 2);
                case WallSide.West:
                    return new Vector3(Left, center.Y, FloorZ + size.Z / 2);
                default:
                    return center;
            }
        }

        #endregion
    }

    /// <summary>
    /// Apertura en una pared de habitación
    /// </summary>
    public class RoomOpening
    {
        public string Id { get; set; }
        public WallSide Side { get; set; }
        public float PositionAlongWall { get; set; }  // 0-1, donde 0.5 = centro
        public float Width { get; set; }
        public float Height { get; set; }
        public float BottomOffset { get; set; }       // Distancia desde el suelo

        /// <summary>
        /// Indica si esta apertura es para una puerta (bottomOffset = 0, height típica de puerta)
        /// </summary>
        public bool IsDoorOpening => BottomOffset < 0.01f && Height >= 2f && Height <= 2.5f;
    }
}
