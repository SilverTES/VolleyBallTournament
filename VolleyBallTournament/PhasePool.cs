using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Mugen.Core;
using Mugen.GFX;
using Mugen.GUI;
using Mugen.Input;

namespace VolleyBallTournament
{
    public class PhasePool : Node
    {
        private readonly string _title;
        private readonly Container _divMain;
        private readonly Container _divTimer;
        private readonly Container _divMatch;
        private readonly Container _divGroup;

        private readonly Match[] _matchs = new Match[3];
        private readonly Group[] _groups = new Group[4];
        private readonly Team[] _teams = new Team[16];

        private readonly Timer _timer;

        public PhasePool(string title)
        {
            _title = title;
            SetSize(Screen.Width, Screen.Height);

            _divMain = new Container(Style.Space.One * 10, Style.Space.Zero, Mugen.Physics.Position.VERTICAL);
            _divMatch = new Container(Style.Space.One * 10, new Style.Space(0, 0, 0, 0), Mugen.Physics.Position.HORIZONTAL);
            _divGroup = new Container(Style.Space.One * 10, new Style.Space(80, 0, 60, 60), Mugen.Physics.Position.HORIZONTAL);
            _divTimer = new Container(Style.Space.One * 10, new Style.Space(0, 60, 0, 0), Mugen.Physics.Position.HORIZONTAL);

            int teamNumber = 0;
            for (int i = 0; i < _groups.Length; i++)
            {
                _groups[i] = (Group)new Group($"{i+1}").AppendTo(this);
                
                for (int t = 0; t < 4; t++)
                {
                    var team = new Team($"Team {teamNumber+1}", _groups[i]);
                    _teams[teamNumber] = team;
                    team.AddPointTotal(Misc.Rng.Next(0, 0));

                    _groups[i].AddTeam(team);

                    teamNumber++;
                }

                _divGroup.Insert(_groups[i]);

                
            }

            int group = 0;
            for (int i = 0; i < _matchs.Length; i++)
            {
                var teamA = _teams[group * 3 + i];
                var teamB = _teams[group * 3 + i + 1];
                var teamReferee = _teams[group * 3 + i + 1];

                _matchs[i] = (Match)new Match(new ScorePanel(teamA, teamB), new Court($"{i + 1}", teamReferee)).AppendTo(this);
                
                _divMatch.Insert(_matchs[i]);

                group++;
            }

            _timer = (Timer)new Timer().AppendTo(this);
            _divTimer.Insert(_timer);

            _divMain.Insert(_divTimer);
            _divMain.Insert(_divMatch);
            _divMain.Insert(_divGroup);

            _divMain.SetPosition((Screen.Width - _divMain.Rect.Width) / 2, (Screen.Height - _divMain.Rect.Height) / 2);
            _divMain.Refresh();

            //_teams[0].TeamName = "The Little Giant";
            //_teams[0].NbMatchWin = 2;

            //_teams[1].TeamName = "Les nuls du volley";
            //_teams[1].NbMatchWin = 1;

            //_teams[2].TeamName = "Lili Hina";
            //_teams[2].NbMatchWin = 3;
        }
        public void ShuffleTeamsTotalPoint()
        {
            for (int i = 0; i < _teams.Length; i++)
            {
                var team = _teams[i];
                team.TotalPoint = 0;
                team.AddPointTotal(Misc.Rng.Next(0, 9));
            }
            for (int i = 0; i < _groups.Length; i++)
            {
                var group = _groups[i];
                group.Refresh();
            }
        }
        public Team[] GetTeams()
        {
            return _teams;
        }
        public Team GetTeam(int index)
        {
            return _teams[index];
        }
        public Match GetMatch(int index)
        {
            return _matchs[index];
        }
        public Group GetGroup(int index)
        {
            return _groups[index];
        }
        public override Node Update(GameTime gameTime)
        {
            UpdateRect();

            for (int i = 0; i < _matchs.Length; i++)
            {
                var match = _matchs[i];

                if (ButtonControl.OnePress($"AddPointA{i}", Keyboard.GetState().IsKeyDown((Keys)112 + i * 4)))
                {
                    match.ScorePanel.AddPointA(+1);
                }
                if (ButtonControl.OnePress($"SubPointA{i}", Keyboard.GetState().IsKeyDown((Keys)113 + i * 4)))
                {
                    match.ScorePanel.AddPointA(-1);
                }
                if (ButtonControl.OnePress($"AddPointB{i}", Keyboard.GetState().IsKeyDown((Keys)114 + i * 4)))
                {
                    match.ScorePanel.AddPointB(-1);
                }
                if (ButtonControl.OnePress($"SubPointB{i}", Keyboard.GetState().IsKeyDown((Keys)115 + i * 4)))
                {
                    match.ScorePanel.AddPointB(+1);
                }
            }

            if (ButtonControl.OnePress("ToggleTimer", Keyboard.GetState().IsKeyDown(Keys.Space)))
            {
                //_teams[3].AddPointTotal(1);
                //_groups[0].Refresh();

                _timer.ToggleTimer();
                Misc.Log("Toggle Timer");

                for (int i = 0; i < _matchs.Length; i++)
                {
                    _matchs[i].Court.State.Set(_timer.IsRunning ? Court.States.Play : Court.States.Ready);
                }
            }

            if (ButtonControl.OnePress("ShuffleTotalPoint", Keyboard.GetState().IsKeyDown(Keys.D1)))
            {
                ShuffleTeamsTotalPoint();
            }

            //if (ButtonControl.OnePress("Stop", Keyboard.GetState().IsKeyDown(Keys.Back)))
            //{
            //    _timer.StopTimer();
            //    Misc.Log("Stop Timer");
            //}

            UpdateChilds(gameTime);

            return base.Update(gameTime);
        }
        public override Node Draw(SpriteBatch batch, GameTime gameTime, int indexLayer)
        {

            if (indexLayer == (int)Layers.Main)
            {
                //batch.GraphicsDevice.Clear(Color.DarkSlateBlue);
                
                batch.FillRectangle(AbsRectF, Color.DarkSlateBlue * .5f);

                //batch.Grid(Vector2.Zero, Screen.Width, Screen.Height, 40, 40, Color.Black * .5f, 1f);

                batch.LeftTopString(Static.FontMain2, _title, AbsRectF.TopLeft + Vector2.UnitX * 40 + Vector2.One * 6, Color.Black);
                batch.LeftTopString(Static.FontMain2, _title, AbsRectF.TopLeft + Vector2.UnitX * 40, Color.White);
            }

            if (indexLayer == (int)Layers.Debug)
            {
                batch.LeftMiddleString(Static.FontMain, "Classement par nombre de victoires + bonus écart de point à la fin des matchs en cas d'égalité", AbsRectF.BottomLeft + Vector2.UnitX * 10 - Vector2.UnitY * 20, Color.Gray);

                //Static.DrawRoundedRectangle(batch, Static.TexLine, new Rectangle(100, 100, 800, 400),  Color.White, 30, 30, 30, 80, 3, 24);

                //batch.LineTexture(Static.TexLine, Vector2.Zero, Static.MousePos, 9, Color.Gold);

                //Static.DrawArcFilled(batch, Vector2.One * 600, 1200, Geo.RAD_0, Geo.RAD_90, Color.Red);
            }

            DrawChilds(batch, gameTime, indexLayer);

            return base.Draw(batch, gameTime, indexLayer);
        }
    }
}
