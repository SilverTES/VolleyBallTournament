using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Mugen.Core;
using Mugen.GFX;
using Mugen.GUI;
using Mugen.Input;
using System;
using System.Collections.Generic;

namespace VolleyBallTournament
{
    public class GroupRegister : Node
    {
        int _groupId = 0;
        Container _div;
        public GroupRegister(Game game, int nbTeam, int groupId)
        {
            _groupId = groupId;
            _div = new Container(Style.Space.One * 20, new Style.Space(30, 20, 10, 10), Mugen.Physics.Position.VERTICAL);

            for (int i = 0; i < nbTeam; i++)
            {
                var textBox = new TextBox(game, new Rectangle(0, 0, 320, 64), Static.FontMain, Color.Black * .75f, Color.Yellow, Color.Gold, 50).AppendTo(this).This<TextBox>();
                textBox.SetTitle($"Equipe {i + 1}", Color.White, Static.FontMini);
                textBox.SetId(_groupId * nbTeam + i);

                _div.Insert(textBox);
            }
            _div.Refresh();
            SetSize(_div.Rect.Width, _div.Rect.Height);
        }
        public override Node Update(GameTime gameTime)
        {
            //UpdateRect();
            _div.SetPosition(XY);

            UpdateChilds(gameTime);

            return base.Update(gameTime);   
        }
        public override Node Draw(SpriteBatch batch, GameTime gameTime, int indexLayer)
        {
            if (indexLayer == (int)Layers.Main)
            {
                batch.FillRectangle(AbsRectF, Color.DarkSlateBlue * .5f);

                batch.Rectangle(AbsRectF, Color.DarkSlateBlue * 1f);

                batch.CenterStringXY(Static.FontMain, $"Groupe {_groupId + 1}", AbsRectF.TopCenter - Vector2.UnitY * 20 + Vector2.One * 6, Color.Black * .5f);
                batch.CenterStringXY(Static.FontMain, $"Groupe {_groupId + 1}", AbsRectF.TopCenter - Vector2.UnitY * 20, Color.White);
            }

            if (indexLayer == (int)Layers.Debug)
            {
                //_div.DrawDebug(batch, Color.Red, 3f);
            }

            DrawChilds(batch, gameTime, indexLayer);

            return base.Draw(batch, gameTime, indexLayer);
        }
    }

    public class PhaseRegister : Node   
    {
        public bool IsPaused = false;
        private string _title = "Enregistrement des équipes";
        private KeyboardState _key;

        Container _div;

        List<GroupRegister> _groupRegister = [];

        public int NbGroup => _nbGroup;
        private int _nbGroup;
        public int NbTeamPerGroup => _nbTeamPerGroup;
        private int _nbTeamPerGroup;
        public int NbMatch => _nbMatch;
        private int _nbMatch;

        public Sequence Sequence => _sequence;
        private Sequence _sequence;

        public PhaseRegister(Game game, int nbGroup, int nbTeamPerGroup, int nbMatch, Sequence sequence)
        {
            _nbGroup = nbGroup;
            _nbTeamPerGroup = nbTeamPerGroup;
            _nbMatch = nbMatch;
            _sequence = sequence;

            SetSize(Screen.Width, Screen.Height);

            _div = new Container(Style.Space.One * 20, Style.Space.One * 20);

            for (int i = 0; i < nbGroup; i++)
            {
                var groupRegister = new GroupRegister(game, nbTeamPerGroup, i).AppendTo(this).This<GroupRegister>();
                _div.Insert(groupRegister);
                _groupRegister.Add(groupRegister);
            }

            _div.SetPosition((Screen.Width - _div.Rect.Width) / 2, (Screen.Height - _div.Rect.Height) / 2);
            _div.Refresh();
        }
        public List<TextBox> GetTexBoxs()
        {
            var texBoxs = new List<TextBox>();

            for (int i = 0; i < _groupRegister.Count; i++)
            {
                var textBoxs = _groupRegister[i].GroupOf<TextBox>();

                for (int t = 0; t < textBoxs.Count; t++)
                {
                    var textBox = textBoxs[t];
                    texBoxs.Add(textBox);
                }
            }

            return texBoxs;
        }
        public void FocusNextTextBox()
        {   
            var textBoxs = GetTexBoxs();

            if (textBoxs.Count == 0) return;

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
            var textBoxs = GetTexBoxs();

            if (textBoxs.Count == 0) return;

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
            else
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
                //for (int i = 0; i < 4; i++)
                //{
                //    batch.FillRectangle(AbsXY + new Vector2(i * 480 + 40, 280), new Vector2(400, 600), Color.DarkSlateBlue * .5f);

                //    batch.CenterStringXY(Static.FontMain, $"Groupe {i+1}", AbsXY + new Vector2(i * 480 + 40 + 200, 280) + Vector2.One * 6, Color.Black *.5f);
                //    batch.CenterStringXY(Static.FontMain, $"Groupe {i+1}", AbsXY + new Vector2(i * 480 + 40 + 200, 280), Color.White);
                //}

                batch.LeftTopString(Static.FontMain, _title, AbsRectF.TopLeft + Vector2.UnitX * 40 + Vector2.One * 6, Color.Black);
                batch.LeftTopString(Static.FontMain, _title, AbsRectF.TopLeft + Vector2.UnitX * 40, Color.White);
            }

            DrawChilds(batch, gameTime, indexLayer);
            return base.Draw(batch, gameTime, indexLayer);
        }
    }
}
