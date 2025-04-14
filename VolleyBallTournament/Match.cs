using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Framework.Devices.Sensors;
using Mugen.Core;
using Mugen.GFX;
using Mugen.GUI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VolleyBallTournament
{
    public class Match : Node
    {
        public enum States
        {
            Pause,
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
            //LastPoint,
        }
        public State<States> State { get; private set; } = new State<States>(States.Ready);

        public Dictionary<States, string> Infos = new Dictionary<States, string>()
        {
            {States.Pause, "Prochain match"},
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
            //{States.LastPoint, "Dernière balle"},

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

            State.Set(States.Pause);

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
            return team == _teamA ? _teamB : _teamA;
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
        public void AddPointA(int points)
        {
            //Misc.Log($"{points}");

            if (State.CurState == States.Play1 || State.CurState == States.Play2 || State.CurState == States.Finish)
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

            if (State.CurState == States.Play1 || State.CurState == States.Play2 || State.CurState == States.Finish)
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
                case States.Pause:

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
