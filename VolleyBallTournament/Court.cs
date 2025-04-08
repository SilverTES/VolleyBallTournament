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

        public Court(string courtName, Match match) 
        {
            _courtName = courtName;
            _match = match;

            SetSize(480, 240);

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

            _ticWave += 0.1f;
            _waveValue = MathF.Sin(_ticWave) * 4f;

            if (_match.State.CurState == Match.States.Play)
                _rotation += .05f;

            _teamRefereePos = AbsRectF.TopCenter + Vector2.UnitY * 30;

            return base.Update(gameTime);
        }
        public override Node Draw(SpriteBatch batch, GameTime gameTime, int indexLayer)
        {
            if (indexLayer == (int)Layers.Main)
            {
                Color color = Color.White * .25f;

                batch.FillRectangle(AbsRectF.Extend(8f), Color.MonoGameOrange * (_match.State.CurState == Match.States.Play ? .25f : .1f));
                batch.Rectangle(AbsRectF, color, 3f);

                batch.Line(AbsRectF.TopCenter, AbsRectF.BottomCenter, color, 3f);

                var threeMeter = Vector2.UnitX * _rect.Width / 6;

                batch.Line(AbsRectF.TopCenter - threeMeter, AbsRectF.BottomCenter - threeMeter, color, 3f);
                batch.Line(AbsRectF.TopCenter + threeMeter, AbsRectF.BottomCenter + threeMeter, color, 3f);

                //if (State.CurState == States.Play)
                    DrawVBall(batch);


                batch.CenterStringXY(Static.FontMain, $"Terrain {_courtName}", AbsRectF.BottomCenter - Vector2.UnitY * 20, Color.Yellow);

                batch.CenterStringXY(Static.FontMain, $"{_match.Infos[_match.State.CurState]}", AbsRectF.Center + Vector2.UnitY * _waveValue + Vector2.One * 6, Color.Black);
                batch.CenterStringXY(Static.FontMain, $"{_match.Infos[_match.State.CurState]}", AbsRectF.Center + Vector2.UnitY * _waveValue, Color.Cyan);


                batch.Draw(Static.TexReferee, Color.Black, 0, _teamRefereePos - Vector2.UnitY * 60 + Vector2.One * 6, Position.CENTER, Vector2.One / 4);
                batch.Draw(Static.TexReferee, Color.White, 0, _teamRefereePos - Vector2.UnitY * 60, Position.CENTER, Vector2.One / 4);
                batch.CenterStringXY(Static.FontMain, $"{_match.TeamReferee.TeamName}", _teamRefereePos + Vector2.One * 6, Color.Black);
                batch.CenterStringXY(Static.FontMain, $"{_match.TeamReferee.TeamName}", _teamRefereePos, Color.White);

            }

            if (indexLayer == (int)Layers.Debug)
            {
                //batch.CenterStringXY(Static.FontMain, $"{State.CurState}", AbsRectF.BottomCenter + Vector2.UnitY * 20, Color.Green);
            }

            return base.Draw(batch, gameTime, indexLayer);
        }

        private void DrawVBall(SpriteBatch batch)
        {
            batch.Draw(Static.TexVBall, Color.Black * .5f, _rotation, (_serviceSide ? AbsRectF.LeftMiddle : AbsRectF.RightMiddle) + Vector2.One * 16, Mugen.Physics.Position.CENTER, Vector2.One / 2);
            batch.Draw(Static.TexVBall, Color.White, _rotation, _serviceSide ? AbsRectF.LeftMiddle : AbsRectF.RightMiddle, Mugen.Physics.Position.CENTER, Vector2.One / 2);
        }
    }
}
