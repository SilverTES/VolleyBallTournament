using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Mugen.Animation;
using Mugen.Core;
using Mugen.GFX;
using Mugen.Input;

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

        private Sequence _sequence;

        KeyboardState _key;

        Animate _animate;

        float _cameraX;

        Vector2 _versionPos;

        int _step = 0;

        public ScreenPlay(Game game) 
        {
            SetSize(Screen.Width, Screen.Height);

            _sequence = new Sequence();

            _phaseRegister = new PhaseRegister(game).SetX(Screen.Width * 0f).AppendTo(this).This<PhaseRegister>();
            _phasePool1 = new PhasePool("Phase de pool 1", _sequence).SetX(Screen.Width * 1f).AppendTo(this).This<PhasePool>();
            _phasePool2 = new PhasePool("Phase de pool 2", _sequence).SetX(Screen.Width * 2f).AppendTo(this).This<PhasePool>();

            _sequence.Init("SetupPool.xml", _phasePool1.GetTeams());
            _phasePool1.LoadSequence(_sequence, _step);

            _animate = new Animate();
            _animate.Add("SlideLeft");
            _animate.Add("SlideRight");

            _versionPos = AbsRectF.BottomRight - Vector2.One * 24;

            var textBoxs = _phaseRegister.GroupOf<TextBox>();
            for (int i = 0; i < textBoxs.Count; i++)
            {
                var textBox = textBoxs[i];
                textBox.OnChange += (t) => 
                { 
                    _phasePool1.GetTeam(t.Id).SetTeamName(textBox.Text);
                    _phasePool2.GetTeam(t.Id).SetTeamName(textBox.Text);
                };
            }

            //Debug
            _cameraX = -Screen.Width;
            SetPosition(-Screen.Width, 0);
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
                    Misc.Log("Slide Left");

                    _animate.SetMotion("SlideLeft", Easing.QuadraticEaseOut, _cameraX, _cameraX = _cameraX + Screen.Width, 24);

                    _animate.Start("SlideLeft");
                }

            }
            if (ButtonControl.OnePress("SlideRight", _key.IsKeyDown(Keys.Right) && _key.IsKeyDown(Keys.LeftControl)))
            {
                if (_cameraX > -Screen.Width * 2)
                {
                    Misc.Log("Slide Right");

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
                batch.RightMiddleString(Static.FontMini, $"©SilverTES V{0}.{1}", _versionPos, Color.White);
            }

            //batch.String(Static.FontMain, $"{_cameraX}", Vector2.One * 200, Color.White);
            //batch.String(Static.FontMain, $"{PhaseRegister.IsPaused} : {PhasePool1.IsPaused} : {PhasePool2.IsPaused}", Vector2.One * 200, Color.White);
            DrawChilds(batch, gameTime, indexLayer);


            return base.Draw(batch, gameTime, indexLayer);
        }
    }
}
