using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mugen.Animation;
using Mugen.Core;
using Mugen.GFX;
using Mugen.GUI;
using Mugen.Physics;
using System;


namespace VolleyBallTournament
{
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

    public struct NextTurn()
    {
        public DateTime Time;
        public string Message; // message
        public int IdTerrain;
    }

    public class Team : Node  
    {
        public Stats Stats => _stats;
        private Stats _stats;

        public bool IsMove => _isMove;
        private bool _isMove = false;

        private Match _match = null;

        private Vector2 _newPosition = Vector2.Zero;

        public bool IsMatchPoint => _isMatchPoint;
        private bool _isMatchPoint = false;
        //public bool IsWinner => _isWinner;
        private bool _isWinner = false;
        public bool IsPlaying => _isPlaying;
        private bool _isPlaying = false;
        public bool IsReferee => _isReferee;
        private bool _isReferee = false;

        private bool _isShowStats = true;
        private bool _isShowSets = true;

        private bool _isShowNextTurn = true;
        private NextTurn _nextTurn;
        //private DateTime _nextTurnTime = new();

        public bool HasService => _hasService;
        private bool _hasService = false;

        Animate _animate;

        public static int Width = 360;
        public static int Height = 64;
        public static RectangleF Bound;

        private float _ticWave;
        private float _waveValue;

        public Team(string teamName)//, Match match = null, ScorePanel scorePanel = null, Court court = null)
        {
            _type = UID.Get<Team>();
            SetSize(Width, Height);

            _stats = new Stats();

            Bound = new RectangleF(0, 0, Width, Height);

            _stats.SetTeamName(teamName);
            //Match = match;
            //ScorePanel = scorePanel;
            //Court = court;

            _animate = new Animate();
            _animate.Add("move");
        }
        public NextTurn FindNextTurnTime(int currentRotation, RotationManager rotationManager) 
        { 
            int rotation = 0;
            _nextTurn.Time = DateTime.Now;
            double totalTime = 0;

            for (int j = currentRotation + 1; j < rotationManager.GridMatchConfig.Height; j++)
            {
                var matchTime = rotationManager.GetMatchTime();
                var warmUpTime = rotationManager.GetWarmUpTime();
                totalTime += matchTime * 2 + warmUpTime;

                //Misc.Log($"/////////////{rotation} {totalTime}");

                for (int i = 0; i < rotationManager.GridMatchConfig.Width; i++)
                {
                    var matchConfig = rotationManager.GridMatchConfig.Get(i, j);

                    if (matchConfig != null)
                    {
                        if (matchConfig.TeamReferee == this)
                        {
                            _nextTurn.Message = "Arbitre";
                            _nextTurn.IdTerrain = matchConfig.IdTerrain;
                            _nextTurn.Time = _nextTurn.Time.AddSeconds(totalTime);
                            return _nextTurn;
                        }
                        if (matchConfig.TeamA == this || matchConfig.TeamB == this)
                        {
                            _nextTurn.Message = "Joue";
                            _nextTurn.IdTerrain = matchConfig.IdTerrain;
                            _nextTurn.Time = _nextTurn.Time.AddSeconds(totalTime);
                            return _nextTurn;
                        }
                    }

                }


                rotation++;
            }
            
            
            return _nextTurn; 
        }
        public void ResetSets()
        {
            Stats.Sets.Clear();
        }
        public void ResetScorePoints()
        {
            Stats.SetScorePoint(0);
        }
        public void ResetTeamStatus()
        {
            //SetIsWinner(false);
            SetIsMatchPoint(false);
            SetIsPlaying(false);
            SetIsReferee(false);
            SetMatch(null);
        }
        //public void ResetMatchScorePoints()
        //{
        //    _match.ResetScorePoints();
        //}
        //public void ResetAllMatchSetPoints(List<Match> matchs)
        //{
        //    for (int i = 0; i < matchs.Count; i++)
        //    {
        //        var match = matchs[i];
        //        match.ResetSets();
        //    }
        //}
        public void SetNextTurn (NextTurn nextTurn) { _nextTurn = nextTurn; }
        public void SetStats(Stats stats) { _stats = stats; }
        public void SetService(bool hasService) { _hasService = hasService; }
        public void SetIsMatchPoint(bool isMatchPoint) { _isMatchPoint = isMatchPoint; }
        public void SetIsWinner(bool isWinner) { _isWinner = isWinner; }
        public void SetIsPlaying(bool isPlaying) { _isPlaying = isPlaying; }
        public void SetIsReferee(bool isReferee) { _isReferee = isReferee; }
        public void SetMatch(Match match) { _match = match; if (match == null) return; }
        public void SetIsShowStats(bool isShowStats) { _isShowStats = isShowStats; }
        public void SetIsShowNextTurns(bool isShowNextTurn) { _isShowNextTurn = isShowNextTurn; }
        public void MoveToPosition(Vector2 position, int duration = 64)
        {
            if (position == XY) 
                return;

            //SetIsMove(true);
            _newPosition = position;
            _animate.SetMotion("move", Easing.QuarticEaseOut, _y, _newPosition.Y, duration);
            _animate.Start("move");

        }
        public void SetIsMove(bool isMove) { _isMove = isMove; }
        public override Node Update(GameTime gameTime)
        {
            UpdateRect();

            if (_animate.IsPlay())
            {
                _y = _animate.Value();
            }

            if (_animate.Off("move"))
            {
                //SetIsMove(false);
                //Misc.Log("End move");
            }
            _animate.NextFrame();

            _stats.Update(_match, this);

            if (_isPlaying)
            {
                _ticWave += .05f;
                _waveValue = Math.Abs(MathF.Sin(_ticWave)) * 1f;
            }
            else
            {
                _waveValue = 0f;
            }

            return base.Update(gameTime);
        }
        public override Node Draw(SpriteBatch batch, GameTime gameTime, int indexLayer)
        {
            if (indexLayer == (int)Layers.Main)
            {
                DrawBasicTeam(batch, AbsRectF, _parent);

                if (_isShowStats)
                {
                    DrawStats(batch);
                    DrawMatchResults(batch);
                }

            }

            if (indexLayer == (int)Layers.HUD)
            {
                DrawReferee(batch, AbsRectF);
                DrawWinner(batch, AbsRectF);

                if (_isShowSets)
                    Court.DrawSet(batch, this, AbsRectF.TopRight - Vector2.UnitY * 10 + Vector2.UnitX * 32 - Vector2.UnitX * 48 * Stats.Sets.Count);

                if (_isShowNextTurn && !_isPlaying && !_isReferee)
                    DrawNextTurn(batch);
            }
            
            if (indexLayer == (int)Layers.Debug)
            {
                //batch.CenterStringXY(Static.FontMini, $"{_rank}", AbsRectF.LeftMiddle + Vector2.UnitX * 10, Color.Cyan);
            }

            return base.Draw(batch, gameTime, indexLayer);
        }
        public void DrawNextTurn(SpriteBatch batch)
        {
            float alpha = 1f;
            var text = $" {_nextTurn.Message} à {_nextTurn.Time:HH:mm} T{_nextTurn.IdTerrain + 1} ";

            var pos = AbsRectF.TopCenter;// - Vector2.UnitX * 100;

            batch.FillRectangleCentered(pos, Static.FontMini.MeasureString(text) + new Vector2(0, -20), Color.Black * .75f, 0);
            batch.CenterStringXY(Static.FontMini, text, pos, Color.NavajoWhite * alpha);
        }
        public void DrawStats(SpriteBatch batch)
        {
            batch.RightMiddleString(Static.FontMain, $"{Stats.RankingPoint}", AbsRectF.LeftMiddle - Vector2.UnitX * 10, Color.White);

            int bonus = Stats.BonusPoint + Stats.CurrentBonusPoint;
            batch.LeftMiddleString(Static.FontMini, bonus > 0 ? $"+{bonus}": $"{bonus}", AbsRectF.RightMiddle + Vector2.UnitX * 10 - Vector2.UnitY * 14, bonus > 0 ? Color.GreenYellow : Color.OrangeRed);
            batch.LeftMiddleString(Static.FontMini, $"{Stats.TotalPoint}", AbsRectF.RightMiddle + Vector2.UnitX * 10 + Vector2.UnitY * 18, Color.Yellow);
        }
        public void DrawBasicTeam(SpriteBatch batch, RectangleF rectF, Node parent)
        {
            if (_isMove)
            {
                new Trail(rectF.Extend(-4f), Vector2.One, .05f, Color.WhiteSmoke).AppendTo(parent);
            }

            batch.FillRectangle(rectF.Extend(-4f) + Vector2.One * 8, Color.Black * .5f);
            batch.FillRectangle(rectF.Extend(-4f), !(_isPlaying || _isReferee) ? Style.ColorValue.ColorFromHexa("#003366") * 1f : Color.DarkSlateBlue * 1f);

            batch.Rectangle(rectF.Extend(-2f), !(_isPlaying || _isReferee) ? Color.Black * 1f : HSV.ToRGB(150, 1, _waveValue) * 1f, 1f);
            //batch.Rectangle(rectF.Extend(-8f), !(_isPlaying || _isReferee) ? Color.Black * .5f : Color.Gray * .5f, 1f);
            batch.Rectangle(rectF.Extend(-6f), Color.Gray * .5f, 1f);

            batch.LeftMiddleString(Static.FontMain, $"{Stats.TeamName}", rectF.LeftMiddle + Vector2.UnitX * 20, _isPlaying ? Color.GreenYellow : _isReferee ? Color.Orange : Color.Gray * 1f);
        }
        public void DrawReferee(SpriteBatch batch, RectangleF rectF)
        {
            if (_isReferee)
            {
                if (_match != null)
                {
                    string text = $" Arbitre T{_match.Court.CourtName} ";
                    Vector2 pos = rectF.TopCenter - Vector2.UnitY * 4;
                    batch.FillRectangleCentered(pos, Static.FontMini.MeasureString(text) + new Vector2(12, -20), Color.Black *.75f, 0f);
                    batch.RectangleCentered(pos, Static.FontMini.MeasureString(text) + new Vector2(12, -20), Color.Gray, 1f);

                    batch.CenterStringXY(Static.FontMini, text, pos + Vector2.One * 2, Color.Black *.5f);
                    batch.CenterStringXY(Static.FontMini, text, pos, Color.Orange);
                }
            }
        }
        public void DrawMatchPoint(SpriteBatch batch, RectangleF rectF)
        {
            if (!_isReferee)
            {
                if (_match != null)
                {
                    string text = $"Balle de Match";
                    Vector2 pos = rectF.BottomCenter - Vector2.UnitY * 4;
                    batch.FillRectangleCentered(pos, Static.FontMini.MeasureString(text) + new Vector2(12, -20), Color.Black * .75f, 0f);
                    batch.RectangleCentered(pos, Static.FontMini.MeasureString(text) + new Vector2(12, -20), Color.Gray, 1f);

                    batch.CenterStringXY(Static.FontMini, text, pos + Vector2.One * 2, Color.Black * .5f);
                    batch.CenterStringXY(Static.FontMini, text, pos, Color.Orange);
                }
            }
        }
        public void DrawWinner(SpriteBatch batch, RectangleF rectF)
        {
            if (_isWinner)
            {
                string text = $"Vainqueur";
                Vector2 pos = rectF.TopCenter - Vector2.UnitY * 4 + Vector2.UnitY * _waveValue * 4f;
                batch.FillRectangleCentered(pos, Static.FontMini.MeasureString(text) + new Vector2(12, -20), Color.Blue * .75f, 0f);
                batch.RectangleCentered(pos, Static.FontMini.MeasureString(text) + new Vector2(12, -20), Color.Gray, 2f);

                batch.CenterStringXY(Static.FontMini, text, pos + Vector2.One * 2, Color.Black * .5f);
                batch.CenterStringXY(Static.FontMini, text, pos, Color.Gold);
            }
        }

        private void DrawMatchResults(SpriteBatch batch)
        {
            for (int i = 0; i < Stats.NbMatchPlayed; i++)
            {
            var pos = new Vector2(AbsRectF.RightMiddle.X + (i%3) * 20 - (Stats.NbMatchPlayed < 3 ? Stats.NbMatchPlayed * 20 : 3 * 20), AbsRectF.Center.Y + (i < 3 ? -10 : 10));
                batch.FilledCircle(Static.TexCircle, pos, 20, Color.Gray);

                if (Stats.Results[i] == Result.Null)
                {
                    batch.FilledCircle(Static.TexCircle, pos, 20, Color.Gray);
                    batch.CenterStringXY(Static.FontMicro, "N", pos, Color.White);
                }

                if (Stats.Results[i] == Result.Win)
                {
                    batch.FilledCircle(Static.TexCircle, pos, 20, Color.Green);
                    batch.CenterStringXY(Static.FontMicro, "G", pos, Color.White);
                }

                if (Stats.Results[i] == Result.Loose)
                {
                    batch.FilledCircle(Static.TexCircle, pos, 20, Color.Red);
                    batch.CenterStringXY(Static.FontMicro, "P", pos, Color.White);
                }

                //if (i < NbMatchWin)
                //    batch.FilledCircle(Static.TexCircle, pos, 20, Color.Gold);
                //batch.Point(pos, 10, Color.Gold);
            }

        }
    }
}
