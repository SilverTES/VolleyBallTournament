using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mugen.Core;
using Mugen.GFX;
using Mugen.GUI;

namespace VolleyBallTournament
{
    public class Match : Node
    {
        public ScorePanel ScorePanel;
        public Court Court;
        private Container _div;
        
        public Match(ScorePanel scorePanel, Court court)
        {
            _div = new Container(Style.Space.One * 10, Style.Space.One * 10, Mugen.Physics.Position.VERTICAL);
            
            var teamA = scorePanel.TeamA;
            var teamB = scorePanel.TeamB;
            
            ScorePanel = (ScorePanel)scorePanel.AppendTo(this);
            ScorePanel.Match = this;

            Court = (Court)court.AppendTo(this);

            ScorePanel.Court = Court;

            teamA.Match = this;
            teamB.Match = this;

            teamA.Court = Court;
            teamB.Court = Court;

            _div.Insert(ScorePanel);
            _div.Insert(Court);
            _div.Refresh();

            SetSize(_div._rect.Width, _div._rect.Height);

        }
        public override Node Update(GameTime gameTime)
        {

            UpdateChilds(gameTime);

            return base.Update(gameTime);
        }
        public override Node Draw(SpriteBatch batch, GameTime gameTime, int indexLayer)
        {
            if (indexLayer == (int)Layers.Main)
            {
                //batch.FillRectangle(AbsRectF, Color.MonoGameOrange * .25f);
                //batch.Rectangle(AbsRectF, Color.White, 3f);

                //batch.Line(AbsRectF.LeftMiddle, AbsRectF.RightMiddle, Color.White, 3f);

                //var threeMeter = Vector2.UnitY * 50;

                //batch.Line(AbsRectF.LeftMiddle - threeMeter, AbsRectF.RightMiddle - threeMeter, Color.White, 1f);
                //batch.Line(AbsRectF.LeftMiddle + threeMeter, AbsRectF.RightMiddle + threeMeter, Color.White, 1f);

                //batch.CenterBorderedStringXY(Static.FontMain, _courtName, AbsRectF.TopCenter - Vector2.UnitY * 20, Color.Yellow, Color.Black);
            }

            DrawChilds(batch, gameTime, indexLayer);

            return base.Draw(batch, gameTime, indexLayer);
        }
    }
}
