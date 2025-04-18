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
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace VolleyBallTournament
{
    public class Match : Node
    {
        //static int Terminator = 0;

        public enum States
        {
            PoolNextMatch,
            PoolWarmUp,
            PoolReady,
            PoolCountDown1,
            PoolPlay1,
            PoolPreSwap,
            PoolSwapSide,
            PoolCountDown2,
            PoolPlay2,
            PoolFinishMatch,
            PoolValidPoints,

            DemiFinalBegin,
            
            DemiNextMatch,
            DemiWarmUp,
            DemiReady,
            DemiPlay,
            DemiFinishSet,
            DemiValidPoints,
            DemiSwapSide,
            DemiFinishMatch,

            DemiFinalEnd,
            
        }
        public static List<int> Process { get; private set; } = Enums.GetList<States>();
        public State<States> State { get; private set; } = new State<States>(States.PoolReady);

        public Dictionary<States, string> Infos = new Dictionary<States, string>()
        {
            {States.PoolNextMatch, "Prochain match"},
            {States.PoolWarmUp, "Echauffement"},
            {States.PoolReady, "Joueurs prêt"},
            {States.PoolCountDown1, "Début du Match"},
            {States.PoolPlay1, "Manche 1 en cours"},
            {States.PoolPreSwap, "Fin de Manche 1"},
            {States.PoolSwapSide, "Changement de côté"},
            {States.PoolCountDown2, "Reprise du Match"},
            {States.PoolPlay2, "Manche 2 en cours"},
            {States.PoolFinishMatch, "Dernière balle si égalité"},
            {States.PoolValidPoints, "Validation des Points"},

            {States.DemiFinalBegin, "Debut des Demi Finales"},

            {States.DemiNextMatch, "Prochaine Demi Finale"},
            {States.DemiWarmUp, "Echauffement"},

            {States.DemiReady, "Joueurs en place"},
            {States.DemiPlay, "Set en cours"},
            {States.DemiFinishSet, "Fin du Set"},
            {States.DemiValidPoints, "Validation des Points"},
            {States.DemiSwapSide, "Changement de côté"},
            {States.DemiFinishMatch, "Fin du Match"},
            {States.DemiFinalEnd, "Fin de Demi Finales"},

        };

        public bool IsFreeCourt => _isFreeCourt;
        private bool _isFreeCourt = false; // terrain libre

        public int IdTerrain => _idTerrain;
        private int _idTerrain = Const.NoIndex;

        public Court Court => _court;
        private Court _court;

        private Container _div;

        private List<MatchConfig> _matchConfigs;
        private MatchConfig _currentMatchConfig;
        private static int IndexMatch = 0;
        private int _ticState = 0;

        private int _pointGap = 1;

        public Team TeamA => _teamA;
        private Team _teamA;
        public Team TeamB => _teamB;
        private Team _teamB;
        public Team TeamReferee => _teamReferee;
        private Team _teamReferee;

        private Team _teamHasService;
        private Team _lastTeamHasService;

        public Action<Team> OnChangeService;

        Game _game;
        public Match(Game game, string courtName, MatchConfig matchConfig)//, int nbTeamPerGroup)
        {
            _idTerrain = matchConfig.IdTerrain;

            _div = new Container(Style.Space.One * 10, new Style.Space(110,80,160,160), Mugen.Physics.Position.VERTICAL);

            _court = new Court(courtName, this);
            _court.AppendTo(this);

            SetMatchConfig(matchConfig);

            _div.Insert(_court);
            _div.Refresh();

            SetSize(_div._rect.Width, _div._rect.Height);

            DefineStates();

        }
        private void DefineStates()
        {
            State.On(States.DemiFinalBegin, () => 
            {
                _pointGap = 2;
            });

            State.On(States.DemiWarmUp, () =>
            {
                Static.SoundStart.Play(0.5f * Static.VolumeMaster, .01f, 0f);

            });
            State.Off(States.DemiWarmUp, () =>
            {
                Static.SoundStart.Play(0.5f * Static.VolumeMaster, .01f, 0f);
            });

            State.On(States.DemiPlay, () =>
            {
                Static.SoundStart.Play(0.5f * Static.VolumeMaster, 0.01f, 0f);
            });

            State.On(States.DemiNextMatch, () =>
            {
                Static.SoundVictory.Play(0.5f * Static.VolumeMaster, 0.01f, 0f);
                Misc.Log("SET NEXT MATCH");
                ResetTeamsStatus();
                //ResetScorePoints();
                //ResetSets();
                //ResetResults();

                if (!ImportMatchConfigDemi())
                {
                    SetTicState((int)States.DemiFinalEnd);
                }

            });
            State.On(States.DemiReady, () =>
            {
                //_timer.StopTimer();
                
            });
            State.On(States.DemiFinishSet, () =>
            {
                Static.SoundStart.Play(0.5f * Static.VolumeMaster, 0.01f, 0f);
            });
            State.On(States.DemiValidPoints, () =>
            {
                Static.SoundBeep.Play(1f * Static.VolumeMaster, 0.0001f, 0f);
                ValidSet(_pointGap);

                if (GetLooser(_pointGap) != null)
                    GetLooser(_pointGap).Stats.AddResult(Result.Loose);

                if (GetWinner(_pointGap) != null)
                    GetWinner(_pointGap).Stats.AddResult(Result.Win);

            });
            State.Off(States.DemiValidPoints, () =>
            {
                //Static.SoundBeep.Play(1f * Static.VolumeMaster, 0.0001f, 0f);
            });
            State.On(States.DemiSwapSide, () =>
            {
                // On change de côté
                Court.SwapTeams();
                ResetScorePoints();
            });
            State.On(States.DemiFinishMatch, () =>
            {
                Misc.Log("Fin du Match de Demi");
                //_currentMatchConfig = null;
                //SetIsFreeCourt(true);
            });
            State.On(States.DemiFinalEnd, () =>
            {
                Misc.Log("************************************ FIN DES DEMI FINALES");
                _matchConfigs = null;
                SetIsFreeCourt(true);
            });

        }
        private bool ImportMatchConfigDemi()
        {
            Misc.Log($"ImportMatchConfigDemi {IndexMatch}");

            if (_matchConfigs == null) return false;

            if (IndexMatch < 0 || IndexMatch > _matchConfigs.Count - 1) return false;

            var matchConfig = _matchConfigs[IndexMatch];
            
            if (matchConfig != null)
            {
                SetMatchConfig(matchConfig);
                IndexMatch++;
                return true;
            }

            return false;
        }
        public void GotoNextMatch()
        {
            SetTicState((int)States.DemiNextMatch);
        }
        public void BeginDemiFinal()
        {
            SetTicState((int)States.DemiFinalBegin);
            ImportMatchConfigDemi();
        }
        private void SetTicState(int ticProcess)
        {
            _ticState = ticProcess;
            States nextState = (States)Process[_ticState];
            State.Change(nextState);
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
        public void ValidSet(int pointGap)
        {
            var winner = GetWinner(pointGap);
            TeamA.Stats.AddScoreSet(new Set(winner == TeamA, TeamA.Stats.ScorePoint));
            TeamB.Stats.AddScoreSet(new Set(winner == TeamB, TeamB.Stats.ScorePoint));
        }
        public Team GetWinner(int pointGap = 1)
        {
            if (_teamA == null || _teamB == null) return null;
            if (_teamA.Stats.ScorePoint == _teamB.Stats.ScorePoint) return null;
            
            if (Math.Abs(_teamA.Stats.ScorePoint - _teamB.Stats.ScorePoint) > pointGap - 1)
            {
                if (_teamA.Stats.ScorePoint > _teamB.Stats.ScorePoint ) return _teamA;
                if (_teamB.Stats.ScorePoint > _teamA.Stats.ScorePoint ) return _teamB;
            }

            return null;
        }
        public Team GetLooser(int pointGap = 1)
        {
            if (_teamA == null || _teamB == null) return null;
            if (_teamA.Stats.ScorePoint == _teamB.Stats.ScorePoint) return null;
            if (pointGap > Math.Abs(_teamA.Stats.ScorePoint - _teamB.Stats.ScorePoint)) return null;

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
        public Team GetTeamHasMatchPoint()
        {
            var winner = GetWinner(_pointGap - 1);
            if (winner == null) return null;

            if (winner.Stats.ScorePoint >= _currentMatchConfig.NbPointToWinSet && winner.Stats.IsCloseToWinMatch(_currentMatchConfig))
                return winner;

            return null;
        }
        public void SetMatchConfigs(List<MatchConfig> matchConfigs)
        {
            _matchConfigs = matchConfigs;
        }
        public void SetMatchConfig(MatchConfig matchConfig)
        {
            SetIsFreeCourt(false);

            _teamA = matchConfig.TeamA;
            _teamB = matchConfig.TeamB;
            _teamReferee = matchConfig.TeamReferee;

            if (_teamA != null)
            {
                _teamA.SetIsPlaying(true);
                _teamA.SetMatch(this);
                SetTeamHasService(_teamA);
            }
            if (_teamB != null)
            {
                _teamB.SetIsPlaying(true);
                _teamB.SetMatch(this);
            }
            if (_teamReferee != null)
            {
                _teamReferee.SetIsReferee(true);
                _teamReferee.SetMatch(this);
            }

            _currentMatchConfig = matchConfig;
        }
        public void ResetResults()
        {
            _teamA.Stats.ResetResult();
            _teamB.Stats.ResetResult();
            
        }
        public void ResetSets()
        {
            _teamA.Stats.Sets.Clear();
            _teamB.Stats.Sets.Clear();
        }
        public void ResetScorePoints()
        {
            _teamA.Stats.SetScorePoint(0);
            _teamB.Stats.SetScorePoint(0);
        }
        public void ResetTeamsStatus()
        {
            _teamA.ResetTeamStatus();
            _teamB.ResetTeamStatus();
            if (_teamReferee != null)
                _teamReferee.ResetTeamStatus();
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

            if (State.CurState == States.PoolPlay1 || State.CurState == States.PoolPlay2 || State.CurState == States.PoolFinishMatch || State.CurState == States.DemiPlay)
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
            else if (State.CurState == States.PoolReady || State.CurState == States.DemiReady)
            {
                ChangeTeamHasService(TeamA);
            }
        }
        public void AddPointB(int points)
        {
            //Misc.Log($"{points}");

            if (State.CurState == States.PoolPlay1 || State.CurState == States.PoolPlay2 || State.CurState == States.PoolFinishMatch || State.CurState == States.DemiPlay)
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
            else if (State.CurState == States.PoolReady || State.CurState == States.DemiReady)
            {
                ChangeTeamHasService(TeamB);
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
                    break;

                case States.PoolPlay1:

                    break;
                case States.PoolCountDown1:
                    break;

                case States.PoolCountDown2:
                    break;

                case States.PoolSwapSide:
                    break;

                case States.PoolPlay2:
                    break;

                case States.PoolValidPoints:
                    break;
                case States.PoolPreSwap:
                    break;
                case States.DemiNextMatch:
                    break;
                case States.DemiWarmUp:
                    break;
                case States.DemiReady:
                    break;
                case States.DemiPlay:

                    if (GetWinner(_pointGap) != null)
                        if (GetWinner(_pointGap).Stats.ScorePoint >= _currentMatchConfig.NbPointToWinSet)
                            GotoNextState();

                    if (GetTeamHasMatchPoint() != null)
                        GetTeamHasMatchPoint().SetIsMatchPoint(true);
                    else
                    {
                        TeamA.SetIsMatchPoint(false);
                        TeamB.SetIsMatchPoint(false);
                    }    

                        break;
                case States.DemiFinishSet:
                    break;
                case States.DemiValidPoints:

                    if (GetWinner(_pointGap) != null)
                    {
                        if (GetWinner(_pointGap).Stats.IsWinMatch(_currentMatchConfig))
                        {
                            GetWinner(_pointGap).SetIsWinner(true);

                            ResetScorePoints();
                            SetTicState((int)States.DemiFinishMatch); // Ne Pas utiliser State.Change dans un trigger On Off comportement imprévu

                        }
                    }

                    break;
                case States.DemiSwapSide:
                    break;
                case States.DemiFinalBegin:
                    break;

                case States.DemiFinishMatch:

                    // Debug
                    if (ButtonControl.OnePress($"Space{_idTerrain}Demi", Keyboard.GetState().IsKeyDown(Keys.Space))) SetTicState((int)States.DemiNextMatch);

                    break;

                default:
                    break;
            }
        }
        public override Node Update(GameTime gameTime)
        {
            UpdateRect();

            RunState();

            UpdateChilds(gameTime);

            return base.Update(gameTime);
        }
        public void GotoNextState()
        {
            if (IndexMatch > 7 && _ticState == (int)States.DemiFinalEnd) return;

            _ticState++;

            if (_ticState > (int)States.DemiSwapSide)
            {
                _ticState = (int)States.DemiReady;
            }

            Misc.Log($"Etape {(States)_ticState} Terrain {_idTerrain}");

            SetTicState(_ticState); // reviens a la première étape automatiquement si _ticState atteint la dernière étape + 1
        }
        public override Node Draw(SpriteBatch batch, GameTime gameTime, int indexLayer)
        {
            if (indexLayer == (int)Layers.Main)
            {

            }

            if (indexLayer == (int)Layers.Debug)
            {
                //batch.LeftTopString(Static.FontMini, $"{_teamHasService.TeamName}", AbsXY + new Vector2(10, 10), Color.Red);
                //batch.LeftTopString(Static.FontMini, $"{_lastTeamHasService.TeamName}", AbsXY + new Vector2(10, 40), Color.Red);
                batch.BottomCenterString(Static.FontMini, $"{State.CurState}", AbsXY + new Vector2(10, 40), Color.Red);
            }

            DrawChilds(batch, gameTime, indexLayer);

            return base.Draw(batch, gameTime, indexLayer);
        }
    }
}
