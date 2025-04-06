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
        public bool IsPaused = false;
        private readonly string _title;
        private readonly Container _divMain;
        private readonly Container _divTimer;
        private readonly Container _divMatch;
        private readonly Container _divGroup;

        private readonly Match[] _matchs = new Match[3];
        private readonly Group[] _groups = new Group[4];
        private readonly Team[] _teams = new Team[16];

        public Timer Timer => _timer;
        private readonly Timer _timer;

        private int _step = 0;
        private int _nbStep = 8;

        public PhasePool(string title)
        {
            _title = title;
            SetSize(Screen.Width, Screen.Height);

            _divMain = new Container(Style.Space.One * 10, Style.Space.Zero, Mugen.Physics.Position.VERTICAL);
            _divMatch = new Container(Style.Space.One * 10, new Style.Space(0, 0, 0, 0), Mugen.Physics.Position.HORIZONTAL);
            _divGroup = new Container(Style.Space.One * 10, new Style.Space(40, 0, 60, 60), Mugen.Physics.Position.HORIZONTAL);
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

            Init();

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
        public override Node Init()
        {
            int group = 0;
            for (int i = 0; i < _matchs.Length; i++)
            {
                var teamA = new Team("TeamA", null);
                var teamB = new Team("TeamB", null);
                var teamReferee = new Team("TeamR", null);

                _matchs[i] = (Match)new Match(new ScorePanel(teamA, teamB), new Court($"{i + 1}", teamReferee)).AppendTo(this);

                _divMatch.Insert(_matchs[i]);
                group++;
            }

            return base.Init();
        }
        public void ResetTeamsStatus()
        {
            for (int i = 0; i < _teams.Length; i++)
            {
                var team = _teams[i];
                team.IsPlaying = false;
                team.IsReferee = false;
            }
        }
        public void LoadSequence(Sequence sequence, int step)
        {
            _step = step;
            var sets = sequence.GetList(step);

            for (int i = 0; i < sets.Count; i++)
            {
                var set = sets[i];

                //if (set == null) continue;

                var teamA = set.TeamA;
                var teamB = set.TeamB;
                var teamReferee = set.TeamReferee;

                teamA.IsPlaying = true;
                teamB.IsPlaying = true;
                teamReferee.IsReferee = true;

                _matchs[i].ScorePanel.TeamA = teamA;
                _matchs[i].ScorePanel.TeamB = teamB;
                _matchs[i].Court.TeamReferee = teamReferee;

            }
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
        public void ResetTeamsTotalPoint()
        {
            for (int i = 0; i < _teams.Length; i++)
            {
                var team = _teams[i];
                team.SetPointTotal(0);
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
        public Match[] GetMatchs() 
        { 
            return _matchs; 
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
            if (!IsPaused)
            {
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

            }

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

                batch.LeftTopString(Static.FontMain, _title, AbsRectF.TopLeft + Vector2.UnitX * 40 + Vector2.One * 6, Color.Black);
                batch.LeftTopString(Static.FontMain, _title, AbsRectF.TopLeft + Vector2.UnitX * 40, Color.White);

                DrawStep(batch, AbsXY + new Vector2((Screen.Width - 700)/2, 40));
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

        private void DrawStep(SpriteBatch batch, Vector2 position)
        {
            for (int i = 0; i < _nbStep; i++)
            {
                var pos = new Vector2(position.X + (i+0) * 100, position.Y);
                var pos2 = new Vector2(position.X + (i+1) * 100, position.Y);

                if (i < _nbStep - 1)
                    batch.LineTexture(Static.TexLine, pos, pos2, 7f, i >= _step ? Color.Black * .5f : Color.Gold);

                if (i != _step)
                    batch.FilledCircle(Static.TexCircle, pos, 16f, i >= _step ? Color.Black : Color.Gold);
                else
                {
                    batch.FilledCircle(Static.TexCircle, pos, 40f, Color.Gold);
                    batch.CenterStringXY(Static.FontMain, $"{_step + 1}", pos, Color.Red);
                }
            }
        }
    }
}
