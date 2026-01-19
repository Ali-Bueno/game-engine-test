using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Game3.Audio;
using vaudio;

namespace Game3.GameMap
{
    /// <summary>
    /// Sistema de mapas para audiojuegos con vaudio raytracing.
    /// Gestiona habitaciones, pasillos, puertas, escaleras y objetos.
    /// </summary>
    public class GameMap
    {
        private AudioManager audioManager;
        private GamePlayer player;

        // Colecciones principales
        private List<GameRoom> rooms = new List<GameRoom>();
        private List<Corridor> corridors = new List<Corridor>();
        private List<GameDoor> doors = new List<GameDoor>();
        private List<GameStair> stairs = new List<GameStair>();
        private List<BoxCollider> colliders = new List<BoxCollider>();
        private List<Platform> platforms = new List<Platform>();
        private List<SoundSource> soundSources = new List<SoundSource>();

        // Primitivas pendientes (para añadir durante Build cuando vaudio esté listo)
        private List<PendingPrimitive> pendingPrimitives = new List<PendingPrimitive>();

        private struct PendingPrimitive
        {
            public Vector3 Position;
            public Vector3 Size;
            public MaterialType Material;
            public bool HasCollision;
        }

        // Configuración del mundo
        private Vector3 worldMin;
        private Vector3 worldMax;
        private float defaultWallHeight = 5f;
        private float defaultWallThickness = 0.3f;
        private MaterialType defaultWallMaterial = MaterialType.Brick;
        private MaterialType defaultFloorMaterial = MaterialType.Concrete;
        private MaterialType defaultCeilingMaterial = MaterialType.Concrete;

        // Propiedades públicas
        public AudioManager AudioManager => audioManager;
        public GamePlayer Player => player;
        public List<GameRoom> Rooms => rooms;
        public List<Corridor> Corridors => corridors;
        public List<GameDoor> Doors => doors;
        public List<GameStair> Stairs => stairs;
        public List<BoxCollider> Colliders => colliders;
        public List<Platform> Platforms => platforms;

        public float DefaultWallHeight { get => defaultWallHeight; set => defaultWallHeight = value; }
        public float DefaultWallThickness { get => defaultWallThickness; set => defaultWallThickness = value; }
        public MaterialType DefaultWallMaterial { get => defaultWallMaterial; set => defaultWallMaterial = value; }
        public MaterialType DefaultFloorMaterial { get => defaultFloorMaterial; set => defaultFloorMaterial = value; }
        public MaterialType DefaultCeilingMaterial { get => defaultCeilingMaterial; set => defaultCeilingMaterial = value; }

        public GameMap(AudioManager audioManager = null)
        {
            this.audioManager = audioManager;
        }

        /// <summary>
        /// Asigna el AudioManager (necesario antes de Build si no se pasó en constructor)
        /// </summary>
        public void SetAudioManager(AudioManager manager)
        {
            this.audioManager = manager;
        }

        /// <summary>
        /// Calcula los bounds del mapa basándose en las habitaciones, escaleras y plataformas.
        /// Llamar después de crear todos los elementos pero antes de Build().
        /// </summary>
        public (Vector3 min, Vector3 max) CalculateBounds()
        {
            Vector3 min = new Vector3(float.MaxValue);
            Vector3 max = new Vector3(float.MinValue);

            // Incluir habitaciones
            foreach (var room in rooms)
            {
                min = Vector3.Min(min, room.Min);
                max = Vector3.Max(max, room.Max);
            }

            // Incluir escaleras
            foreach (var stair in stairs)
            {
                min = Vector3.Min(min, stair.Min);
                max = Vector3.Max(max, stair.Max);
            }

            // Incluir plataformas
            foreach (var platform in platforms)
            {
                min = Vector3.Min(min, new Vector3(platform.Min.X, platform.Min.Y, 0));
                max = Vector3.Max(max, new Vector3(platform.Max.X, platform.Max.Y, platform.Height + 5f));
            }

            // Añadir margen de seguridad
            min -= new Vector3(5f);
            max += new Vector3(5f);

            return (min, max);
        }

        #region Creación de Elementos

        /// <summary>
        /// Crea una habitación rectangular
        /// </summary>
        public GameRoom CreateRoom(string name, Vector3 center, Vector3 size)
        {
            var room = new GameRoom(this, name, center, size);
            rooms.Add(room);
            return room;
        }

        /// <summary>
        /// Crea una habitación rectangular con posición especificada por esquina
        /// </summary>
        public GameRoom CreateRoomFromCorner(string name, Vector3 corner, Vector3 size)
        {
            Vector3 center = corner + size / 2f;
            center.Z = corner.Z; // La Z es la altura del suelo, no del centro
            return CreateRoom(name, center, size);
        }

        /// <summary>
        /// Crea un pasillo que conecta dos habitaciones
        /// </summary>
        public Corridor CreateCorridor(GameRoom from, WallSide fromSide, GameRoom to, WallSide toSide, float width)
        {
            var corridor = new Corridor(this, from, fromSide, to, toSide, width);
            corridors.Add(corridor);
            return corridor;
        }

        /// <summary>
        /// Crea una puerta en una apertura de habitación
        /// </summary>
        public GameDoor CreateDoor(GameRoom room, string openingId, string soundFolder = "sounds/doors/door1")
        {
            var opening = room.GetOpening(openingId);
            if (opening == null)
            {
                Program.Log($"GameMap: Opening '{openingId}' not found in room '{room.Name}'");
                return null;
            }

            var door = new GameDoor(this, room, opening, soundFolder);
            doors.Add(door);
            return door;
        }

        /// <summary>
        /// Crea una escalera
        /// </summary>
        public GameStair CreateStair(Vector3 startPosition, float length, float width, float heightChange, StairDirection direction)
        {
            var stair = new GameStair(this, startPosition, length, width, heightChange, direction);
            stairs.Add(stair);
            return stair;
        }

        /// <summary>
        /// Añade una fuente de sonido ambiental
        /// </summary>
        public SoundSource AddSoundSource(Vector3 position, string soundPath, bool looping = true, float volume = 1f)
        {
            var source = new SoundSource(this, position, soundPath, looping, volume);
            soundSources.Add(source);
            return source;
        }

        /// <summary>
        /// Crea el jugador en una posición inicial
        /// </summary>
        public GamePlayer CreatePlayer(Vector3 position, float angle = 0f)
        {
            player = new GamePlayer(this);
            player.Position = position;
            player.Angle = angle;
            return player;
        }

        #endregion

        #region Construcción

        /// <summary>
        /// Construye todo el mapa (llamar después de crear todos los elementos)
        /// </summary>
        public void Build()
        {
            Program.Log("=== Building GameMap ===");

            // 0. Añadir primitivas pendientes (creadas antes de que vaudio estuviera listo)
            if (pendingPrimitives.Count > 0)
            {
                Program.Log($"Adding {pendingPrimitives.Count} pending primitives...");
                foreach (var pending in pendingPrimitives)
                {
                    var primitive = new PrismPrimitive()
                    {
                        material = pending.Material,
                        size = new Vector3F(pending.Size.X, pending.Size.Y, pending.Size.Z),
                        transform = Matrix4F.CreateTranslation(pending.Position.X, pending.Position.Y, pending.Position.Z)
                    };
                    audioManager.AddPrimitive(primitive);

                    if (pending.HasCollision)
                    {
                        colliders.Add(new BoxCollider(pending.Position, pending.Size));
                    }
                }
                pendingPrimitives.Clear();
            }

            // 1. Construir habitaciones (paredes, suelo, techo)
            foreach (var room in rooms)
            {
                room.Build();
            }

            // 2. Construir pasillos
            foreach (var corridor in corridors)
            {
                corridor.Build();
            }

            // 3. Construir puertas
            foreach (var door in doors)
            {
                door.Build();
            }

            // 4. Construir escaleras
            foreach (var stair in stairs)
            {
                stair.Build();
            }

            // 5. Iniciar fuentes de sonido
            foreach (var source in soundSources)
            {
                source.Build();
            }

            // Calcular límites del mundo
            CalculateWorldBounds();

            Program.Log($"GameMap built: {rooms.Count} rooms, {corridors.Count} corridors, {doors.Count} doors, {stairs.Count} stairs");
            Program.Log($"World bounds: ({worldMin.X}, {worldMin.Y}, {worldMin.Z}) to ({worldMax.X}, {worldMax.Y}, {worldMax.Z})");
        }

        private void CalculateWorldBounds()
        {
            worldMin = new Vector3(float.MaxValue);
            worldMax = new Vector3(float.MinValue);

            foreach (var room in rooms)
            {
                worldMin = Vector3.Min(worldMin, room.Min);
                worldMax = Vector3.Max(worldMax, room.Max);
            }

            // Añadir margen
            worldMin -= new Vector3(5f);
            worldMax += new Vector3(5f);
        }

        #endregion

        #region Primitivas (helpers internos)

        /// <summary>
        /// Añade una primitiva de prisma al raytracing.
        /// Si vaudio no está inicializado, la guarda para añadirla durante Build().
        /// </summary>
        internal void AddPrimitive(Vector3 position, Vector3 size, MaterialType material, bool hasCollision = true)
        {
            // Si vaudio no está listo, guardar para después
            if (audioManager == null || !audioManager.IsVaudioInitialized)
            {
                pendingPrimitives.Add(new PendingPrimitive
                {
                    Position = position,
                    Size = size,
                    Material = material,
                    HasCollision = hasCollision
                });
                return;
            }

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

        /// <summary>
        /// Añade una primitiva y devuelve la referencia (para poder quitarla después)
        /// </summary>
        internal PrismPrimitive AddPrimitiveTracked(Vector3 position, Vector3 size, MaterialType material)
        {
            var primitive = new PrismPrimitive()
            {
                material = material,
                size = new Vector3F(size.X, size.Y, size.Z),
                transform = Matrix4F.CreateTranslation(position.X, position.Y, position.Z)
            };
            audioManager.AddPrimitive(primitive);
            return primitive;
        }

        /// <summary>
        /// Quita una primitiva del raytracing
        /// </summary>
        internal void RemovePrimitive(Primitive primitive)
        {
            audioManager.RemovePrimitive(primitive);
        }

        /// <summary>
        /// Añade un collider
        /// </summary>
        internal BoxCollider AddCollider(Vector3 position, Vector3 size)
        {
            var collider = new BoxCollider(position, size);
            colliders.Add(collider);
            return collider;
        }

        /// <summary>
        /// Quita un collider
        /// </summary>
        internal void RemoveCollider(BoxCollider collider)
        {
            colliders.Remove(collider);
        }

        #endregion

        #region Update

        private int listenerLogCounter = 0;

        public void Update(KeyboardState keystate, GameTime gameTime)
        {
            // Actualizar jugador
            player?.Update(keystate, gameTime);

            // Actualizar listener de audio
            if (player != null)
            {
                float yaw = player.Angle * MathF.PI / 180f;
                float listenerZ = player.Position.Z + 1.7f;  // Altura del oído

                // Log cada 60 frames (~1 segundo)
                listenerLogCounter++;
                if (listenerLogCounter >= 60)
                {
                    listenerLogCounter = 0;
                    Program.Log($"Listener: X={player.Position.X:F1}, Y={player.Position.Y:F1}, Z={listenerZ:F1} (playerZ={player.Position.Z:F1})");
                }

                audioManager.UpdateListener(
                    player.Position.X,
                    player.Position.Y,
                    listenerZ,
                    yaw
                );
            }

            // Actualizar puertas
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            foreach (var door in doors)
            {
                door.Update(deltaTime);
            }

            // Actualizar fuentes de sonido
            foreach (var source in soundSources)
            {
                source.Update(deltaTime);
            }
        }

        #endregion

        #region Queries

        /// <summary>
        /// Encuentra la habitación en la que está un punto
        /// </summary>
        public GameRoom GetRoomAt(Vector3 position)
        {
            foreach (var room in rooms)
            {
                if (room.Contains(position))
                    return room;
            }
            return null;
        }

        /// <summary>
        /// Obtiene la altura del suelo en una posición (considerando escaleras y plataformas)
        /// </summary>
        public float GetFloorHeight(Vector3 position)
        {
            // Primero verificar escaleras
            foreach (var stair in stairs)
            {
                float height = stair.GetHeightAtPosition(position);
                if (height >= 0)
                    return height;
            }

            // Luego verificar plataformas
            foreach (var platform in platforms)
            {
                if (position.X >= platform.Min.X && position.X <= platform.Max.X &&
                    position.Y >= platform.Min.Y && position.Y <= platform.Max.Y)
                {
                    return platform.Height;
                }
            }

            // Altura base
            return 0f;
        }

        /// <summary>
        /// Encuentra la puerta más cercana al jugador
        /// </summary>
        public GameDoor GetNearestDoor(Vector3 position, float maxDistance = 2f)
        {
            GameDoor nearest = null;
            float nearestDist = maxDistance;

            foreach (var door in doors)
            {
                float dist = Vector3.Distance(position, door.Position);
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearest = door;
                }
            }

            return nearest;
        }

        #endregion
    }

    /// <summary>
    /// Fuente de sonido ambiental
    /// </summary>
    public class SoundSource
    {
        private GameMap map;
        private Vector3 position;
        private string soundPath;
        private bool looping;
        private float volume;
        private AudioSource audioSource;

        public Vector3 Position => position;
        public AudioSource AudioSource => audioSource;

        public SoundSource(GameMap map, Vector3 position, string soundPath, bool looping, float volume)
        {
            this.map = map;
            this.position = position;
            this.soundPath = soundPath;
            this.looping = looping;
            this.volume = volume;
        }

        public void Build()
        {
            audioSource = map.AudioManager.Play3D(soundPath, position.X, position.Y, position.Z, looping, volume);
            if (audioSource != null)
            {
                Program.Log($"SoundSource: {soundPath} at ({position.X:F1}, {position.Y:F1}, {position.Z:F1})");
            }
        }

        public void Update(float deltaTime) { }

        public void SetVolume(float vol)
        {
            volume = vol;
            audioSource?.SetVolume(vol);
        }

        public void Stop()
        {
            audioSource?.Stop();
        }
    }
}
