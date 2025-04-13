using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mugen.Core;
using Mugen.GFX;
using Mugen.GUI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VolleyBallTournament
{
    public class PhaseFinal : Node
    {
        string _title;
        public bool IsLocked = false;

        List<Team> _teams = [];

        Container _divSemiAB;
        Container _divSemiA;
        Container _divSemiB;

        Container _divSemiCD;
        Container _divSemiC;
        Container _divSemiD;

        Vector2 _pos = new Vector2(0, 100f);
        public PhaseFinal(string title) 
        { 
            _title = title;
            SetSize(Screen.Width, Screen.Height);   


            _divSemiA = new Container(Style.Space.One * 10, Style.Space.One * 10, Mugen.Physics.Position.VERTICAL);
            _divSemiB = new Container(Style.Space.One * 10, Style.Space.One * 10, Mugen.Physics.Position.VERTICAL);

            _divSemiAB = new Container(Style.Space.One * 10, Style.Space.One * 10, Mugen.Physics.Position.VERTICAL);

            CreateTeams();

            _divSemiA.Insert(_teams[0]);
            _divSemiA.Insert(_teams[1]);
            _divSemiB.Insert(_teams[2]);
            _divSemiB.Insert(_teams[3]);
            
            _divSemiAB.Insert(_divSemiA);
            _divSemiAB.Insert(_divSemiB);

            //_divSemiAB.SetPosition((Screen.Width - _divSemiAB.Rect.Width) / 2, (Screen.Height - _divSemiAB.Rect.Height) / 2);
            _divSemiAB.Refresh();


            Misc.Log($"-------------- {GroupOf<Team>().Count}");

        }
        private void CreateTeams(int nbTeams = 16)
        {
            for (int i = 0; i < nbTeams; i++)
            {
                var team = new Team($"Team{i}").AppendTo(this).This<Team>();
                _teams.Add(team);
            }
        }
        public override Node Update(GameTime gameTime)
        {
            UpdateRect();

            _pos.X += 1f;

            if (IsLocked)
            {

            }
            else
            {

            }

            UpdateChilds(gameTime);

            return base.Update(gameTime);
        }
        public override Node Draw(SpriteBatch batch, GameTime gameTime, int indexLayer)
        {
            if (indexLayer ==(int)Layers.Debug)
            {
                batch.FilledCircle(Static.TexCircle, _pos, 40, Color.Yellow);

                batch.Rectangle(_divSemiA.Rect, Color.Red, 3f);
                batch.Rectangle(_divSemiB.Rect, Color.Red, 3f);
                batch.Rectangle(_divSemiAB.Rect, Color.Red, 3f);
            }
            if (indexLayer ==(int)Layers.Main)
            {
                batch.FillRectangle(AbsRectF, Color.Black * .25f);

                batch.Grid(AbsXY, Screen.Width, Screen.Height, 40, 40, Color.Black * .5f);

                batch.LeftTopString(Static.FontMain, _title, AbsRectF.TopLeft + Vector2.UnitX * 40 + Vector2.One * 6, Color.Black);
                batch.LeftTopString(Static.FontMain, _title, AbsRectF.TopLeft + Vector2.UnitX * 40, Color.White);
            }

            DrawChilds(batch, gameTime, indexLayer);

            return base.Draw(batch, gameTime, indexLayer);
        }
    }
}
