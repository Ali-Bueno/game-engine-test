using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using DavyKager;
using Game3.Audio;

namespace Game3
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        public Map map;
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
                Program.Log("Creating AudioManager...");
                // World size must cover the entire map:
                // X: -20 to 30 (50m), Y: 0 to 75 (75m), Z: 0 to 10 (10m for two floors)
                // Add margin for safety: 60x80x12
                audioManager = new AudioManager(new vaudio.Vector3F(60, 80, 12), enableDebugWindow: true);
                Program.Log("AudioManager created");
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
                Program.Log("Creating Map...");
                map = new Map(audioManager);
                Program.Log("Map created, calling BuildMap...");
                map.BuildMap();
                Program.Log("BuildMap finished");
            }
            catch (System.Exception ex)
            {
                Program.Log($"Map error: {ex}");
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
                map?.Update(Input.keystate, gameTime);
                audioManager?.Update();
            }
            catch (System.Exception ex)
            {
                Program.Log($"Update error: {ex}");
            }

            // Press C to speak coordinates
            if (Input.WasKeyPressed(Keys.C))
            {
                var pos = map.Player.Position;
                Tolk.Speak($"{pos.X:F1}, {pos.Y:F1}, {pos.Z:F1}, angle {map.Player.Angle:F0}", true);
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            base.Draw(gameTime);
        }
    }
}
