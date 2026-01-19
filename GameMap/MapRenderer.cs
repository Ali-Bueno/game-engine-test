using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Arch.Core;
using Game3.ECS;
using Game3.ECS.Components;

namespace Game3.GameMap
{
    /// <summary>
    /// Renderizador 3D mejorado para visualizar el mapa.
    /// Incluye iluminación, figuras humanoides y geometría detallada.
    /// </summary>
    public class MapRenderer
    {
        private GraphicsDevice graphicsDevice;
        private BasicEffect effect;
        private GameMap map;

        // Optional ECS World for direct queries
        private World ecsWorld;
        private QueryDescription playerQuery;

        // Geometría estática del mapa
        private VertexBuffer mapVertexBuffer;
        private IndexBuffer mapIndexBuffer;
        private int mapTriangleCount;

        // Geometría dinámica (puertas)
        private List<DoorGeometry> doorGeometries = new List<DoorGeometry>();

        // Cámara
        private float cameraLag = 0.08f;
        private Vector3 smoothCameraPosition;
        private bool cameraInitialized = false;
        private int drawLogCounter = 0;

        // Colores mejorados
        private static readonly Color FloorColor = new Color(80, 70, 60);        // Marrón suelo
        private static readonly Color CeilingColor = new Color(200, 195, 190);   // Blanco techo
        private static readonly Color WallColor = new Color(180, 160, 140);      // Beige paredes
        private static readonly Color WallColorDark = new Color(140, 125, 110);  // Sombra paredes
        private static readonly Color DoorFrameColor = new Color(101, 67, 33);   // Marrón marco
        private static readonly Color DoorPanelColor = new Color(139, 90, 43);   // Madera puerta
        private static readonly Color StairColor = new Color(160, 140, 100);     // Madera escalera
        private static readonly Color StairStepColor = new Color(140, 120, 80);  // Peldaño
        private static readonly Color RailingColor = new Color(60, 60, 65);      // Metal barandilla

        // Colores del jugador (humanoid)
        private static readonly Color SkinColor = new Color(255, 220, 185);
        private static readonly Color ShirtColor = new Color(70, 130, 180);      // Azul acero
        private static readonly Color PantsColor = new Color(50, 50, 60);        // Gris oscuro
        private static readonly Color ShoeColor = new Color(40, 30, 20);         // Marrón zapatos
        private static readonly Color HairColor = new Color(60, 40, 20);         // Marrón pelo

        private bool geometryBuilt = false;

        private class DoorGeometry
        {
            public GameDoor Door;
            public Vector3 Position;
            public Vector3 Size;
            public bool IsNorthSouth;  // Orientación
        }

        public MapRenderer(GraphicsDevice graphicsDevice, GameMap map)
        {
            this.graphicsDevice = graphicsDevice;
            this.map = map;

            effect = new BasicEffect(graphicsDevice)
            {
                VertexColorEnabled = true,
                LightingEnabled = false,  // Usamos colores pre-calculados para simular luz
                PreferPerPixelLighting = false
            };

            smoothCameraPosition = Vector3.Zero;
        }

        /// <summary>
        /// Sets the ECS World for direct player queries.
        /// When set, the renderer will query ECS for player position/rotation.
        /// </summary>
        public void SetEcsWorld(World world)
        {
            this.ecsWorld = world;
            this.playerQuery = new QueryDescription()
                .WithAll<PlayerControlled, Position, Rotation>();
            Program.Log("MapRenderer: ECS World set for direct queries");
        }

        /// <summary>
        /// Gets the player position and angle from ECS.
        /// </summary>
        private (Vector3 position, float angle) GetPlayerPositionAndAngle()
        {
            if (ecsWorld != null)
            {
                Vector3 pos = Vector3.Zero;
                float angle = 0f;

                ecsWorld.Query(in playerQuery, (ref Position p, ref Rotation r) =>
                {
                    pos = new Vector3(p.X, p.Y, p.Z);
                    angle = r.Yaw;
                });

                return (pos, angle);
            }

            return (Vector3.Zero, 0f);
        }

        public void BuildGeometry()
        {
            var vertices = new List<VertexPositionColor>();
            var indices = new List<int>();

            // Renderizar habitaciones
            foreach (var room in map.Rooms)
            {
                AddRoomGeometry(room, vertices, indices);
            }

            // Renderizar escaleras con peldaños
            foreach (var stair in map.Stairs)
            {
                AddStairGeometry(stair, vertices, indices);
            }

            // Preparar geometría de puertas (se actualiza dinámicamente)
            foreach (var door in map.Doors)
            {
                var opening = GetDoorOpening(door);
                if (opening != null)
                {
                    doorGeometries.Add(new DoorGeometry
                    {
                        Door = door,
                        Position = door.Position,
                        Size = new Vector3(opening.Width, 0.1f, opening.Height),
                        IsNorthSouth = opening.Side == WallSide.North || opening.Side == WallSide.South
                    });
                }
            }

            // Crear buffers del mapa
            if (vertices.Count > 0)
            {
                // Convertir índices a short si es posible, o usar int
                mapVertexBuffer = new VertexBuffer(graphicsDevice, typeof(VertexPositionColor), vertices.Count, BufferUsage.WriteOnly);
                mapVertexBuffer.SetData(vertices.ToArray());

                var shortIndices = new short[indices.Count];
                for (int i = 0; i < indices.Count; i++)
                    shortIndices[i] = (short)indices[i];

                mapIndexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.SixteenBits, indices.Count, BufferUsage.WriteOnly);
                mapIndexBuffer.SetData(shortIndices);
                mapTriangleCount = indices.Count / 3;
            }

            geometryBuilt = true;
            Program.Log($"MapRenderer: Built geometry with {vertices.Count} vertices, {mapTriangleCount} triangles");
        }

        private RoomOpening GetDoorOpening(GameDoor door)
        {
            // Buscar el opening asociado a esta puerta
            foreach (var room in map.Rooms)
            {
                foreach (var openingId in new[] { "door1", "door2", "door3", "door1_south", "door2_south", "door3_south" })
                {
                    var opening = room.GetOpening(openingId);
                    if (opening != null)
                    {
                        var worldPos = room.GetOpeningWorldPosition(openingId);
                        if (Vector3.Distance(worldPos, door.Position) < 1f)
                            return opening;
                    }
                }
            }
            return null;
        }

        private void AddRoomGeometry(GameRoom room, List<VertexPositionColor> vertices, List<int> indices)
        {
            float thickness = 0.3f;

            // Suelo con patrón de tablones
            AddFloorWithPattern(room, vertices, indices);

            // Sin techo para que la cámara pueda ver desde cualquier ángulo

            // Paredes a altura completa
            float wallHeight = room.Size.Z;
            float halfHeight = wallHeight / 2;

            // Norte (más iluminada)
            AddBox(vertices, indices,
                new Vector3(room.Center.X, room.Top, room.FloorZ + halfHeight),
                new Vector3(room.Size.X, thickness, wallHeight),
                WallColor, WallColorDark);

            // Sur (sombra)
            AddBox(vertices, indices,
                new Vector3(room.Center.X, room.Bottom, room.FloorZ + halfHeight),
                new Vector3(room.Size.X, thickness, wallHeight),
                WallColorDark, WallColor);

            // Este
            AddBox(vertices, indices,
                new Vector3(room.Right, room.Center.Y, room.FloorZ + halfHeight),
                new Vector3(thickness, room.Size.Y, wallHeight),
                WallColor, WallColorDark);

            // Oeste
            AddBox(vertices, indices,
                new Vector3(room.Left, room.Center.Y, room.FloorZ + halfHeight),
                new Vector3(thickness, room.Size.Y, wallHeight),
                WallColorDark, WallColor);
        }

        private void AddFloorWithPattern(GameRoom room, List<VertexPositionColor> vertices, List<int> indices)
        {
            float thickness = 0.3f;

            // Suelo base
            Color baseColor = FloorColor;
            Color darkColor = new Color(
                (int)(FloorColor.R * 0.85f),
                (int)(FloorColor.G * 0.85f),
                (int)(FloorColor.B * 0.85f));

            // Crear un patrón simple de cuadrícula
            float tileSize = 2f;
            int tilesX = (int)(room.Size.X / tileSize);
            int tilesY = (int)(room.Size.Y / tileSize);

            for (int tx = 0; tx < tilesX; tx++)
            {
                for (int ty = 0; ty < tilesY; ty++)
                {
                    float x = room.Left + tx * tileSize + tileSize / 2;
                    float y = room.Bottom + ty * tileSize + tileSize / 2;

                    Color tileColor = ((tx + ty) % 2 == 0) ? baseColor : darkColor;

                    AddBox(vertices, indices,
                        new Vector3(x, y, room.FloorZ - thickness / 2),
                        new Vector3(tileSize - 0.05f, tileSize - 0.05f, thickness),
                        tileColor, tileColor);
                }
            }
        }

        private void AddStairGeometry(GameStair stair, List<VertexPositionColor> vertices, List<int> indices)
        {
            // Calcular número de peldaños
            int stepCount = (int)(stair.HeightChange / 0.2f);  // ~20cm por peldaño
            if (stepCount < 3) stepCount = 3;

            float stepHeight = stair.HeightChange / stepCount;
            float stepDepth = stair.Length / stepCount;

            Vector3 currentPos = stair.StartPosition;

            for (int i = 0; i < stepCount; i++)
            {
                float stepZ = currentPos.Z + stepHeight * (i + 0.5f);
                Vector3 stepPos;
                Vector3 stepSize;

                switch (stair.Direction)
                {
                    case StairDirection.North:
                        stepPos = new Vector3(currentPos.X, currentPos.Y + stepDepth * (i + 0.5f), stepZ);
                        stepSize = new Vector3(stair.Width, stepDepth - 0.02f, stepHeight);
                        break;
                    case StairDirection.South:
                        stepPos = new Vector3(currentPos.X, currentPos.Y - stepDepth * (i + 0.5f), stepZ);
                        stepSize = new Vector3(stair.Width, stepDepth - 0.02f, stepHeight);
                        break;
                    case StairDirection.East:
                        stepPos = new Vector3(currentPos.X + stepDepth * (i + 0.5f), currentPos.Y, stepZ);
                        stepSize = new Vector3(stepDepth - 0.02f, stair.Width, stepHeight);
                        break;
                    case StairDirection.West:
                        stepPos = new Vector3(currentPos.X - stepDepth * (i + 0.5f), currentPos.Y, stepZ);
                        stepSize = new Vector3(stepDepth - 0.02f, stair.Width, stepHeight);
                        break;
                    default:
                        continue;
                }

                // Alternar colores de peldaños
                Color stepColor = (i % 2 == 0) ? StairColor : StairStepColor;
                AddBox(vertices, indices, stepPos, stepSize, stepColor, stepColor);
            }

            // Barandillas laterales
            AddStairRailings(stair, vertices, indices);
        }

        private void AddStairRailings(GameStair stair, List<VertexPositionColor> vertices, List<int> indices)
        {
            float railHeight = 1.0f;
            float railThickness = 0.05f;

            Vector3 railSize;
            Vector3 leftRailPos, rightRailPos;
            float midZ = stair.StartPosition.Z + stair.HeightChange / 2 + railHeight / 2;

            switch (stair.Direction)
            {
                case StairDirection.North:
                    railSize = new Vector3(railThickness, stair.Length, railHeight);
                    leftRailPos = new Vector3(stair.StartPosition.X - stair.Width / 2, stair.StartPosition.Y + stair.Length / 2, midZ);
                    rightRailPos = new Vector3(stair.StartPosition.X + stair.Width / 2, stair.StartPosition.Y + stair.Length / 2, midZ);
                    break;
                case StairDirection.South:
                    railSize = new Vector3(railThickness, stair.Length, railHeight);
                    leftRailPos = new Vector3(stair.StartPosition.X - stair.Width / 2, stair.StartPosition.Y - stair.Length / 2, midZ);
                    rightRailPos = new Vector3(stair.StartPosition.X + stair.Width / 2, stair.StartPosition.Y - stair.Length / 2, midZ);
                    break;
                case StairDirection.East:
                    railSize = new Vector3(stair.Length, railThickness, railHeight);
                    leftRailPos = new Vector3(stair.StartPosition.X + stair.Length / 2, stair.StartPosition.Y - stair.Width / 2, midZ);
                    rightRailPos = new Vector3(stair.StartPosition.X + stair.Length / 2, stair.StartPosition.Y + stair.Width / 2, midZ);
                    break;
                case StairDirection.West:
                    railSize = new Vector3(stair.Length, railThickness, railHeight);
                    leftRailPos = new Vector3(stair.StartPosition.X - stair.Length / 2, stair.StartPosition.Y - stair.Width / 2, midZ);
                    rightRailPos = new Vector3(stair.StartPosition.X - stair.Length / 2, stair.StartPosition.Y + stair.Width / 2, midZ);
                    break;
                default:
                    return;
            }

            AddBox(vertices, indices, leftRailPos, railSize, RailingColor, RailingColor);
            AddBox(vertices, indices, rightRailPos, railSize, RailingColor, RailingColor);
        }

        private void AddBox(List<VertexPositionColor> vertices, List<int> indices,
            Vector3 center, Vector3 size, Color topColor, Color sideColor)
        {
            int baseIndex = vertices.Count;

            Vector3 half = size / 2;
            Vector3 min = center - half;
            Vector3 max = center + half;

            // Colores por cara para simular iluminación
            Color bottomColor = new Color(
                (int)(sideColor.R * 0.7f),
                (int)(sideColor.G * 0.7f),
                (int)(sideColor.B * 0.7f));

            // 8 vértices con colores diferentes según la cara
            // Bottom face vertices (0-3)
            vertices.Add(new VertexPositionColor(new Vector3(min.X, min.Y, min.Z), bottomColor));
            vertices.Add(new VertexPositionColor(new Vector3(max.X, min.Y, min.Z), bottomColor));
            vertices.Add(new VertexPositionColor(new Vector3(max.X, max.Y, min.Z), bottomColor));
            vertices.Add(new VertexPositionColor(new Vector3(min.X, max.Y, min.Z), bottomColor));

            // Top face vertices (4-7)
            vertices.Add(new VertexPositionColor(new Vector3(min.X, min.Y, max.Z), topColor));
            vertices.Add(new VertexPositionColor(new Vector3(max.X, min.Y, max.Z), topColor));
            vertices.Add(new VertexPositionColor(new Vector3(max.X, max.Y, max.Z), topColor));
            vertices.Add(new VertexPositionColor(new Vector3(min.X, max.Y, max.Z), topColor));

            // Índices para 12 triángulos
            // Bottom
            indices.Add(baseIndex + 0); indices.Add(baseIndex + 2); indices.Add(baseIndex + 1);
            indices.Add(baseIndex + 0); indices.Add(baseIndex + 3); indices.Add(baseIndex + 2);
            // Top
            indices.Add(baseIndex + 4); indices.Add(baseIndex + 5); indices.Add(baseIndex + 6);
            indices.Add(baseIndex + 4); indices.Add(baseIndex + 6); indices.Add(baseIndex + 7);
            // Front (Y+)
            indices.Add(baseIndex + 3); indices.Add(baseIndex + 7); indices.Add(baseIndex + 6);
            indices.Add(baseIndex + 3); indices.Add(baseIndex + 6); indices.Add(baseIndex + 2);
            // Back (Y-)
            indices.Add(baseIndex + 0); indices.Add(baseIndex + 1); indices.Add(baseIndex + 5);
            indices.Add(baseIndex + 0); indices.Add(baseIndex + 5); indices.Add(baseIndex + 4);
            // Right (X+)
            indices.Add(baseIndex + 1); indices.Add(baseIndex + 2); indices.Add(baseIndex + 6);
            indices.Add(baseIndex + 1); indices.Add(baseIndex + 6); indices.Add(baseIndex + 5);
            // Left (X-)
            indices.Add(baseIndex + 0); indices.Add(baseIndex + 4); indices.Add(baseIndex + 7);
            indices.Add(baseIndex + 0); indices.Add(baseIndex + 7); indices.Add(baseIndex + 3);
        }

        public void Draw()
        {
            if (!geometryBuilt) return;

            // Get player position from ECS
            var (playerPosition, playerAngle) = GetPlayerPositionAndAngle();
            if (ecsWorld == null) return;

            // Calcular posición de cámara (tercera persona con colisión)
            float angleRad = playerAngle * MathF.PI / 180f;
            Vector3 forward = new Vector3(MathF.Sin(angleRad), MathF.Cos(angleRad), 0);

            // Posición ideal de la cámara: detrás y arriba del jugador
            float idealDistance = 5f;
            float cameraHeight = 2f;

            Vector3 playerHead = playerPosition + new Vector3(0, 0, WorldBuilder.EyeHeight);
            Vector3 targetCameraPos = playerHead - forward * idealDistance + new Vector3(0, 0, cameraHeight);

            // Colisión de cámara: acercar si hay pared en medio
            targetCameraPos = ApplyCameraCollision(playerHead, targetCameraPos, idealDistance);

            if (!cameraInitialized)
            {
                smoothCameraPosition = targetCameraPos;
                cameraInitialized = true;
            }
            else
            {
                smoothCameraPosition = Vector3.Lerp(smoothCameraPosition, targetCameraPos, cameraLag);
            }

            // Mirar hacia la cabeza del jugador
            Vector3 lookAt = playerPosition + new Vector3(0, 0, WorldBuilder.EyeHeight * 0.8f);

            // Configurar matrices
            effect.View = Matrix.CreateLookAt(smoothCameraPosition, lookAt, Vector3.UnitZ);
            effect.Projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(70),  // FOV más amplio
                graphicsDevice.Viewport.AspectRatio,
                0.1f,
                200f
            );
            effect.World = Matrix.Identity;

            // Configurar render state
            graphicsDevice.DepthStencilState = DepthStencilState.Default;
            graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            // Log de debug
            drawLogCounter++;
            if (drawLogCounter >= 60)
            {
                drawLogCounter = 0;
                Program.Log($"MapRenderer: cam=({smoothCameraPosition.X:F1}, {smoothCameraPosition.Y:F1}, {smoothCameraPosition.Z:F1})");
            }

            // Dibujar geometría del mapa
            if (mapVertexBuffer != null && mapIndexBuffer != null)
            {
                graphicsDevice.SetVertexBuffer(mapVertexBuffer);
                graphicsDevice.Indices = mapIndexBuffer;

                foreach (var pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    graphicsDevice.DrawIndexedPrimitives(
                        PrimitiveType.TriangleList,
                        0, 0, mapTriangleCount
                    );
                }
            }

            // Dibujar puertas (dinámico según estado)
            DrawDoors();

            // Dibujar jugador humanoid
            DrawHumanoidPlayer(playerPosition, playerAngle);
        }

        private void DrawDoors()
        {
            foreach (var dg in doorGeometries)
            {
                if (!dg.Door.IsOpen)
                {
                    // Puerta cerrada - dibujar panel
                    var doorVerts = new List<VertexPositionColor>();
                    var doorIndices = new List<int>();

                    // Dimensiones de la puerta
                    float doorHeight = dg.Size.Z;
                    float doorWidth = dg.Size.X > dg.Size.Y ? dg.Size.X : dg.Size.Y;

                    // Panel de la puerta
                    Vector3 panelSize = dg.IsNorthSouth
                        ? new Vector3(doorWidth - 0.1f, 0.08f, doorHeight - 0.1f)
                        : new Vector3(0.08f, doorWidth - 0.1f, doorHeight - 0.1f);

                    AddBox(doorVerts, doorIndices, dg.Position, panelSize, DoorPanelColor, DoorFrameColor);

                    // Pomo de la puerta
                    Vector3 knobOffset = dg.IsNorthSouth
                        ? new Vector3(doorWidth * 0.35f, 0.1f, 0)
                        : new Vector3(0.1f, doorWidth * 0.35f, 0);

                    AddBox(doorVerts, doorIndices,
                        dg.Position + knobOffset,
                        new Vector3(0.08f, 0.08f, 0.08f),
                        Color.Gold, Color.DarkGoldenrod);

                    DrawDynamicGeometry(doorVerts, doorIndices);
                }
            }
        }

        private void DrawHumanoidPlayer(Vector3 playerPosition, float playerAngle)
        {
            var verts = new List<VertexPositionColor>();
            var inds = new List<int>();

            Vector3 basePos = playerPosition;
            float angleRad = playerAngle * MathF.PI / 180f;

            // Dimensiones del cuerpo
            float bodyHeight = 0.6f;
            float bodyWidth = 0.4f;
            float bodyDepth = 0.25f;

            float headSize = 0.25f;
            float legHeight = 0.8f;
            float legWidth = 0.15f;
            float armLength = 0.5f;
            float armWidth = 0.12f;

            // Rotación del jugador
            Matrix rotation = Matrix.CreateRotationZ(angleRad);

            // Piernas
            float legY = legHeight / 2;
            Vector3 leftLegOffset = Vector3.Transform(new Vector3(-0.12f, 0, 0), rotation);
            Vector3 rightLegOffset = Vector3.Transform(new Vector3(0.12f, 0, 0), rotation);

            AddBox(verts, inds, basePos + leftLegOffset + new Vector3(0, 0, legY),
                new Vector3(legWidth, legWidth, legHeight), PantsColor, PantsColor);
            AddBox(verts, inds, basePos + rightLegOffset + new Vector3(0, 0, legY),
                new Vector3(legWidth, legWidth, legHeight), PantsColor, PantsColor);

            // Zapatos
            Vector3 shoeSize = new Vector3(legWidth + 0.02f, 0.2f, 0.1f);
            Vector3 leftShoeOffset = Vector3.Transform(new Vector3(-0.12f, 0.05f, 0), rotation);
            Vector3 rightShoeOffset = Vector3.Transform(new Vector3(0.12f, 0.05f, 0), rotation);

            AddBox(verts, inds, basePos + leftShoeOffset + new Vector3(0, 0, 0.05f), shoeSize, ShoeColor, ShoeColor);
            AddBox(verts, inds, basePos + rightShoeOffset + new Vector3(0, 0, 0.05f), shoeSize, ShoeColor, ShoeColor);

            // Torso
            float torsoZ = legHeight + bodyHeight / 2;
            AddBox(verts, inds, basePos + new Vector3(0, 0, torsoZ),
                new Vector3(bodyWidth, bodyDepth, bodyHeight), ShirtColor, ShirtColor);

            // Brazos
            float armZ = torsoZ + 0.1f;
            Vector3 leftArmOffset = Vector3.Transform(new Vector3(-bodyWidth / 2 - armWidth / 2, 0, 0), rotation);
            Vector3 rightArmOffset = Vector3.Transform(new Vector3(bodyWidth / 2 + armWidth / 2, 0, 0), rotation);

            AddBox(verts, inds, basePos + leftArmOffset + new Vector3(0, 0, armZ - armLength / 4),
                new Vector3(armWidth, armWidth, armLength), ShirtColor, SkinColor);
            AddBox(verts, inds, basePos + rightArmOffset + new Vector3(0, 0, armZ - armLength / 4),
                new Vector3(armWidth, armWidth, armLength), ShirtColor, SkinColor);

            // Manos
            Vector3 handSize = new Vector3(0.1f, 0.08f, 0.12f);
            AddBox(verts, inds, basePos + leftArmOffset + new Vector3(0, 0, armZ - armLength + 0.05f),
                handSize, SkinColor, SkinColor);
            AddBox(verts, inds, basePos + rightArmOffset + new Vector3(0, 0, armZ - armLength + 0.05f),
                handSize, SkinColor, SkinColor);

            // Cabeza
            float headZ = legHeight + bodyHeight + headSize / 2 + 0.05f;
            AddBox(verts, inds, basePos + new Vector3(0, 0, headZ),
                new Vector3(headSize, headSize * 0.9f, headSize), SkinColor, SkinColor);

            // Pelo (parte superior de la cabeza)
            AddBox(verts, inds, basePos + new Vector3(0, 0, headZ + headSize * 0.4f),
                new Vector3(headSize * 1.05f, headSize * 0.95f, headSize * 0.3f), HairColor, HairColor);

            // Indicador de dirección (flecha en el suelo)
            Vector3 arrowDir = Vector3.Transform(new Vector3(0, 1.2f, 0), rotation);
            Vector3 arrowPos = basePos + arrowDir + new Vector3(0, 0, 0.05f);
            AddBox(verts, inds, arrowPos, new Vector3(0.15f, 0.4f, 0.02f), Color.Red, Color.DarkRed);

            // Punta de la flecha
            Vector3 arrowTipDir = Vector3.Transform(new Vector3(0, 1.5f, 0), rotation);
            AddBox(verts, inds, basePos + arrowTipDir + new Vector3(0, 0, 0.05f),
                new Vector3(0.3f, 0.2f, 0.02f), Color.Red, Color.DarkRed);

            DrawDynamicGeometry(verts, inds);
        }

        private void DrawDynamicGeometry(List<VertexPositionColor> vertices, List<int> indices)
        {
            if (vertices.Count == 0) return;

            var shortIndices = new short[indices.Count];
            for (int i = 0; i < indices.Count; i++)
                shortIndices[i] = (short)indices[i];

            using (var vb = new VertexBuffer(graphicsDevice, typeof(VertexPositionColor), vertices.Count, BufferUsage.WriteOnly))
            using (var ib = new IndexBuffer(graphicsDevice, IndexElementSize.SixteenBits, indices.Count, BufferUsage.WriteOnly))
            {
                vb.SetData(vertices.ToArray());
                ib.SetData(shortIndices);

                graphicsDevice.SetVertexBuffer(vb);
                graphicsDevice.Indices = ib;

                foreach (var pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    graphicsDevice.DrawIndexedPrimitives(
                        PrimitiveType.TriangleList,
                        0, 0, indices.Count / 3
                    );
                }
            }
        }

        /// <summary>
        /// Aplica colisión a la cámara para evitar que atraviese paredes.
        /// Acerca la cámara al jugador si hay obstáculos.
        /// </summary>
        private Vector3 ApplyCameraCollision(Vector3 playerPos, Vector3 cameraPos, float maxDistance)
        {
            Vector3 direction = cameraPos - playerPos;
            float distance = direction.Length();
            if (distance < 0.1f) return cameraPos;

            direction.Normalize();

            // Probar distancias decrecientes hasta encontrar una posición válida
            float testDistance = distance;
            float minDistance = 1.5f;  // Distancia mínima de la cámara al jugador
            float step = 0.5f;

            while (testDistance > minDistance)
            {
                Vector3 testPos = playerPos + direction * testDistance;

                // Verificar si esta posición está dentro de algún collider
                bool collision = false;
                foreach (var collider in map.Colliders)
                {
                    // Verificar si el punto está dentro del collider
                    if (testPos.X > collider.Min.X && testPos.X < collider.Max.X &&
                        testPos.Y > collider.Min.Y && testPos.Y < collider.Max.Y &&
                        testPos.Z > collider.Min.Z && testPos.Z < collider.Max.Z)
                    {
                        collision = true;
                        break;
                    }
                }

                if (!collision)
                {
                    return testPos;
                }

                testDistance -= step;
            }

            // Si no encontramos posición válida, usar distancia mínima
            return playerPos + direction * minDistance;
        }

        public void RebuildGeometry()
        {
            mapVertexBuffer?.Dispose();
            mapIndexBuffer?.Dispose();
            doorGeometries.Clear();
            geometryBuilt = false;
            BuildGeometry();
        }
    }
}
