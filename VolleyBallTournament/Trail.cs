using Microsoft.Xna.Framework.Graphics.PackedVector;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Mugen.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mugen.Physics;
using Mugen.GFX;

namespace VolleyBallTournament
{
    public class Trail : Node
    {
        Color _color;
        Vector2 _scale;
        float _stepAlpha;
        public Trail(RectangleF rectF, Vector2 scale, float stepAplha = 0.5f, Color color = default)
        {
            _x = rectF.X;
            _y = rectF.Y;

            _rect = rectF;
            _scale = scale;
            _color = color;
            _stepAlpha = stepAplha;
        }

        public override Node Update(GameTime gameTime)
        {
            UpdateRect();

            _alpha += -_stepAlpha;

            if (_alpha <= 0f)
                KillMe();
            return base.Update(gameTime);
        }

        public override Node Draw(SpriteBatch batch, GameTime gameTime, int indexLayer)
        {

            if (indexLayer == (int)Layers.BackFX)
            {
                batch.FillRectangle(_rect, _color * _alpha);
            }


            return base.Draw(batch, gameTime, indexLayer);
        }
    }
}
