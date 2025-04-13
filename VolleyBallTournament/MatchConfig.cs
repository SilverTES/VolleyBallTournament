using Mugen.Core;

namespace VolleyBallTournament
{
    public class MatchConfig
    {
        public int IdTerrain;
        public int NbSetToWin;
        public Team TeamA;
        public Team TeamB;
        public Team TeamReferee;

        public MatchConfig()
        {
            IdTerrain = Const.NoIndex;
            NbSetToWin = 1;
            TeamA = null;
            TeamB = null;
            TeamReferee = null;
        }
        public MatchConfig(int idTerrain, int NbSetToWin, Team teamA, Team teamB, Team teamReferee)
        {
            IdTerrain = idTerrain;
            TeamA = teamA;
            TeamB = teamB;
            TeamReferee = teamReferee;
        }
    }
}
