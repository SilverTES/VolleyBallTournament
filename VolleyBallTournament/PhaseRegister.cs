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
        public bool IsPaused = false;
        private string _title = "Enregistrement des équipes";
        private KeyboardState _key;
        public PhaseRegister(Game game) 
        {
            SetSize(Screen.Width, Screen.Height);

            int index = 0;
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    var textBox = new TextBox(game, new Rectangle(80 + i * 480, 400 + j * 120, 320, 64), Static.FontMain, Color.Black * .75f, Color.Yellow, Color.Gold, 50).AppendTo(this).This<TextBox>();
                    textBox.SetTitle($"Equipe {index+1}", Color.White, Static.FontMini);
                    textBox.SetId(index);

                    //textBox.OnChange += (tb) => 
                    //{ 
                    //    Misc.Log($"{tb.Id} : {tb.Title} : {tb.Text}"); 
                    //};
                    
                    if (index == 0)
                        textBox.SetFocus(true);

                    index++;
                }
            }

        }
        public void FocusNextTextBox()
        {   
            var textBoxs = GroupOf<TextBox>();
            bool oneFocus = false;
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
                    oneFocus = true;
                    break;
                }
            }
            if (!oneFocus)
                textBoxs[0].SetFocus(true);
        }
        public void FocusPrevTextBox()
        {
            var textBoxs = GroupOf<TextBox>();
            bool oneFocus = false;
            // search focused textBox
            for (int i = 0; i < textBoxs.Count; i++)
            {
                var textBox = textBoxs[i];

                if (textBox.IsFocus)
                {
                    if (i > 0)
                    {
                        textBox.SetFocus(false);
                        textBoxs[i - 1].SetFocus(true);
                    }
                    else
                    {
                        textBox.SetFocus(false);
                        textBoxs[textBoxs.Count-1].SetFocus(true);
                    }
                    oneFocus = true;
                    break;
                }
            }
            if (!oneFocus)
                textBoxs[0].SetFocus(true);
        }
        public override Node Update(GameTime gameTime)
        {
            UpdateRect();

            if (IsPaused)
            {
                var textBoxs = GroupOf<TextBox>();
                for (int i = 0; i < textBoxs.Count; i++)
                {
                    var textBox = textBoxs[i];
                    textBox.SetFocus(false);
                }
            }

            if (!IsPaused)
            {
                _key = Static.Key;

                if (ButtonControl.OnePress("Tab", _key.IsKeyDown(Keys.Tab))) FocusNextTextBox();
                if (ButtonControl.OnePress("Up", _key.IsKeyDown(Keys.Up))) FocusPrevTextBox();
                if (ButtonControl.OnePress("Down", _key.IsKeyDown(Keys.Down))) FocusNextTextBox();
                if (ButtonControl.OnePress("Enter", _key.IsKeyDown(Keys.Enter))) FocusNextTextBox();

            }

            UpdateChilds(gameTime);

            return base.Update(gameTime);
        }
        public override Node Draw(SpriteBatch batch, GameTime gameTime, int indexLayer)
        {
            if (indexLayer == (int)Layers.Main)
            {
                batch.Draw(Static.TexBG00, AbsRect, Color.White);
                batch.FillRectangle(AbsRectF, Color.Black * .25f);

                //batch.Grid(AbsXY + Vector2.Zero, Screen.Width, Screen.Height, 40, 40, Color.Black * .5f);
                for (int i = 0; i < 4; i++)
                {
                    batch.FillRectangle(AbsXY + new Vector2(i * 480 + 40, 280), new Vector2(400, 600), Color.DarkSlateBlue * .5f);

                    batch.CenterStringXY(Static.FontMain, $"Groupe {i+1}", AbsXY + new Vector2(i * 480 + 40 + 200, 280) + Vector2.One * 6, Color.Black *.5f);
                    batch.CenterStringXY(Static.FontMain, $"Groupe {i+1}", AbsXY + new Vector2(i * 480 + 40 + 200, 280), Color.White);
                }

                batch.LeftTopString(Static.FontMain, _title, AbsRectF.TopLeft + Vector2.UnitX * 40 + Vector2.One * 6, Color.Black);
                batch.LeftTopString(Static.FontMain, _title, AbsRectF.TopLeft + Vector2.UnitX * 40, Color.White);
            }

            DrawChilds(batch, gameTime, indexLayer);
            return base.Draw(batch, gameTime, indexLayer);
        }
    }
}
