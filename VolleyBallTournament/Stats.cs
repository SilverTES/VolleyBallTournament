using System;
using System.Collections.Generic;
using System.Linq;

namespace VolleyBallTournament
{
    public class Stats()
    {
        public string TeamName => _teamName;
        private string _teamName;
        public int ScorePoint => _scorePoint;
        private int _scorePoint = 0;
        public List<Set> Sets => _sets;
        private List<Set> _sets = [];
        private int _rank = 0;
        public int RankingPoint => _rankingPoint;
        private int _rankingPoint = 0;
        public int CurrentBonusPoint => _currentBonusPoint;
        private int _currentBonusPoint = 0;
        public int BonusPoint => _bonusPoint;
        private int _bonusPoint = 0;
        public int TotalPoint => _totalPoint;
        private int _totalPoint = 0;
        public int NbMatchPlayed => _results.Count;
        public List<Result> Results => _results;
        private List<Result> _results = [];

        public void Update(Match match, Team team)
        {
            _scorePoint = int.Clamp(_scorePoint, 0, 99);

            if (match != null)
            {
                if (!team.IsReferee) // Seul l'équipe qui joue on leur bonus qui changent !
                    if (match.GetTeamOppenent(team) != null)
                        _currentBonusPoint = _scorePoint - match.GetTeamOppenent(team).Stats._scorePoint;
            }
        }
        public bool IsWinMatch(MatchConfig matchConfig)
        {
            return Results.Count(e => e == Result.Win) >= matchConfig.NbSetToWin;
        }
        public bool IsCloseToWinMatch(MatchConfig matchConfig)
        {
            return Results.Count(e => e == Result.Win) == matchConfig.NbSetToWin - 1;
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
        public void SetBonusPoint(int bonusPoint) {  _bonusPoint = bonusPoint; _currentBonusPoint = bonusPoint; }
        public void SetTeamName(string teamName) { _teamName = teamName; }
        public void AddRankingPoint(int points) { _rankingPoint += points; }
        public void SetRankingPoint(int points) { _rankingPoint = points; }
        public void AddTotalPoint(int points) { _totalPoint += points; }
        public void SetTotalPoint(int points) { _totalPoint = points; }
        public void ValidBonusPoint() { _bonusPoint += _currentBonusPoint; _currentBonusPoint = 0; }
        public void AddPoint(int points)
        {
            _scorePoint += points;
            _scorePoint = int.Clamp(_scorePoint, 0, 99);
            AddTotalPoint(points);
        }
        public void SetScorePoint(int points) { _scorePoint = points; }
        public void AddScoreSet(Set set) { _sets.Add(set); }
        public void SetRank(int rank) { _rank = rank; }

        public Stats Clone()
        {
            Stats clone = (Stats)MemberwiseClone();

            return clone;
        }
    }
}
