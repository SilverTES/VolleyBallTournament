using Mugen.Core;
using System.Collections.Generic;

namespace VolleyBallTournament
{
    public class MatchConfig
    {
        public int IdTerrain;
        public int NbSetToWin;
        public int NbPointToWinSet;
        public Team TeamA;
        public Team TeamB;
        public Team TeamReferee;

        public MatchConfig()
        {
            IdTerrain = Const.NoIndex;
            NbSetToWin = 1;
            NbPointToWinSet = 25;
            TeamA = null;
            TeamB = null;
            TeamReferee = null;
        }
        public MatchConfig(int idTerrain, int nbSetToWin, int nbPointToWinSet, Team teamA, Team teamB, Team teamReferee)
        {
            IdTerrain = idTerrain;
            NbSetToWin = nbSetToWin;
            NbPointToWinSet = nbPointToWinSet;

            TeamA = teamA;
            TeamB = teamB;
            TeamReferee = teamReferee;
        }
        public static MatchConfig CreateMatchConfigsByTeams(List<Team> teams, int nbSetToWin, int nbPointToWinSet, int idTeamA, int idTeamB, int idTeamReferee)
        {
            return new MatchConfig(0, nbSetToWin, nbPointToWinSet, teams[idTeamA], teams[idTeamB], teams[idTeamReferee]);
        }

        public static List<MatchConfig> CreateMatchConfigsDemiFinal(List<Team> teams, int nbSetToWin, int nbPointToWinSet)
        {
            List<MatchConfig> matchConfigs = [];
            // Demi consolante Looser
            matchConfigs.Add(CreateMatchConfigsByTeams(teams, nbSetToWin, nbPointToWinSet, 12, 13, 8));
            matchConfigs.Add(CreateMatchConfigsByTeams(teams, nbSetToWin, nbPointToWinSet, 14, 15, 10));

            // demi consolante Looser
            matchConfigs.Add(CreateMatchConfigsByTeams(teams, nbSetToWin, nbPointToWinSet, 8, 9, 12));
            matchConfigs.Add(CreateMatchConfigsByTeams(teams, nbSetToWin, nbPointToWinSet, 10, 11, 14));

            // demi principale Looser
            matchConfigs.Add(CreateMatchConfigsByTeams(teams, nbSetToWin, nbPointToWinSet, 4, 5, 0));
            matchConfigs.Add(CreateMatchConfigsByTeams(teams, nbSetToWin, nbPointToWinSet, 6, 7, 2));

            // demi principale Winner
            matchConfigs.Add(CreateMatchConfigsByTeams(teams, nbSetToWin, nbPointToWinSet, 0, 1, 4));
            matchConfigs.Add(CreateMatchConfigsByTeams(teams, nbSetToWin, nbPointToWinSet, 2, 3, 6));

            return matchConfigs;
        }

    }
}
