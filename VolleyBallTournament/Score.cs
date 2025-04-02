using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mugen.Core;
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
            SetSize(480, 240);
        }
        public override Node Update(GameTime gameTime)
        {
            return base.Update(gameTime);
        }
        public override Node Draw(SpriteBatch batch, GameTime gameTime, int indexLayer)
        {
            return base.Draw(batch, gameTime, indexLayer);
        }
    }
}
