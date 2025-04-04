using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mugen.Core;
using Mugen.GFX;
using Mugen.GUI;
using System.Text.RegularExpressions;

namespace VolleyBallTournament
{
    public class ScorePanel : Node
    {
        public Match Match;
        public Court Court;

        public Team TeamA;
        public Team TeamB;

        public int NbSetToWin = 2;

        public int SetA;
        public int SetB;
        public int ScoreB;
        public int ScoreA;

        Container _div;

        public Vector2 SetAPos;
        public Vector2 SetBPos;

        public Vector2 ScoreAPos;
        public Vector2 ScoreBPos;

        public Vector2 TeamAPos;
        public Vector2 TeamBPos;

        public ScorePanel(Team teamA, Team teamB)
        {
            TeamA = teamA;
            TeamB = teamB;

            teamA.ScorePanel = this;
            teamB.ScorePanel = this;

            _div = new Container(Style.Space.One * 10, Style.Space.One * 10, Mugen.Physics.Position.HORIZONTAL);

            SetSize(560, 120);
        }
        public void AddPointA(int points = 1)
        {
            if (Court.State.CurState == Court.States.Play)
            {
                if (points == 0) return;

                if (points > 0)
                {
                    if (!Court.ServiceSide)
                        Court.ChangeServiceSide();
                    else
                        Court.SetServiceSideA();
                }

                if (points < 0) 
                    Court.CancelChangeServiceSide();

                TeamA.AddPoint(points);
                new PopInfo(points > 0 ? $"+{points}" : $"{points}", points > 0 ? Color.GreenYellow : Color.Red, Color.Black, 0, 16, 32).SetPosition(ScoreAPos - Vector2.UnitY * 64).AppendTo(Match._parent);
                new FxExplose(ScoreAPos, points > 0 ? Color.GreenYellow : Color.Red, 20, 20, 80).AppendTo(Match._parent);

                Static.SoundPoint.Play(.25f, .1f, 0f);
            }
            else
            {
                Court.SetServiceSideA();
            }
        }
        public void AddPointB(int points = 1)
        {
            if (Court.State.CurState == Court.States.Play)
            {
                if (points == 0) return;

                if (points > 0)
                {
                    if (Court.ServiceSide)
                        Court.ChangeServiceSide();
                    else
                        Court.SetServiceSideB();
                }

                if (points < 0) 
                    Court.CancelChangeServiceSide();

                TeamB.AddPoint(points);
                new PopInfo(points > 0 ? $"+{points}" : $"{points}", points > 0 ? Color.GreenYellow : Color.Red, Color.Black, 0, 16, 32).SetPosition(ScoreBPos - Vector2.UnitY * 64).AppendTo(Match._parent);
                new FxExplose(ScoreBPos, points > 0 ? Color.GreenYellow : Color.Red, 20, 20, 80).AppendTo(Match._parent);

                Static.SoundPoint.Play(.25f, .1f, 0f);
            }
            else
            {
                Court.SetServiceSideB();
            }
        }

        public void SetNbSetToWin(int nbSetToWin)
        {
            NbSetToWin = nbSetToWin;
        }
        public override Node Update(GameTime gameTime)
        {
            ScoreA = TeamA.Score;
            ScoreB = TeamB.Score;

            SetA = TeamA.Set;
            SetB = TeamB.Set;

            SetAPos = AbsRectF.Center - Vector2.UnitX * 20;
            SetBPos = AbsRectF.Center + Vector2.UnitX * 20;

            ScoreAPos = AbsRectF.Center - Vector2.UnitX * 80;
            ScoreBPos = AbsRectF.Center + Vector2.UnitX * 80;

            TeamAPos = AbsRectF.TopLeft - Vector2.UnitY * 10;
            TeamBPos = AbsRectF.TopRight - Vector2.UnitY * 10;

            return base.Update(gameTime);
        }
        public override Node Draw(SpriteBatch batch, GameTime gameTime, int indexLayer)
        {
            if (indexLayer == (int)Layers.Main)
            {
                batch.FillRectangle(AbsRectF, Color.DarkSlateBlue * .75f);
                //batch.Rectangle(AbsRectF, Color.Gray, 3f);
                batch.Line(AbsRectF.TopCenter, AbsRectF.BottomCenter, Color.Black, 3f);

                batch.LeftMiddleString(Static.FontMain, TeamA.TeamName, TeamAPos + Vector2.One * 6, Color.Black);
                batch.RightMiddleString(Static.FontMain, TeamB.TeamName, TeamBPos + Vector2.One * 6, Color.Black);

                batch.LeftMiddleString(Static.FontMain, TeamA.TeamName, TeamAPos, Color.White);
                batch.RightMiddleString(Static.FontMain, TeamB.TeamName, TeamBPos , Color.White);

                //batch.LeftMiddleBorderedString(Static.FontMain, _teamA.Group.GroupName, AbsRectF.LeftMiddle - Vector2.UnitX * 20, Color.White, Color.Black);
                //batch.RightMiddleBorderedString(Static.FontMain, _teamB.Group.GroupName, AbsRectF.RightMiddle + Vector2.UnitX * 20, Color.White, Color.Black);

                if (NbSetToWin > 1)
                {
                    batch.CenterBorderedStringXY(Static.FontMain, SetA.ToString(), SetAPos, Color.Cyan, Color.Black);
                    batch.CenterBorderedStringXY(Static.FontMain, SetB.ToString(), SetBPos, Color.Cyan, Color.Black);
                }

                batch.CenterBorderedStringXY(Static.FontMain2, ScoreA.ToString(), ScoreAPos, Color.Gold, Color.Black);
                batch.CenterBorderedStringXY(Static.FontMain2, ScoreB.ToString(), ScoreBPos, Color.Gold, Color.Black);

                batch.CenterBorderedStringXY(Static.FontMain, "VS", AbsRectF.TopCenter + Vector2.One * 6, Color.Black, Color.Black);
                batch.CenterBorderedStringXY(Static.FontMain, "VS", AbsRectF.TopCenter, Color.Gold, Color.Black);
            }

            return base.Draw(batch, gameTime, indexLayer);
        }
    }
}
