using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Mugen.Core;
using Mugen.GUI;
using Mugen.GFX;

namespace VolleyBallTournament
{
    public class GroupRegister : Node
    {
        public int IdGroupRegister => _idGroupRegister;
        int _idGroupRegister = 0;
        Container _div;
        public GroupRegister(Game game, int nbTeam, int groupId)
        {
            _idGroupRegister = groupId;
            _div = new Container(Style.Space.One * 20, new Style.Space(30, 20, 10, 10), Mugen.Physics.Position.VERTICAL);

            for (int i = 0; i < nbTeam; i++)
            {
                var textBox = new TextBox(game, new Rectangle(0, 0, 320, 64), Static.FontMain, Color.Black * .75f, Color.Yellow, Color.Gold, 50).AppendTo(this).This<TextBox>();
                textBox.SetTitle($"Equipe {i + 1 + groupId * nbTeam}", Color.White, Static.FontMini);
                textBox.SetId(_idGroupRegister * nbTeam + i);

                _div.Insert(textBox);
            }
            _div.Refresh();
            SetSize(_div.Rect.Width, _div.Rect.Height);
        }
        public override Node Update(GameTime gameTime)
        {
            _div.SetPosition(XY);

            UpdateChilds(gameTime);

            return base.Update(gameTime);
        }
        public override Node Draw(SpriteBatch batch, GameTime gameTime, int indexLayer)
        {
            if (indexLayer == (int)Layers.Main)
            {
                batch.FillRectangle(AbsRectF, Color.DarkSlateBlue * .75f);

                batch.Rectangle(AbsRectF, Color.DarkSlateBlue * 1f);
                batch.Rectangle(AbsRectF.Extend(-4f), Color.Gray * .75f);

                batch.CenterStringXY(Static.FontMain, $"Groupe {_idGroupRegister + 1}", AbsRectF.TopCenter - Vector2.UnitY * 20 + Vector2.One * 6, Color.Black * .5f);
                batch.CenterStringXY(Static.FontMain, $"Groupe {_idGroupRegister + 1}", AbsRectF.TopCenter - Vector2.UnitY * 20, Color.White);
            }

            if (indexLayer == (int)Layers.Debug)
            {
                //_div.DrawDebug(batch, Color.Red, 3f);
            }

            DrawChilds(batch, gameTime, indexLayer);

            return base.Draw(batch, gameTime, indexLayer);
        }
    }
}
