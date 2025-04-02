using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mugen.Core;
using Mugen.GFX;
using Mugen.GUI;

namespace VolleyBallTournament
{
    internal class Score : Node
    {
        Team _teamA;
        Team _teamB;


        int _nbSetToWin = 2;
        int _setA = 0;
        int _setB = 0;
        int _scoreA = 0;
        int _scoreB = 0;

        Container _div;

        public Score(Team teamA, Team teamB) 
        {
            _teamA = teamA;
            _teamB = teamB;

            _div = new Container(Style.Space.One * 10, Style.Space.One * 10, Mugen.Physics.Position.HORIZONTAL);

            SetSize(580, 80);
        }
        public void SetNbSetToWin(int nbSetToWin)
        {
            _nbSetToWin = nbSetToWin;
        }
        public override Node Update(GameTime gameTime)
        {
            _scoreA = _teamA.Score;
            _scoreB = _teamB.Score;

            _setA = _teamA.Set;
            _setB = _teamB.Set;

            return base.Update(gameTime);
        }
        public override Node Draw(SpriteBatch batch, GameTime gameTime, int indexLayer)
        {
            if (indexLayer == (int)Layers.Main)
            {
                batch.FillRectangle(AbsRectF, Color.DarkSlateBlue * .5f);
                //batch.Rectangle(AbsRectF, Color.Gray, 3f);
                batch.Line(AbsRectF.TopCenter, AbsRectF.BottomCenter, Color.Black, 3f);

                batch.LeftMiddleBorderedString(Static.FontMain, _teamA.TeamName, AbsRectF.LeftMiddle + Vector2.UnitX * 10 , Color.White, Color.Black);
                batch.RightMiddleBorderedString(Static.FontMain, _teamB.TeamName, AbsRectF.RightMiddle - Vector2.UnitX * 10 , Color.White, Color.Black);

                //batch.LeftMiddleBorderedString(Static.FontMain, _teamA.Group.GroupName, AbsRectF.LeftMiddle - Vector2.UnitX * 20, Color.White, Color.Black);
                //batch.RightMiddleBorderedString(Static.FontMain, _teamB.Group.GroupName, AbsRectF.RightMiddle + Vector2.UnitX * 20, Color.White, Color.Black);

                if (_nbSetToWin > 1)
                {
                    batch.CenterBorderedStringXY(Static.FontMain, _setA.ToString(), AbsRectF.Center - Vector2.UnitX * 20, Color.Cyan, Color.Black);
                    batch.CenterBorderedStringXY(Static.FontMain, _setB.ToString(), AbsRectF.Center + Vector2.UnitX * 20, Color.Cyan, Color.Black);
                }

                batch.CenterBorderedStringXY(Static.FontMain2, _scoreA.ToString(), AbsRectF.Center - Vector2.UnitX * 64, Color.Gold, Color.Black);
                batch.CenterBorderedStringXY(Static.FontMain2, _scoreB.ToString(), AbsRectF.Center + Vector2.UnitX * 64, Color.Gold, Color.Black);
            }

            return base.Draw(batch, gameTime, indexLayer);
        }
    }
}
