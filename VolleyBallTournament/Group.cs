using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mugen.Core;
using Mugen.GFX;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace VolleyBallTournament
{
    public class Group : Node
    {

        public string GroupName;

        List<Team> _teams = [];

        public int NbTeam => _teams.Count;

        public Group(string groupName) 
        { 
            GroupName = groupName;
        }
        public void AddTeam(Team team)
        {
            team.AppendTo(this);
            _teams.Add(team);

            Refresh();

            SetSize(team._rect.Width, team._rect.Height * _teams.Count);
        }
        public void Refresh()
        {
            _teams = _teams.OrderByDescending(p => p.TotalPoint).ToList();

            for (int i = 0; i < _teams.Count; i++)
            {
                var team = _teams[i];

                team.MoveToRank(i + 1);
                //team.Rank = i + 1;
                team.MoveToPosition(new Vector2(0, team._rect.Height * i));
                //team.SetPosition(0, team._rect.Height * i);
            }

        }
        public override Node Update(GameTime gameTime)
        {
            //Refresh();

            UpdateRect();

            UpdateChilds(gameTime);

            return base.Update(gameTime);
        }
        public override Node Draw(SpriteBatch batch, GameTime gameTime, int indexLayer)
        {
            if (indexLayer == (int)Layers.Main)
            {
                //batch.FillRectangle(AbsRectF.Extend(32f), Color.Black * .5f);
                //batch.Rectangle(AbsRectF, Color.White * .8f, 3f);

            }

            if (indexLayer == (int)Layers.HUD)
            {
                batch.CenterStringXY(Static.FontMain, $"Groupe {GroupName}", AbsRectF.TopCenter - Vector2.UnitY * 30 + Vector2.One * 6, Color.Black * .5f);
                batch.CenterStringXY(Static.FontMain, $"Groupe {GroupName}", AbsRectF.TopCenter - Vector2.UnitY * 30, Color.LightSeaGreen);
            }

            DrawChilds(batch, gameTime, indexLayer);

            return base.Draw(batch, gameTime, indexLayer);
        }
    }
}
