using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mugen.Animation;
using Mugen.Core;
using Mugen.GFX;
using Mugen.Physics;
using System;

namespace VolleyBallTournament
{
    public class Court : Node
    {
        private Match _match;

        private Vector2 _teamRefereePos;
        //private bool _prevServiceSide = true;
        //public bool ServiceSide => _serviceSide;
        //private bool _serviceSide = true; // true A , false B

        private bool _changeSide = false;
        public string CourtName => _courtName;
        string _courtName;
        float _ticWave = 0;
        float _waveValue = 0;
        float _rotation = 0f;

        public Vector2 SetAPos;
        public Vector2 SetBPos;
        public Vector2 ScoreAPos;
        public Vector2 ScoreBPos;
        
        public Vector2 TeamAPos;
        public Vector2 TeamBPos;
        public Vector2 TeamRefereePos;

        public Vector2 VBallAPos;
        public Vector2 VBallBPos;
        public Vector2 VBallCurrentPos;

        public Vector2 InfosPos;
        public Vector2 CourtNamePos;

        Animate2D _animate2D = new();

        public Court(string courtName, Match match) 
        {
            _courtName = courtName;
            _match = match;

            SetSize(200, 280);

            _rotation = (float)Misc.Rng.NextDouble() * Geo.RAD_360;

            _animate2D.Add("SwapA");
            _animate2D.Add("SwapB");
        }
        public void SwapTeams()
        {
            _animate2D.SetMotion("SwapA", Easing.QuadraticEaseOut, TeamAPos, TeamBPos, 80);
            _animate2D.Start("SwapA");
            _animate2D.SetMotion("SwapB", Easing.QuadraticEaseOut, TeamBPos, TeamAPos, 80);
            _animate2D.Start("SwapB");
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
            if (!_animate2D.IsPlay("SwapA"))
            {
                if (_changeSide)
                {
                    TeamAPos = AbsRectF.TopCenter - Vector2.UnitY * 40 - Vector2.UnitY * Team.Height/2 - Vector2.UnitX * Team.Width/ 2;
                    TeamBPos = AbsRectF.BottomCenter + Vector2.UnitY * 40 - Vector2.UnitY * Team.Height/2 - Vector2.UnitX * Team.Width/2;
                }
                else
                {
                    TeamBPos = AbsRectF.TopCenter - Vector2.UnitY * 40 - Vector2.UnitY * Team.Height / 2 - Vector2.UnitX * Team.Width / 2;
                    TeamAPos = AbsRectF.BottomCenter + Vector2.UnitY * 40 - Vector2.UnitY * Team.Height / 2 - Vector2.UnitX * Team.Width / 2;
                }
            }
            else
            {
                TeamAPos = _animate2D.Value("SwapA");
                TeamBPos = _animate2D.Value("SwapB");

            }

            if (_animate2D.OnFinish("SwapA"))
            {
                Misc.Log("On Finish Animation2d ");
                _changeSide = !_changeSide;
            }



            ScoreAPos = Team.Bound.TopRight + TeamAPos - Vector2.UnitX * 10;
            ScoreBPos = Team.Bound.TopRight + TeamBPos - Vector2.UnitX * 10;

            SetAPos = Team.Bound.RightMiddle + TeamAPos + Vector2.UnitX * 0 - Vector2.UnitY * 50;
            SetBPos = Team.Bound.RightMiddle + TeamBPos + Vector2.UnitX * 0 - Vector2.UnitY * 50;

        }
        public override Node Update(GameTime gameTime)
        {
            _animate2D.Update();

            UpdateRect();

            UpdateTeamsPosition();

            TeamRefereePos = AbsRectF.Center + Vector2.UnitY * 0 - Vector2.UnitY * Team.Height/2 - Vector2.UnitX * Team.Width/2 + Vector2.UnitX * 120;

            VBallAPos = Team.Bound.LeftMiddle + TeamAPos - Vector2.UnitX * 32;
            VBallBPos = Team.Bound.LeftMiddle + TeamBPos - Vector2.UnitX * 32;

            if (_match.TeamA.HasService) VBallCurrentPos = VBallAPos;
            if (_match.TeamB.HasService) VBallCurrentPos = VBallBPos;

            _teamRefereePos = AbsRectF.Center;
            InfosPos = AbsRectF.Center - Vector2.UnitY * 90 + Vector2.UnitY * _waveValue;
            CourtNamePos = AbsRectF.Center + Vector2.UnitY * 90;

            _ticWave += 0.1f;
            _waveValue = MathF.Sin(_ticWave) * 4f;

            if (_match.State.CurState == Match.States.Play)
                _rotation += .05f;


            return base.Update(gameTime);
        }
        public override Node Draw(SpriteBatch batch, GameTime gameTime, int indexLayer)
        {
            if (indexLayer == (int)Layers.Main)
            {
                DrawCourt(batch);
                
                _match.TeamA.DrawBasicTeam(batch, Team.Bound + TeamAPos);
                _match.TeamB.DrawBasicTeam(batch, Team.Bound + TeamBPos);

                _match.TeamReferee.DrawBasicTeam(batch, Team.Bound + TeamRefereePos);

                if (_match.State.CurState == Match.States.Play || _match.State.CurState == Match.States.Ready || _match.State.CurState == Match.States.CountDown)
                    DrawVBall(batch, VBallCurrentPos);

                batch.CenterStringXY(Static.FontMain, $"Terrain {_courtName}", CourtNamePos, Color.Yellow);

                //batch.CenterStringXY(Static.FontMain, $"{_match.Infos[_match.State.CurState]}", InfosPos + Vector2.One * 6, Color.Black);
                batch.CenterStringXY(Static.FontMain, $"{_match.Infos[_match.State.CurState]}", InfosPos, Color.White);


                //batch.Draw(Static.TexReferee, Color.Black, 0, _teamRefereePos + Vector2.One * 6, Position.CENTER, Vector2.One / 4);
                //batch.Draw(Static.TexReferee, Color.White, 0, _teamRefereePos - Vector2.UnitY * 50, Position.CENTER, Vector2.One / 4);

                //batch.CenterStringXY(Static.FontMain, $"Arbitre {_match.TeamReferee.TeamName}", _teamRefereePos + Vector2.One * 6, Color.Black);
                //batch.CenterStringXY(Static.FontMain, $"Arbitre {_match.TeamReferee.TeamName}", _teamRefereePos, Color.Orange);

                if (_match.NbSetToWin > 1)
                {
                    DrawSet(batch);
                }

                batch.RightMiddleString(Static.FontMain3, _match.TeamA.ScorePoint.ToString(), ScoreAPos + Vector2.One * 6, Color.Black * .5f);
                batch.RightMiddleString(Static.FontMain3, _match.TeamB.ScorePoint.ToString(), ScoreBPos + Vector2.One * 6, Color.Black * .5f);

                batch.RightMiddleString(Static.FontMain3, _match.TeamA.ScorePoint.ToString(), ScoreAPos, Color.Gold);
                batch.RightMiddleString(Static.FontMain3, _match.TeamB.ScorePoint.ToString(), ScoreBPos, Color.Gold);

            }

            if (indexLayer == (int)Layers.HUD)
            {
                _match.TeamReferee.DrawReferee(batch, Team.Bound + TeamRefereePos);
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

                batch.FillRectangleCentered(SetAPos + offset + Vector2.One * 6, Vector2.One * 26, Color.Black, 0);
                batch.FillRectangleCentered(SetBPos + offset + Vector2.One * 6, Vector2.One * 26, Color.Black, 0);
                batch.FillRectangleCentered(SetAPos + offset, Vector2.One * 26, Color.Gray, 0);
                batch.FillRectangleCentered(SetAPos + offset, Vector2.One * 20, i >= _match.TeamA.ScoreSet ? Color.Black * .75f: Color.Gold, 0);
                batch.FillRectangleCentered(SetBPos + offset, Vector2.One * 26, Color.Gray, 0);
                batch.FillRectangleCentered(SetBPos + offset, Vector2.One * 20, i >= _match.TeamB.ScoreSet ? Color.Black * .75f: Color.Gold, 0);
            }
        }
        private void DrawCourt(SpriteBatch batch)
        {
            Color color = Color.White * .25f;
            float thickness = 3f;

            batch.FillRectangle(AbsRectF.Extend(64f), Color.MonoGameOrange * (_match.State.CurState == Match.States.Play ? .25f : .1f));
            batch.Rectangle(AbsRectF, color, thickness);

            batch.Line(AbsRectF.LeftMiddle, AbsRectF.RightMiddle, color, thickness);

            var threeMeter = Vector2.UnitY * _rect.Height / 6;

            batch.Line(AbsRectF.LeftMiddle - threeMeter, AbsRectF.RightMiddle - threeMeter, color, thickness);
            batch.Line(AbsRectF.LeftMiddle + threeMeter, AbsRectF.RightMiddle + threeMeter, color, thickness);
        }
        private void DrawVBall(SpriteBatch batch, Vector2 position)
        {
            batch.Draw(Static.TexVBall, Color.Black * .5f, _rotation, position + Vector2.One * 16, Mugen.Physics.Position.CENTER, Vector2.One / 2);
            batch.Draw(Static.TexVBall, Color.White, _rotation, position, Mugen.Physics.Position.CENTER, Vector2.One / 2);
        }
    }
}
