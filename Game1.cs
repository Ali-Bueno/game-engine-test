using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using DavyKager;
using Arch.Core;
using Game3.Audio;
using Game3.GameMap;
using Game3.ECS;
using Game3.ECS.Components;
using Game3.ECS.Systems;

namespace Game3
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        public GameMap.GameMap gameMap;
        public AudioManager audioManager;
        private MapRenderer mapRenderer;

        // ECS
        private World ecsWorld;
        private SharedResources sharedResources;
        private Entity playerEntity;

        // ECS Systems
        private PlayerInputSystem playerInputSystem;
        private MovementSystem movementSystem;
        private GravitySystem gravitySystem;
        private CollisionSystem collisionSystem;
        private FootstepSystem footstepSystem;
        private AudioListenerSystem audioListenerSystem;
        private DoorInteractionSystem doorInteractionSystem;
        private DoorSystem doorSystem;
        private AmbientSoundSystem ambientSoundSystem;

        public Game1()
        {
            Program.Log("Game1 constructor started");
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
            graphics.PreferMultiSampling = true;
            graphics.PreferredDepthStencilFormat = DepthFormat.Depth24;
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            Content.RootDirectory = "Content";
            Program.Log("Game1 constructor finished");
        }

        protected override void Initialize()
        {
            Program.Log("Initialize started");

            try
            {
                Program.Log("Loading Tolk...");
                Tolk.Load();
                Program.Log("Tolk loaded");
            }
            catch (System.Exception ex)
            {
                Program.Log($"Tolk error: {ex.Message}");
            }

            try
            {
                Program.Log("Creating AudioManager (without vaudio - will initialize after map bounds calculation)...");
                audioManager = new AudioManager(enableDebugWindow: false);  // Desactivar debug de vaudio para ver gráficos
                Program.Log("AudioManager created (OpenAL ready, vaudio pending)");
            }
            catch (System.Exception ex)
            {
                Program.Log($"AudioManager error: {ex}");
            }

            // Initialize ECS World
            Program.Log("Creating ECS World...");
            ecsWorld = World.Create();
            sharedResources = new SharedResources(audioManager);
            Program.Log("ECS World created");

            base.Initialize();
            Program.Log("Initialize finished");
        }

        protected override void LoadContent()
        {
            Program.Log("LoadContent started");
            spriteBatch = new SpriteBatch(GraphicsDevice);

            try
            {
                Program.Log("Creating GameMap elements...");
                gameMap = ExampleMap.Create();

                // Calcular bounds del mapa
                var (min, max) = gameMap.CalculateBounds();
                Program.Log($"Map bounds: min=({min.X:F1}, {min.Y:F1}, {min.Z:F1}), max=({max.X:F1}, {max.Y:F1}, {max.Z:F1})");

                // Inicializar vaudio con los bounds calculados
                audioManager.InitializeVaudioWithBounds(
                    new vaudio.Vector3F(min.X, min.Y, min.Z),
                    new vaudio.Vector3F(max.X, max.Y, max.Z));

                // Asignar AudioManager al mapa y construir
                gameMap.SetAudioManager(audioManager);
                gameMap.Build();
                Program.Log("GameMap built");

                // Crear renderer 3D
                mapRenderer = new MapRenderer(GraphicsDevice, gameMap);
                mapRenderer.BuildGeometry();
                mapRenderer.SetEcsWorld(ecsWorld);  // Enable direct ECS queries
                Program.Log("MapRenderer created and geometry built");

                // Setup SharedResources with ECS World
                sharedResources.EcsWorld = ecsWorld;
                sharedResources.StaticColliders.AddRange(gameMap.Colliders);
                sharedResources.Rooms.AddRange(gameMap.Rooms);
                Program.Log($"SharedResources synced: {sharedResources.StaticColliders.Count} colliders, {sharedResources.Rooms.Count} rooms");

                // Create ECS entities from GameMap (pure data extraction)
                var worldBuilder = new WorldBuilder(ecsWorld, sharedResources);
                var (player, doors, soundSources, stairs, platforms) = worldBuilder.BuildFromGameMap(gameMap);
                playerEntity = player;
                Program.Log($"ECS entities created: 1 player, {doors.Count} doors, {soundSources.Count} sound sources, {stairs.Count} stairs, {platforms.Count} platforms");

                // Initialize ECS queries in SharedResources (must be after entities are created)
                sharedResources.InitializeQueries();

                // Initialize ECS systems
                playerInputSystem = new PlayerInputSystem(ecsWorld, sharedResources);
                movementSystem = new MovementSystem(ecsWorld);
                gravitySystem = new GravitySystem(ecsWorld, sharedResources);
                collisionSystem = new CollisionSystem(ecsWorld, sharedResources);
                footstepSystem = new FootstepSystem(ecsWorld, sharedResources);
                audioListenerSystem = new AudioListenerSystem(ecsWorld, sharedResources);
                doorInteractionSystem = new DoorInteractionSystem(ecsWorld, sharedResources);
                doorSystem = new DoorSystem(ecsWorld, sharedResources);
                ambientSoundSystem = new AmbientSoundSystem(ecsWorld, sharedResources);
                Program.Log("ECS systems initialized");

                // Log raytracing state after map is built
                audioManager.LogRaytracingState();
            }
            catch (System.Exception ex)
            {
                Program.Log($"GameMap error: {ex}");
            }

            Program.Log("LoadContent finished");
        }

        protected override void UnloadContent()
        {
            Program.Log("UnloadContent");
            audioManager?.Dispose();
            World.Destroy(ecsWorld);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            Input.Update();

            try
            {
                // Run ECS systems in order
                playerInputSystem?.Update(gameTime);
                movementSystem?.Update(gameTime);
                gravitySystem?.Update(gameTime);
                collisionSystem?.Update(gameTime);
                footstepSystem?.Update(gameTime);
                audioListenerSystem?.Update(gameTime);
                doorInteractionSystem?.Update(gameTime);
                doorSystem?.Update(gameTime);
                ambientSoundSystem?.Update(gameTime);

                // Update AudioManager (raytracing, etc.)
                audioManager?.Update();
            }
            catch (System.Exception ex)
            {
                Program.Log($"Update error: {ex}");
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            // Cielo azul claro
            GraphicsDevice.Clear(new Color(135, 180, 220));

            // Dibujar mapa 3D
            mapRenderer?.Draw();

            base.Draw(gameTime);
        }
    }
}
