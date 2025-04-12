using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Mugen.Core;
using Mugen.GFX;
using Mugen.Input;
using System;

namespace VolleyBallTournament
{
    public class Screen
    {
        public const int Width = 1920;
        public const int Height = 1080;
    }
    public enum Layers
    {
        BackFX,
        Main,
        HUD,
        FrontFX,
        Debug,
    }

    public struct Static
    {
        public static float VolumeMaster = 0.5f;

        public static NetworkServer Server;

        public static KeyboardState Key;
        public static MouseState Mouse;
        public static Vector2 MousePos;

        public static RasterizerState RasterizerState;

        public static SpriteFont FontMicro;
        public static SpriteFont FontMini;
        public static SpriteFont FontMain;
        public static SpriteFont FontMain2;
        public static SpriteFont FontMain3;
        public static SpriteFont FontDigitMono;
        public static SpriteFont FontDigitMonoBG;

        public static Texture2D TexBG00;
        public static Texture2D TexVBall;
        public static Texture2D TexReferee;
        public static Texture2D TexCircle;
        public static Texture2D TexLine;

        public static SoundEffect SoundPoint;
        public static SoundEffect SoundCountDown;
        public static SoundEffect SoundStart;
        public static SoundEffect SoundSwap;

        public static void DrawRoundedRectangle(SpriteBatch batch, Texture2D texLine, Rectangle rect, Color color, int topLeftRadius, int topRightRadius, int bottomRightRadius, int bottomLeftRadius, int thickness, int segments = 4)
        {
            var topLeft = new Vector2(rect.X + topLeftRadius, rect.Y + topLeftRadius);
            var topRight = new Vector2(rect.X + rect.Width - topRightRadius, rect.Y + topRightRadius);
            var bottomLeft = new Vector2(rect.X + bottomLeftRadius, rect.Y + rect.Height - bottomLeftRadius);
            var bottomRight = new Vector2(rect.X + rect.Width - bottomRightRadius, rect.Y + rect.Height - bottomRightRadius);
            // Dessiner les parties droites du rectangle
            // Horizontal
            batch.LineTexture(texLine, topLeft - Vector2.UnitY * topLeftRadius, topRight - Vector2.UnitY * topRightRadius, thickness, color);
            batch.LineTexture(texLine, bottomLeft + Vector2.UnitY * bottomLeftRadius, bottomRight + Vector2.UnitY * bottomRightRadius, thickness, color);
            // Vertical
            batch.LineTexture(texLine, topLeft - Vector2.UnitX * topLeftRadius, bottomLeft - Vector2.UnitX * bottomLeftRadius, thickness, color);
            batch.LineTexture(texLine, topRight + Vector2.UnitX * topRightRadius, bottomRight + Vector2.UnitX * bottomRightRadius, thickness, color);

            // Dessiner les arcs de cercle pour les coins
            DrawArc(batch, texLine, new Vector2(rect.X + topLeftRadius, rect.Y + topLeftRadius), topLeftRadius, 180, 270, color, thickness, segments);
            DrawArc(batch, texLine, new Vector2(rect.X + rect.Width - topRightRadius, rect.Y + topRightRadius), topRightRadius, 270, 360, color, thickness, segments);
            DrawArc(batch, texLine, new Vector2(rect.X + rect.Width - bottomRightRadius, rect.Y + rect.Height - bottomRightRadius), bottomRightRadius, 0, 90, color, thickness, segments);
            DrawArc(batch, texLine, new Vector2(rect.X + bottomLeftRadius, rect.Y + rect.Height - bottomLeftRadius), bottomLeftRadius, 90, 180, color, thickness, segments);
        }
        public static void DrawArc(SpriteBatch batch, Texture2D texLine,Vector2 center, int radius, float startAngle, float endAngle, Color color, int thickness, int segments = 4)
        {
            segments = Math.Max(segments, 1);
            float angleStep = MathHelper.ToRadians((endAngle - startAngle) / segments);

            for (int i = 0; i < segments; i++)
            {
                float angle = MathHelper.ToRadians(startAngle) + angleStep * i;
                Vector2 start = center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
                Vector2 end = center + new Vector2((float)Math.Cos(angle + angleStep), (float)Math.Sin(angle + angleStep)) * radius;

                // Calculer l'angle de rotation pour la ligne
                float rotation = (float)Math.Atan2(end.Y - start.Y, end.X - start.X);

                // Calculer la longueur de la ligne
                float length = Vector2.Distance(start, end);

                // Dessiner la ligne avec la rotation appropriée
                //batch.Draw(texLine, start, null, color, rotation, Vector2.Zero, new Vector2(length, thickness), SpriteEffects.None, 0);

                batch.LineTexture(texLine, start, end, thickness, color);
                //batch.LineIn(start, end, color, thickness);
            }
        }
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
            base.Initialize();

            _screenPlay = new ScreenPlay(this);
            ScreenManager.Init(_screenPlay, Enums.GetList<Layers>());

            Static.RasterizerState = new RasterizerState { ScissorTestEnable = true };
            ScreenManager.SetLayerParameter((int)Layers.BackFX, samplerState: SamplerState.LinearWrap, blendState: BlendState.Additive);
            ScreenManager.SetLayerParameter((int)Layers.Main, samplerState: SamplerState.LinearWrap);
            ScreenManager.SetLayerParameter((int)Layers.HUD, samplerState: SamplerState.LinearWrap, rasterizerState : Static.RasterizerState, sortMode: SpriteSortMode.Immediate);
            ScreenManager.SetLayerParameter((int)Layers.FrontFX, samplerState: SamplerState.LinearWrap, blendState: BlendState.Additive);
            ScreenManager.SetLayerParameter((int)Layers.Debug, samplerState: SamplerState.LinearWrap);

            Static.Server = new NetworkServer(_screenPlay);


            Static.Server.StartServer();
        }

        protected override void LoadContent()
        {
            Static.FontMicro = Content.Load<SpriteFont>("Fonts/fontMicro");
            Static.FontMini = Content.Load<SpriteFont>("Fonts/fontMini");
            Static.FontMain = Content.Load<SpriteFont>("Fonts/fontMain");
            Static.FontMain2 = Content.Load<SpriteFont>("Fonts/fontMain2");
            Static.FontMain3 = Content.Load<SpriteFont>("Fonts/fontMain3");
            Static.FontDigitMono = Content.Load<SpriteFont>("Fonts/fontDigitMono");
            Static.FontDigitMonoBG = Content.Load<SpriteFont>("Fonts/fontDigitMonoBG");

            Static.TexBG00 = Content.Load<Texture2D>("Images/bg00");
            Static.TexVBall = Content.Load<Texture2D>("Images/vballmini");
            Static.TexReferee = Content.Load<Texture2D>("Images/referee00");

            Static.SoundPoint = Content.Load<SoundEffect>("Sounds/slide-ping");
            Static.SoundCountDown = Content.Load<SoundEffect>("Sounds/countdown");
            Static.SoundStart = Content.Load<SoundEffect>("Sounds/race-start");
            Static.SoundSwap = Content.Load<SoundEffect>("Sounds/electric_door_opening_1");

            Static.TexCircle = GFX.CreateCircleTextureAA(GraphicsDevice, 100, 4);
            Static.TexLine = GFX.CreateLineTextureAA(GraphicsDevice, 100, 15, 7);

        }

        protected override void Update(GameTime gameTime)
        {
            Static.Server.Update();

            WindowManager.Update(gameTime);

            Static.Key = Keyboard.GetState();
            Static.Mouse = Mouse.GetState();

            Static.MousePos = WindowManager.GetMousePosition();

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Static.Key.IsKeyDown(Keys.Escape))
                Exit();

            if (ButtonControl.OnePress("ToggleFullscreen", Static.Key.IsKeyDown(Keys.LeftAlt) && Static.Key.IsKeyDown(Keys.Enter)))
                WindowManager.ToggleFullscreen();

            ScreenManager.Update(gameTime);

            base.Update(gameTime);
        }
        protected override void OnExiting(object sender, ExitingEventArgs args)
        {
            Static.Server.Stop();
            base.OnExiting(sender, args);
        }

        protected override void Draw(GameTime gameTime)
        {
            ScreenManager.DrawScreen(gameTime);
            ScreenManager.ShowScreen(gameTime, blendState: BlendState.AlphaBlend, samplerState: SamplerState.LinearWrap);

            base.Draw(gameTime);
        }
    }
}
