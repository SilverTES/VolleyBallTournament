using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Mugen.Core;
using Mugen.GFX;
using Mugen.GUI;
using Mugen.Input;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace VolleyBallTournament
{
    public class SemiFinal
    {
        public Container Div => _div;
        private Container _div = new Container(Style.Space.One * 10, Style.Space.One * 10, Mugen.Physics.Position.VERTICAL);
        private Container _div1 = new Container(Style.Space.One * 10, Style.Space.One * 10, Mugen.Physics.Position.VERTICAL);
        private Container _div2 = new Container(Style.Space.One * 10, Style.Space.One * 10, Mugen.Physics.Position.VERTICAL);

        public SemiFinal(Team teamA1, Team teamB1, Team teamA2, Team teamB2)
        {
            _div1.Insert(teamA1);
            _div1.Insert(teamB1);
            _div2.Insert(teamA2);
            _div2.Insert(teamB2);

            _div.Insert(_div1);
            _div.Insert(_div2);
        }

    }
    public class PhaseFinal : Node
    {
        string _title;
        public bool IsLocked = false;

        List<Team> _teams = [];
        List<Match> _matchs = [];

        List<SemiFinal> _divSemiPrincipal = [];
        List<SemiFinal> _divSemiConsolante = [];

        Container _divMain;
        Container _divMatch;
        Container _divSemi;


        public PhaseFinal(string title) 
        { 
            _title = title;
            SetSize(Screen.Width, Screen.Height);
            
            CreateTeams();

            _divSemiPrincipal.Add(new SemiFinal(_teams[0], _teams[1], _teams[2], _teams[3]));
            _divSemiPrincipal.Add(new SemiFinal(_teams[4], _teams[5], _teams[6], _teams[7]));

            _divSemiConsolante.Add(new SemiFinal(_teams[8], _teams[9], _teams[10], _teams[11]));
            _divSemiConsolante.Add(new SemiFinal(_teams[12], _teams[13], _teams[14], _teams[15]));


            _divMain = new Container(Style.Space.One * 10, Style.Space.One * 10, Mugen.Physics.Position.VERTICAL);
            
            _divMatch = new Container(Style.Space.One * 10, new Style.Space(20, 0, 40, 40), Mugen.Physics.Position.HORIZONTAL);
            _divSemi = new Container(Style.Space.One * 10, new Style.Space(20, 0, 20, 20), Mugen.Physics.Position.HORIZONTAL);
            
            CreateMatchs(3);

            _divSemi.Insert(_divSemiPrincipal[0].Div);
            _divSemi.Insert(_divSemiPrincipal[1].Div);

            _divSemi.Insert(_divSemiConsolante[0].Div);
            _divSemi.Insert(_divSemiConsolante[1].Div);

            _divMain.Insert(_divMatch);
            _divMain.Insert(_divSemi);

            _divMain.SetPosition((Screen.Width - _divMain.Rect.Width) / 2, (Screen.Height - _divMain.Rect.Height) / 2);
            _divMain.Refresh();


            _matchs[1].SetIsFreeCourt(true);
        }
        private void CreateTeams(int nbTeams = 16)
        {
            for (int i = 0; i < nbTeams; i++)
            {
                var team = new Team($"Team{i}").AppendTo(this).This<Team>();
                team.SetIsShowStats(false);
                _teams.Add(team);
            }
        }
        private void CreateMatchs(int nbMatch)
        {
            Misc.Log($"Create Matchs --------------------------");
            for (int i = 0; i < nbMatch; i++)
            {
                var teamA = new Team("TeamA");
                var teamB = new Team("TeamB");
                var teamReferee = new Team("TeamR");

                var match = new Match(i, $"{i + 1}", teamA, teamB, teamReferee).AppendTo(this).This<Match>();
                _matchs.Add(match);

                Misc.Log($"Create Match {i + 1}");

                _divMatch.Insert(match);
            }
        }
        public override Node Update(GameTime gameTime)
        {
            UpdateRect();

            if (IsLocked)
            {

            }
            else
            {
                // Debug
                if (ButtonControl.OnePress($"SwapTeams", Static.Key.IsKeyDown(Keys.S)))
                {
                    for (int i = 0; i < _matchs.Count; i++)
                        _matchs[i].Court.SwapTeams();
                }
            }

            UpdateChilds(gameTime);

            return base.Update(gameTime);
        }
        public override Node Draw(SpriteBatch batch, GameTime gameTime, int indexLayer)
        {
            if (indexLayer ==(int)Layers.Debug)
            {

            }
            if (indexLayer ==(int)Layers.Main)
            {
                batch.FillRectangle(AbsRectF, Color.Black * .25f);

                batch.Grid(AbsXY, Screen.Width, Screen.Height, 40, 40, Color.Black * .1f);

            }

            if (indexLayer == (int)Layers.HUD)
            {
                batch.LeftTopString(Static.FontMain, _title, AbsRectF.TopLeft + Vector2.UnitX * 40 + Vector2.One * 6, Color.Black);
                batch.LeftTopString(Static.FontMain, _title, AbsRectF.TopLeft + Vector2.UnitX * 40, Color.White);
            }


            DrawChilds(batch, gameTime, indexLayer);

            return base.Draw(batch, gameTime, indexLayer);
        }
    }
}
