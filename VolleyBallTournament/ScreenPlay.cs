using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Mugen.Animation;
using Mugen.Core;
using Mugen.GFX;
using Mugen.Input;
using System.IO;
using System;

namespace VolleyBallTournament
{
    public class ScreenPlay : Node  
    {
        public PhaseRegister PhaseRegister => _phaseRegister;
        private PhaseRegister _phaseRegister;
        public PhasePool PhasePool1 => _phasePool1;
        private PhasePool _phasePool1;
        public PhasePool PhasePool2 => _phasePool2;
        private PhasePool _phasePool2;

        private RotationManager _rotationManager;

        KeyboardState _key;

        Animate _animate;

        float _cameraX;

        Vector2 _versionPos;

        int _rotation = 0;

        public ScreenPlay(Game game)
        {

            var files = GetFilesInDirectory(Directory.GetCurrentDirectory(), "*.xml");


            for (int i = 0; i < files.Length; i++)
            {
                var file = files[i];
                Misc.Log($"-> {file} ");
            }
            var configFile = files[0];



            SetSize(Screen.Width, Screen.Height);

            _phaseRegister = new PhaseRegister(game, 4, 4, 3).SetX(Screen.Width * 0f).AppendTo(this).This<PhaseRegister>();

            _rotationManager = new RotationManager();
            _rotationManager.LoadFile(configFile, _phaseRegister.GetTeams(), _phaseRegister.GetMatchs());

            _phasePool1 = new PhasePool("Phase de poule Brassage", _rotationManager, _phaseRegister).SetX(Screen.Width * 1f).AppendTo(this).This<PhasePool>();
            _phasePool2 = new PhasePool("Phase de poule Qualification", _rotationManager).SetX(Screen.Width * 2f).AppendTo(this).This<PhasePool>();

            _phasePool1.SetRotation(_rotation = 0);

            _animate = new Animate();
            _animate.Add("SlideLeft");
            _animate.Add("SlideRight");

            _versionPos = AbsRectF.BottomRight - Vector2.One * 16;

            //Debug
            _cameraX = -Screen.Width;
            SetPosition(-Screen.Width, 0);
        }
        public static string[] GetFilesInDirectory(string directoryPath, string filter)
        {
            try
            {
                if (Directory.Exists(directoryPath))
                {
                    return Directory.GetFiles(directoryPath, filter); // Filtre personnalisable
                }
                return new string[0];
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur : {ex.Message}");
                return new string[0];
            }
        }

        public override Node Update(GameTime gameTime)
        {
            UpdateRect();

            _phaseRegister.IsPaused = true;
            _phasePool1.IsPaused = true;
            _phasePool2.IsPaused = true;

            if (_cameraX == -_phaseRegister._x) _phaseRegister.IsPaused = false;
            if (_cameraX == -_phasePool1._x) _phasePool1.IsPaused = false;
            if (_cameraX == -_phasePool2._x) _phasePool2.IsPaused = false;

            _key = Keyboard.GetState();

            if (ButtonControl.OnePress("SlideLeft", _key.IsKeyDown(Keys.Left) && _key.IsKeyDown(Keys.LeftControl)))
            {
                if (_cameraX < 0)
                {
                    //Misc.Log("Slide Left");

                    _animate.SetMotion("SlideLeft", Easing.QuadraticEaseOut, _cameraX, _cameraX = _cameraX + Screen.Width, 24);

                    _animate.Start("SlideLeft");
                }

            }
            if (ButtonControl.OnePress("SlideRight", _key.IsKeyDown(Keys.Right) && _key.IsKeyDown(Keys.LeftControl)))
            {
                if (_cameraX > -Screen.Width * 2)
                {
                    //Misc.Log("Slide Right");

                    _animate.SetMotion("SlideRight", Easing.QuadraticEaseOut, _cameraX, _cameraX = _cameraX - Screen.Width, 24);

                    _animate.Start("SlideRight");
                }
            }


            if (_animate.IsPlay())
            {
                SetPosition(_animate.Value(), 0);
            }

            _animate.NextFrame();

            UpdateChilds(gameTime);

            return base.Update(gameTime);
        }
        public override Node Draw(SpriteBatch batch, GameTime gameTime, int indexLayer)
        {
            batch.GraphicsDevice.Clear(Color.Transparent);

            if (indexLayer == (int)Layers.Debug)
            { 
                batch.RightMiddleString(Static.FontMicro, $"©SilverTES V{0}.{1}", _versionPos, Color.White);
            }

            //batch.String(Static.FontMain, $"{_cameraX}", Vector2.One * 200, Color.White);
            //batch.String(Static.FontMain, $"{PhaseRegister.IsPaused} : {PhasePool1.IsPaused} : {PhasePool2.IsPaused}", Vector2.One * 200, Color.White);
            DrawChilds(batch, gameTime, indexLayer);


            return base.Draw(batch, gameTime, indexLayer);
        }
    }
}
