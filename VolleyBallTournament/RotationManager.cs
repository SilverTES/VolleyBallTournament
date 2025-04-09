using Mugen.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace VolleyBallTournament
{
    public class MatchConfig
    {
        public int IdTerrain;
        public Team TeamA;
        public Team TeamB;
        public Team TeamReferee;

        public MatchConfig()
        {
            IdTerrain = Const.NoIndex;
            TeamA = null;
            TeamB = null;
            TeamReferee = null;
        }
        public MatchConfig(int idTerrain, Team teamA, Team teamB, Team teamReferee)
        {
            IdTerrain = idTerrain;
            TeamA = teamA;
            TeamB = teamB;
            TeamReferee = teamReferee;
        }
    }

    public class RotationManager
    {
        public int Time;
        public int NbGroup => _nbGroup;
        private int _nbGroup;
        public int NbTeamPerGroup => _nbTeamPerGroup;
        private int _nbTeamPerGroup;
        public int NbTerrain => _nbTerrain;
        private int _nbTerrain;

        public int NbRotation => _nbRotation;
        private int _nbRotation;

        Grid2D<MatchConfig> _grid;

        public static List<string> TeamLetters =
        [
            "A","B","C","D","E","F","G","H","I","J","K","L","M",
            "N","O","P","Q","R","S","T","U","V","W","X","Y","Z",
        ];

        public static Dictionary<string, int> Indexs { get; private set; } = new Dictionary<string, int>();

        public RotationManager() 
        {

        }
        private void CreateIndexs(int nbGroup, int nbTeamPerGroup)
        {
            int indexTeam = 0;
            for (int i = 0; i < nbGroup; i++)
            {
                for(int t = 0; t < nbTeamPerGroup; t++)
                {
                    var teamLetter = $"{TeamLetters[t]}{i + 1}";
                    Indexs[teamLetter] = indexTeam;
                    
                    Misc.Log($"{teamLetter} {Indexs[teamLetter]}");

                    indexTeam++;
                }
            }
        }

        public List<MatchConfig> GetMatchs(int step)
        {
            var list = new List<MatchConfig>();
            for (int i = 0; i < _grid.Width; i++)
            {
                list.Add(_grid.Get(i, step));
                Misc.Log($"Load Match {i}");
            }
            return list;
        }
        public void LoadFile(string xmlFile, List<Team> teams, List<Match> matchs)
        {
            XmlTextReader reader = new XmlTextReader(xmlFile);

            XDocument doc = XDocument.Load(xmlFile);
            int nbRotation = doc.Descendants("rotation").Count();
            _nbRotation = nbRotation;

            Misc.Log($"nbRotation = {nbRotation}");

            int index = 0;
            int step = 0;

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element: // Le nœud est un élément.

                        if (reader.Name == "config")
                        {
                            while (reader.MoveToNextAttribute()) // Lire les attributs.
                            {
                                if (reader.Name == "ngroupe") _nbGroup = int.Parse(reader.Value);
                                if (reader.Name == "nequipeParGroupe") _nbTeamPerGroup = int.Parse(reader.Value);
                                if (reader.Name == "nterrain") _nbTerrain = int.Parse(reader.Value);
                            }

                            _grid = new Grid2D<MatchConfig>(_nbTerrain, _nbRotation);
                            CreateIndexs(_nbGroup, _nbTeamPerGroup);
                        }

                        if (reader.Name == "match")
                        {
                            MatchConfig matchConfig = new();

                            while (reader.MoveToNextAttribute()) // Lire les attributs.
                            {
                                if (reader.Name == "terrain") matchConfig.IdTerrain = matchs[int.Parse(reader.Value) - 1].IdTerrain;
                                if (reader.Name == "teamA") matchConfig.TeamA = teams[Indexs[reader.Value]];
                                if (reader.Name == "teamB") matchConfig.TeamB = teams[Indexs[reader.Value]];
                                if (reader.Name == "referee") matchConfig.TeamReferee = teams[Indexs[reader.Value]];
                            }

                            //Console.WriteLine($"<<{set.TeamA.TeamName} vs {set.TeamB.TeamName}>> = {set.TeamReferee.TeamName}");

                            _grid.Set(index, step-1, matchConfig); // Important le step-1 , parceque le tableau débute à Zero et step est déja 1 quand il renconte l'élément "<step>"

                            index++;
                        }

                        if (reader.Name == "rotation")
                        {
                            reader.MoveToNextAttribute();
                            Console.WriteLine($"Temps = {reader.Value}");

                            index = 0;

                            step++;
                        }

                        break;

                    case XmlNodeType.Text: // Afficher le texte dans chaque élément.

                        Console.WriteLine(reader.Value);

                        break;

                    case XmlNodeType.EndElement: // Afficher la fin de l'élément.

                        //Console.Write("/>");

                        break;
                }
            }

        }
    }
}
