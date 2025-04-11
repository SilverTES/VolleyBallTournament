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
        Container _div;

        private Match _match;

        public Vector2 SetAPos;
        public Vector2 SetBPos;
        public Vector2 ScoreAPos;
        public Vector2 ScoreBPos;
        public Vector2 TeamAPos;
        public Vector2 TeamBPos;

        public ScorePanel(Match match)
        {
            //SetMatch(match);

            _div = new Container(Style.Space.One * 10, Style.Space.One * 10, Mugen.Physics.Position.HORIZONTAL);

            SetSize(560, 120);
            SetVisible(false);
        }
        //public void SetMatch(Match match)
        //{
        //    ResetScore();
        //    _match = match;
        //}
        //public void ResetScore()
        //{
        //    if (_match == null) return;

        //    _match.TeamA.SetScoreSet(0);
        //    _match.TeamB.SetScoreSet(0);
        //    _match.TeamA.SetScorePoint(0);
        //    _match.TeamB.SetScorePoint(0);
            
        //}
        public override Node Update(GameTime gameTime)
        {
            UpdateRect();

            //ScoreA = TeamA.ScorePoint;
            //ScoreB = TeamB.ScorePoint;

            //SetA = TeamA.ScoreSet;
            //SetB = TeamB.ScoreSet;

            SetAPos = AbsRectF.Center - Vector2.UnitX * 20;
            SetBPos = AbsRectF.Center + Vector2.UnitX * 20;

            ScoreAPos = AbsRectF.Center - Vector2.UnitX * 120;
            ScoreBPos = AbsRectF.Center + Vector2.UnitX * 120;

            TeamAPos = AbsRectF.TopLeft - Vector2.UnitY * 10;
            TeamBPos = AbsRectF.TopRight - Vector2.UnitY * 10;

            

            return base.Update(gameTime);
        }
        public override Node Draw(SpriteBatch batch, GameTime gameTime, int indexLayer)
        {
            if (indexLayer == (int)Layers.Main)
            {
                batch.FillRectangle(AbsRectF, Color.DarkSlateBlue * .75f);
                batch.Rectangle(AbsRectF, Color.Gray, 1f);
                batch.Line(AbsRectF.TopCenter, AbsRectF.BottomCenter, Color.Black, 3f);

                batch.LeftMiddleString(Static.FontMain, _match.TeamA.TeamName, TeamAPos + Vector2.One * 6, Color.Black);
                batch.LeftMiddleString(Static.FontMain, _match.TeamA.TeamName, TeamAPos, Color.GreenYellow);

                batch.RightMiddleString(Static.FontMain, _match.TeamB.TeamName, TeamBPos + Vector2.One * 6, Color.Black);
                batch.RightMiddleString(Static.FontMain, _match.TeamB.TeamName, TeamBPos , Color.GreenYellow);

                //batch.LeftMiddleBorderedString(Static.FontMain, _teamA.Group.GroupName, AbsRectF.LeftMiddle - Vector2.UnitX * 20, Color.White, Color.Black);
                //batch.RightMiddleBorderedString(Static.FontMain, _teamB.Group.GroupName, AbsRectF.RightMiddle + Vector2.UnitX * 20, Color.White, Color.Black);

                if (_match.NbSetToWin > 1)
                {
                    batch.CenterBorderedStringXY(Static.FontMain, _match.TeamA.ScoreSet.ToString(), SetAPos, Color.Cyan, Color.Black);
                    batch.CenterBorderedStringXY(Static.FontMain, _match.TeamB.ScoreSet.ToString(), SetBPos, Color.Cyan, Color.Black);
                }

                batch.CenterBorderedStringXY(Static.FontMain3, _match.TeamA.ScorePoint.ToString(), ScoreAPos, Color.Gold, Color.Black);
                batch.CenterBorderedStringXY(Static.FontMain3, _match.TeamB.ScorePoint.ToString(), ScoreBPos, Color.Gold, Color.Black);

                batch.CenterBorderedStringXY(Static.FontMain, "VS", AbsRectF.TopCenter + Vector2.One * 6, Color.Black, Color.Black);
                batch.CenterBorderedStringXY(Static.FontMain, "VS", AbsRectF.TopCenter, Color.Gold, Color.Black);
            }

            if (indexLayer == (int)Layers.Debug)
            {
                string result = "Match Nul";
                var winner = _match.GetWinner();
                if (winner != null)
                    result = $"Vainqueur {winner.TeamName}";

                //batch.CenterStringXY(Static.FontMini, result, AbsRectF.TopCenter - Vector2.UnitY * 40, Color.ForestGreen);
            }

            return base.Draw(batch, gameTime, indexLayer);
        }
    }
}
