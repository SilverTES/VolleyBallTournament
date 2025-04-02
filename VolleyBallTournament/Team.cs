using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mugen.Core;
using Mugen.GFX;


namespace VolleyBallTournament
{
    internal class Team : Node  
    {
        int _rank = 0;
        int _pointTotal = 0;
        string _teamName;

        public Team(string teamName)
        {
            SetSize(320, 80);
            _teamName = teamName;
        }
        public void SetRank(int rank)
        { 
            _rank = rank; 
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


                batch.CenterBorderedStringXY(Static.FontMain, $"{_teamName}", AbsRectF.Center, Color.White, Color.Black);
                batch.CenterBorderedStringXY(Static.FontMain, $"{_rank}", AbsRectF.LeftMiddle - Vector2.UnitX * 20, Color.White, Color.Black);
            }   

            return base.Draw(batch, gameTime, indexLayer);
        }
    }
}
