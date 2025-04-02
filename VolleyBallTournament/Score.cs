using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mugen.Core;
using Mugen.GFX;
using Mugen.GUI;

namespace VolleyBallTournament
{
    internal class Score : Node
    {
        public Team TeamA => _teamA;
        Team _teamA;
        public Team TeamB => _teamB;
        Team _teamB;

        public int NbSetToWin => _nbSetToWin;
        int _nbSetToWin = 2;

        public int SetA => _setA;
        int _setA = 0;
        public int SetB => _setB;
        int _setB = 0;
        public int ScoreB => _scoreB;
        int _scoreA = 0;
        public int ScoreA => _scoreA;
        int _scoreB = 0;

        Container _div;

        public Vector2 SetAPos;
        public Vector2 SetBPos;

        public Vector2 ScoreAPos;
        public Vector2 ScoreBPos;

        public Score(Team teamA, Team teamB) 
        {
            _teamA = teamA;
            _teamB = teamB;

            _div = new Container(Style.Space.One * 10, Style.Space.One * 10, Mugen.Physics.Position.HORIZONTAL);

            SetSize(480, 80);
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

                batch.LeftMiddleString(Static.FontMain, _teamA.TeamName, AbsRectF.LeftMiddle - Vector2.UnitY * 48 + Vector2.One * 6, Color.Black);
                batch.RightMiddleString(Static.FontMain, _teamB.TeamName, AbsRectF.RightMiddle - Vector2.UnitY * 48 + Vector2.One * 6, Color.Black);

                batch.LeftMiddleString(Static.FontMain, _teamA.TeamName, AbsRectF.LeftMiddle - Vector2.UnitY * 48, Color.White);
                batch.RightMiddleString(Static.FontMain, _teamB.TeamName, AbsRectF.RightMiddle - Vector2.UnitY * 48 , Color.White);

                //batch.LeftMiddleBorderedString(Static.FontMain, _teamA.Group.GroupName, AbsRectF.LeftMiddle - Vector2.UnitX * 20, Color.White, Color.Black);
                //batch.RightMiddleBorderedString(Static.FontMain, _teamB.Group.GroupName, AbsRectF.RightMiddle + Vector2.UnitX * 20, Color.White, Color.Black);

                if (_nbSetToWin > 1)
                {
                    batch.CenterBorderedStringXY(Static.FontMain, _setA.ToString(), SetAPos, Color.Cyan, Color.Black);
                    batch.CenterBorderedStringXY(Static.FontMain, _setB.ToString(), SetBPos, Color.Cyan, Color.Black);
                }

                batch.CenterBorderedStringXY(Static.FontMain2, _scoreA.ToString(), ScoreAPos, Color.Gold, Color.Black);
                batch.CenterBorderedStringXY(Static.FontMain2, _scoreB.ToString(), ScoreBPos, Color.Gold, Color.Black);

                batch.CenterBorderedStringXY(Static.FontMain, "VS", AbsRectF.TopCenter + Vector2.One * 6, Color.Black, Color.Black);
                batch.CenterBorderedStringXY(Static.FontMain, "VS", AbsRectF.TopCenter, Color.Gold, Color.Black);
            }

            return base.Draw(batch, gameTime, indexLayer);
        }
    }
}
