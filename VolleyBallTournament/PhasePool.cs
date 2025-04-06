using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Mugen.Core;
using Mugen.GFX;
using Mugen.GUI;
using Mugen.Input;
using System.Collections.Generic;
using static VolleyBallTournament.Match;

namespace VolleyBallTournament
{
    public class PhasePool : Node
    {
        public State<States> State { get; private set; } = new State<States>(States.Ready);

        private List<int> _process = Enums.GetList<States>();
        private int _ticProcess = 0;

        public bool IsPaused = false;
        private readonly string _title;
        private readonly Container _divMain;
        private readonly Container _divTimer;
        private readonly Container _divMatch;
        private readonly Container _divGroup;

        private Match[] _matchs = new Match[3];
        private Group[] _groups = new Group[4];
        private Team[] _teams = new Team[16];

        public Timer Timer => _timer;
        private readonly Timer _timer;

        private int _step = 0;
        private int _nbStep = 8;
        private Sequence _sequence;


        public PhasePool(string title, Sequence sequence)
        {
            _title = title;
            _sequence = sequence;

            SetSize(Screen.Width, Screen.Height);

            _divMain = new Container(Style.Space.One * 10, Style.Space.Zero, Mugen.Physics.Position.VERTICAL);
            _divMatch = new Container(Style.Space.One * 10, new Style.Space(0, 0, 0, 0), Mugen.Physics.Position.HORIZONTAL);
            _divGroup = new Container(Style.Space.One * 10, new Style.Space(40, 0, 60, 60), Mugen.Physics.Position.HORIZONTAL);
            _divTimer = new Container(Style.Space.One * 10, new Style.Space(0, 60, 0, 0), Mugen.Physics.Position.HORIZONTAL);

            CreateTeams();
            CreateMatchs();

            _timer = (Timer)new Timer().AppendTo(this);
            _divTimer.Insert(_timer);

            _divMain.Insert(_divTimer);
            _divMain.Insert(_divMatch);
            _divMain.Insert(_divGroup);

            _divMain.SetPosition((Screen.Width - _divMain.Rect.Width) / 2, (Screen.Height - _divMain.Rect.Height) / 2);
            _divMain.Refresh();

            State.Set(States.Pause);

            State.On(States.Play, () => 
            {
                Timer.StartTimer();
                Misc.Log("On Play");
            });
            State.Off(States.Play, () =>
            {
                Timer.StopTimer();
                Misc.Log("Off Play");
            });

            State.On(States.Pause, () =>
            {
                _step++;
                _step = int.Clamp(_step, 0, 7);

                ResetTeamsStatus();
                ResetScoresPoint();
                LoadSequence(_sequence, _step);
            });
        }
        public void CreateTeams()
        {
            int teamNumber = 0;
            for (int i = 0; i < _groups.Length; i++)
            {
                _groups[i] = (Group)new Group($"{i + 1}").AppendTo(this);

                for (int t = 0; t < 4; t++)
                {
                    var team = new Team($"Team {teamNumber + 1}");
                    _teams[teamNumber] = team;
                    team.AddPointTotal(Misc.Rng.Next(0, 0));

                    _groups[i].AddTeam(team);

                    teamNumber++;
                }

                _divGroup.Insert(_groups[i]);
            }
        }
        public void CreateMatchs()
        {
            int group = 0;
            for (int i = 0; i < _matchs.Length; i++)
            {
                var teamA = new Team("TeamA");
                var teamB = new Team("TeamB");
                var teamReferee = new Team("TeamR");

                _matchs[i] = (Match)new Match($"{i + 1}", teamA, teamB, teamReferee, _sequence).AppendTo(this);

                _divMatch.Insert(_matchs[i]);
                group++;
            }
        }
        public void ResetTeamsStatus()
        {
            for (int i = 0; i < _teams.Length; i++)
            {
                var team = _teams[i];
                team.SetIsPlaying(false);
                team.SetIsReferee(false);

                team.SetMatch(null);
            }
        }
        public void ResetScoresPoint()
        {
            for (int i = 0; i < _matchs.Length; i++)
            {
                _matchs[i].ResetScore();
            }
        }
        public void LoadSequence(Sequence sequence, int step)
        {
            if (sequence == null) return;
            if (step >= sequence.NbStep()) return;

            _step = step;
            var sets = sequence.GetList(step);

            for (int i = 0; i < sets.Count; i++)
            {
                var set = sets[i];
                _matchs[i].SetTeam(set.TeamA, set.TeamB, set.TeamReferee);
            }
        }
        public void ShuffleTeamsTotalPoint()
        {
            for (int i = 0; i < _teams.Length; i++)
            {
                var team = _teams[i];
                team.SetPointTotal(0);
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
        private void RunState()
        {
            switch (State.CurState)
            {
                case States.Ready:

                    break;
                case States.Pause:

                    break;
                case States.Finish:

                    break;
                case States.LastPoint:

                    break;
                case States.WarmUp:
                    break;

                case States.Play:

                    for (int i = 0; i < _matchs.Length; i++)
                    {
                        var match = _matchs[i];

                        if (ButtonControl.OnePress($"AddPointA{i}", Keyboard.GetState().IsKeyDown((Keys)112 + i * 4)))
                        {
                            match.AddPointA(+1);
                        }
                        if (ButtonControl.OnePress($"SubPointA{i}", Keyboard.GetState().IsKeyDown((Keys)113 + i * 4)))
                        {
                            match.AddPointA(-1);
                        }
                        if (ButtonControl.OnePress($"AddPointB{i}", Keyboard.GetState().IsKeyDown((Keys)114 + i * 4)))
                        {
                            match.AddPointB(-1);
                        }
                        if (ButtonControl.OnePress($"SubPointB{i}", Keyboard.GetState().IsKeyDown((Keys)115 + i * 4)))
                        {
                            match.AddPointB(+1);
                        }
                    }

                    break;

                default:
                    break;
            }
        }
        public override Node Update(GameTime gameTime)
        {
            UpdateRect();

            if (!IsPaused)
            {
                RunState();

                if (ButtonControl.OnePress("Process", Keyboard.GetState().IsKeyDown(Keys.Space)))
                {
                    States nextState = (States)_process[_ticProcess];

                    State.Set(nextState);

                    var matchs = GetMatchs();
                    for (int i = 0; i < matchs.Length; i++)
                    {
                        matchs[i].State.Set(nextState);
                    }


                    // Using the modulus operator
                    _ticProcess = (_ticProcess + 1) % Enums.Count<States>();

                }

                // Debug
                //if (ButtonControl.OnePress("ToggleTimer", Keyboard.GetState().IsKeyDown(Keys.Space)))
                //{
                //    Timer.ToggleTimer();

                //    var matchs = GetMatchs();

                //    for (int i = 0; i < matchs.Length; i++)
                //    {
                //        matchs[i].State.Set(Timer.IsRunning ? Match.States.Play : Match.States.Ready);
                //    }

                //    if (!Timer.IsRunning)
                //    {
                //        _step++;
                //        _step = int.Clamp(_step, 0, 7);

                //        ResetTeamsStatus();
                //        ResetScoresPoint();
                //        LoadSequence(_sequence, _step);

                //        State.Set(States.Ready);
                //    }
                //    else
                //    {
                //        State.Set(States.Play);
                //    }
                //}

                //if (ButtonControl.OnePress("ShuffleTotalPoint", Keyboard.GetState().IsKeyDown(Keys.D1)))
                //{
                //    ShuffleTeamsTotalPoint();
                //}

                if (ButtonControl.OnePress("Stop", Keyboard.GetState().IsKeyDown(Keys.Back)))
                {
                    ResetTeamsStatus();
                    LoadSequence(_sequence, _step = 0);
                    ResetTeamsTotalPoint();
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
                batch.RightMiddleString(Static.FontMini, $"{(int)State.CurState} {State.CurState}", AbsRectF.TopRight + Vector2.UnitY * 20 - Vector2.UnitX * 20, Color.Cyan);
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

            batch.RightMiddleString(Static.FontMini, "Rotation", position + Vector2.UnitX * 400 - Vector2.UnitY * 24, Color.White);
        }
    }
}
