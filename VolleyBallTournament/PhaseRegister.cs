using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Mugen.Core;
using Mugen.GFX;
using Mugen.Input;

namespace VolleyBallTournament
{
    public class PhaseRegister : Node   
    {
        private KeyboardState _key;
        public PhaseRegister(Game game) 
        {
            SetSize(Screen.Width, Screen.Height);

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    new TextBox(game, new Rectangle(40 + i * 480, 200 + j * 80, 320, 64), Static.FontMain, Color.Black * .5f, Color.Yellow, Color.Gold, 50).AppendTo(this);
                }
            }

        }
        public void FocusNextTextBox()
        {   
            var textBoxs = GroupOf<TextBox>();
            // search focused textBox
            for (int i = 0;i < textBoxs.Count;i++)
            {
                var textBox = textBoxs[i];
                    
                if (textBox.IsFocus)
                {
                    if (i < textBoxs.Count - 1)
                    {
                        textBox.SetFocus(false);
                        textBoxs[i + 1].SetFocus(true);
                    }
                    else
                    {
                        textBox.SetFocus(false);
                        textBoxs[0].SetFocus(true);
                    }
                    break;
                }
            }
        }
        public override Node Update(GameTime gameTime)
        {
            _key = Static.Key;
            UpdateRect();

            if (ButtonControl.OnePress("Tab", _key.IsKeyDown(Keys.Tab)))
            {
                FocusNextTextBox();
            }

            UpdateChilds(gameTime);
            return base.Update(gameTime);
        }
        public override Node Draw(SpriteBatch batch, GameTime gameTime, int indexLayer)
        {
            if (indexLayer == (int)Layers.Main)
            {
                batch.FillRectangle(AbsRectF, Color.DarkSlateBlue * .5f);

                batch.Grid(AbsXY + Vector2.Zero, Screen.Width, Screen.Height, 40, 40, Color.Black * .5f);
            }

            DrawChilds(batch, gameTime, indexLayer);
            return base.Draw(batch, gameTime, indexLayer);
        }
    }
}
