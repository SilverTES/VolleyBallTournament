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
    public class SemiFinal : Node
    {
        string _name1;
        string _name2;
        public Container Div => _div;
        private Container _div = new Container(Style.Space.One * 10, Style.Space.One * 10, Mugen.Physics.Position.VERTICAL);
        private Container _div1 = new Container(Style.Space.One * 10, new Style.Space(10, 10, 10, 10), Mugen.Physics.Position.VERTICAL);
        private Container _div2 = new Container(Style.Space.One * 10, new Style.Space(10, 10, 10, 10), Mugen.Physics.Position.VERTICAL);

        public SemiFinal(string name, string name1, string name2) 
        { 
            _name = name;
            _name1 = name1;
            _name2 = name2;
        }
        public void Set(Team teamA1, Team teamB1, Team teamA2, Team teamB2)
        {
            _div1.Insert(teamA1);
            _div1.Insert(teamB1);
            _div2.Insert(teamA2);
            _div2.Insert(teamB2);

            _div.Insert(_div1);
            _div.Insert(_div2);
        }
        public SemiFinal(string name, string name1, string name2, Team teamA1, Team teamB1, Team teamA2, Team teamB2)
        {
            _name = name;
            _name1 = name1;
            _name2 = name2;

            _div1.Insert(teamA1);
            _div1.Insert(teamB1);
            _div2.Insert(teamA2);
            _div2.Insert(teamB2);

            _div.Insert(_div1);
            _div.Insert(_div2);
        }

        public override Node Update(GameTime gameTime)
        {
            UpdateRect();

            return base.Update(gameTime);
        }

        public override Node Draw(SpriteBatch batch, GameTime gameTime, int indexLayer)
        {
            var rectDiv = _div.Rect.Translate(_parent.AbsXY);

            var rectDiv1 = _div1.Rect.Translate(_parent.AbsXY);
            var rectDiv2 = _div2.Rect.Translate(_parent.AbsXY);

            if (indexLayer == (int)Layers.Main)
            {
                batch.FillRectangle(rectDiv, Color.DarkSlateBlue * .5f);
                batch.Rectangle(rectDiv, Color.Gray * .5f);
                //batch.Rectangle(rectDiv.Extend(-4f), Color.Gray * .5f);

                var pos = rectDiv.TopCenter - Vector2.UnitY * 40;
                Static.DrawTextFrame(batch, Static.FontMain, pos, _name, Color.Cyan * .5f, Color.Black * .5f, Vector2.UnitY * 3);
                batch.CenterStringXY(Static.FontMain, _name, pos, Color.Cyan);

                batch.FillRectangle(rectDiv1, Color.Black * .5f);
                batch.FillRectangle(rectDiv2, Color.Black * .5f);

            }
            if (indexLayer == (int)Layers.HUD)
            {

                batch.CenterStringXY(Static.FontMain, _name1, rectDiv1.Center - Vector2.UnitY, Color.Gold);
                batch.CenterStringXY(Static.FontMain, _name2, rectDiv2.Center - Vector2.UnitY, Color.Gold);
            }

            return base.Draw(batch, gameTime, indexLayer);
        } 
    }
    public class PhaseDemiFinal : Node
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


        public PhaseDemiFinal(string title) 
        { 
            _title = title;
            SetSize(Screen.Width, Screen.Height);

            SemiFinal principale1 = new SemiFinal("Principale 1", "Winner", "Winner").AppendTo(this).This<SemiFinal>();
            SemiFinal principale2 = new SemiFinal("Principale 2", "Looser", "Looser").AppendTo(this).This<SemiFinal>();

            SemiFinal consolante1 = new SemiFinal("Consolante 1", "Winner", "Winner").AppendTo(this).This<SemiFinal>();
            SemiFinal consolante2 = new SemiFinal("Consolante 2", "Looser", "Looser").AppendTo(this).This<SemiFinal>();

            CreateTeams();

            principale1.Set(_teams[0], _teams[1], _teams[2], _teams[3]);
            principale2.Set(_teams[4], _teams[5], _teams[6], _teams[7]);

            consolante1.Set(_teams[8], _teams[9], _teams[10], _teams[11]);
            consolante2.Set(_teams[12], _teams[13], _teams[14], _teams[15]);

            _divSemiPrincipal.Add(principale1);
            _divSemiPrincipal.Add(principale2);

            _divSemiConsolante.Add(consolante1);
            _divSemiConsolante.Add(consolante2);


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
            if (indexLayer == (int)Layers.BackGround)
            {
                batch.Draw(Static.TexBG02, AbsXY, Color.White);
            }

            if (indexLayer ==(int)Layers.Main)
            {
                batch.FillRectangle(AbsRectF, Color.Black * .25f);

                //batch.Grid(AbsXY, Screen.Width, Screen.Height, 40, 40, Color.Black * .1f);

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
