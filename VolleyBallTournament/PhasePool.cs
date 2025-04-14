using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
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
        private int _ticRotation = 0;

        public bool IsLocked = false;
        private readonly string _title;
        private readonly Container _divMain;
        private readonly Container _divTimer;
        private readonly Container _divMatch;
        private readonly Container _divGroup;

        private List<Match> _matchs = new();
        private List<Group> _groups = new();
        private List<Team> _teams = new();

        public TimerCountDown Timer => _timer;
        private readonly TimerCountDown _timer;

        private int _currentRotation = 0;

        private RotationManager _rotationManager;

        private int Id;

        public PhasePool(int id, string title, RotationManager rotationManager, PhaseRegister phaseRegister = null, int nbGroup = 4, int nbTeamPerGroup = 4, int nbMatch = 3)
        {
            Id = id;

            _title = title;
            _rotationManager = rotationManager;

            SetSize(Screen.Width, Screen.Height);

            _divMain = new Container(Style.Space.One * 0, Style.Space.Zero, Mugen.Physics.Position.VERTICAL);
            _divMatch = new Container(Style.Space.One * 10, new Style.Space(20, 0, 40, 40), Mugen.Physics.Position.HORIZONTAL);
            _divGroup = new Container(Style.Space.One * 10, new Style.Space(20, 80, 50, 50), Mugen.Physics.Position.HORIZONTAL);
            _divTimer = new Container(Style.Space.One * 10, new Style.Space(0, 0, 0, 0), Mugen.Physics.Position.HORIZONTAL);

            if (phaseRegister != null)
            {
                ImportGroups(phaseRegister);
                ImportMatchs(phaseRegister);
            }
            else
            {
                CreateGroups(nbGroup, nbTeamPerGroup);
                CreateMatchs(nbMatch);
            }

            _timer = (TimerCountDown)new TimerCountDown().AppendTo(this);
            _divTimer.Insert(_timer);

            _divMain.Insert(_divTimer);
            _divMain.Insert(_divMatch);
            _divMain.Insert(_divGroup);

            _divMain.SetPosition((Screen.Width - _divMain.Rect.Width) / 2, (Screen.Height - _divMain.Rect.Height) / 2);
            _divMain.Refresh();

            State.Set(States.Pause);

            State.On(States.WarmUp, () =>
            {
                Static.SoundStart.Play(0.5f * Static.VolumeMaster, .01f, 0f);

                Timer.SetDuration(_rotationManager.GetWarmUpTime(_currentRotation));
                Timer.StartTimer();
            });
            State.Off(States.WarmUp, () =>
            {
                Static.SoundStart.Play(0.5f * Static.VolumeMaster, .01f, 0f);

                Timer.SetDuration(0);
            });

            State.On(States.CountDown1, () =>
            {
                Static.SoundCountDown.Play(0.5f * Static.VolumeMaster, .1f, 0f);
                Timer.SetDuration(3);
                Timer.StartTimer();
            });
            State.On(States.CountDown2, () =>
            {
                Static.SoundCountDown.Play(0.5f * Static.VolumeMaster, .1f, 0f);
                Timer.SetDuration(3);
                Timer.StartTimer();
            });

            State.On(States.Play1, () => 
            {
                Static.SoundStart.Play(0.5f * Static.VolumeMaster, 0.01f, 0f);
                Timer.SetDuration(_rotationManager.GetMatchTime(_currentRotation));
                Timer.StartTimer();
                //Misc.Log("On Play");
            });
            State.Off(States.Play1, () =>
            {
                //Static.SoundStart.Play(0.5f * Static.VolumeMaster, 0.1f, 0f);
                Timer.StopTimer();
                //Misc.Log("Off Play");
            });

            State.On(States.Play2, () =>
            {
                Static.SoundStart.Play(0.5f * Static.VolumeMaster, 0.01f, 0f);
                Timer.SetDuration(_rotationManager.GetMatchTime(_currentRotation));
                Timer.StartTimer();
                //Misc.Log("On Play");
            });
            State.Off(States.Play2, () =>
            {
                //Static.SoundStart.Play(0.5f * Static.VolumeMaster, 0.1f, 0f);
                Timer.StopTimer();
                //Misc.Log("Off Play");
            });

            State.On(States.PreSwap, () => 
            {
                Static.SoundStart.Play(0.5f * Static.VolumeMaster, 0.01f, 0f);
                Timer.SetDuration(2);
                Timer.StartTimer();

                //ValidSets();
            });

            State.On(States.SwapSide, () =>
            {
                Timer.SetDuration(0);

                // On change de côté
                var matchs = GetMatchs();
                for (int i = 0; i < matchs.Count; i++)
                {
                    var match = matchs[i];
                    match.Court.SwapTeams();
                }
            });


            State.On(States.Pause, () =>
            {
                ResetTeamsStatus();
                ResetAllMatchScorePoints();
                ResetAllMatchSetPoints();

                SetRotation(_currentRotation);

            });
            State.Off(States.Ready, () =>
            {
                

            });
            State.On(States.Finish, () =>
            {
                Static.SoundStart.Play(0.5f * Static.VolumeMaster, 0.01f, 0f);
            });
            State.On(States.ValidPoints, () =>
            {
                ValidSets();
            });
            State.Off(States.ValidPoints, () =>
            {
                Static.SoundRanking.Play(1f * Static.VolumeMaster, 0.0001f, 0f);
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
                            match.TeamA.Stats.AddRankingPoint(1);
                            match.TeamB.Stats.AddRankingPoint(1);

                            match.TeamA.Stats.AddResult(Result.Null);
                            match.TeamB.Stats.AddResult(Result.Null);
                        }
                        else
                        {
                            match.GetWinner().Stats.AddRankingPoint(3);
                            match.GetWinner().Stats.AddResult(Result.Win);
                            match.GetLooser().Stats.AddResult(Result.Loose);
                        }

                        match.TeamA.Stats.ValidBonusPoint();
                        match.TeamB.Stats.ValidBonusPoint();
                    }
                }

                for (int i = 0; i < _groups.Count; i++)
                {
                    var group = _groups[i];
                    group.Refresh();
                }

                _currentRotation++;
                //_step = int.Clamp(_step, 0, _nbStep - 1);
            });
        }

        public void ValidSets()
        {
            var matchs = GetMatchs();
            for (int i = 0; i < matchs.Count; i++)
            {
                var match = matchs[i];
                if (match != null)
                    match.ValidSet();
            }
        }
        private void CreateGroups(int nbGroup, int nbTeamPerGroup)
        {
            Misc.Log($"Create Groups --------------------------");
            for (int i = 0;i < nbGroup;i++)
            {
                var group = new Group($"{i+1}").AppendTo(this).This<Group>();
                _groups.Add(group);

                Misc.Log($"Create Group {i + 1}");

                _divGroup.Insert(group);
                for (int t = 0; t < nbTeamPerGroup; t++)
                {
                    var team = new Team($"Team{i * nbTeamPerGroup + t}").AppendTo(this).This<Team>();
                    _teams.Add(team);

                    group.AddTeam(team);
                }
            }
        }
        private void CreateMatchs(int nbMatch)
        {
            Misc.Log($"Create Matchs --------------------------");
            for (int i = 0;i < nbMatch ; i++)
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
        private void ImportGroups(PhaseRegister phaseRegister)
        {
            _teams = phaseRegister.GetTeams();
            _groups = phaseRegister.GetGroups();

            for (int i = 0; i < phaseRegister.NbGroup; i++)
            {
                var group = _groups[i].AppendTo(this).This<Group>();
                _divGroup.Insert(group);
            }
        }
        private void ImportMatchs(PhaseRegister phaseRegister)
        {
            _matchs = phaseRegister.GetMatchs();
            for (int i = 0; i < phaseRegister.NbMatch; i++)
            {
                var match = _matchs[i].AppendTo(this).This<Match>();
                _divMatch.Insert(match);
            }
        }
        public void SetRotation(int rotation)
        {
            if (_rotationManager == null) return;
            if (rotation >= _rotationManager.NbRotation) return;

            _currentRotation = rotation;
            var matchConfigs = _rotationManager.GetMatchConfigs(rotation);

            for (int i = 0; i < matchConfigs.Count; i++)
            {
                var matchConfig = matchConfigs[i];
                if (matchConfig !=  null)
                {
                    
                    //Misc.Log($"SET ROTATION : {matchConfig.IdTerrain}");

                    var match = _matchs[matchConfig.IdTerrain];

                    match.SetTeam(matchConfig.TeamA, matchConfig.TeamB, matchConfig.TeamReferee);
                    match.SetNbSetToWin(matchConfig.NbSetToWin);
                }
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
        public void ResetAllMatchScorePoints()
        {
            for (int i = 0; i < _matchs.Count; i++)
            {
                var match = _matchs[i];
                match.ResetScorePoints();
            }
        }
        public void ResetAllMatchSetPoints()
        {
            for (int i = 0; i < _matchs.Count; i++)
            {
                var match = _matchs[i];
                match.ResetSets();
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
        public void ShuffleTeamsPoint()
        {
            for (int i = 0; i < _teams.Count; i++)
            {
                var team = _teams[i];
                team.Stats.SetScorePoint(Misc.Rng.Next(8, 20));
            }
            //for (int i = 0; i < _groups.Count; i++)
            //{
            //    var group = _groups[i];
            //    group.Refresh();
            //}
        }
        public void ResetTeamsTotalPoint()
        {
            for (int i = 0; i < _teams.Count; i++)
            {
                var team = _teams[i];
                team.Stats.SetRankingPoint(0);
                team.Stats.SetTotalPoint(0);
                team.Stats.ResetResult();
                team.Stats.ResetAllPoints();
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
        private void PlayCountDown(SoundEffect sound, double secondRemaining = 3d, float volume = .25f, float pitch = .1f, float pan = 0f)
        {
            if (Timer.OnRemaingTime(secondRemaining) && sound != null)
            {
                sound.Play(volume * Static.VolumeMaster, pitch, pan);
            }

            if (Timer.IsFinish())
            {
                Misc.Log("Finish CountDown");
                Timer.StopTimer();
                SetTicRotation((_ticRotation + 1) % Enums.Count<States>());
            }
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

                    PlayCountDown(null);
                    break;

                case States.Play1:

                    PlayCountDown(Static.SoundCountDown, 3);
                    break;

                case States.Play2:

                    PlayCountDown(Static.SoundCountDown, 3);
                    break;

                case States.CountDown1:

                    PlayCountDown(Static.SoundCountDown, 3);
                    break;
                case States.CountDown2:

                    PlayCountDown(Static.SoundCountDown, 3);
                    break;


                case States.ValidPoints:
                    break;

                case States.SwapSide:
                    break;

                case States.PreSwap:

                    PlayCountDown(null);
                    break;

                default:
                    break;
            }
        }
        private void SetTicRotation(int ticProcess)
        {
            _ticRotation = ticProcess;

            States nextState = (States)_process[_ticRotation];

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

            if (!IsLocked)
            {
                RunState();

                // Debug
                if (ButtonControl.OnePress($"SwapTeams", Static.Key.IsKeyDown(Keys.S)))
                {
                    for (int i = 0;i <_matchs.Count;i++)
                        _matchs[i].Court.SwapTeams();
                }

                if (ButtonControl.OnePress($"Space{Id}", Keyboard.GetState().IsKeyDown(Keys.Space)))
                {
                    Misc.Log("Rotation");

                    if (_currentRotation < _rotationManager.NbRotation)
                        SetTicRotation((_ticRotation + 1) % Enums.Count<States>());
                }

                if (ButtonControl.OnePress($"Shuffle{Id}", Keyboard.GetState().IsKeyDown(Keys.NumPad0)))
                {
                    if (State.CurState == States.Play1)
                        ShuffleTeamsPoint();
                }

                if (ButtonControl.OnePress($"Reset{Id}", Keyboard.GetState().IsKeyDown(Keys.Back)))
                {
                    SetTicRotation(0);
                    ResetTeamsStatus();
                    ResetTeamsTotalPoint();
                    SetRotation(0);
                }

                for (int i = 0; i < _matchs.Count; i++)
                {
                    var match = _matchs[i];

                    if (ButtonControl.OnePress($"AddPointA{i}{Id}", Keyboard.GetState().IsKeyDown((Keys)112 + i * 4)))
                    {
                        match.AddPointA(+1);
                    }
                    if (ButtonControl.OnePress($"SubPointA{i}{Id}", Keyboard.GetState().IsKeyDown((Keys)113 + i * 4)))
                    {
                        match.AddPointA(-1);
                    }
                    if (ButtonControl.OnePress($"AddPointB{i}{Id}", Keyboard.GetState().IsKeyDown((Keys)114 + i * 4)))
                    {
                        match.AddPointB(-1);
                    }
                    if (ButtonControl.OnePress($"SubPointB{i}{Id}", Keyboard.GetState().IsKeyDown((Keys)115 + i * 4)))
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
            //batch.GraphicsDevice.Clear(Color.Transparent);

            if (indexLayer == (int)Layers.BackGround)
            {
                batch.Draw(Static.TexBG01, Vector2.Zero, Color.White);
            }

            if (indexLayer == (int)Layers.Main)
            {
                //batch.GraphicsDevice.Clear(Color.DarkSlateBlue);
                batch.FillRectangle(AbsRectF, Color.Black * .25f);
                //batch.FillRectangle(AbsRectF, Color.DarkSlateBlue * .5f);

                //batch.FillRectangle(_divGroup.Rect.Translate(AbsXY), Color.Black * .25f);

                //batch.Grid(Vector2.Zero, Screen.Width, Screen.Height, 40, 40, Color.Black * .5f, 1f);


            }

            if (indexLayer == (int)Layers.HUD)
            {
                batch.LeftTopString(Static.FontMain, _title, AbsRectF.TopLeft + Vector2.UnitX * 40 + Vector2.One * 6, Color.Black);
                batch.LeftTopString(Static.FontMain, _title, AbsRectF.TopLeft + Vector2.UnitX * 40, Color.White);
                DrawRotation(batch, AbsXY + Vector2.UnitX * 620 + new Vector2((Screen.Width - (_rotationManager.NbRotation - 1) * 64)/2, 80));
            }

            if (indexLayer == (int)Layers.Debug)
            {
                //batch.LeftTopString(Static.FontMini, "  Victoire = 3 pts\n  Nul = 1 pts\n  Défaite = 0 pts\n+ Bonus écart et points total", AbsRectF.TopLeft + Vector2.UnitX * 40 + Vector2.UnitY * 60, Color.Gray);

                //Static.DrawRoundedRectangle(batch, Static.TexLine, new Rectangle(100, 100, 800, 400),  Color.White, 30, 30, 30, 80, 3, 24);

                //batch.LineTexture(Static.TexLine, Vector2.Zero, Static.MousePos, 9, Color.Gold);

                //Static.DrawArcFilled(batch, Vector2.One * 600, 1200, Geo.RAD_0, Geo.RAD_90, Color.Red);

                //batch.RightMiddleString(Static.FontMini, $"{(int)State.CurState} {State.CurState} Temps = {_rotationManager.Duration}s", AbsRectF.TopRight + Vector2.UnitY * 20 - Vector2.UnitX * 20, Color.Cyan);

                
            }

            DrawChilds(batch, gameTime, indexLayer);

            return base.Draw(batch, gameTime, indexLayer);
        }

        private void DrawRotation(SpriteBatch batch, Vector2 position)
        {
            for (int i = 0; i < _rotationManager.NbRotation; i++)
            {
                var pos = new Vector2(position.X + (i+0) * 64, position.Y);
                var pos2 = new Vector2(position.X + (i+1) * 64, position.Y);

                if (i == 0)
                    batch.CenterStringXY(Static.FontMini, "Début des Matchs", pos + Vector2.UnitY * 40, Color.Yellow);

                if (i == _rotationManager.NbRotation - 2)
                    batch.CenterStringXY(Static.FontMini, "Fin des Matchs", pos2 + Vector2.UnitY * 40, Color.Yellow);


                if (i < _rotationManager.NbRotation - 1)
                    batch.LineTexture(Static.TexLine, pos, pos2, 7f, i >= _currentRotation ? Color.Gray * .5f : Color.Gold);

                if (i != _currentRotation)
                    batch.FilledCircle(Static.TexCircle, pos, 16f, i >= _currentRotation ? Color.Gray : Color.Gold);
                else
                {
                    batch.FilledCircle(Static.TexCircle, pos, 40f, Color.Gold);
                    batch.CenterStringXY(Static.FontMain, $"{_currentRotation + 1}", pos, Color.Red);
                }
            }

            //batch.CenterStringXY(Static.FontMini, "Rotation", position - Vector2.UnitY*32 + Vector2.UnitX * ((_rotationManager.NbRotation - 1) * 40) / 2, Color.White);
        }
    }
}
