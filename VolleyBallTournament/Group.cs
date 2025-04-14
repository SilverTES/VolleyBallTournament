﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mugen.Core;
using Mugen.GFX;
using System.Collections.Generic;
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
        public Team GetTeam(int index)
        {
            return _teams[index];
        }
        public void Refresh()
        {
            //_teams = _teams.OrderByDescending(p => p.TotalPoint).ToList();

            _teams = _teams
                .OrderByDescending(e => e.Stats.RankingPoint)
                .ThenByDescending(e => e.Stats.BonusPoint)
                .ThenByDescending(e => e.Stats.TotalPoint)
                .ToList();

            for (int i = 0; i < _teams.Count; i++)
            {
                var team = _teams[i];

                team.Stats.SetRank(i + 1);
                team.MoveToPosition(new Vector2(0, (team._rect.Height + 8) * i));
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
                //batch.FillRectangle(AbsRectF.Extend(48f), Color.Black * .25f);
                //batch.Rectangle(AbsRectF.Extend(48f), Color.Gray * .5f, 1f);

            }

            if (indexLayer == (int)Layers.HUD)
            {
                //batch.CenterStringXY(Static.FontMain, $"Groupe {GroupName}", AbsRectF.TopCenter - Vector2.UnitY * 30 + Vector2.One * 6, Color.Black * .5f);
                batch.CenterStringXY(Static.FontMain, $"Groupe {GroupName}", AbsRectF.TopCenter - Vector2.UnitY * 50 + Vector2.One * 6, Color.Black *.5f);
                batch.CenterStringXY(Static.FontMain, $"Groupe {GroupName}", AbsRectF.TopCenter - Vector2.UnitY * 50, Color.Cyan);
            }

            DrawChilds(batch, gameTime, indexLayer);

            return base.Draw(batch, gameTime, indexLayer);
        }
    }
}
