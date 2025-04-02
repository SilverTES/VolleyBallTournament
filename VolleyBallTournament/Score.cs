using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mugen.Core;
using Mugen.GFX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VolleyBallTournament
{
    internal class Score : Node
    {
        public Score() 
        {
            SetSize(480, 60);
        }
        public override Node Update(GameTime gameTime)
        {
            return base.Update(gameTime);
        }
        public override Node Draw(SpriteBatch batch, GameTime gameTime, int indexLayer)
        {
            if (indexLayer == (int)Layers.Main)
            {
                batch.Rectangle(AbsRectF, Color.White, 3f);
            }

            return base.Draw(batch, gameTime, indexLayer);
        }
    }
}
