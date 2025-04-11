using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
        private bool _prevServiceSide = true;
        public bool ServiceSide => _serviceSide;
        private bool _serviceSide = true; // true A , false B

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

        public Vector2 InfosPos;
        public Vector2 CourtNamePos;

        public Court(string courtName, Match match) 
        {
            _courtName = courtName;
            _match = match;

            SetSize(200, 280);

            _rotation = (float)Misc.Rng.NextDouble() * Geo.RAD_360;
        }
        public void SetServiceSideA()
        {
            _prevServiceSide = _serviceSide;
            _serviceSide = true;
        }
        public void SetServiceSideB()
        {
            _prevServiceSide = _serviceSide;
            _serviceSide = false;
        }

        public void ChangeServiceSide()
        {
            _prevServiceSide = _serviceSide;
            _serviceSide = !_serviceSide;
        }
        public void CancelChangeServiceSide()
        {
            _serviceSide = _prevServiceSide;
        }
        public override Node Update(GameTime gameTime)
        {
            UpdateRect();

            TeamAPos = AbsRectF.TopCenter - Vector2.UnitY * 40 - Vector2.UnitY * Team.Height/2 - Vector2.UnitX * Team.Width/2;
            TeamBPos = AbsRectF.BottomCenter + Vector2.UnitY * 40 - Vector2.UnitY * Team.Height/2 - Vector2.UnitX * Team.Width/2;

            TeamRefereePos = AbsRectF.Center + Vector2.UnitY * 0 - Vector2.UnitY * Team.Height/2 - Vector2.UnitX * Team.Width/2;

            VBallAPos = AbsRectF.TopCenter;
            VBallBPos = AbsRectF.BottomCenter;

            ScoreAPos = Team.Bound.LeftMiddle + TeamAPos - Vector2.UnitX * 10;
            ScoreBPos = Team.Bound.LeftMiddle + TeamBPos - Vector2.UnitX * 10;

            SetAPos = Team.Bound.RightMiddle + TeamAPos - Vector2.UnitX * 50;
            SetBPos = Team.Bound.RightMiddle + TeamBPos - Vector2.UnitX * 50;


            _teamRefereePos = AbsRectF.Center;
            InfosPos = AbsRectF.Center - Vector2.UnitY * 80 + Vector2.UnitY * _waveValue;
            CourtNamePos = AbsRectF.Center + Vector2.UnitY * 80;

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
                    DrawVBall(batch);

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
                batch.FilledCircle(Static.TexCircle, SetAPos + new Vector2(i * 24, 0), 24, i >= _match.TeamA.ScoreSet ? Color.Black: Color.Gold);
                batch.FilledCircle(Static.TexCircle, SetBPos + new Vector2(i * 24, 0), 24, i >= _match.TeamB.ScoreSet ? Color.Black: Color.Gold);
            }
        }
        private void DrawCourt(SpriteBatch batch)
        {
            Color color = Color.White * .25f;

            batch.FillRectangle(AbsRectF.Extend(64f), Color.MonoGameOrange * (_match.State.CurState == Match.States.Play ? .25f : .1f));
            batch.Rectangle(AbsRectF, color, 3f);

            batch.Line(AbsRectF.LeftMiddle, AbsRectF.RightMiddle, color, 3f);

            var threeMeter = Vector2.UnitY * _rect.Height / 6;

            batch.Line(AbsRectF.LeftMiddle - threeMeter, AbsRectF.RightMiddle - threeMeter, color, 3f);
            batch.Line(AbsRectF.LeftMiddle + threeMeter, AbsRectF.RightMiddle + threeMeter, color, 3f);
        }
        private void DrawVBall(SpriteBatch batch)
        {
            batch.Draw(Static.TexVBall, Color.Black * .5f, _rotation, (_serviceSide ? VBallAPos : VBallBPos) + Vector2.One * 16, Mugen.Physics.Position.CENTER, Vector2.One / 2);
            batch.Draw(Static.TexVBall, Color.White, _rotation, _serviceSide ? VBallAPos : VBallBPos, Mugen.Physics.Position.CENTER, Vector2.One / 2);
        }
    }
}
