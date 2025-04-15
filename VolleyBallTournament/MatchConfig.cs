using Mugen.Core;

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
    }
}
