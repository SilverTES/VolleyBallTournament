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
using System.Text.RegularExpressions;


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

    public class Stats()
    {
        //private Match _match = match;
        //private Team _team = team;
        public string TeamName => _teamName;
        private string _teamName;
        public int ScorePoint => _scorePoint;
        private int _scorePoint = 0;
        public List<Set> Sets => _sets;
        private List<Set> _sets = [];
        private int _rank = 0;
        public int RankingPoint => _rankingPoint;
        private int _rankingPoint = 0; 
        //public EasingValue EaseRankingPoint => _easeRankingPoint;
        //EasingValue _easeRankingPoint = new(0);
        public int CurrentBonusPoint => _currentBonusPoint;
        private int _currentBonusPoint = 0;
        public int BonusPoint => _bonusPoint;
        private int _bonusPoint = 0;
        public int TotalPoint => _totalPoint;
        private int _totalPoint = 0; 
        //public EasingValue EaseTotalPoint => _easeTotalPoint;
        //EasingValue _easeTotalPoint = new(0);

        public int NbMatchPlayed => _results.Count;
        public List<Result> Results => _results;
        private List<Result> _results = new List<Result>();

        public void Update(Match match, Team team)
        {
            _scorePoint = int.Clamp(_scorePoint, 0, 99);

            if (match != null)
            {
                if (!team.IsReferee) // Seul l'équipe qui joue on leur bonus qui changent !
                    _currentBonusPoint = _scorePoint - match.GetTeamOppenent(team).Stats._scorePoint;
            }
        }
        public void ResetResult() { _results.Clear(); }
        public void AddResult(Result result) { _results.Add(result); }
        public void ResetAllPoints()
        {
            _rank = 0;
            _scorePoint = 0;
            _rankingPoint = 0;
            _bonusPoint = 0;
            _totalPoint = 0;
        }
        //public void SetMatch(Match match) { _match = match; }
        public void SetTeamName(string teamName) { _teamName = teamName; }
        public void AddRankingPoint(int points)
        {
            _rankingPoint += points;
            //_easeRankingPoint.SetValue(_rankingPoint);
        }
        public void SetRankingPoint(int points)
        {
            _rankingPoint = points;
            //_easeRankingPoint.SetValue(_rankingPoint);
        }
        public void AddTotalPoint(int points)
        {
            _totalPoint += points;
            //_easeTotalPoint.SetValue(_totalPoint);
        }
        public void SetTotalPoint(int points)
        {
            _totalPoint = points;
            //_easeTotalPoint.SetValue(_totalPoint);
        }
        public void ValidBonusPoint()
        {
            _bonusPoint += _currentBonusPoint;
            _currentBonusPoint = 0;
        }
        public void AddPoint(int points) { _scorePoint += points; AddTotalPoint(points); }
        public void SetScorePoint(int points) { _scorePoint = points; }
        public void AddScoreSet(Set set) { _sets.Add(set); }
        public void SetRank(int rank) { _rank = rank; }

        public Stats Clone()
        {
            Stats clone = (Stats)MemberwiseClone();

            return clone;
        }
    }

    public class Team : Node  
    {
        public Stats Stats => _stats;
        private Stats _stats;

        public bool IsMove => _isMove;
        private bool _isMove = false;

        private Match _match = null;

        private Vector2 _newPosition = Vector2.Zero;

        public bool IsPlaying => _isPlaying;
        private bool _isPlaying = false;
        public bool IsReferee => _isReferee;
        private bool _isReferee = false;

        private bool _isShowStats = true;

        //public int NbMaxMatchPlayed = 3;
        public bool HasService => _hasService;
        private bool _hasService = false;
        //private Team _lastTeamHasService = null;

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
        public void SetStats(Stats stats) { _stats = stats; }

        public void SetService(bool hasService) { _hasService = hasService; }
        public void SetIsPlaying(bool isPlaying) { _isPlaying = isPlaying; }
        public void SetIsReferee(bool isReferee) { _isReferee = isReferee; }
        public void SetMatch(Match match) { _match = match; if (match == null) return; }
        public void SetIsShowStats(bool isShowStats) { _isShowStats = isShowStats; }
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
                _ticWave += .025f;
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
                    DrawStats(batch);

                DrawVictory(batch);
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
        public void DrawStats(SpriteBatch batch)
        {
            batch.RightMiddleString(Static.FontMain, $"{Stats.RankingPoint}", AbsRectF.LeftMiddle - Vector2.UnitX * 10, Color.White);

            int bonus = Stats.BonusPoint + Stats.CurrentBonusPoint;
            batch.LeftMiddleString(Static.FontMain, bonus > 0 ? $"+{bonus}": $"{bonus}", AbsRectF.RightMiddle + Vector2.UnitX * 10 - Vector2.UnitY * 14, bonus > 0 ? Color.GreenYellow : Color.OrangeRed);
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

            batch.Rectangle(rectF.Extend(-4f), !(_isPlaying || _isReferee) ? Color.Black * 1f : HSV.ToRGB(150, 1, _waveValue) * 1f, 1f);
            batch.Rectangle(rectF.Extend(-8f), !(_isPlaying || _isReferee) ? Color.Black * .5f : Color.Gray * .5f, 1f);

            batch.LeftMiddleString(Static.FontMain, $"{Stats.TeamName}", rectF.LeftMiddle + Vector2.UnitX * 20, _isPlaying ? Color.GreenYellow : _isReferee ? Color.Orange : Color.Gray);
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
