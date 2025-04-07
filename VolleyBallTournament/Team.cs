using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mugen.Animation;
using Mugen.Core;
using Mugen.GFX;
using Mugen.Physics;
using System;
using System.Collections.Generic;
using System.IO.Pipes;


namespace VolleyBallTournament
{
    public enum Result
    {
        Null,
        Win,
        Loose,
    }

    public class Team : Node  
    {
        private Match _match = null;

        private int _rank = 0;
        private int _newRank = 0;
        private Vector2 _newPosition = Vector2.Zero;
        public int ScorePoint => _scorePoint;
        private int _scorePoint = 0;
        public int ScoreSet => _scoreSet;
        private int _scoreSet = 0;

        private bool _isPlaying = false;
        private bool _isReferee = false;
        public int TotalPoint => _totalPoint;
        private int _totalPoint = 0; EasingValue _easePointTotal = new(0);
        private int _currentBonusPoint = 0;
        public int BonusPoint => _bonusPoint;
        private int _bonusPoint = 0;

        public int NbMatchPlayed => _results.Count;
        private List<Result> _results = new List<Result>();
        public string TeamName => _teamName;
        private string _teamName;

        Animate _animate;
        public Team(string teamName)//, Match match = null, ScorePanel scorePanel = null, Court court = null)
        {
            SetSize(360, 64);
            _teamName = teamName;
            //Match = match;
            //ScorePanel = scorePanel;
            //Court = court;

            _animate = new Animate();
            _animate.Add("move");
        }
        //public void ResetAll()
        //{
        //    _scoreSet = 0;
        //    _scorePoint = 0;
        //    _totalPoint = 0;
        //    _bonusPoint = 0;
        //    _nbMatchWin = 0;
        //    _nbMatchPlayed = 0;
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
        public Team AddTotalPoint(int points)
        { 
            _totalPoint += points; 
            _easePointTotal.SetValue(_totalPoint);
            return this;
        }
        public Team SetTotalPoint(int points)
        {
            _totalPoint = points;
            _easePointTotal.SetValue(_totalPoint);
            return this;
        }
        public void ValidBonusPoint()
        {
            _bonusPoint += _currentBonusPoint;
            _currentBonusPoint = 0;
        }
        public void AddPoint(int points) { _scorePoint += points; }
        public void SetScorePoint(int points) { _scorePoint = points; }
        public void SetScoreSet(int points) { _scoreSet = points; }
        public void MoveToRank(int rank)
        {
            if (rank == _rank) 
                return;
            _newRank = rank;
        }
        public void MoveToPosition(Vector2 position, int duration = 32)
        {
            if (position == XY) 
                return;

            _newPosition = position;
            _animate.SetMotion("move", Easing.QuadraticEaseOut, _y, _newPosition.Y, duration);
            _animate.Start("move");

        }
        public override Node Update(GameTime gameTime)
        {
            UpdateRect();

            if (_animate.IsPlay != null)
            {
                _y = _animate.Value();
            }

            if (_animate.Off("move"))
            {
                //Misc.Log("End move");
            }
            _animate.NextFrame();

            _scorePoint = int.Clamp(_scorePoint, 0, 99);
            _scoreSet = int.Clamp(_scoreSet, 0, 3);

            if (_match != null)
                if (_match.ScorePanel != null)
                {
                    _currentBonusPoint = _scorePoint - _match.GetTeamOppenent(this)._scorePoint;
                }

            return base.Update(gameTime);
        }
        public override Node Draw(SpriteBatch batch, GameTime gameTime, int indexLayer)
        {
            if (indexLayer == (int)Layers.Main)
            {
                batch.FillRectangle(AbsRectF.Extend(-4f), !(_isPlaying || _isReferee) ? Color.DarkSlateBlue * .25f: Color.DarkSlateBlue * .75f);
                

                batch.LeftMiddleString(Static.FontMain, $"{_teamName}", AbsRectF.LeftMiddle + Vector2.UnitX * 20, _isPlaying ? Color.GreenYellow : _isReferee ? Color.White : Color.Gray);
                //batch.CenterStringXY(Static.FontMain, $"{Rank}", AbsRectF.LeftMiddle - Vector2.UnitX * 10, Color.Orange);
                batch.RightMiddleString(Static.FontMain, $"{_easePointTotal.GetValue()}", AbsRectF.LeftMiddle - Vector2.UnitX * 10, Color.Yellow);
                
                int bonus = _bonusPoint + _currentBonusPoint;

                batch.LeftMiddleString(Static.FontMini, bonus > 0 ? $"+{bonus}": $"{bonus}", AbsRectF.RightMiddle + Vector2.UnitX * 10, bonus > 0 ? Color.GreenYellow : Color.OrangeRed);

                DrawVictory(batch);

                if (_isPlaying)
                {
                    //batch.Rectangle(AbsRectF.Extend(-4f), Color.White, 3f);
                }

                if (_isReferee)
                {
                    //batch.Rectangle(AbsRectF.Extend(-4f), Color.Yellow, 3f);
                    batch.Draw(Static.TexReferee, Color.White, 0, AbsRectF.Center + Vector2.UnitX * 10, Position.CENTER, Vector2.One / 4);
                }
            }   

            return base.Draw(batch, gameTime, indexLayer);
        }

        private void DrawVictory(SpriteBatch batch)
        {
            for (int i = 0; i < NbMatchPlayed; i++)
            {
                var pos = new Vector2(AbsRectF.RightMiddle.X + i * 28 - (24 * NbMatchPlayed), AbsRectF.Center.Y);

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
