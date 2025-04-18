using Mugen.Core;
using System.Collections.Generic;

namespace VolleyBallTournament
{
    public class MatchConfig
    {
        public int IdTerrain;
        public int NbSetToWin;
        public int NbPointToWinSet;
        //public int PointGap; // différence de points pour la victoire du set ou match 1 ou 2  
        public Team TeamA;
        public Team TeamB;
        public Team TeamReferee;

        public MatchConfig()
        {
            IdTerrain = Const.NoIndex;
            NbSetToWin = 1;
            NbPointToWinSet = 25;
            //PointGap = 1;
            TeamA = null;
            TeamB = null;
            TeamReferee = null;
        }
        public MatchConfig(int idTerrain, int nbSetToWin, int nbPointToWinSet, Team teamA, Team teamB, Team teamReferee)
        {
            IdTerrain = idTerrain;
            NbSetToWin = nbSetToWin;
            NbPointToWinSet = nbPointToWinSet;
            //PointGap = pointGap;

            TeamA = teamA;
            TeamB = teamB;
            TeamReferee = teamReferee;
        }
        public static MatchConfig CreateMatchConfigsByTeams(List<Team> teams, int nbSetToWin, int nbPointToWinSet, int idTeamA, int idTeamB, int idTeamReferee)
        {   
            Team teamA = null;
            Team teamB = null;
            Team teamReferee = null;

            if (idTeamA >= 0 && idTeamA < teams.Count) teamA = teams[idTeamA];
            if (idTeamB >= 0 && idTeamB < teams.Count) teamB = teams[idTeamB];
            if (idTeamReferee >= 0 && idTeamReferee < teams.Count) teamReferee = teams[idTeamReferee];

            return new MatchConfig(0, nbSetToWin, nbPointToWinSet, teamA, teamB, teamReferee);
        }

        public static List<MatchConfig> CreateMatchConfigsDemiFinal(List<Team> teams, int nbSetToWin, int nbPointToWinSet)
        {
            List<MatchConfig> matchConfigs = [];
            // Demi consolante Looser
            matchConfigs.Add(CreateMatchConfigsByTeams(teams, nbSetToWin, nbPointToWinSet, 12, 13, -1));
            matchConfigs.Add(CreateMatchConfigsByTeams(teams, nbSetToWin, nbPointToWinSet, 14, 15, -1));

            // demi consolante Looser
            matchConfigs.Add(CreateMatchConfigsByTeams(teams, nbSetToWin, nbPointToWinSet, 8, 9, -1));
            matchConfigs.Add(CreateMatchConfigsByTeams(teams, nbSetToWin, nbPointToWinSet, 10, 11, -1));

            // demi principale Looser
            matchConfigs.Add(CreateMatchConfigsByTeams(teams, nbSetToWin, nbPointToWinSet, 4, 5, -1));
            matchConfigs.Add(CreateMatchConfigsByTeams(teams, nbSetToWin, nbPointToWinSet, 6, 7, -1));

            // demi principale Winner
            matchConfigs.Add(CreateMatchConfigsByTeams(teams, nbSetToWin, nbPointToWinSet, 0, 1, -1));
            matchConfigs.Add(CreateMatchConfigsByTeams(teams, nbSetToWin, nbPointToWinSet, 2, 3, -1));

            return matchConfigs;
        }

    }
}
