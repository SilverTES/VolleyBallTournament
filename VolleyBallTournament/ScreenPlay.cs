using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Mugen.Animation;
using Mugen.Core;
using Mugen.GFX;
using Mugen.Input;
using System.Text.RegularExpressions;

namespace VolleyBallTournament
{
    public class ScreenPlay : Node  
    {
        public PhaseRegister PhaseRegister;
        public PhasePool PhasePool1;
        public PhasePool PhasePool2;

        public Sequence Sequence;

        KeyboardState _key;

        Animate _animate;

        float _cameraX;

        Vector2 _versionPos;

        int _step = 0;

        public ScreenPlay(Game game) 
        {
            SetSize(Screen.Width, Screen.Height);

            PhaseRegister = new PhaseRegister(game).SetX(Screen.Width * 0f).AppendTo(this).This<PhaseRegister>();
            PhasePool1 = new PhasePool("Phase de pool 1").SetX(Screen.Width * 1f).AppendTo(this).This<PhasePool>();
            PhasePool2 = new PhasePool("Phase de pool 2").SetX(Screen.Width * 2f).AppendTo(this).This<PhasePool>();

            _animate = new Animate();
            _animate.Add("SlideLeft");
            _animate.Add("SlideRight");

            _versionPos = AbsRectF.BottomRight - Vector2.One * 24;

            var textBoxs = PhaseRegister.GroupOf<TextBox>();

            for (int i = 0; i < textBoxs.Count; i++)
            {
                var textBox = textBoxs[i];

                textBox.OnChange += (t) => 
                { 
                    PhasePool1.GetTeam(t.Id).TeamName = textBox.Text;
                    PhasePool2.GetTeam(t.Id).TeamName = textBox.Text;
                };
            }

            Sequence = new Sequence();
            Sequence.Init("SetupPool.xml", PhasePool1.GetTeams());

            PhasePool1.LoadSequence(Sequence, _step);

            //Debug
            _cameraX = -Screen.Width;
            SetPosition(-Screen.Width, 0);

        }
        public override Node Update(GameTime gameTime)
        {
            UpdateRect();

            PhaseRegister.IsPaused = true;
            PhasePool1.IsPaused = true;
            PhasePool2.IsPaused = true;

            if (_cameraX == -PhaseRegister._x) PhaseRegister.IsPaused = false;
            if (_cameraX == -PhasePool1._x) PhasePool1.IsPaused = false;
            if (_cameraX == -PhasePool2._x) PhasePool2.IsPaused = false;

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

            if (!PhasePool1.IsPaused)
            {
                if (ButtonControl.OnePress("ToggleTimer", Keyboard.GetState().IsKeyDown(Keys.Space)))
                {
                    PhasePool1.Timer.ToggleTimer();

                    var matchs = PhasePool1.GetMatchs();

                    for (int i = 0; i < matchs.Length; i++)
                    {
                        matchs[i].Court.State.Set(PhasePool1.Timer.IsRunning ? Court.States.Play : Court.States.Ready);
                    }

                    if (!PhasePool1.Timer.IsRunning)
                    {
                        _step++;
                        _step = int.Clamp(_step, 0, 7);

                        PhasePool1.ResetTeamsStatus();
                        PhasePool1.LoadSequence(Sequence, _step);
                    }
                }

                if (ButtonControl.OnePress("ShuffleTotalPoint", Keyboard.GetState().IsKeyDown(Keys.D1)))
                {
                    PhasePool1.ShuffleTeamsTotalPoint();
                }

                if (ButtonControl.OnePress("Stop", Keyboard.GetState().IsKeyDown(Keys.Back)))
                {
                    PhasePool1.ResetTeamsStatus();
                    PhasePool1.LoadSequence(Sequence, _step = 0);
                    PhasePool1.ResetTeamsTotalPoint();
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
