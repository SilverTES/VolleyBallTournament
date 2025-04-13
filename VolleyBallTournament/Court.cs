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

        private float _ticBallSize = 0;
        private float _ballSize = 0;
        private float _rotationBall = 0f;

        //private Vector2 _setAPos;
        //private Vector2 _setBPos;
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
        //private Team _prevTeamHasService = null;
        //private Team _teamHasService = null;

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

            _animate2D.Add("VBallMoveToA");
            _animate2D.Add("VBallMoveToB");

            _match.OnChangeService += OnChangeService;

        }
        public void SwapTeams()
        {
            _animate2D.SetMotion("SwapA", Easing.QuadraticEaseInOut, _teamAPos, _teamBPos, 64);
            _animate2D.Start("SwapA");
            _animate2D.SetMotion("SwapB", Easing.QuadraticEaseInOut, _teamBPos, _teamAPos, 64);
            _animate2D.Start("SwapB");

            _match.TeamA.SetIsMove(true);
            _match.TeamB.SetIsMove(true);

            Static.SoundSwap.Play(0.5f * Static.VolumeMaster, 0.1f, 0f);
        }
        public void UpdateTeamsPosition()
        {
            if (_animate2D.OnFinish("SwapA") && _animate2D.OnFinish("SwapB"))
            {
                _match.TeamA.SetIsMove(false);
                _match.TeamB.SetIsMove(false);
                //Misc.Log("On Finish Animation2d ");
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

            //_setAPos = Team.Bound.RightMiddle + _teamAPos + Vector2.UnitX * 0 - Vector2.UnitY * 50;
            //_setBPos = Team.Bound.RightMiddle + _teamBPos + Vector2.UnitX * 0 - Vector2.UnitY * 50;

        }
        public void UpdateVBallPosition()
        {
            _vBallAPos = Team.Bound.LeftMiddle + _teamAPos - Vector2.UnitX * 32;
            _vBallBPos = Team.Bound.LeftMiddle + _teamBPos - Vector2.UnitX * 32;

            if (_match.TeamHasService == _match.LastTeamHasService)
            {
                //Misc.Log("======");
                if (_match.TeamA.HasService) _vBallCurrentPos = _vBallAPos;
                if (_match.TeamB.HasService) _vBallCurrentPos = _vBallBPos;
            }

            if (_animate2D.IsPlay("VBallMoveToA")) _vBallCurrentPos = _animate2D.Value("VBallMoveToA");
            if (_animate2D.IsPlay("VBallMoveToB")) _vBallCurrentPos = _animate2D.Value("VBallMoveToB");

            //if (_animate2D.IsPlay("VBallMoveToA"))
            //    _vBallCurrentPos = _animate2D.Value("VBallMoveToA");
            //else
            //    if (_match.TeamA.HasService) {_vBallCurrentPos = _vBallAPos; }

            //if (_animate2D.IsPlay("VBallMoveToB"))
            //    _vBallCurrentPos = _animate2D.Value("VBallMoveToB");
            //else
            //    if (_match.TeamB.HasService) { _vBallCurrentPos = _vBallBPos; }

        }
        public void OnChangeService(Team team)
        {
            // Si changement de service on lance l'animation du déplacement de la balle !
            Misc.Log("Move VBALL !");
            if (team == _match.TeamA) VBallMoveToA();
            if (team == _match.TeamB) VBallMoveToB();
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

            UpdateVBallPosition();

            _teamRefereePos = AbsRectF.Center + Vector2.UnitY * 0 - Vector2.UnitY * Team.Height/2 - Vector2.UnitX * Team.Width/2 + Vector2.UnitX * 120;

            _infosPos = AbsRectF.Center - Vector2.UnitY * 90 + _waveValue;
            _courtNamePos = AbsRectF.Center + Vector2.UnitY * 90;

            _ticWave += 0.1f;
            //_waveValue.X = MathF.Cos(_ticWave) * 8f;
            _waveValue.Y = MathF.Sin(_ticWave) * 8f;

            if (_match.State.CurState == Match.States.Play1 || _match.State.CurState == Match.States.Play2)
                _rotationBall += .05f;

            _ticBallSize += 0.1f;
            _ballSize = 1.2f - Math.Abs(MathF.Sin(_ticBallSize) * .2f);

            return base.Update(gameTime);
        }
        public void VBallMoveToA()
        {
            _animate2D.SetMotion("VBallMoveToA", Easing.QuadraticEaseInOut, _vBallBPos, _vBallAPos, 32);
            _animate2D.Start("VBallMoveToA");

            //Static.SoundRanking.Play(.5f * Static.VolumeMaster, 0.1f, 0f);
        }
        public void VBallMoveToB()
        {
            _animate2D.SetMotion("VBallMoveToB", Easing.QuadraticEaseInOut, _vBallAPos, _vBallBPos, 32);
            _animate2D.Start("VBallMoveToB");

            //Static.SoundRanking.Play(.5f * Static.VolumeMaster, 0.1f, 0f);
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


                DrawInfos(batch);


                //batch.Draw(Static.TexReferee, Color.Black, 0, _teamRefereePos + Vector2.One * 6, Position.CENTER, Vector2.One / 4);
                //batch.Draw(Static.TexReferee, Color.White, 0, _teamRefereePos - Vector2.UnitY * 50, Position.CENTER, Vector2.One / 4);

                //batch.CenterStringXY(Static.FontMain, $"Arbitre {_match.TeamReferee.TeamName}", _teamRefereePos + Vector2.One * 6, Color.Black);
                //batch.CenterStringXY(Static.FontMain, $"Arbitre {_match.TeamReferee.TeamName}", _teamRefereePos, Color.Orange);

                //if (_match.NbSetToWin > 0)
                {
                    DrawSets(batch);
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
        private void DrawSets(SpriteBatch batch)
        {
            DrawSet(batch, _match.TeamA, _scoreAPos);
            DrawSet(batch, _match.TeamB, _scoreBPos);
        }
        private void DrawSet(SpriteBatch batch, Team team, Vector2 position)
        {
            for (int i = 0; i < team.Sets.Count; i++)
            {
                var set = team.Sets[i];

                var offset = new Vector2(i * 48, 0) + Vector2.UnitX * 20;
                batch.FillRectangleCentered(position + offset + Vector2.One * 6, Vector2.One * 42, Color.Black *.5f, 0);
                batch.FillRectangleCentered(position + offset, Vector2.One * 42, Color.Black, 0);
                batch.RectangleCentered(position + offset, Vector2.One * 42, Color.Gray, 1f);
                batch.CenterStringXY(Static.FontMain, $"{set.Points}", position + offset, set.IsWin ? Color.GreenYellow : Color.Red);
            }
        }
        private void DrawInfos(SpriteBatch batch)
        {
            string text = $"{_match.Infos[_match.State.CurState]}";
            Vector2 pos = _infosPos;
            Vector2 size = Static.FontMain.MeasureString(text) + new Vector2(24, -20);
            batch.FillRectangleCentered(pos, size, Color.Black * .75f, 0f);
            batch.RectangleCentered(pos, size, Color.Gray, 1f);
            batch.CenterStringXY(Static.FontMain, text, _infosPos, Color.White);
        }
        private void DrawCourt(SpriteBatch batch)
        {
            Color color = Color.White * .25f;
            float thickness = 3f;

            batch.FillRectangle(AbsRectF.Extend(64f), Color.MonoGameOrange * (_match.State.CurState == Match.States.Play1 || _match.State.CurState == Match.States.Play2 ? .5f : .25f));
            batch.FillRectangle(AbsRectF.Extend(0f), Color.Goldenrod * (_match.State.CurState == Match.States.Play1 || _match.State.CurState == Match.States.Play2 ? 1f : .5f));
            batch.Rectangle(AbsRectF, color, thickness);

            batch.Line(AbsRectF.LeftMiddle, AbsRectF.RightMiddle, color, thickness);

            var threeMeter = Vector2.UnitY * _rect.Height / 6;

            batch.Line(AbsRectF.LeftMiddle - threeMeter, AbsRectF.RightMiddle - threeMeter, color, thickness);
            batch.Line(AbsRectF.LeftMiddle + threeMeter, AbsRectF.RightMiddle + threeMeter, color, thickness);
        }
        private void DrawVBall(SpriteBatch batch, Vector2 position)
        {
            batch.Draw(Static.TexVBall, Color.Black * .5f, _rotationBall, position + Vector2.One * 16, Mugen.Physics.Position.CENTER, Vector2.One / 2 * _ballSize);
            batch.Draw(Static.TexVBall, Color.White, _rotationBall, position, Mugen.Physics.Position.CENTER, Vector2.One / 2 * _ballSize);
        }
    }
}
