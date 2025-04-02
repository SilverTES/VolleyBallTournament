using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Mugen.Core;
using Mugen.GFX;
using Mugen.Input;

namespace VolleyBallTournament
{
    public class Screen
    {
        public const int Width = 1920;
        public const int Height = 1080;
    }
    public enum Layers
    {
        Main,
        Debug,
    }

    public struct Static
    {
        public static KeyboardState Key;
        public static MouseState Mouse;

        public static SpriteFont FontMain;
        public static SpriteFont FontMain2;
        public static SpriteFont FontMain3;
        public static SpriteFont FontDigitMono;

        public static Texture2D TexBG00;
        public static Texture2D TexCircle;
    }

    public class Game1 : Game
    {

        private ScreenPlay _screenPlay;    

        public Game1()
        {

            WindowManager.Init(this, Screen.Width, Screen.Height);

            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Window.AllowUserResizing = true;
        }

        protected override void Initialize()
        {
            _screenPlay = new ScreenPlay();
            ScreenManager.Init(_screenPlay, Enums.GetList<Layers>());

            ScreenManager.SetLayerParameter((int)Layers.Main, samplerState: SamplerState.LinearWrap);
            ScreenManager.SetLayerParameter((int)Layers.Debug, samplerState: SamplerState.LinearWrap);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            Static.FontMain = Content.Load<SpriteFont>("Fonts/fontMain");
            Static.FontMain2 = Content.Load<SpriteFont>("Fonts/fontMain2");
            Static.FontMain3 = Content.Load<SpriteFont>("Fonts/fontMain3");
            Static.FontDigitMono = Content.Load<SpriteFont>("Fonts/fontDigitMono");

            Static.TexBG00 = Content.Load<Texture2D>("Images/bg00");

            Static.TexCircle = GFX.CreateCircleTextureAA(GraphicsDevice, 100, 4);

        }

        protected override void Update(GameTime gameTime)
        {
            WindowManager.Update(gameTime);

            Static.Key = Keyboard.GetState();
            Static.Mouse = Mouse.GetState();

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Static.Key.IsKeyDown(Keys.Escape))
                Exit();

            if (ButtonControl.OnePress("ToggleFullscreen", Static.Key.IsKeyDown(Keys.F11)))
                WindowManager.ToggleFullscreen();

            ScreenManager.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            ScreenManager.DrawScreen(gameTime);
            ScreenManager.ShowScreen(gameTime, blendState: BlendState.AlphaBlend, samplerState: SamplerState.LinearWrap);

            base.Draw(gameTime);
        }
    }
}
