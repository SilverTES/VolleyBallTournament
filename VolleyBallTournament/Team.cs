using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mugen.Core;
using Mugen.GFX;
using System;


namespace VolleyBallTournament
{
    internal class Team : Node  
    {
        int _rank = 0;
        int _pointTotal = 0;
        int _nbWin = 1;
        int _nbMatch = 3;
        string _teamName;
        public string TeamName => _teamName;

        public Team(string teamName)
        {
            SetSize(360, 64);
            _teamName = teamName;
        }
        public void SetRank(int rank)
        { 
            _rank = rank; 
        }
        public void SetTeamName(string teamName)
        { 
            _teamName = teamName; 
        }
        public override Node Update(GameTime gameTime)
        {
            UpdateRect();

            return base.Update(gameTime);
        }
        public override Node Draw(SpriteBatch batch, GameTime gameTime, int indexLayer)
        {
            if (indexLayer == (int)Layers.Main)
            {
                batch.FillRectangle(AbsRectF, Color.Black * .5f);
                batch.Rectangle(AbsRectF, Color.Gray, 1f);


                batch.LeftMiddleBorderedString(Static.FontMain, $"{_teamName}", AbsRectF.LeftMiddle + Vector2.UnitX * 20, Color.White, Color.Black);
                batch.CenterBorderedStringXY(Static.FontMain2, $"{_rank}", AbsRectF.LeftMiddle - Vector2.UnitX * 20, Color.Orange, Color.Black);

                for (int i = 0; i < _nbMatch; i++)
                {
                    var pos = new Vector2(AbsRectF.RightMiddle.X - i * 24 - 20, AbsRectF.Center.Y);
                    
                    batch.Circle(pos, 10, 16, Color.White, 2f);
                    
                    if ( i < _nbWin)
                        batch.Point(pos, 10, Color.Gold);
                }
            }   

            return base.Draw(batch, gameTime, indexLayer);
        }
    }
}
