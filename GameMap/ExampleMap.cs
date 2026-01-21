using System;
using Microsoft.Xna.Framework;
using vaudio;

namespace Game3.GameMap
{
    /// <summary>
    /// Ejemplo de cómo crear un mapa usando el nuevo sistema GameMap.
    /// Este mapa tiene 4 habitaciones conectadas con puertas, escaleras y balcón.
    /// </summary>
    public static class ExampleMap
    {
        /// <summary>
        /// Crea un mapa de ejemplo con varias habitaciones.
        /// No llama a Build() - el llamador debe hacerlo después de inicializar vaudio.
        /// </summary>
        public static GameMap Create(Audio.AudioManager audioManager = null)
        {
            var map = new GameMap(audioManager);

            // Configuración global del mapa
            map.DefaultWallHeight = 5f;
            map.DefaultWallThickness = 0.3f;
            map.DefaultWallMaterial = MaterialType.Brick;
            map.DefaultFloorMaterial = MaterialType.Concrete;
            map.DefaultCeilingMaterial = MaterialType.Concrete;

            // ============================================
            // HABITACIÓN 1: PEQUEÑA (10x10m) - Spawn del jugador
            // ============================================
            var room1 = map.CreateRoom("Habitación Pequeña",
                center: new Vector3(5f, 5f, 0f),
                size: new Vector3(10f, 10f, 5f))
                .AddDoorOpening("door1", WallSide.North, 0.5f, 2f);

            // Fuente de sonido en room1
            map.AddSoundSource(new Vector3(5f, 5f, 1.5f), "sounds/rooms/window.mp3", true, 0.5f);

            // ============================================
            // HABITACIÓN 2: MEDIANA (20x15m)
            // ============================================
            var room2 = map.CreateRoom("Habitación Mediana",
                center: new Vector3(5f, 17.5f, 0f),
                size: new Vector3(20f, 15f, 5f))
                .AddDoorOpening("door1_south", WallSide.South, 0.5f, 2f)  // Conecta con room1
                .AddDoorOpening("door2", WallSide.North, 0.5f, 2f);        // Conecta con room3

            map.AddSoundSource(new Vector3(5f, 17f, 1.5f), "sounds/rooms/rain house.mp3", true, 0.5f);

            // ============================================
            // HABITACIÓN 3: GRANDE (30x20m)
            // ============================================
            var room3 = map.CreateRoom("Habitación Grande",
                center: new Vector3(5f, 35f, 0f),
                size: new Vector3(30f, 20f, 5f))
                .AddDoorOpening("door2_south", WallSide.South, 0.5f, 2f)  // Conecta con room2
                .AddDoorOpening("door3", WallSide.North, 0.5f, 2f);        // Conecta con hall

            map.AddSoundSource(new Vector3(5f, 35f, 1.5f), "sounds/rooms/window.mp3", true, 0.5f);

            // ============================================
            // HABITACIÓN 4: HALL GRANDE (50x30m) con escaleras
            // ============================================
            var hall = map.CreateRoom("Gran Salón",
                center: new Vector3(5f, 60f, 0f),
                size: new Vector3(50f, 30f, 5f))
                .AddDoorOpening("door3_south", WallSide.South, 0.5f, 2f);  // Conecta con room3

            map.AddSoundSource(new Vector3(10f, 60f, 1.5f), "sounds/rooms/rain house.mp3", true, 0.5f);

            // ============================================
            // PUERTAS
            // ============================================
            map.CreateDoor(room1, "door1", "sounds/doors/door1");
            map.CreateDoor(room2, "door2", "sounds/doors/door1");
            map.CreateDoor(room3, "door3", "sounds/doors/door1");

            // ============================================
            // ESCALERAS hacia el balcón
            // ============================================
            float stairX = -10f;
            float stairY = 47f;
            float stairLength = 14f;
            float stairWidth = 4f;
            float upperFloorHeight = 4f;

            var stair = map.CreateStair(
                startPosition: new Vector3(stairX, stairY, 0f),
                length: stairLength,
                width: stairWidth,
                heightChange: upperFloorHeight,
                direction: StairDirection.North
            ).WithSideWalls(1.5f);

            // Sonido para localizar las escaleras
            map.AddSoundSource(new Vector3(stairX, stairY, 1.0f), "sounds/UI/menumove.mp3", true, 0.3f);

            // ============================================
            // BALCÓN SUPERIOR (habitación a Z=4)
            // ============================================
            float balconyStartY = stairY + stairLength;  // Donde terminan las escaleras
            float balconyWidth = 14f;   // X: -20 a -6
            float balconyLength = 75f - balconyStartY;  // Y: desde escaleras hasta el norte
            float balconyHeight = 3f;   // Altura de la habitación del balcón

            // Crear balcón como habitación cerrada para que funcione el reverb
            // La apertura debe estar centrada exactamente donde termina la escalera
            // Stair center X = -10, room center X = -13, room width = 14
            // Position = 0.5 + (stairX - roomCenterX) / roomWidth = 0.5 + 3/14 ≈ 0.714
            // Width = stairWidth + 1 para margen con radio de colisión del jugador
            float openingPosition = 0.5f + (stairX - (-13f)) / balconyWidth;  // ≈ 0.714
            float openingWidth = stairWidth + 1f;  // 5 units (4 + margin)

            var balcony = map.CreateRoom("Balcón Superior",
                center: new Vector3(-13f, (balconyStartY + 75f) / 2f, upperFloorHeight),
                size: new Vector3(balconyWidth, balconyLength, balconyHeight))
                .SetFloorMaterial(MaterialType.WoodIndoor)
                .SetWallMaterial(MaterialType.Brick)
                .AddOpening("stair_entrance", WallSide.South, openingPosition, openingWidth, balconyHeight, 0f);  // Entrada desde escaleras

            // Añadir plataforma para que el jugador pueda caminar
            // Extender ligeramente hacia el sur para asegurar transición suave con la escalera
            map.Platforms.Add(new Platform(-20f, balconyStartY - 0.5f, -6f, 75f, upperFloorHeight));

            // Barandilla del balcón (borde este) - decorativa, el este tiene pared de la habitación
            float railHeight = 1.2f;
            map.AddPrimitive(
                new Vector3(-6.5f, (balconyStartY + 75f) / 2f, upperFloorHeight + railHeight / 2),
                new Vector3(0.3f, balconyLength - 1f, railHeight),
                MaterialType.Metal,
                false  // Sin colisión, es decorativa
            );

            // Sonido en el balcón (altura Z=4+1.5=5.5)
            map.AddSoundSource(new Vector3(-15f, 65f, upperFloorHeight + 1.5f), "sounds/rooms/window.mp3", true, 0.5f);

            // Fuente de sonido de prueba para verificar HRTF con altura
            // Esta fuente está a Z=0.5 (cerca del suelo)
            // Cuando el jugador sube al balcón (Z=4+1.7=5.7), debería escucharse "desde abajo"
            map.AddSoundSource(new Vector3(5f, 60f, 0.5f), "sounds/UI/menumove.mp3", true, 0.4f);

            // ============================================
            // SPAWN POINT (posición inicial del jugador)
            // ============================================
            map.SetSpawnPoint(new Vector3(5f, 5f, 0f), angle: 0f);

            Program.Log("ExampleMap elements created (call Build() after initializing vaudio)");
            Program.Log($"  Rooms: {map.Rooms.Count}");
            Program.Log($"  Doors: {map.Doors.Count}");
            Program.Log($"  Stairs: {map.Stairs.Count}");

            // NOTA: No se llama a Build() aquí.
            // El llamador debe:
            // 1. Calcular bounds con map.CalculateBounds()
            // 2. Inicializar vaudio con audioManager.InitializeVaudioWithBounds()
            // 3. Llamar a map.Build()

            return map;
        }
    }
}
