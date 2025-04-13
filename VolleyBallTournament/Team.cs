using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mugen.Animation;
using Mugen.Core;
using Mugen.Event;
using Mugen.GFX;
using Mugen.GUI;
using Mugen.Physics;
using System;
using System.Collections.Generic;
using System.IO.Pipes;


namespace VolleyBallTournament
{
    public class Trail : Node
    {
        Color _color;
        Vector2 _scale;
        float _stepAlpha;
        public Trail(RectangleF rectF, Vector2 scale, float stepAplha = 0.5f, Color color = default)
        {
            _x = rectF.X;
            _y = rectF.Y;

            _rect = rectF;
            _scale = scale;
            _color = color;
            _stepAlpha = stepAplha;
        }

        public override Node Update(GameTime gameTime)
        {
            UpdateRect();

            _alpha += -_stepAlpha;

            if (_alpha <= 0f)
                KillMe();
            return base.Update(gameTime);
        }

        public override Node Draw(SpriteBatch batch, GameTime gameTime, int indexLayer)
        {

            if (indexLayer == (int)Layers.BackFX)
            {
                batch.FillRectangle(_rect, _color * _alpha);
            }


            return base.Draw(batch, gameTime, indexLayer);
        }
    }

    public enum Result
    {
        Null,
        Win,
        Loose,
    }

    public struct Set(bool isWin, int scorePoint)
    {
        public bool IsWin = isWin; // true win, false loose
        public int Points = scorePoint; // points in Set
    }

    public class Team : Node  
    {
        public enum Timers
        {
            None,
            Trail,
        }
        Timer<Timers> _timer = new Timer<Timers>();

        public bool IsMove => _isMove;
        private bool _isMove = false;

        private Match _match = null;

        private int _rank = 0;
        private Vector2 _newPosition = Vector2.Zero;
        public int ScorePoint => _scorePoint;
        private int _scorePoint = 0;

        public List<Set> Sets => _sets;
        private List<Set> _sets = [];

        private bool _isPlaying = false;
        private bool _isReferee = false;
        public int RankingPoint => _rankingPoint;
        private int _rankingPoint = 0; EasingValue _easeRankingPoint = new(0);
        private int _currentBonusPoint = 0;
        public int BonusPoint => _bonusPoint;
        private int _bonusPoint = 0;

        public int TotalPoint => _totalPoint;
        private int _totalPoint = 0; EasingValue _easeTotalPoint = new(0);

        //public int NbMaxMatchPlayed = 3;
        public bool HasService => _hasService;
        private bool _hasService = false;
        //private Team _lastTeamHasService = null;
        public int NbMatchPlayed => _results.Count;
        private List<Result> _results = new List<Result>();
        public string TeamName => _teamName;
        private string _teamName;

        Animate _animate;

        public static int Width = 360;
        public static int Height = 64;
        public static RectangleF Bound;

        public Team(string teamName)//, Match match = null, ScorePanel scorePanel = null, Court court = null)
        {
            _type = UID.Get<Team>();
            SetSize(Width, Height);

            Bound = new RectangleF(0, 0, Width, Height);

            _teamName = teamName;
            //Match = match;
            //ScorePanel = scorePanel;
            //Court = court;

            _timer.Set(Timers.Trail, Timer.Time(0, 0, .01f), true);
            _timer.Start(Timers.Trail);

            _animate = new Animate();
            _animate.Add("move");
        }
        public void ResetAllPoints()
        {
            _rank = 0;
            _scorePoint = 0;
            _rankingPoint = 0;
            _bonusPoint = 0;
            _totalPoint = 0;
        }
        public void SetService(bool hasService)
        { 
            _hasService = hasService;
        }
        //public void TakeService(Team opponent)
        //{
        //    if (opponent == null) return;

        //    if (_hasService) _lastTeamHasService = this;
        //    if (opponent.HasService) _lastTeamHasService = opponent;

        //    _hasService = true;
        //    opponent.SetService(false);
        //}
        //public void CancelService(Team opponent)
        //{
        //    if (opponent == null) return;

        //    if (_lastTeamHasService != null)
            
        //    if (_lastTeamHasService == this)
        //        TakeService(opponent);

        //    if (_lastTeamHasService == opponent)
        //        opponent.TakeService(this);
            
        //}
        public void SetIsPlaying(bool isPlaying)
        {
            _isPlaying = isPlaying;
        }
        public void SetIsReferee(bool isReferee)
        {
            _isReferee = isReferee;
        }
        public void SetMatch(Match match)
        {
            _match = match;

            if (match == null) return;
        }
        public void SetTeamName(string teamName) { _teamName = teamName; }
        public void ResetResult()
        {
            _results.Clear();
        }
        public void AddResult(Result result)
        {
            _results.Add(result);
        }
        public Team AddRankingPoint(int points)
        { 
            _rankingPoint += points; 
            _easeRankingPoint.SetValue(_rankingPoint);
            return this;
        }
        public Team SetRankingPoint(int points)
        {
            _rankingPoint = points;
            _easeRankingPoint.SetValue(_rankingPoint);
            return this;
        }
        public Team AddTotalPoint(int points)
        {
            _totalPoint += points;
            _easeTotalPoint.SetValue(_totalPoint);
            return this;
        }
        public Team SetTotalPoint(int points)
        {
            _totalPoint = points;
            _easeTotalPoint.SetValue(_totalPoint);
            return this;
        }
        public void ValidBonusPoint()
        {
            _bonusPoint += _currentBonusPoint;
            _currentBonusPoint = 0;
        }
        public void AddPoint(int points) { _scorePoint += points; AddTotalPoint(points); }
        public void SetScorePoint(int points) { _scorePoint = points; }
        public void AddScoreSet(Set set) { _sets.Add(set); }
        public void SetRank(int rank)
        {
            _rank = rank;
        }
        public void MoveToPosition(Vector2 position, int duration = 64)
        {
            if (position == XY) 
                return;

            //_isMove = true;
            _newPosition = position;
            _animate.SetMotion("move", Easing.QuadraticEaseOut, _y, _newPosition.Y, duration);
            _animate.Start("move");

        }
        public void SetIsMove(bool isMove)
        {
            _isMove = isMove;
        }
        public override Node Update(GameTime gameTime)
        {
            _timer.Update();
            UpdateRect();

            if (_animate.IsPlay != null)
            {
                _y = _animate.Value();
            }

            if (_animate.Off("move"))
            {
                _isMove = false;
                //Misc.Log("End move");
            }
            _animate.NextFrame();

            _scorePoint = int.Clamp(_scorePoint, 0, 99);
            //_scoreSet = int.Clamp(_scoreSet, 0, 3);

            if (_match != null)
            {
                if (!_isReferee) // Seul l'équipe qui joue on leur bonus qui changent !
                    _currentBonusPoint = _scorePoint - _match.GetTeamOppenent(this)._scorePoint;
            }

            return base.Update(gameTime);
        }
        public override Node Draw(SpriteBatch batch, GameTime gameTime, int indexLayer)
        {
            if (indexLayer == (int)Layers.Main)
            {
                DrawBasicTeam(batch, AbsRectF);

                //batch.CenterStringXY(Static.FontMain, $"{Rank}", AbsRectF.LeftMiddle - Vector2.UnitX * 10, Color.Orange);
                batch.RightMiddleString(Static.FontMain, $"{_easeRankingPoint.GetValue()}", AbsRectF.LeftMiddle - Vector2.UnitX * 10, Color.White);
                
                int bonus = _bonusPoint + _currentBonusPoint;

                batch.LeftMiddleString(Static.FontMain, bonus > 0 ? $"+{bonus}": $"{bonus}", AbsRectF.RightMiddle + Vector2.UnitX * 10 - Vector2.UnitY * 14, bonus > 0 ? Color.GreenYellow : Color.OrangeRed);
                batch.LeftMiddleString(Static.FontMini, $"{_easeTotalPoint.GetValue()}", AbsRectF.RightMiddle + Vector2.UnitX * 10 + Vector2.UnitY * 18, Color.Yellow);

                DrawVictory(batch);

                if (_isPlaying)
                {
                    //batch.Rectangle(AbsRectF.Extend(-4f), Color.White, 3f);
                }

            }

            if (indexLayer == (int)Layers.HUD)
            {
                DrawReferee(batch, AbsRectF);
            }
            
            if (indexLayer == (int)Layers.Debug)
            {
                //batch.CenterStringXY(Static.FontMini, $"{_rank}", AbsRectF.LeftMiddle + Vector2.UnitX * 10, Color.Cyan);
            }

            return base.Draw(batch, gameTime, indexLayer);
        }

        public void DrawBasicTeam(SpriteBatch batch, RectangleF rectF)
        {
            if (_timer.On(Timers.Trail) && _isMove)
            {
                //Misc.Log("Move");
                new Trail(rectF.Extend(-4f), Vector2.One, .05f, Color.WhiteSmoke).AppendTo(_parent);
            }

            batch.FillRectangle(rectF.Extend(-4f) + Vector2.One * 8, Color.Black * .5f);
            batch.FillRectangle(rectF.Extend(-4f), !(_isPlaying || _isReferee) ? Style.ColorValue.ColorFromHexa("#003366") * 1f : Color.DarkSlateBlue * 1f);

            batch.Rectangle(rectF.Extend(-4f), !(_isPlaying || _isReferee) ? Color.Black * 1f : Color.Gray * 1f, 1f);
            batch.Rectangle(rectF.Extend(-8f), !(_isPlaying || _isReferee) ? Color.Black * .5f : Color.Gray * .5f, 1f);

            batch.LeftMiddleString(Static.FontMain, $"{_teamName}", rectF.LeftMiddle + Vector2.UnitX * 20, _isPlaying ? Color.GreenYellow : _isReferee ? Color.Orange : Color.Gray);
        }
        public void DrawReferee(SpriteBatch batch, RectangleF rectF)
        {
            if (_isReferee)
            {
                //batch.Rectangle(AbsRectF.Extend(-4f), Color.Yellow, 3f);
                //batch.Draw(Static.TexReferee, Color.White, 0, rectF.Center + Vector2.UnitX * 10, Position.CENTER, Vector2.One / 4);
                if (_match != null)
                {
                    string text = $"Arbitre Terrain {_match.Court.CourtName}";
                    Vector2 pos = rectF.TopCenter - Vector2.UnitY * 4;
                    batch.FillRectangleCentered(pos, Static.FontMini.MeasureString(text) + new Vector2(12, -20), Color.Black *.75f, 0f);
                    batch.RectangleCentered(pos, Static.FontMini.MeasureString(text) + new Vector2(12, -20), Color.Gray, 1f);

                    batch.CenterStringXY(Static.FontMini, text, pos + Vector2.One * 2, Color.Black *.5f);
                    batch.CenterStringXY(Static.FontMini, text, pos, Color.Orange);
                }
            }
        }

        private void DrawVictory(SpriteBatch batch)
        {
            for (int i = 0; i < NbMatchPlayed; i++)
            {
                var pos = new Vector2(AbsRectF.RightMiddle.X + i * 30 - (28 * NbMatchPlayed), AbsRectF.Center.Y);

                batch.FilledCircle(Static.TexCircle, pos, 30, Color.Gray);

                if (_results[i] == Result.Null)
                {
                    batch.FilledCircle(Static.TexCircle, pos, 30, Color.Gray);
                    batch.CenterStringXY(Static.FontMini, "N", pos, Color.White);
                }

                if (_results[i] == Result.Win)
                {
                    batch.FilledCircle(Static.TexCircle, pos, 30, Color.Green);
                    batch.CenterStringXY(Static.FontMini, "G", pos, Color.White);
                }

                if (_results[i] == Result.Loose)
                {
                    batch.FilledCircle(Static.TexCircle, pos, 30, Color.Red);
                    batch.CenterStringXY(Static.FontMini, "P", pos, Color.White);
                }


                //if (i < NbMatchWin)
                //    batch.FilledCircle(Static.TexCircle, pos, 20, Color.Gold);
                //batch.Point(pos, 10, Color.Gold);
            }
        }
    }
}
