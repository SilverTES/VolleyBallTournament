﻿using Microsoft.Xna.Framework;
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

        public ScorePanel(Match match, Team teamA, Team teamB) 
        {
            Match = match;
            TeamA = teamA;
            TeamB = teamB;

            teamA.ScorePanel = this;
            teamB.ScorePanel = this;

            _div = new Container(Style.Space.One * 10, Style.Space.One * 10, Mugen.Physics.Position.HORIZONTAL);

            SetSize(480, 80);
        }
        public void AddPointA(int points = 1)
        {
            if (points == 0) return;

            TeamA.AddPoint(points);
            new PopInfo(points > 0 ? $"+{points}" : $"{points}", points > 0 ? Color.GreenYellow : Color.Red, Color.Black, 0, 16, 32).SetPosition(ScoreAPos - Vector2.UnitY * 64).AppendTo(Match._parent);
            new FxExplose(ScoreAPos, points > 0 ? Color.GreenYellow : Color.Red, 20, 20, 80).AppendTo(Match._parent);
        }
        public void AddPointB(int points = 1)
        {
            if (points == 0) return;

            TeamB.AddPoint(points);
            new PopInfo(points > 0 ? $"+{points}" : $"{points}", points > 0 ? Color.GreenYellow : Color.Red, Color.Black, 0, 16, 32).SetPosition(ScoreBPos - Vector2.UnitY * 64).AppendTo(Match._parent);
            new FxExplose(ScoreBPos, points > 0 ? Color.GreenYellow : Color.Red, 20, 20, 80).AppendTo(Match._parent);
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

            return base.Update(gameTime);
        }
        public override Node Draw(SpriteBatch batch, GameTime gameTime, int indexLayer)
        {
            if (indexLayer == (int)Layers.Main)
            {
                batch.FillRectangle(AbsRectF, Color.DarkSlateBlue * .75f);
                //batch.Rectangle(AbsRectF, Color.Gray, 3f);
                batch.Line(AbsRectF.TopCenter, AbsRectF.BottomCenter, Color.Black, 3f);

                batch.LeftMiddleString(Static.FontMain, TeamA.TeamName, AbsRectF.LeftMiddle - Vector2.UnitY * 48 + Vector2.One * 6, Color.Black);
                batch.RightMiddleString(Static.FontMain, TeamB.TeamName, AbsRectF.RightMiddle - Vector2.UnitY * 48 + Vector2.One * 6, Color.Black);

                batch.LeftMiddleString(Static.FontMain, TeamA.TeamName, AbsRectF.LeftMiddle - Vector2.UnitY * 48, Color.White);
                batch.RightMiddleString(Static.FontMain, TeamB.TeamName, AbsRectF.RightMiddle - Vector2.UnitY * 48 , Color.White);

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
