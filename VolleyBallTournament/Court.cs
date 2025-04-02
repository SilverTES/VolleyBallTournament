using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mugen.Core;
using Mugen.GFX;

namespace VolleyBallTournament
{
    internal class Court : Node
    {
        string _courtName;
        string _info = "Set en cours";
        public Court(string courtName) 
        {
            _courtName = courtName;
            SetSize(240, 320);
        }
        public override Node Update(GameTime gameTime)
        {

            return base.Update(gameTime);
        }
        public override Node Draw(SpriteBatch batch, GameTime gameTime, int indexLayer)
        {
            if (indexLayer == (int)Layers.Main)
            {
                batch.FillRectangle(AbsRectF, Color.MonoGameOrange * .25f);
                batch.Rectangle(AbsRectF, Color.White * .25f, 3f);

                batch.Line(AbsRectF.LeftMiddle, AbsRectF.RightMiddle, Color.White *.25f, 3f);

                var threeMeter = Vector2.UnitY * 50;

                batch.Line(AbsRectF.LeftMiddle - threeMeter, AbsRectF.RightMiddle - threeMeter, Color.White * .25f, 3f);
                batch.Line(AbsRectF.LeftMiddle + threeMeter, AbsRectF.RightMiddle + threeMeter, Color.White * .25f, 3f);

                batch.CenterBorderedStringXY(Static.FontMain, $"{_courtName}", AbsRectF.Center - Vector2.UnitY * 20, Color.Yellow, Color.Black);
                batch.CenterBorderedStringXY(Static.FontMain, $"{_info}", AbsRectF.Center + Vector2.UnitY * 20, Color.Cyan, Color.Black);
            }

            return base.Draw(batch, gameTime, indexLayer);
        }
    }
}
