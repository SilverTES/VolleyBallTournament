using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Mugen.Animation;
using Mugen.Core;
using Mugen.GFX;
using Mugen.Input;
using Mugen.Physics;
using System;

namespace VolleyBallTournament
{
    public class Court : Node
    {
        private Match _match;

        private bool _changeSide = false;
        public string CourtName => _courtName;
        private string _courtName;
        private float _ticWave = 0;
        private Vector2 _waveValue = new Vector2();

        private float _rotationBall = 0f;

        private Vector2 _setAPos;
        private Vector2 _setBPos;
        public Vector2 ScoreAPos => _scoreAPos;
        private Vector2 _scoreAPos;
        public Vector2 ScoreBPos => _scoreBPos;
        private Vector2 _scoreBPos;
        private Vector2 _teamAPos;
        private Vector2 _teamBPos;
        private Vector2 _teamRefereePos;
        private Vector2 _vBallAPos;
        private Vector2 _vBallBPos;
        private Vector2 _vBallCurrentPos;
        private Vector2 _infosPos;
        private Vector2 _courtNamePos;

        private Animate2D _animate2D;

        public Court(string courtName, Match match) 
        {
            _courtName = courtName;
            _match = match;

            SetSize(200, 280);

            _rotationBall = (float)Misc.Rng.NextDouble() * Geo.RAD_360;

            _animate2D = new();

            _animate2D.Add("SwapA");
            _animate2D.Add("SwapB");
        }
        public void SwapTeams()
        {
            _animate2D.SetMotion("SwapA", Easing.QuadraticEaseInOut, _teamAPos, _teamBPos, 64);
            _animate2D.Start("SwapA");
            _animate2D.SetMotion("SwapB", Easing.QuadraticEaseInOut, _teamBPos, _teamAPos, 64);
            _animate2D.Start("SwapB");

            _match.TeamA.SetIsMove(true);
            _match.TeamB.SetIsMove(true);

            Static.SoundSwap.Play(0.25f * Static.VolumeMaster, 0.1f, 0f);
        }
        //public void SetServiceSideA()
        //{
        //    _prevServiceSide = _serviceSide;
        //    _serviceSide = true;
        //}
        //public void SetServiceSideB()
        //{
        //    _prevServiceSide = _serviceSide;
        //    _serviceSide = false;
        //}

        //public void ChangeServiceSide()
        //{
        //    _prevServiceSide = _serviceSide;
        //    _serviceSide = !_serviceSide;
        //}
        //public void CancelChangeServiceSide()
        //{
        //    _serviceSide = _prevServiceSide;
        //}
        public void UpdateTeamsPosition()
        {
            if (_animate2D.OnFinish("SwapA") && _animate2D.OnFinish("SwapB"))
            {
                _match.TeamA.SetIsMove(false);
                _match.TeamB.SetIsMove(false);
                Misc.Log("On Finish Animation2d ");
                _changeSide = !_changeSide;
            }

            if (!_animate2D.IsPlay("SwapA") && !_animate2D.IsPlay("SwapB"))
            {
                if (_changeSide)
                {
                    _teamAPos = AbsRectF.TopCenter - Vector2.UnitY * 40 - Vector2.UnitY * Team.Height/2 - Vector2.UnitX * Team.Width/ 2;
                    _teamBPos = AbsRectF.BottomCenter + Vector2.UnitY * 40 - Vector2.UnitY * Team.Height/2 - Vector2.UnitX * Team.Width/2;
                }
                else
                {
                    _teamBPos = AbsRectF.TopCenter - Vector2.UnitY * 40 - Vector2.UnitY * Team.Height / 2 - Vector2.UnitX * Team.Width / 2;
                    _teamAPos = AbsRectF.BottomCenter + Vector2.UnitY * 40 - Vector2.UnitY * Team.Height / 2 - Vector2.UnitX * Team.Width / 2;
                }
            }
            else
            {
                _teamAPos = _animate2D.Value("SwapA");
                _teamBPos = _animate2D.Value("SwapB");

            }

            _scoreAPos = Team.Bound.TopRight + _teamAPos - Vector2.UnitX * 10;
            _scoreBPos = Team.Bound.TopRight + _teamBPos - Vector2.UnitX * 10;

            _setAPos = Team.Bound.RightMiddle + _teamAPos + Vector2.UnitX * 0 - Vector2.UnitY * 50;
            _setBPos = Team.Bound.RightMiddle + _teamBPos + Vector2.UnitX * 0 - Vector2.UnitY * 50;

        }
        public override Node Update(GameTime gameTime)
        {
            // Debug
            if (ButtonControl.OnePress($"Swap{_courtName}", Static.Key.IsKeyDown(Keys.S)))
            {
                SwapTeams();
            }

            _animate2D.Update();

            UpdateRect();

            UpdateTeamsPosition();

            _teamRefereePos = AbsRectF.Center + Vector2.UnitY * 0 - Vector2.UnitY * Team.Height/2 - Vector2.UnitX * Team.Width/2 + Vector2.UnitX * 120;

            _vBallAPos = Team.Bound.LeftMiddle + _teamAPos - Vector2.UnitX * 32;
            _vBallBPos = Team.Bound.LeftMiddle + _teamBPos - Vector2.UnitX * 32;

            if (_match.TeamA.HasService) _vBallCurrentPos = _vBallAPos;
            if (_match.TeamB.HasService) _vBallCurrentPos = _vBallBPos;

            _infosPos = AbsRectF.Center - Vector2.UnitY * 90 + _waveValue;
            _courtNamePos = AbsRectF.Center + Vector2.UnitY * 90;

            _ticWave += 0.1f;
            //_waveValue.X = MathF.Cos(_ticWave) * 8f;
            _waveValue.Y = MathF.Sin(_ticWave) * 8f;

            if (_match.State.CurState == Match.States.Play1 || _match.State.CurState == Match.States.Play2)
                _rotationBall += .05f;

            return base.Update(gameTime);
        }
        public override Node Draw(SpriteBatch batch, GameTime gameTime, int indexLayer)
        {
            if (indexLayer == (int)Layers.Main)
            {
                DrawCourt(batch);
                
                _match.TeamA.DrawBasicTeam(batch, Team.Bound + _teamAPos);
                _match.TeamB.DrawBasicTeam(batch, Team.Bound + _teamBPos);

                _match.TeamReferee.DrawBasicTeam(batch, Team.Bound + _teamRefereePos);

                //if (_match.State.CurState == Match.States.Play1 || _match.State.CurState == Match.States.Ready || _match.State.CurState == Match.States.CountDown)
                    DrawVBall(batch, _vBallCurrentPos);

                batch.CenterStringXY(Static.FontMain, $"Terrain {_courtName}", _courtNamePos, Color.Yellow);

                //batch.CenterStringXY(Static.FontMain, $"{_match.Infos[_match.State.CurState]}", InfosPos + Vector2.One * 6, Color.Black);
                batch.CenterStringXY(Static.FontMain, $"{_match.Infos[_match.State.CurState]}", _infosPos, Color.White);


                //batch.Draw(Static.TexReferee, Color.Black, 0, _teamRefereePos + Vector2.One * 6, Position.CENTER, Vector2.One / 4);
                //batch.Draw(Static.TexReferee, Color.White, 0, _teamRefereePos - Vector2.UnitY * 50, Position.CENTER, Vector2.One / 4);

                //batch.CenterStringXY(Static.FontMain, $"Arbitre {_match.TeamReferee.TeamName}", _teamRefereePos + Vector2.One * 6, Color.Black);
                //batch.CenterStringXY(Static.FontMain, $"Arbitre {_match.TeamReferee.TeamName}", _teamRefereePos, Color.Orange);

                if (_match.NbSetToWin > 1)
                {
                    DrawSet(batch);
                }

                batch.RightMiddleString(Static.FontMain3, _match.TeamA.ScorePoint.ToString(), _scoreAPos + Vector2.One * 6, Color.Black * .5f);
                batch.RightMiddleString(Static.FontMain3, _match.TeamB.ScorePoint.ToString(), _scoreBPos + Vector2.One * 6, Color.Black * .5f);

                batch.RightMiddleString(Static.FontMain3, _match.TeamA.ScorePoint.ToString(), _scoreAPos, Color.Gold);
                batch.RightMiddleString(Static.FontMain3, _match.TeamB.ScorePoint.ToString(), _scoreBPos, Color.Gold);

            }

            if (indexLayer == (int)Layers.HUD)
            {
                _match.TeamReferee.DrawReferee(batch, Team.Bound + _teamRefereePos);
            }


            if (indexLayer == (int)Layers.Debug)
            {
                //batch.CenterStringXY(Static.FontMain, $"{State.CurState}", AbsRectF.BottomCenter + Vector2.UnitY * 20, Color.Green);
            }

            return base.Draw(batch, gameTime, indexLayer);
        }
        private void DrawSet(SpriteBatch batch)
        {
            //batch.CenterStringXY(Static.FontMain, _match.TeamA.ScoreSet.ToString(), SetAPos, Color.Cyan);
            //batch.CenterStringXY(Static.FontMain, _match.TeamB.ScoreSet.ToString(), SetBPos, Color.Cyan);

            for (int i = 0; i < _match.NbSetToWin; i++)
            {
                var offset = new Vector2(i * 30, 0) + Vector2.UnitX * 10;

                batch.FillRectangleCentered(_setAPos + offset + Vector2.One * 6, Vector2.One * 26, Color.Black, 0);
                batch.FillRectangleCentered(_setBPos + offset + Vector2.One * 6, Vector2.One * 26, Color.Black, 0);
                batch.FillRectangleCentered(_setAPos + offset, Vector2.One * 26, Color.Gray, 0);
                batch.FillRectangleCentered(_setAPos + offset, Vector2.One * 20, i >= _match.TeamA.ScoreSet ? Color.Black * .75f: Color.Gold, 0);
                batch.FillRectangleCentered(_setBPos + offset, Vector2.One * 26, Color.Gray, 0);
                batch.FillRectangleCentered(_setBPos + offset, Vector2.One * 20, i >= _match.TeamB.ScoreSet ? Color.Black * .75f: Color.Gold, 0);
            }
        }
        private void DrawCourt(SpriteBatch batch)
        {
            Color color = Color.White * .25f;
            float thickness = 3f;

            batch.FillRectangle(AbsRectF.Extend(64f), Color.MonoGameOrange * (_match.State.CurState == Match.States.Play1 || _match.State.CurState == Match.States.Play2 ? .25f : .1f));
            batch.Rectangle(AbsRectF, color, thickness);

            batch.Line(AbsRectF.LeftMiddle, AbsRectF.RightMiddle, color, thickness);

            var threeMeter = Vector2.UnitY * _rect.Height / 6;

            batch.Line(AbsRectF.LeftMiddle - threeMeter, AbsRectF.RightMiddle - threeMeter, color, thickness);
            batch.Line(AbsRectF.LeftMiddle + threeMeter, AbsRectF.RightMiddle + threeMeter, color, thickness);
        }
        private void DrawVBall(SpriteBatch batch, Vector2 position)
        {
            batch.Draw(Static.TexVBall, Color.Black * .5f, _rotationBall, position + Vector2.One * 16, Mugen.Physics.Position.CENTER, Vector2.One / 2);
            batch.Draw(Static.TexVBall, Color.White, _rotationBall, position, Mugen.Physics.Position.CENTER, Vector2.One / 2);
        }
    }
}
