using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mugen.Core;
using Mugen.GFX;
using Mugen.GUI;
using System.Collections.Generic;

namespace VolleyBallTournament
{
    public class Match : Node
    {
        public enum States
        {
            Pause,
            WarmUp,
            Ready,
            CountDown,
            Play,
            Finish,
            ValidPoints,
            //LastPoint,
        }
        public State<States> State { get; private set; } = new State<States>(States.Ready);

        public Dictionary<States, string> Infos = new Dictionary<States, string>()
        {
            {States.Pause, "Prochain match"},
            {States.WarmUp, "Echauffement"},
            {States.Ready, "Prêt a jouer"},
            {States.CountDown, "Début du Match"},
            {States.Play, "Match en cours"},
            {States.Finish, "Fin du match"},
            {States.ValidPoints, "Validation des Points"},
            //{States.LastPoint, "Dernière balle"},

        };

        public int NbSetToWin => _nbSetToWin;
        private int _nbSetToWin = 2;

        public int IdTerrain => _idTerrain;
        private int _idTerrain = Const.NoIndex;

        //public ScorePanel ScorePanel => _scorePanel;
        //private ScorePanel _scorePanel;
        public Court Court => _court;
        private Court _court;

        private Container _div;

        private int _nbTeamPerGroup = 3;

        public Team TeamA => _teamA;
        private Team _teamA;
        public Team TeamB => _teamB;
        private Team _teamB;
        public Team TeamReferee => _teamReferee;
        private Team _teamReferee;

        Timer _timerCountDown;
        
        public Match(int idTerrain, string courtName, Team teamA, Team teamB, Team teamReferee, int nbTeamPerGroup)
        {
            _idTerrain = idTerrain;

            _div = new Container(Style.Space.One * 10, new Style.Space(80,80,160,160), Mugen.Physics.Position.VERTICAL);

            //_scorePanel = new ScorePanel(this);
            _court = new Court(courtName, this);

            //_scorePanel.AppendTo(this);
            _court.AppendTo(this);

            SetTeam(teamA, teamB, teamReferee, nbTeamPerGroup);

            //_div.Insert(_scorePanel);
            _div.Insert(_court);
            _div.Refresh();

            SetSize(_div._rect.Width, _div._rect.Height);

            State.Set(States.Pause);

            //_timerCountDown = new Timer();
            //_timerCountDown.Start();
        }
        public void SetNbSetToWin(int nbSetToWin)
        {
            _nbSetToWin = nbSetToWin;
        }
        public Team GetWinner()
        {
            if (_teamA.ScorePoint == _teamB.ScorePoint) return null;
            
            return _teamA.ScorePoint > _teamB.ScorePoint ? _teamA : _teamB;
        }
        public Team GetLooser()
        {
            if (_teamA.ScorePoint == _teamB.ScorePoint) return null;

            return _teamA.ScorePoint < _teamB.ScorePoint ? _teamA : _teamB;
        }
        public Team GetTeamOppenent(Team team)
        {
            return team == _teamA ? _teamB : _teamA;
        }
        public void SetTeam(Team teamA, Team teamB, Team teamReferee, int nbTeamPerGroup)
        {
            _teamA = teamA;
            _teamB = teamB;
            _teamReferee = teamReferee;

            _teamA.SetIsPlaying(true);
            _teamB.SetIsPlaying(true);
            _teamReferee.SetIsReferee(true);

            _teamA.TakeService(_teamB);

            if (_teamA.NbMatchPlayed < nbTeamPerGroup - 1)
                _teamA.SetMatch(this);
            
            if (_teamB.NbMatchPlayed < nbTeamPerGroup - 1)
                _teamB.SetMatch(this);

            if (_teamReferee.NbMatchPlayed < nbTeamPerGroup - 1)
                _teamReferee.SetMatch(this);
        }
        public void ResetScore()
        {
            TeamA.SetScoreSet(0);
            TeamB.SetScoreSet(0);
            TeamA.SetScorePoint(0);
            TeamB.SetScorePoint(0);
        }
        public void AddPointA(int points = 1)
        {
            if (State.CurState == States.Play || State.CurState == States.Finish)
            {
                if (points == 0) return;

                if (points > 0)
                {
                    TeamA.TakeService(TeamB);
                }

                if (points < 0)
                    TeamA.CancelService(TeamB);

                _teamA.AddPoint(points);
                new PopInfo(points > 0 ? $"+{points}" : $"{points}", points > 0 ? Color.GreenYellow : Color.Red, Color.Black, 0, 16, 32).SetPosition(_court.ScoreAPos - Vector2.UnitY * 64).AppendTo(_parent);
                new FxExplose(_court.ScoreAPos, points > 0 ? Color.GreenYellow : Color.Red, 20, 20, 80).AppendTo(_parent);

                Static.SoundPoint.Play(.25f, .01f, 0f);
            }
            else if (State.CurState == States.Ready)
            {
                TeamA.TakeService(TeamB);
            }
        }
        public void AddPointB(int points = 1)
        {
            if (State.CurState == States.Play || State.CurState == States.Finish)
            {
                if (points == 0) return;

                if (points > 0)
                {
                    TeamB.TakeService(TeamA);
                }

                if (points < 0)
                    TeamB.CancelService(TeamA);

                _teamB.AddPoint(points);
                new PopInfo(points > 0 ? $"+{points}" : $"{points}", points > 0 ? Color.GreenYellow : Color.Red, Color.Black, 0, 16, 32).SetPosition(_court.ScoreBPos - Vector2.UnitY * 64).AppendTo(_parent);
                new FxExplose(_court.ScoreBPos, points > 0 ? Color.GreenYellow : Color.Red, 20, 20, 80).AppendTo(_parent);

                Static.SoundPoint.Play(.25f, .01f, 0f);
            }
            else if (State.CurState == States.Ready)
            {
                TeamB.TakeService(TeamA);
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

                case States.Play:


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

            DrawChilds(batch, gameTime, indexLayer);

            return base.Draw(batch, gameTime, indexLayer);
        }
    }
}
