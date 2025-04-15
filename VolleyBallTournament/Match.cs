using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Framework.Devices.Sensors;
using Mugen.Core;
using Mugen.GFX;
using Mugen.GUI;
using Mugen.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace VolleyBallTournament
{
    public class Match : Node
    {
        public enum States
        {
            NextMatch,
            WarmUp,
            Ready,
            CountDown1,
            Play1,
            PreSwap,
            SwapSide,
            CountDown2,
            Play2,
            Finish,
            ValidPoints,

            BeginDemiFinal,
            SetNextMatch,
            SetWarmUp,

            SetReady,
            SetPlay,
            SetFinish,
            SetValidPoints,
            SetSwapSide,
        }
        public static List<int> Process { get; private set; } = Enums.GetList<States>();
        public State<States> State { get; private set; } = new State<States>(States.Ready);

        public Dictionary<States, string> Infos = new Dictionary<States, string>()
        {
            {States.NextMatch, "Prochain match"},
            {States.WarmUp, "Echauffement"},
            {States.Ready, "Joueurs en place"},
            {States.CountDown1, "Début du Match"},
            {States.Play1, "Manche 1 en cours"},
            {States.PreSwap, "Fin de Manche 1"},
            {States.SwapSide, "Changement de côté"},
            {States.CountDown2, "Reprise du Match"},
            {States.Play2, "Manche 2 en cours"},
            {States.Finish, "Fin du match"},
            {States.ValidPoints, "Validation des Points"},

            {States.BeginDemiFinal, "Debut des Demi Finales"},

            {States.SetNextMatch, "Prochaine Demi Finale"},
            {States.SetWarmUp, "Echauffement"},

            {States.SetReady, "Joueurs en place"},
            {States.SetPlay, "Set en cours"},
            {States.SetFinish, "Fin du Set"},
            {States.SetValidPoints, "Validation des Points"},
            {States.SetSwapSide, "Changement de côté"},

        };

        public bool IsFreeCourt => _isFreeCourt;
        private bool _isFreeCourt = false; // terrain libre
        public int NbSetToWin => _nbSetToWin;
        private int _nbSetToWin = 2;

        public int IdTerrain => _idTerrain;
        private int _idTerrain = Const.NoIndex;

        //public ScorePanel ScorePanel => _scorePanel;
        //private ScorePanel _scorePanel;
        public Court Court => _court;
        private Court _court;

        private Container _div;

        //private int _nbTeamPerGroup = 3;
        private List<MatchConfig> _matchConfigs;
        private static int IndexMatch = 0;
        private int _ticState = 0;

        public Team TeamA => _teamA;
        private Team _teamA;
        public Team TeamB => _teamB;
        private Team _teamB;
        public Team TeamReferee => _teamReferee;
        private Team _teamReferee;

        public Team TeamHasService => _teamHasService;
        private Team _teamHasService;
        public Team LastTeamHasService => _lastTeamHasService;
        private Team _lastTeamHasService;

        public Action<Team> OnChangeService;
        
        public Match(int idTerrain, string courtName, Team teamA, Team teamB, Team teamReferee)//, int nbTeamPerGroup)
        {
            _idTerrain = idTerrain;

            _div = new Container(Style.Space.One * 10, new Style.Space(80,80,160,160), Mugen.Physics.Position.VERTICAL);

            //_scorePanel = new ScorePanel(this);
            _court = new Court(courtName, this);

            //_scorePanel.AppendTo(this);
            _court.AppendTo(this);

            SetTeam(teamA, teamB, teamReferee);//, nbTeamPerGroup);

            //_div.Insert(_scorePanel);
            _div.Insert(_court);
            _div.Refresh();

            SetSize(_div._rect.Width, _div._rect.Height);

            //State.Set(States.NextMatch);

            DefineStates();

        }
        public void SetPhaseDemiFinal(PhaseDemiFinal phaseDemiFinal)
        {
            _matchConfigs = phaseDemiFinal.MatchConfigs;
        }
        private void DefineStates()
        {
            State.On(States.SetWarmUp, () =>
            {
                Static.SoundStart.Play(0.5f * Static.VolumeMaster, .01f, 0f);

            });
            State.Off(States.SetWarmUp, () =>
            {
                Static.SoundStart.Play(0.5f * Static.VolumeMaster, .01f, 0f);
            });

            State.On(States.SetPlay, () =>
            {
                Static.SoundStart.Play(0.5f * Static.VolumeMaster, 0.01f, 0f);
            });

            State.On(States.SetSwapSide, () =>
            {
                // On change de côté
                 Court.SwapTeams();
            });

            State.On(States.SetNextMatch, () =>
            {
                ResetTeamsStatus();
                ResetScorePoints();
                ResetSets();

                //SetRotation(_currentRotation, _matchConfigs);
                ImportMatchConfigDemi(_matchConfigs);

            });
            State.On(States.SetReady, () =>
            {
                //_timer.StopTimer();
                ResetScorePoints();
            });
            State.On(States.SetFinish, () =>
            {
                Static.SoundStart.Play(0.5f * Static.VolumeMaster, 0.01f, 0f);
            });
            State.On(States.SetValidPoints, () =>
            {
                Static.SoundBeep.Play(1f * Static.VolumeMaster, 0.0001f, 0f);
                ValidSet();
            });
            State.Off(States.SetValidPoints, () =>
            {
                //Static.SoundRanking.Play(1f * Static.VolumeMaster, 0.0001f, 0f);
                Static.SoundBeep.Play(1f * Static.VolumeMaster, 0.0001f, 0f);
                // On retribut les points dans les team respectif qui viennent de finir de jouer
                if (GetWinner() == null)
                {
                    Misc.Log("Match Null");
                    TeamA.Stats.AddRankingPoint(1);
                    TeamB.Stats.AddRankingPoint(1);

                    TeamA.Stats.AddResult(Result.Null);
                    TeamB.Stats.AddResult(Result.Null);
                }
                else
                {
                    GetWinner().Stats.AddRankingPoint(3);
                    GetWinner().Stats.AddResult(Result.Win);
                    GetLooser().Stats.AddResult(Result.Loose);
                }

                TeamA.Stats.ValidBonusPoint();
                TeamB.Stats.ValidBonusPoint();

                //_currentRotation++;
                ////_step = int.Clamp(_step, 0, _nbStep - 1);
                //if (_currentRotation >= NbRotation)
                //{
                //    Misc.Log("Fin des matchs");
                //    for (int i = 0; i < _matchs.Count; i++)
                //        _matchs[i].SetIsFreeCourt(true);

                //    if (OnFinishPhase != null)
                //        OnFinishPhase(this);
                //}

            });

        }
        private void ImportMatchConfigDemi(List<MatchConfig> matchConfigs)
        {
            Misc.Log($"ImportMatchConfigDemi {IndexMatch}");
            if (matchConfigs == null) return;

            if (IndexMatch < 0 || IndexMatch > matchConfigs.Count) return;

            var matchConfig = matchConfigs[IndexMatch];
            if (matchConfig != null)
            {
                SetTeam(matchConfig.TeamA, matchConfig.TeamB, matchConfig.TeamReferee);
                SetNbSetToWin(matchConfig.NbSetToWin);
            }

            IndexMatch++;
            IndexMatch = int.Clamp(IndexMatch, 0, matchConfigs.Count);
        }
        public void SetTicState(int ticProcess)
        {
            _ticState = ticProcess;
            States nextState = (States)Process[_ticState];
            State.Set(nextState);
        }
        public void SetIsFreeCourt(bool isFreeCourt) { _isFreeCourt = isFreeCourt; }
        public void ChangeTeamHasService(Team team)
        {
            _lastTeamHasService = _teamHasService;

            _teamHasService = team;

            if (_teamHasService != _lastTeamHasService)
            {
                if (OnChangeService != null)
                    OnChangeService(_teamHasService);
            }

            GetTeamOppenent(_teamHasService).SetService(false);
            _teamHasService.SetService(true);

        }
        public void SetTeamHasService(Team team)
        {
            _teamHasService = team;
            _teamHasService.SetService(true);
            _lastTeamHasService = team;
            GetTeamOppenent(_teamHasService).SetService(false);
        }
        public void CancelAction()
        {
            Misc.Log("Cancel Action");
            ChangeTeamHasService(_lastTeamHasService);
        }
        public void ValidSet()
        {
            var winner = GetWinner();
            TeamA.Stats.AddScoreSet(new Set(winner == TeamA, TeamA.Stats.ScorePoint));
            TeamB.Stats.AddScoreSet(new Set(winner == TeamB, TeamB.Stats.ScorePoint));
        }
        public void SetNbSetToWin(int nbSetToWin)
        {
            _nbSetToWin = nbSetToWin;
        }
        public Team GetWinner()
        {
            if (_teamA.Stats.ScorePoint == _teamB.Stats.ScorePoint) return null;
            
            return _teamA.Stats.ScorePoint > _teamB.Stats.ScorePoint ? _teamA : _teamB;
        }
        public Team GetLooser()
        {
            if (_teamA.Stats.ScorePoint == _teamB.Stats.ScorePoint) return null;

            return _teamA.Stats.ScorePoint < _teamB.Stats.ScorePoint ? _teamA : _teamB;
        }
        public Team GetTeamOppenent(Team team)
        {
            if (team == _teamA)
                return _teamB;
            if (team == _teamB)
                return _teamA;

            return null;
        }
        public void SetTeam(Team teamA, Team teamB, Team teamReferee)//, int nbTeamPerGroup)
        {
            _teamA = teamA;
            _teamB = teamB;
            _teamReferee = teamReferee;

            _teamA.SetIsPlaying(true);
            _teamB.SetIsPlaying(true);
            _teamReferee.SetIsReferee(true);

            SetTeamHasService(_teamA);

            //if (_teamA.NbMatchPlayed < nbTeamPerGroup - 1)
                _teamA.SetMatch(this);
            
            //if (_teamB.NbMatchPlayed < nbTeamPerGroup - 1)
                _teamB.SetMatch(this);

            //if (_teamReferee.NbMatchPlayed < nbTeamPerGroup - 1)
                _teamReferee.SetMatch(this);
        }
        public void ResetSets()
        {
            TeamA.Stats.Sets.Clear();
            TeamB.Stats.Sets.Clear();
        }
        public void ResetScorePoints()
        {
            TeamA.Stats.SetScorePoint(0);
            TeamB.Stats.SetScorePoint(0);
        }
        public void ResetTeamsStatus()
        {
            TeamA.ResetTeamStatus();
            TeamB.ResetTeamStatus();
        }
        public static void ResetTeamsStatus(List<Team> teams)
        {
            for (int i = 0; i < teams.Count; i++)
            {
                var team = teams[i];
                team.ResetTeamStatus();
            }
        }
        public static void ResetAllMatchScorePoints(List<Match> matchs)
        {
            for (int i = 0; i < matchs.Count; i++)
            {
                var match = matchs[i];
                match.ResetScorePoints();
            }
        }
        public static void ResetAllMatchSetPoints(List<Match> matchs)
        {
            for (int i = 0; i < matchs.Count; i++)
            {
                var match = matchs[i];
                match.ResetSets();
            }
        }

        public void AddPointA(int points)
        {
            //Misc.Log($"{points}");

            if (State.CurState == States.Play1 || State.CurState == States.Play2 || State.CurState == States.Finish || State.CurState == States.SetPlay)
            {
                if (points == 0) return;

                if (points > 0)
                {
                    ChangeTeamHasService(TeamA);
                }

                if (points < 0)
                    CancelAction();

                _teamA.Stats.AddPoint(points);
                new PopInfo(points > 0 ? $"+{points}" : $"{points}", points > 0 ? Color.GreenYellow : Color.Red, Color.Black, 0, 16, 32).SetPosition(_court.ScoreAPos - Vector2.UnitY * 64).AppendTo(_parent);
                new FxExplose(_court.ScoreAPos, points > 0 ? Color.GreenYellow : Color.Red, 20, 20, 80).AppendTo(_parent);

                Static.SoundPoint.Play(.25f, .01f, 0f);
            }
            else if (State.CurState == States.Ready)
            {
                ChangeTeamHasService(TeamA);
            }
        }
        public void AddPointB(int points)
        {
            //Misc.Log($"{points}");

            if (State.CurState == States.Play1 || State.CurState == States.Play2 || State.CurState == States.Finish || State.CurState == States.SetPlay)
            {
                if (points == 0) return;

                if (points > 0)
                {
                    ChangeTeamHasService(TeamB);
                }

                if (points < 0)
                    CancelAction();

                _teamB.Stats.AddPoint(points);
                new PopInfo(points > 0 ? $"+{points}" : $"{points}", points > 0 ? Color.GreenYellow : Color.Red, Color.Black, 0, 16, 32).SetPosition(_court.ScoreBPos - Vector2.UnitY * 64).AppendTo(_parent);
                new FxExplose(_court.ScoreBPos, points > 0 ? Color.GreenYellow : Color.Red, 20, 20, 80).AppendTo(_parent);

                Static.SoundPoint.Play(.25f, .01f, 0f);
            }
            else if (State.CurState == States.Ready)
            {
                ChangeTeamHasService(TeamB);
            }
        }
        private void RunState()
        {
            switch (State.CurState)
            {
                case States.Ready:

                    break;
                case States.NextMatch:

                    break;
                case States.Finish:

                    break;
                case States.WarmUp:
                    break;

                case States.Play1:

                    break;
                case States.CountDown1:
                    break;

                case States.CountDown2:
                    break;

                case States.SwapSide:
                    break;

                case States.Play2:
                    break;

                case States.ValidPoints:
                    break;
                case States.PreSwap:
                    break;
                case States.SetNextMatch:
                    break;
                case States.SetWarmUp:
                    break;
                case States.SetReady:
                    break;
                case States.SetPlay:

                    if (ButtonControl.OnePress($"AddPointA{_idTerrain}", Keyboard.GetState().IsKeyDown((Keys)112 + _idTerrain * 4)))
                    {
                        AddPointA(+1);
                    }
                    if (ButtonControl.OnePress($"SubPointA{_idTerrain}", Keyboard.GetState().IsKeyDown((Keys)113 + _idTerrain * 4)))
                    {
                        AddPointA(-1);
                    }
                    if (ButtonControl.OnePress($"AddPointB{_idTerrain}", Keyboard.GetState().IsKeyDown((Keys)114 + _idTerrain * 4)))
                    {
                        AddPointB(-1);
                    }
                    if (ButtonControl.OnePress($"SubPointB{_idTerrain}", Keyboard.GetState().IsKeyDown((Keys)115 + _idTerrain * 4)))
                    {
                        AddPointB(+1);
                    }

                    if (GetWinner() != null)
                        if (GetWinner().Stats.ScorePoint >= 25)
                            SetTicState(_ticState + 1);

                    break;
                case States.SetFinish:
                    break;
                case States.SetValidPoints:
                    break;
                case States.SetSwapSide:
                    break;
                default:
                    break;
            }
        }
        public override Node Update(GameTime gameTime)
        {
            UpdateRect();

            if ((int)State.CurState >= (int)States.BeginDemiFinal)
            {
                // Debug
                if (ButtonControl.OnePress($"SwapTeams{_idTerrain}Demi", Static.Key.IsKeyDown(Keys.S)))
                {
                    Court.SwapTeams();
                }

                if (ButtonControl.OnePress($"Space{_idTerrain}Demi", Keyboard.GetState().IsKeyDown(Keys.Space))) GotoNextState();
            }

            RunState();

            UpdateChilds(gameTime);

            return base.Update(gameTime);
        }
        private void GotoNextState()
        {
            Misc.Log($"Etape {_ticState} Terrain {_idTerrain}");

            _ticState++;

            if (_ticState > (int)States.SetValidPoints)
            {
                _ticState = (int)States.SetReady;
            }

            SetTicState(_ticState); // reviens a la première étape automatiquement si _ticState atteint la dernière étape + 1
        }
        public override Node Draw(SpriteBatch batch, GameTime gameTime, int indexLayer)
        {
            if (indexLayer == (int)Layers.Main)
            {
                //batch.FillRectangle(AbsRectF, Color.MonoGameOrange * .25f);
                //batch.Rectangle(AbsRectF, Color.White, 3f);

                //batch.Line(AbsRectF.LeftMiddle, AbsRectF.RightMiddle, Color.White, 3f);

                //var threeMeter = Vector2.UnitY * 50;

                //batch.Line(AbsRectF.LeftMiddle - threeMeter, AbsRectF.RightMiddle - threeMeter, Color.White, 1f);
                //batch.Line(AbsRectF.LeftMiddle + threeMeter, AbsRectF.RightMiddle + threeMeter, Color.White, 1f);

                //batch.CenterBorderedStringXY(Static.FontMain, _courtName, AbsRectF.TopCenter - Vector2.UnitY * 20, Color.Yellow, Color.Black);
            }

            if (indexLayer == (int)Layers.Debug)
            {
                //batch.LeftTopString(Static.FontMini, $"{_teamHasService.TeamName}", AbsXY + new Vector2(10, 10), Color.Red);
                //batch.LeftTopString(Static.FontMini, $"{_lastTeamHasService.TeamName}", AbsXY + new Vector2(10, 40), Color.Red);
            }

            DrawChilds(batch, gameTime, indexLayer);

            return base.Draw(batch, gameTime, indexLayer);
        }
    }
}
