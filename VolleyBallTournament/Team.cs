using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mugen.Animation;
using Mugen.Core;
using Mugen.GFX;
using Mugen.Physics;
using System;


namespace VolleyBallTournament
{
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
        private int _bonusPoint = 0;
        private int _nbMatchWin = 0;
        public int NbMatchPlayed => _nbMatchPlayed;
        private int _nbMatchPlayed = 0;
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
            
            _nbMatchPlayed++;
        }
        public void SetTeamName(string teamName)
        {
            _teamName = teamName;
        }
        public Team AddPointTotal(int points)
        { 
            _totalPoint += points; 
            _easePointTotal.SetValue(_totalPoint);
            return this;
        }
        public Team SetPointTotal(int points)
        {
            _totalPoint = points;
            _easePointTotal.SetValue(_totalPoint);
            return this;
        }
        public Team AddPoint(int points) 
        { 
            _scorePoint += points; 
            return this; 
        }
        public void SetScorePoint(int points)
        {
            _scorePoint = points;
        }
        public void SetScoreSet(int points)
        {
            _scoreSet = points;
        }
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
                    _bonusPoint = _scorePoint - _match.GetTeamOppenent(this)._scorePoint;
                }

            return base.Update(gameTime);
        }
        public override Node Draw(SpriteBatch batch, GameTime gameTime, int indexLayer)
        {
            if (indexLayer == (int)Layers.Main)
            {
                batch.FillRectangle(AbsRectF.Extend(-4f), !(_isPlaying || _isReferee) ? Color.DarkSlateBlue * .5f: Color.Black * .5f);
                

                batch.LeftMiddleString(Static.FontMain, $"{_teamName}", AbsRectF.LeftMiddle + Vector2.UnitX * 20, _isPlaying ? Color.GreenYellow : _isReferee ? Color.White : Color.Gray);
                //batch.CenterStringXY(Static.FontMain, $"{Rank}", AbsRectF.LeftMiddle - Vector2.UnitX * 10, Color.Orange);
                batch.RightMiddleString(Static.FontMain, $"{_easePointTotal.GetValue()}", AbsRectF.LeftMiddle - Vector2.UnitX * 10, Color.Yellow);
                
                batch.LeftMiddleString(Static.FontMain, _bonusPoint > 0 ? $"+{_bonusPoint}": $"{_bonusPoint}", AbsRectF.RightMiddle + Vector2.UnitX * 10, _bonusPoint > 0 ? Color.GreenYellow : Color.OrangeRed);

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
            for (int i = 0; i < _nbMatchPlayed; i++)
            {
                var pos = new Vector2(AbsRectF.RightMiddle.X + i * 24 - (24 * _nbMatchPlayed), AbsRectF.Center.Y);

                //batch.Circle(pos, 10, 16, Color.White, 2f);
                batch.FilledCircle(Static.TexCircle, pos, 20, Color.Gray);

                if (i < _nbMatchWin)
                    batch.FilledCircle(Static.TexCircle, pos, 20, Color.Gold);
                //batch.Point(pos, 10, Color.Gold);
            }
        }
    }
}
