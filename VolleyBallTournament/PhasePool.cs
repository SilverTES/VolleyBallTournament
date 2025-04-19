using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Mugen.Core;
using Mugen.GFX;
using Mugen.GUI;
using Mugen.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using static VolleyBallTournament.Match;

namespace VolleyBallTournament
{
    public class PhasePool : Node
    {
        public State<States> State { get; private set; } = new State<States>(States.PoolReady);

        private int _ticState = 0;

        public bool IsLocked = false;
        public string Title => _title;
        private readonly string _title;
        private readonly Container _divMain;
        private readonly Container _divTimer;
        private readonly Container _divMatch;
        private readonly Container _divGroup;

        private List<Match> _matchs = new();
        private List<Group> _groups = new();
        private List<Team> _teams = new();

        public TimerCountDown TimerCountDown => _timerCountDown;
        private readonly TimerCountDown _timerCountDown;

        private int _currentRotation = 0;
        private RotationManager _rotationManager;

        private int Id;

        public Action<PhasePool> OnFinishPhase;

        private DateTime _startPhaseTime;
        private DateTime _endPhaseTime;

        private Stopwatch _chrono;


        Game _game;
        public PhasePool(Game game, int id, string title, RotationManager rotationManager, PhaseRegister phaseRegister = null, int nbGroup = 4, int nbTeamPerGroup = 4, int nbMatch = 3)
        {
            _name = "PhasePool";
            _type = UID.Get<PhasePool>();
            Id = id;
            _game = game;
            _title = title;
            _rotationManager = rotationManager;

            _chrono = new Stopwatch();
            _chrono.Start();

            SetSize(Screen.Width, Screen.Height);

            _divMain = new Container(Style.Space.One * 0, Style.Space.Zero, Mugen.Physics.Position.VERTICAL);
            _divMatch = new Container(Style.Space.One * 10, new Style.Space(20, 0, 40, 40), Mugen.Physics.Position.HORIZONTAL);
            _divGroup = new Container(Style.Space.One * 10, new Style.Space(20, 80, 50, 50), Mugen.Physics.Position.HORIZONTAL);
            _divTimer = new Container(Style.Space.One * 10, new Style.Space(0, 40, 0, 0), Mugen.Physics.Position.HORIZONTAL);

            if (phaseRegister != null)
            {
                ImportGroups(phaseRegister);
                ImportMatchs(phaseRegister);
            }
            else
            {
                CreateGroups(nbGroup, nbTeamPerGroup, ["Consolante A", "Consolante B", "Principale A", "Principale B"]);
                CreateMatchs(nbMatch);
            }

            _timerCountDown = (TimerCountDown)new TimerCountDown().AppendTo(this);
            _divTimer.Insert(_timerCountDown);

            _divMain.Insert(_divTimer);
            _divMain.Insert(_divMatch);
            _divMain.Insert(_divGroup);

            _divMain.SetPosition((Screen.Width - _divMain.Rect.Width) / 2, (Screen.Height - _divMain.Rect.Height) / 2);
            _divMain.Refresh();


            // Initial State
            SetTicState((int)States.PoolNextMatch);

            _startPhaseTime = DateTime.Now;
            RefreshNextTurnTimes(_currentRotation, _rotationManager);

            DefineStates();
        }
        private void DefineStates()
        {
            State.On(States.PoolWarmUp, () =>
            {

                if (_currentRotation == 0)
                    _startPhaseTime = DateTime.Now;

                Static.SoundStart.Play(0.5f * Static.VolumeMaster, .01f, 0f);

                TimerCountDown.SetDuration(_rotationManager.GetWarmUpTime());
                TimerCountDown.StartTimer();
            });
            State.Off(States.PoolWarmUp, () =>
            {
                Static.SoundStart.Play(0.5f * Static.VolumeMaster, .01f, 0f);

                TimerCountDown.SetDuration(0);


            });

            State.On(States.PoolCountDown1, () =>
            {
                Static.SoundCountDown.Play(0.5f * Static.VolumeMaster, .1f, 0f);
                TimerCountDown.SetDuration(3);
                TimerCountDown.StartTimer();
                _chrono.Start();
            });
            State.On(States.PoolCountDown2, () =>
            {
                Static.SoundCountDown.Play(0.5f * Static.VolumeMaster, .1f, 0f);
                TimerCountDown.SetDuration(3);
                TimerCountDown.StartTimer();
                _chrono.Start();
            });
            State.Off(States.PoolCountDown1, () =>
            {
                _chrono.Stop();
            });

            State.Off(States.PoolCountDown2, () =>
            {
                _chrono.Stop();
            });

            State.On(States.PoolPlay1, () =>
            {
                Static.SoundStart.Play(0.5f * Static.VolumeMaster, 0.01f, 0f);
                TimerCountDown.SetDuration(_rotationManager.GetMatchTime());
                TimerCountDown.StartTimer();
                //Misc.Log("On Play");
            });
            State.Off(States.PoolPlay1, () =>
            {
                //Static.SoundStart.Play(0.5f * Static.VolumeMaster, 0.1f, 0f);
                TimerCountDown.StopTimer();
                //Misc.Log("Off Play");
            });

            State.On(States.PoolPlay2, () =>
            {
                Static.SoundStart.Play(0.5f * Static.VolumeMaster, 0.01f, 0f);
                TimerCountDown.SetDuration(_rotationManager.GetMatchTime());
                TimerCountDown.StartTimer();
                //Misc.Log("On Play");
            });
            State.Off(States.PoolPlay2, () =>
            {
                //Static.SoundStart.Play(0.5f * Static.VolumeMaster, 0.1f, 0f);
                TimerCountDown.StopTimer();
                //Misc.Log("Off Play");
            });

            State.On(States.PoolPreSwap, () =>
            {
                Static.SoundStart.Play(0.5f * Static.VolumeMaster, 0.01f, 0f);
                TimerCountDown.SetDuration(2);
                TimerCountDown.StartTimer();

                //ValidSets();
                _chrono.Start();
            });

            State.On(States.PoolSwapSide, () =>
            {
                TimerCountDown.SetDuration(0);

                // On change de côté
                var matchs = GetMatchs();
                for (int i = 0; i < matchs.Count; i++)
                {
                    var match = matchs[i];
                    match.Court.SwapTeams();
                }
            });
            State.Off(States.PoolSwapSide, () =>
            {
                _chrono.Stop();
            });

            State.On(States.PoolNextMatch, () =>
            {
                
                _chrono.Start();

                Static.SoundVictory.Play(1f * Static.VolumeMaster, 0.0001f, 0f);

                ResetTeamsStatus(_teams);
                ResetAllMatchScorePoints(_matchs);
                ResetAllMatchSetPoints(_matchs);

                SetRotation(_currentRotation, _rotationManager);

                Static.Server.SendUpdateToAll(_matchs);
            });
            State.Off(States.PoolNextMatch, () =>
            {
                _chrono.Stop();
                _chrono.Reset();

                RefreshNextTurnTimes(_currentRotation, _rotationManager);
                Misc.Log("START NEXT TURN TIMES -//////////////////////////");

            });

            State.On(States.PoolReady, () =>
            {
                TimerCountDown.ResetTimer();
                TimerCountDown.StopTimer();

                _chrono.Start();
            });            
            State.Off(States.PoolReady, () =>
            {
                _chrono.Stop();
            });
            State.On(States.PoolFinishMatch, () =>
            {
                Static.SoundStart.Play(0.5f * Static.VolumeMaster, 0.01f, 0f);
                _chrono.Start();
            });
            State.Off(States.PoolFinishMatch, () =>
            {
            });

            State.On(States.PoolValidPoints, () =>
            {
                Static.SoundBeep.Play(1f * Static.VolumeMaster, 0.0001f, 0f);
                ValidSets(1);
            });
            State.Off(States.PoolValidPoints, () =>
            {
                _chrono.Stop();

                
                //Static.SoundRanking.Play(1f * Static.VolumeMaster, 0.0001f, 0f);

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

                RefreshGroups();

                _currentRotation++;

                if (_currentRotation == _rotationManager.NbRotation - 1)
                    HideNextTurnTimes(_teams);

                if (_currentRotation >= _rotationManager.NbRotation)
                {
                    Misc.Log("Fin des matchs");
                    for (int i = 0; i < _matchs.Count; i++)
                        _matchs[i].SetIsFreeCourt(true);

                    if (OnFinishPhase != null)
                        OnFinishPhase(this);
                }

                
            });

        }
        private void ValidSets(int pointGap)
        {
            var matchs = GetMatchs();
            for (int i = 0; i < matchs.Count; i++)
            {
                var match = matchs[i];
                if (match != null)
                    match.ValidSet(pointGap);
            }
        }
        private void CreateGroups(int nbGroup, int nbTeamPerGroup, string[] groupNames)
        {
            Misc.Log($"Create Groups --------------------------");
            for (int i = 0;i < nbGroup;i++)
            {
                var group = new Group($"{groupNames[i]}").AppendTo(this).This<Group>();
                _groups.Add(group);

                Misc.Log($"Create Group {i + 1}");

                _divGroup.Insert(group);
                for (int t = 0; t < nbTeamPerGroup; t++)
                {
                    var team = new Team($"Team{i * nbTeamPerGroup + t + 1}").AppendTo(this).This<Team>();
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

                var match = new Match(_game, $"{i + 1}", new MatchConfig(i, 1, 25, teamA, teamB, teamReferee)).AppendTo(this).This<Match>();
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
        public void Import16TeamsBrassageToQualification(string configFile, PhasePool phasePool)
        {
            Misc.Log($"Import Teams from {phasePool.Title} to {Title}");

            List<Team> teamsConsolante = new List<Team>();
            List<Team> teamsPrincipale = new List<Team>();

            var groups = phasePool.GetGroups();

            // Sépare les Principale / Consolante
            foreach (var group in groups)
            {
                for (int i = 0; i < 2; i++)
                {
                    teamsPrincipale.Add(group.GetTeam(i));
                    teamsConsolante.Add(group.GetTeam(i+2));
                }
            }

            // tri par ordre de puissance des équipes
            teamsPrincipale = teamsPrincipale
                .OrderByDescending(e => e.Stats.RankingPoint)
                .ThenByDescending(e => e.Stats.BonusPoint)
                .ThenByDescending(e => e.Stats.TotalPoint)
                .ToList();

            teamsConsolante = teamsConsolante
                .OrderByDescending(e => e.Stats.RankingPoint)
                .ThenByDescending(e => e.Stats.BonusPoint)
                .ThenByDescending(e => e.Stats.TotalPoint)
                .ToList();


            int index = 0;
            foreach (var team in teamsConsolante) // les 8 premières _teams[] sont les consolantes
            {
                Misc.Log($"Consolante {team.Stats.TeamName}");

                _teams[index].SetStats(team.Stats.Clone());
                index++;
            }
            foreach (var team in teamsPrincipale) // les 8 suivantes sont les principales
            {
                Misc.Log($"Principale {team.Stats.TeamName}");

                _teams[index].SetStats(team.Stats.Clone());
                index++;
            }

            int[] groupATeamIndex = [8, 1, 4, 5]; // B C se rencontre dans la dernière rotation les plus fort 1 et 4
            int[] groupBTeamIndex = [7, 2, 3, 6]; // B C se rencontre dans la dernière rotation les plus fort 2 et 3

            // les deux groupe consolante A & B
            for (int i = 0; i < 4; i++)
            {
                _groups[0].CopyTeamStats(i, teamsConsolante[groupATeamIndex[i] - 1]);
                _groups[1].CopyTeamStats(i, teamsConsolante[groupBTeamIndex[i] - 1]);
            }

            // les deux groupe principale A & B
            for(int i = 0; i < 4; i++)
            {
                _groups[2].CopyTeamStats(i, teamsPrincipale[groupATeamIndex[i] - 1]);
                _groups[3].CopyTeamStats(i, teamsPrincipale[groupBTeamIndex[i] - 1]);
            }


            ResetTeamsStatus(_teams);
            ResetAllMatchScorePoints(_matchs);
            ResetAllMatchSetPoints(_matchs);

            ResetTeamsTotalPoint();
            ResetTeamsBonusPoint();
            ResetTeamsScorePoint();

            // On remet à zéro le ranking point des équipes avant de commencer la phase
            for (int i = 0; i < _teams.Count; i++)
            {
                _teams[i].Stats.SetRankingPoint(0);
            }

            // Prépare la première rotation 
            _rotationManager.LoadFile(configFile, GetTeams(), GetMatchs());
            SetRotation(0, _rotationManager);

            // Reclasse les équipes en fonctione des points accumulés
            //for (int i = 0; i < _groups.Count; i++)
            //{
            //    var group = _groups[i];
            //    group.Refresh();
            //}

        }

        public void SetRotation(int rotation, RotationManager rotationManager)
        {
            if (rotationManager == null) return;
            if (rotation >= rotationManager.NbRotation) return;

            _currentRotation = rotation;
            var matchConfigs = rotationManager.GetMatchConfigs(rotation);

            for (int i = 0; i < matchConfigs.Count; i++)
            {
                var matchConfig = matchConfigs[i];
                if (matchConfig !=  null)
                {
                    var match = _matchs[matchConfig.IdTerrain];

                    match.SetMatchConfig(matchConfig);
                    //match.SetNbSetToWin(matchConfig.NbSetToWin);
                }
            }
        }
        private void RefreshGroups()
        {
            for (int i = 0; i < _groups.Count; i++)
            {
                var group = _groups[i];
                group.Refresh();
            }
        }
        //public void ShuffleTeamsTotalPoint()
        //{
        //    for (int i = 0; i < _teams.Count; i++)
        //    {
        //        var team = _teams[i];
        //        team.Stats.SetTotalPoint(0);
        //        team.Stats.SetTotalPoint(Misc.Rng.Next(0, 9));
        //    }

        //    RefreshGroups();
        //}
        //public void ShuffleTeamsPoint()
        //{
        //    for (int i = 0; i < _teams.Count; i++)
        //    {
        //        var team = _teams[i];
        //        team.Stats.SetScorePoint(Misc.Rng.Next(8, 20));
        //    }
        //}
        public void ResetTeamsScorePoint()
        {
            for (int i = 0; i < _teams.Count; i++)
            {
                var team = _teams[i];
                team.Stats.SetScorePoint(0);
            }
        }
        public void ResetTeamsTotalPoint()
        {
            for (int i = 0; i < _teams.Count; i++)
            {
                var team = _teams[i];
                team.Stats.SetTotalPoint(0);
            }
        }
        public void ResetTeamsBonusPoint()
        {
            for (int i = 0; i < _teams.Count; i++)
            {
                var team = _teams[i];
                team.Stats.SetBonusPoint(0);
            }
        }
        //private void RefreshNextTurnTimes(int rotation, RotationManager rotationManager, double delta)
        //{
        //    var matchTime = (rotationManager.GetMatchTime() * 2);
        //    var warmUpTime = rotationManager.GetWarmUpTime();

        //    double timeRemaining = 0;

        //    if ((int)State.CurState < (int)States.PoolValidPoints) timeRemaining = matchTime + warmUpTime;
        //    if ((int)State.CurState < (int)States.PoolPlay2) timeRemaining = matchTime / 2 + warmUpTime;
        //    if ((int)State.CurState < (int)States.PoolPlay1) timeRemaining = warmUpTime;

        //    for (int i = 0; i < _teams.Count; i++)
        //    {
        //        var team = _teams[i];
        //        team.RefreshNextTurnTime(rotation, rotationManager, delta - timeRemaining);
        //    }

        //    _endPhaseTime = DateTime.Now.AddSeconds((matchTime + warmUpTime) * (rotationManager.NbRotation - rotation) + delta);
        //}
        private void RefreshNextTurnTimes(int rotation, RotationManager rotationManager)
        {
            for (int i = 0; i < _teams.Count; i++)
            {
                var team = _teams[i];
                team.RefreshNextTurnTime(rotation, rotationManager);
            }

            var matchTime = rotationManager.GetMatchTime() * 2;
            var warmUpTime = rotationManager.GetWarmUpTime();

            _endPhaseTime = DateTime.Now.AddSeconds((matchTime + warmUpTime) * (rotationManager.NbRotation - rotation));
        }
        private void UpdateNextTurnTimes(TimeSpan delay)
        {
            for (int i = 0; i < _teams.Count; i++)
            {
                var team = _teams[i];
                team.SetNextTurnTime(team.NextTurn.StartTime + delay);
            }
        }
        private void HideNextTurnTimes(List<Team> teams)
        {
            for (int i = 0; i < teams.Count; i++)
            {
                var team = teams[i];
                team.SetIsShowNextTurns(false);
            }
        }
        private List<MatchConfig> GetNextMatchs(int rotation, RotationManager rotationManager)
        {
            if (rotation > rotationManager.NbRotation) return null;

            var matchConfigs = rotationManager.GetMatchConfigs(rotation + 1);

            return matchConfigs;
        }
        public List<Team> GetTeams() { return _teams; }
        public Team GetTeam(int index) { return _teams[index]; }
        public List<Match> GetMatchs() { return _matchs; }
        public Match GetMatch(int index) { return _matchs[index]; }
        public List<Group> GetGroups() { return _groups; }
        public Group GetGroup(int index) { return _groups[index]; }
        private void PlayCountDown(SoundEffect sound, double secondRemaining = 3d, float volume = .25f, float pitch = .1f, float pan = 0f)
        {
            if (TimerCountDown.OnRemaingTime(secondRemaining) && sound != null)
            {
                sound.Play(volume * Static.VolumeMaster, pitch, pan);
            }

            if (TimerCountDown.IsFinish())
            {
                Misc.Log("Finish CountDown");
                TimerCountDown.StopTimer();
                SetTicState((_ticState + 1) % Enums.Count<States>());
            }
        }
        private void RunState()
        {
            switch (State.CurState)
            {
                case States.PoolReady:
                    UpdateNextTurnTimes(_chrono.Elapsed);
                    break;

                case States.PoolNextMatch:
                    UpdateNextTurnTimes(_chrono.Elapsed);
                    break;

                case States.PoolFinishMatch:
                    UpdateNextTurnTimes(_chrono.Elapsed);
                    break;

                case States.PoolWarmUp:

                    PlayCountDown(null);
                    break;

                case States.PoolPlay1:

                    PlayCountDown(Static.SoundCountDown, 3);
                    break;

                case States.PoolPlay2:

                    PlayCountDown(Static.SoundCountDown, 3);
                    break;

                case States.PoolCountDown1:
                    UpdateNextTurnTimes(_chrono.Elapsed);
                    PlayCountDown(Static.SoundCountDown, 3);
                    break;
                case States.PoolCountDown2:
                    UpdateNextTurnTimes(_chrono.Elapsed);
                    PlayCountDown(Static.SoundCountDown, 3);
                    break;


                case States.PoolValidPoints:
                    UpdateNextTurnTimes(_chrono.Elapsed);
                    break;

                case States.PoolSwapSide:
                    UpdateNextTurnTimes(_chrono.Elapsed);
                    break;

                case States.PoolPreSwap:
                    UpdateNextTurnTimes(_chrono.Elapsed);
                    PlayCountDown(null);
                    break;

                default:
                    break;
            }
        }
        private void SetTicState(int ticProcess)
        {
            _ticState = ticProcess;

            States nextState = (States)ProcessStates[_ticState];

            State.Change(nextState);

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
                //if (ButtonControl.OnePress($"RefreshNextTurn", Static.Key.IsKeyDown(Keys.T)))
                //{
                //    RefreshNextTurnTimes(_currentRotation, _rotationManager, 0);
                //}

                if (ButtonControl.OnePress($"SwapTeams", Static.Key.IsKeyDown(Keys.S)))
                {
                    for (int i = 0;i <_matchs.Count;i++)
                        _matchs[i].Court.SwapTeams();
                }

                bool canForward = 
                    State.CurState == States.PoolNextMatch || 
                    State.CurState == States.PoolReady || 
                    State.CurState == States.PoolSwapSide || 
                    State.CurState == States.PoolFinishMatch ||
                    State.CurState == States.PoolValidPoints;

                if (ButtonControl.OnePress($"Space{Id}", Keyboard.GetState().IsKeyDown(Keys.Space)))// && canForward))
                {


                    if (_currentRotation < _rotationManager.NbRotation)
                    {
                        Misc.Log($"Etape {_ticState}");

                        _ticState++;

                        // Boucle sur le prochain match
                        if (_ticState > (int)States.PoolValidPoints)
                            _ticState = (int)States.PoolNextMatch;

                        SetTicState(_ticState);
                    }
                }

                // Debug
                //if (ButtonControl.OnePress($"ShufflePoint{Id}", Keyboard.GetState().IsKeyDown(Keys.NumPad0)))
                //{
                //    if (State.CurState == States.PoolPlay1)
                //        ShuffleTeamsPoint();
                //}
                //if (ButtonControl.OnePress($"ShuffleTotalPoint{Id}", Keyboard.GetState().IsKeyDown(Keys.NumPad1)))
                //{
                //    //if (State.CurState == States.Play1)
                //        ShuffleTeamsTotalPoint();
                //}


                //if (ButtonControl.OnePress($"Reset{Id}", Keyboard.GetState().IsKeyDown(Keys.Back)))
                //{
                //    SetTicRotation(0);
                //    ResetTeamsStatus(_teams);
                //    ResetTeamsTotalPoint();
                //    SetRotation(0, _rotationManager);
                //}

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
                batch.Draw(Static.TexBG01, AbsXY, Color.White);
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


                // Affichage Prochain Matchs
                var nextMatchs = GetNextMatchs(_currentRotation, _rotationManager);
                for (int i = 0; i < _matchs.Count; i++)
                {
                    var match = _matchs[i];
                    var matchConfig = nextMatchs[i];
                    DrawNextMatchs(batch, match.AbsRectF.TopCenter - Vector2.UnitY * 4, matchConfig);
                }
            }

            if (indexLayer == (int)Layers.Debug)
            {
                //batch.LeftTopString(Static.FontMini, "  Victoire = 3 pts\n  Nul = 1 pts\n  Défaite = 0 pts\n+ Bonus écart et points total", AbsRectF.TopLeft + Vector2.UnitX * 40 + Vector2.UnitY * 60, Color.Gray);

                //Static.DrawRoundedRectangle(batch, Static.TexLine, new Rectangle(100, 100, 800, 400),  Color.White, 30, 30, 30, 80, 3, 24);

                //batch.LineTexture(Static.TexLine, Vector2.Zero, Static.MousePos, 9, Color.Gold);

                //Static.DrawArcFilled(batch, Vector2.One * 600, 1200, Geo.RAD_0, Geo.RAD_90, Color.Red);

                //batch.RightMiddleString(Static.FontMini, $"{(int)State.CurState} {State.CurState} Temps = {_rotationManager.Duration}s", AbsRectF.TopRight + Vector2.UnitY * 20 - Vector2.UnitX * 20, Color.Cyan);

                //batch.CenterStringXY(Static.FontMain, $"{_chrono.Elapsed.Seconds}", AbsRectF.TopCenter + Vector2.UnitY * 20, Color.Yellow);
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
                {
                    batch.CenterStringXY(Static.FontMini, "Début des Matchs", pos + Vector2.UnitY * 40, Color.Yellow);
                    batch.CenterStringXY(Static.FontMini, _startPhaseTime.ToString("HH:mm"), pos - Vector2.UnitY * 40, Color.Yellow);
                }

                if (i == _rotationManager.NbRotation - 2)
                {

                    batch.CenterStringXY(Static.FontMini, "Fin des Matchs", pos2 + Vector2.UnitY * 40, Color.Yellow);
                    batch.CenterStringXY(Static.FontMini, _endPhaseTime.ToString("HH:mm"), pos2 - Vector2.UnitY * 40, Color.Yellow);
                }


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

        private void DrawNextMatchs(SpriteBatch batch, Vector2 position, MatchConfig matchConfig)
        {
            if (matchConfig == null) return;

            var vsText = $" {matchConfig.TeamA.Stats.TeamName}   vs   {matchConfig.TeamB.Stats.TeamName} ";
            var aText = $" ¬{matchConfig.TeamReferee.Stats.TeamName} ";

            //batch.FillRectangleCentered(position + Vector2.UnitX * (size.X / 2), size + new Vector2(0, -20), Color.Black * .5f, 0);
            //batch.LeftMiddleString(Static.FontMini, vsText, position, Color.Red);

            batch.FillRectangleCentered(position - Vector2.UnitY * 30, Static.FontMini.MeasureString(" Prochain Match ") + new Vector2(0, -20), Color.Black * .5f, 0);
            batch.CenterStringXY(Static.FontMini, " Prochain Match ", position - Vector2.UnitY * 30, Color.WhiteSmoke);

            //batch.FillRectangleCentered(position, Static.FontMini.MeasureString(vsText) + new Vector2(0, -20), Color.Black * .5f, 0);
            batch.CenterStringXY(Static.FontMini, vsText, position, Color.YellowGreen);

            //batch.FillRectangleCentered(position + Vector2.UnitY * 30, Static.FontMini.MeasureString(aText) + new Vector2(0, -20), Color.Black * .5f, 0);
            batch.CenterStringXY(Static.FontMini, aText, position + Vector2.UnitY * 30, Color.Orange);

        }
    }
}
