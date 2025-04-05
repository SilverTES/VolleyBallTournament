using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mugen.Core;
using Mugen.GFX;
using Mugen.Physics;
using System;
using System.Collections.Generic;

namespace VolleyBallTournament
{
    public class Court : Node
    {
        public enum States
        {
            WarmUp,
            Ready,
            Play,
            Pause,
            Finish,
            LastPoint,
        }
        public State<States> State { get; private set; } = new State<States>(States.Ready);

        public Dictionary<States, string> _infos = new Dictionary<States, string>()
        {
            {States.WarmUp, "Echauffement"},
            {States.Ready, "Prêt a jouer"},
            {States.Play, "Match en cours"},
            {States.Pause, "En attente"},
            {States.Finish, "Fin du match"},
            {States.LastPoint, "Dernière balle"},

        };

        public Team TeamReferee;
        public Vector2 TeamRefereePos;

        string _courtName;

        float _ticWave = 0;
        float _waveValue = 0;

        float _rotation = 0f;

        public bool PrevServiceSide = true;
        public bool ServiceSide = true; // true A , false B
        public Court(string courtName, Team teamReferee) 
        {
            _courtName = courtName;
            TeamReferee = teamReferee;

            SetSize(480, 240);

            State.Set(States.WarmUp);

            _rotation = (float)Misc.Rng.NextDouble() * Geo.RAD_360;
        }
        public void SetServiceSideA()
        {
            PrevServiceSide = ServiceSide;
            ServiceSide = true;
        }
        public void SetServiceSideB()
        {
            PrevServiceSide = ServiceSide;
            ServiceSide = false;
        }

        public void ChangeServiceSide()
        {
            PrevServiceSide = ServiceSide;
            ServiceSide = !ServiceSide;
        }
        public void CancelChangeServiceSide()
        {
            ServiceSide = PrevServiceSide;
        }
        public override Node Update(GameTime gameTime)
        {
            UpdateRect();

            _ticWave += 0.1f;
            _waveValue = MathF.Sin(_ticWave) * 4f;

            if (State.CurState == States.Play)
                _rotation += .05f;

            switch (State.CurState)
            {
                case States.Ready:

                    break;
                case States.Pause:

                    break;
                case States.Finish:

                    break;
                case States.LastPoint:

                    break;
                case States.WarmUp:
                    break;

                case States.Play:


                    break;

                default:
                    break;
            }

            TeamRefereePos = AbsRectF.TopCenter + Vector2.UnitY * 30;

            return base.Update(gameTime);
        }
        public override Node Draw(SpriteBatch batch, GameTime gameTime, int indexLayer)
        {
            if (indexLayer == (int)Layers.Main)
            {
                Color color = Color.White * .25f;

                batch.FillRectangle(AbsRectF.Extend(8f), Color.MonoGameOrange * .25f);
                batch.Rectangle(AbsRectF, color, 3f);

                batch.Line(AbsRectF.TopCenter, AbsRectF.BottomCenter, color, 3f);

                var threeMeter = Vector2.UnitX * _rect.Width / 6;

                batch.Line(AbsRectF.TopCenter - threeMeter, AbsRectF.BottomCenter - threeMeter, color, 3f);
                batch.Line(AbsRectF.TopCenter + threeMeter, AbsRectF.BottomCenter + threeMeter, color, 3f);

                //if (State.CurState == States.Play)
                    DrawVBall(batch);


                batch.CenterStringXY(Static.FontMain, $"Terrain {_courtName}", AbsRectF.BottomCenter - Vector2.UnitY * 20, Color.Yellow);

                batch.CenterStringXY(Static.FontMain, $"{_infos[State.CurState]}", AbsRectF.Center + Vector2.UnitY * _waveValue + Vector2.One * 6, Color.Black);
                batch.CenterStringXY(Static.FontMain, $"{_infos[State.CurState]}", AbsRectF.Center + Vector2.UnitY * _waveValue, Color.Cyan);


                batch.Draw(Static.TexReferee, Color.Black, 0, TeamRefereePos - Vector2.UnitY * 60 + Vector2.One * 6, Position.CENTER, Vector2.One / 4);
                batch.Draw(Static.TexReferee, Color.White, 0, TeamRefereePos - Vector2.UnitY * 60, Position.CENTER, Vector2.One / 4);
                batch.CenterStringXY(Static.FontMain, $"{TeamReferee.TeamName}", TeamRefereePos + Vector2.One * 6, Color.Black);
                batch.CenterStringXY(Static.FontMain, $"{TeamReferee.TeamName}", TeamRefereePos, Color.White);

            }

            if (indexLayer == (int)Layers.Debug)
            {
                //batch.CenterStringXY(Static.FontMain, $"{State.CurState}", AbsRectF.BottomCenter + Vector2.UnitY * 20, Color.Green);
            }

            return base.Draw(batch, gameTime, indexLayer);
        }

        private void DrawVBall(SpriteBatch batch)
        {
            batch.Draw(Static.TexVBall, Color.Black * .5f, _rotation, (ServiceSide ? AbsRectF.LeftMiddle : AbsRectF.RightMiddle) + Vector2.One * 16, Mugen.Physics.Position.CENTER, Vector2.One / 2);
            batch.Draw(Static.TexVBall, Color.White, _rotation, ServiceSide ? AbsRectF.LeftMiddle : AbsRectF.RightMiddle, Mugen.Physics.Position.CENTER, Vector2.One / 2);
        }
    }
}
