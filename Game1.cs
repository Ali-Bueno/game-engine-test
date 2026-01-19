using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using DavyKager;
using Game3.Audio;
using Game3.GameMap;

namespace Game3
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        public GameMap.GameMap gameMap;
        public AudioManager audioManager;

        public Game1()
        {
            Program.Log("Game1 constructor started");
            graphics = new GraphicsDeviceManager(this);
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
                audioManager = new AudioManager(enableDebugWindow: true);
                Program.Log("AudioManager created (OpenAL ready, vaudio pending)");
            }
            catch (System.Exception ex)
            {
                Program.Log($"AudioManager error: {ex}");
            }

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
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            Input.Update();

            try
            {
                gameMap?.Update(Input.keystate, gameTime);
                audioManager?.Update();
            }
            catch (System.Exception ex)
            {
                Program.Log($"Update error: {ex}");
            }

            // Press C to speak coordinates (handled by GamePlayer now)

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            base.Draw(gameTime);
        }
    }
}
