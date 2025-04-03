using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mugen.Core;
using Mugen.GFX;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

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
            team.Rank = _teams.Count;
            
            team.SetPosition(0, team._rect.Height * (_teams.Count - 1));

            SetSize(team._rect.Width, team._rect.Height * _teams.Count);
        }
        public override Node Update(GameTime gameTime)
        {
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

                batch.CenterBorderedStringXY(Static.FontMain, $"Groupe {GroupName}", AbsRectF.TopCenter - Vector2.UnitY * 12, Color.Yellow, Color.Black);
            }

            DrawChilds(batch, gameTime, indexLayer);

            return base.Draw(batch, gameTime, indexLayer);
        }
    }
}
