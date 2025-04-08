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

        private List<Match> _matchs = new();
        private List<Group> _groups = new();
        private List<Team> _teams = new();

        public Timer Timer => _timer;
        private readonly Timer _timer;

        private int _step = 0;
        private int _nbStep = 8;

        private Rotation _rotation;

        public PhasePool(string title, PhaseRegister phaseRegister)
        {
            _title = title;
            _rotation = phaseRegister.Rotation;

            SetSize(Screen.Width, Screen.Height);

            _divMain = new Container(Style.Space.One * 10, Style.Space.Zero, Mugen.Physics.Position.VERTICAL);
            _divMatch = new Container(Style.Space.One * 10, new Style.Space(0, 0, 0, 0), Mugen.Physics.Position.HORIZONTAL);
            _divGroup = new Container(Style.Space.One * 10, new Style.Space(40, 0, 60, 60), Mugen.Physics.Position.HORIZONTAL);
            _divTimer = new Container(Style.Space.One * 10, new Style.Space(0, 60, 0, 0), Mugen.Physics.Position.HORIZONTAL);

            CreatePhasePool(phaseRegister.NbGroup, phaseRegister.NbTeamPerGroup, phaseRegister.NbMatch);

            // Associe les textBox avec les TeanName des joueurs venant d'être créer !
            var textBoxs = phaseRegister.GetTexBoxs();
            for (int i = 0; i < textBoxs.Count; i++)
            {
                var textBox = textBoxs[i];
                textBox.OnChange += (t) =>
                {
                    GetTeam(t.Id).SetTeamName(textBox.Text);
                };
            }

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
                //Misc.Log("On Play");
            });
            State.Off(States.Play, () =>
            {
                Timer.StopTimer();
                //Misc.Log("Off Play");
            });

            State.On(States.Pause, () =>
            {
                ResetTeamsStatus();
                ResetScoresPoint();
                SetRotation(_step);

            });

            State.Off(States.Finish, () =>
            {

            });

            State.Off(States.ValidPoints, () =>
            {
                // On retribut les points dans les team respectif qui viennent de finir de jouer
                var matchs = GetMatchs();
                for (int i = 0; i < matchs.Count; i++)
                {
                    var match = matchs[i];

                    if (match != null)
                    {
                        if (match.GetWinner() == null)
                        {
                            Misc.Log("Match Null");
                            match.TeamA.AddTotalPoint(1);
                            match.TeamB.AddTotalPoint(1);

                            match.TeamA.AddResult(Result.Null);
                            match.TeamB.AddResult(Result.Null);
                        }
                        else
                        {
                            match.GetWinner().AddTotalPoint(3);
                            match.GetWinner().AddResult(Result.Win);
                            match.GetLooser().AddResult(Result.Loose);
                        }

                        match.TeamA.ValidBonusPoint();
                        match.TeamB.ValidBonusPoint();
                    }
                }

                for (int i = 0; i < _groups.Count; i++)
                {
                    var group = _groups[i];
                    group.Refresh();
                }

                _step++;
                //_step = int.Clamp(_step, 0, _nbStep - 1);
            });
        }
        public void CreatePhasePool(int nbGroup, int nbTeamPerGroup, int nbMatch)
        {
            int teamNumber = 0;
            for (int i = 0; i < nbGroup; i++)
            {
                var group = (Group)new Group($"{i + 1}").AppendTo(this);
                _groups.Add(group);

                for (int t = 0; t < nbTeamPerGroup; t++)
                {
                    var team = new Team($"Team {teamNumber + 1}");
                    _teams.Add(team);

                    team.AddTotalPoint(Misc.Rng.Next(0, 0));

                    group.AddTeam(team);

                    teamNumber++;
                }

                _divGroup.Insert(group);
            }

            for (int i = 0; i < nbMatch; i++)
            {
                var teamA = new Team("TeamA");
                var teamB = new Team("TeamB");
                var teamReferee = new Team("TeamR");

                var match = (Match)new Match($"{i + 1}", teamA, teamB, teamReferee, _rotation).AppendTo(this);

                _matchs.Add(match);

                _divMatch.Insert(_matchs[i]);
            }
        }
        public void ResetTeamsStatus()
        {
            for (int i = 0; i < _teams.Count; i++)
            {
                var team = _teams[i];
                team.SetIsPlaying(false);
                team.SetIsReferee(false);

                team.SetMatch(null);
            }
        }
        public void ResetScoresPoint()
        {
            for (int i = 0; i < _matchs.Count; i++)
            {
                _matchs[i].ResetScore();
            }
        }
        public void SetRotation(int step)
        {
            if (_rotation == null) return;
            if (step >= _rotation.NbStep()) return;

            _step = step;
            var sets = _rotation.GetList(step);

            for (int i = 0; i < sets.Count; i++)
            {
                var set = sets[i];
                _matchs[i].SetTeam(set.TeamA, set.TeamB, set.TeamReferee);
            }
        }
        //public void ShuffleTeamsTotalPoint()
        //{
        //    for (int i = 0; i < _teams.Length; i++)
        //    {
        //        var team = _teams[i];
        //        team.SetPointTotal(0);
        //        team.AddPointTotal(Misc.Rng.Next(0, 9));
        //    }
        //    for (int i = 0; i < _groups.Length; i++)
        //    {
        //        var group = _groups[i];
        //        group.Refresh();
        //    }
        //}
        public void ResetTeamsTotalPoint()
        {
            for (int i = 0; i < _teams.Count; i++)
            {
                var team = _teams[i];
                team.SetTotalPoint(0);
                team.ResetResult();
                //team.SetNbMatchPlayed(0);
            }
            for (int i = 0; i < _groups.Count; i++)
            {
                var group = _groups[i];
                group.Refresh();
            }
        }
        public List<Team> GetTeams()
        {
            return _teams;
        }
        public Team GetTeam(int index)
        {
            return _teams[index];
        }
        public List<Match> GetMatchs() 
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

                case States.WarmUp:
                    break;

                case States.Play:

                    break;

                default:
                    break;
            }
        }
        private void SetTicProcess(int ticProcess)
        {
            _ticProcess = ticProcess;

            States nextState = (States)_process[_ticProcess];

            State.Set(nextState);

            var matchs = GetMatchs();
            for (int i = 0; i < matchs.Count; i++)
            {
                matchs[i].State.Set(nextState);
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
                    // Using the modulus operator
                    //_ticProcess = (_ticProcess + 1) % Enums.Count<States>();

                    //States nextState = (States)_process[_ticProcess];

                    //State.Set(nextState);

                    //var matchs = GetMatchs();
                    //for (int i = 0; i < matchs.Length; i++)
                    //{
                    //    matchs[i].State.Set(nextState);
                    //}

                    if (_step < _nbStep)
                        SetTicProcess((_ticProcess + 1) % Enums.Count<States>());
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
                    SetTicProcess(0);
                    ResetTeamsStatus();
                    ResetTeamsTotalPoint();
                    SetRotation(_step = 0);
                }

                for (int i = 0; i < _matchs.Count; i++)
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

            }

            if (indexLayer == (int)Layers.HUD)
            {
                DrawStep(batch, AbsXY + new Vector2((Screen.Width - 700)/2, 50));
            }

            if (indexLayer == (int)Layers.Debug)
            {
                batch.LeftMiddleString(Static.FontMini, "Victoire = 3, Nul = 1, Défaite = 0 + bonus écart de point à la fin du match", AbsRectF.BottomLeft + Vector2.UnitX * 10 - Vector2.UnitY * 20, Color.Gray);

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

                if (i == 0)
                    batch.RightMiddleString(Static.FontMini, "Debut des Matchs", pos - Vector2.UnitX * 20, Color.White);

                if (i == _nbStep - 2)
                    batch.LeftMiddleString(Static.FontMini, "Fin des Matchs", pos2 + Vector2.UnitX * 20, Color.White);


                if (i < _nbStep - 1)
                    batch.LineTexture(Static.TexLine, pos, pos2, 7f, i >= _step ? Color.Gray * .5f : Color.Gold);

                if (i != _step)
                    batch.FilledCircle(Static.TexCircle, pos, 16f, i >= _step ? Color.Gray : Color.Gold);
                else
                {
                    batch.FilledCircle(Static.TexCircle, pos, 40f, Color.Gold);
                    batch.CenterStringXY(Static.FontMain, $"{_step + 1}", pos, Color.Red);
                }
            }

            batch.RightMiddleString(Static.FontMini, "Rotation", position + Vector2.UnitX * 400 - Vector2.UnitY * 32, Color.White);
        }
    }
}
