﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Mugen.Core;
using Mugen.GFX;
using Mugen.GUI;
using Mugen.Input;
using System;
using System.Collections.Generic;
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

        public TimerCountDown Timer => _timer;
        private readonly TimerCountDown _timer;

        private int _currentRotation = 0;
        private RotationManager _rotationManager;

        private int Id;

        public Action<PhasePool> OnFinishPhase;

        private DateTime _startRotationTime;
        private DateTime _endRotationTime;

        Game _game;
        public PhasePool(Game game, int id, string title, RotationManager rotationManager, PhaseRegister phaseRegister = null, int nbGroup = 4, int nbTeamPerGroup = 4, int nbMatch = 3)
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
                CreateGroups(nbGroup, nbTeamPerGroup, ["Consolante A", "Consolante B", "Principale A", "Principale B"]);
                CreateMatchs(nbMatch);
            }

            _timer = (TimerCountDown)new TimerCountDown().AppendTo(this);
            _divTimer.Insert(_timer);

            _divMain.Insert(_divTimer);
            _divMain.Insert(_divMatch);
            _divMain.Insert(_divGroup);

            _divMain.SetPosition((Screen.Width - _divMain.Rect.Width) / 2, (Screen.Height - _divMain.Rect.Height) / 2);
            _divMain.Refresh();

            SetTicState((int)States.PoolNextMatch);

            DefineStates();
        }
        private void DefineStates()
        {
            State.On(States.PoolWarmUp, () =>
            {
                if (_currentRotation == 0)
                    _startRotationTime = DateTime.Now;

                Static.SoundStart.Play(0.5f * Static.VolumeMaster, .01f, 0f);

                Timer.SetDuration(_rotationManager.GetWarmUpTime(_currentRotation));
                Timer.StartTimer();
            });
            State.Off(States.PoolWarmUp, () =>
            {
                Static.SoundStart.Play(0.5f * Static.VolumeMaster, .01f, 0f);

                Timer.SetDuration(0);
            });

            State.On(States.PoolCountDown1, () =>
            {
                Static.SoundCountDown.Play(0.5f * Static.VolumeMaster, .1f, 0f);
                Timer.SetDuration(3);
                Timer.StartTimer();
            });
            State.On(States.PoolCountDown2, () =>
            {
                Static.SoundCountDown.Play(0.5f * Static.VolumeMaster, .1f, 0f);
                Timer.SetDuration(3);
                Timer.StartTimer();
            });

            State.On(States.PoolPlay1, () =>
            {
                Static.SoundStart.Play(0.5f * Static.VolumeMaster, 0.01f, 0f);
                Timer.SetDuration(_rotationManager.GetMatchTime(_currentRotation));
                Timer.StartTimer();
                //Misc.Log("On Play");
            });
            State.Off(States.PoolPlay1, () =>
            {
                //Static.SoundStart.Play(0.5f * Static.VolumeMaster, 0.1f, 0f);
                Timer.StopTimer();
                //Misc.Log("Off Play");
            });

            State.On(States.PoolPlay2, () =>
            {
                Static.SoundStart.Play(0.5f * Static.VolumeMaster, 0.01f, 0f);
                Timer.SetDuration(_rotationManager.GetMatchTime(_currentRotation));
                Timer.StartTimer();
                //Misc.Log("On Play");
            });
            State.Off(States.PoolPlay2, () =>
            {
                //Static.SoundStart.Play(0.5f * Static.VolumeMaster, 0.1f, 0f);
                Timer.StopTimer();
                //Misc.Log("Off Play");
            });

            State.On(States.PoolPreSwap, () =>
            {
                Static.SoundStart.Play(0.5f * Static.VolumeMaster, 0.01f, 0f);
                Timer.SetDuration(2);
                Timer.StartTimer();

                //ValidSets();
            });

            State.On(States.PoolSwapSide, () =>
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

            State.On(States.PoolNextMatch, () =>
            {
                ResetTeamsStatus(_teams);
                ResetAllMatchScorePoints(_matchs);
                ResetAllMatchSetPoints(_matchs);

                SetRotation(_currentRotation, _rotationManager);

            });
            State.On(States.PoolReady, () =>
            {
                _timer.StopTimer();
            });
            State.On(States.PoolFinishMatch, () =>
            {
                Static.SoundStart.Play(0.5f * Static.VolumeMaster, 0.01f, 0f);
            });
            State.On(States.PoolValidPoints, () =>
            {
                Static.SoundBeep.Play(1f * Static.VolumeMaster, 0.0001f, 0f);
                ValidSets(1);
            });
            State.Off(States.PoolValidPoints, () =>
            {
                //Static.SoundRanking.Play(1f * Static.VolumeMaster, 0.0001f, 0f);
                Static.SoundBeep.Play(1f * Static.VolumeMaster, 0.0001f, 0f);
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

        public void ValidSets(int pointGap)
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
        public void ShuffleTeamsTotalPoint()
        {
            for (int i = 0; i < _teams.Count; i++)
            {
                var team = _teams[i];
                team.Stats.SetTotalPoint(0);
                team.Stats.SetTotalPoint(Misc.Rng.Next(0, 9));
            }
            for (int i = 0; i < _groups.Count; i++)
            {
                var group = _groups[i];
                group.Refresh();
            }
        }
        public void ShuffleTeamsPoint()
        {
            for (int i = 0; i < _teams.Count; i++)
            {
                var team = _teams[i];
                team.Stats.SetScorePoint(Misc.Rng.Next(8, 20));
            }
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
        public void RefreshNextTurnTimes(RotationManager rotationManager)
        {
            for (int i = 0; i < _teams.Count; i++)
            {
                var team = _teams[i];
                var nextTurn = team.FindNextTurnTime(_currentRotation, rotationManager);

                //team.SetNextTurn(nextTurn);
                var matchTime = rotationManager.GetMatchTime(0);
                var warmUpTime = rotationManager.GetWarmUpTime(0);
                _endRotationTime =  DateTime.Now.AddSeconds((matchTime * 2 + warmUpTime) * (rotationManager.NbRotation - _currentRotation));
            }
        }
        public List<Team> GetTeams() { return _teams; }
        public Team GetTeam(int index) { return _teams[index]; }
        public List<Match> GetMatchs() { return _matchs; }
        public Match GetMatch(int index) { return _matchs[index]; }
        public List<Group> GetGroups() { return _groups; }
        public Group GetGroup(int index) { return _groups[index]; }
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
                SetTicState((_ticState + 1) % Enums.Count<States>());
            }
        }
        private void RunState()
        {
            switch (State.CurState)
            {
                case States.PoolReady:

                    break;

                case States.PoolNextMatch:

                    break;

                case States.PoolFinishMatch:

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

                    PlayCountDown(Static.SoundCountDown, 3);
                    break;
                case States.PoolCountDown2:

                    PlayCountDown(Static.SoundCountDown, 3);
                    break;


                case States.PoolValidPoints:
                    break;

                case States.PoolSwapSide:
                    break;

                case States.PoolPreSwap:

                    PlayCountDown(null);
                    break;

                default:
                    break;
            }
        }
        private void SetTicState(int ticProcess)
        {
            _ticState = ticProcess;

            States nextState = (States)Process[_ticState];

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
                if (ButtonControl.OnePress($"RefreshNextTurn", Static.Key.IsKeyDown(Keys.T)))
                {
                    RefreshNextTurnTimes(_rotationManager);
                }

                if (ButtonControl.OnePress($"SwapTeams", Static.Key.IsKeyDown(Keys.S)))
                {
                    for (int i = 0;i <_matchs.Count;i++)
                        _matchs[i].Court.SwapTeams();
                }

                if (ButtonControl.OnePress($"Space{Id}", Keyboard.GetState().IsKeyDown(Keys.Space)))
                {
                    // Actualise les horaires d'attente des match et arbitrage !
                    RefreshNextTurnTimes(_rotationManager);

                    if (_currentRotation < _rotationManager.NbRotation)
                    {
                        Misc.Log($"Etape {_ticState}");

                        _ticState++;

                        if (_ticState > (int)States.PoolValidPoints)
                            _ticState = (int)States.PoolNextMatch;

                        SetTicState(_ticState); // reviens a la première étape automatiquement si _ticRotation atteint la dernière étape + 1
                        //SetTicRotation((_ticRotation + 1) % Enums.Count<States>()); // reviens a la première étape automatiquement si _ticRotation atteint la dernière étape + 1
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
                DrawRotation(batch, AbsXY + Vector2.UnitX * 620 + new Vector2((Screen.Width - (_rotationManager.NbRotation - 1) * 64)/2, 120));
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
                {
                    batch.CenterStringXY(Static.FontMini, "Début des Rotations", pos + Vector2.UnitY * 40, Color.Yellow);
                    batch.CenterStringXY(Static.FontMini, _startRotationTime.ToString("HH:mm"), pos - Vector2.UnitY * 40, Color.Yellow);
                }

                if (i == _rotationManager.NbRotation - 2)
                {

                    batch.CenterStringXY(Static.FontMini, "Fin des Rotations", pos2 + Vector2.UnitY * 40, Color.Yellow);
                    batch.CenterStringXY(Static.FontMini, _endRotationTime.ToString("HH:mm"), pos2 - Vector2.UnitY * 40, Color.Yellow);
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
    }
}
