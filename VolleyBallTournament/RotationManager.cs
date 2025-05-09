﻿using Mugen.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace VolleyBallTournament
{
    public class RotationManager
    {
        private double _matchTime;
        private double _warmUpTime;
        public int NbGroup => _nbGroup;
        private int _nbGroup;
        public int NbTeamPerGroup => _nbTeamPerGroup;
        private int _nbTeamPerGroup;
        public int NbTerrain => _nbTerrain;
        private int _nbTerrain;

        public int NbRotation => _nbRotation;
        private int _nbRotation;

        public Grid2D<MatchConfig> GridMatchConfig => _gridMatchConfig;
        Grid2D<MatchConfig> _gridMatchConfig;

        public static List<string> TeamLetters =
        [
            "A","B","C","D","E","F","G","H","I","J","K","L","M",
            "N","O","P","Q","R","S","T","U","V","W","X","Y","Z",
        ];

        public static Dictionary<string, int> Indexs { get; private set; } = [];

        public RotationManager() 
        {

        }
        // Créations des les Indexd d'équipe exemple: "A1" = 0 , "C1" = 2 et aussi selon le nombre d'équipe par groupe 
        private void CreateTeamsIndex(int nbGroup, int nbTeamPerGroup)
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

        public List<MatchConfig> GetMatchConfigs(int rotation)
        {
            //Misc.Log("GET MATCH CONFIG ************");
            List<MatchConfig> list = [];

            for (int i = 0; i < _gridMatchConfig.Width; i++)
            {
                list.Add(_gridMatchConfig.Get(i, rotation));
                //Misc.Log($"Load Match {i} : {_grid.Get(i, rotation).IdTerrain}");
            }

            return list;
        }
        public double GetMatchTime()
        {
            return _matchTime;
        }
        public double GetWarmUpTime()
        {
            return _warmUpTime;
        }
        public void LoadFile(string xmlFile, List<Team> teams, List<Match> matchs)
        {
            //XmlTextReader reader = new XmlTextReader(xmlFile);

            XDocument doc = XDocument.Load(xmlFile);
            int nbRotation = doc.Descendants("rotation").Count();
            _nbRotation = nbRotation;

            //Misc.Log($"nbRotation = {nbRotation}");

            // Lire la config
            var config = doc.Root.Element("config");
            int nbGroupe = int.Parse(config.Attribute("nbGroupe").Value);
            int nbEquipeParGroupe = int.Parse(config.Attribute("nbEquipeParGroupe").Value);
            int nbTerrain = int.Parse(config.Attribute("nbTerrain").Value);
            int tempsMatch = int.Parse(config.Attribute("temps").Value);
            int tempsEchauffement = int.Parse(config.Attribute("echauffement").Value);

            _matchTime = tempsMatch;
            _warmUpTime = tempsEchauffement;

            _gridMatchConfig = new Grid2D<MatchConfig>(_nbTerrain = nbTerrain, _nbRotation = nbRotation);
            CreateTeamsIndex(_nbGroup = nbGroupe, _nbTeamPerGroup = nbEquipeParGroupe);


            Console.WriteLine($"Config : {nbGroupe} groupes, {nbEquipeParGroupe} équipes/groupe, {nbTerrain} terrains\n");

            // Lire les rotations
            var rotations = doc.Root.Elements("rotation");

            int indexRotation = 0;
            foreach (var rotation in rotations)
            {

                Console.WriteLine($"Rotation {rotation} - Temps : {tempsMatch}s - Echauffement : {tempsEchauffement}");

                int indexMatch = 0;
                foreach (var match in rotation.Elements("match"))
                {
                    MatchConfig matchConfig = new();

                    int terrain = int.Parse(match.Attribute("terrain").Value); 
                    int sets = int.Parse(match.Attribute("nbSet").Value); 
                    string equipes = match.Attribute("equipes").Value;
                    string arbitre = match.Attribute("arbitre").Value;

                    matchConfig.IdTerrain = terrain - 1;
                    matchConfig.NbSetToWin = sets;
                    var opponents = equipes.Split(':');
                    //Misc.Log($"{opponents[0]} vs {opponents[1]}");
                    matchConfig.TeamA = teams[Indexs[opponents[0]]];
                    matchConfig.TeamB = teams[Indexs[opponents[1]]];

                    matchConfig.TeamReferee = teams[Indexs[arbitre]];


                    Console.WriteLine($"  Terrain {terrain} : {equipes} (Arbitre : {arbitre})");

                    _gridMatchConfig.Set(indexMatch, indexRotation, matchConfig); // Important le rotation-1 , parceque le tableau débute à Zero et rotation est déja 1 quand il renconte l'élément "<rotation>"

                    indexMatch++;
                }

                indexRotation++;

                Console.WriteLine();
            }

            Misc.Log("---- Show Grid Match ---");
            for (int r = 0; r < _gridMatchConfig.Height; r++)
            {
                for (int c = 0; c < _gridMatchConfig.Width; c++)
                {
                    var match = _gridMatchConfig.Get(c, r);
                    if (match != null)
                    {
                        Console.Write($"{_gridMatchConfig.Get(c, r).IdTerrain}");
                    }
                    else
                    {
                        Console.Write($"x");
                    }
                }
                Console.WriteLine();
            }

        }
    }
}
