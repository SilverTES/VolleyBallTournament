using Mugen.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace VolleyBallTournament
{
    public class Set
    {
        public Team TeamA;
        public Team TeamB;
        public Team TeamReferee;

        public Set()
        {
            TeamA = null;
            TeamB = null;
            TeamReferee = null;
        }
        public Set(Team teamA, Team teamB, Team teamReferee)
        {
            TeamA = teamA;
            TeamB = teamB;
            TeamReferee = teamReferee;
        }
    }

    public class Sequence
    {
        public int Time;

        Grid2D<Set> _grid;

        public static Dictionary<string, int> Indexs { get; private set; } = new Dictionary<string, int>()
        {
            { "A1", 0},{ "B1", 1},{ "C1", 2},{ "D1", 3},
            { "A2", 4},{ "B2", 5},{ "C2", 6},{ "D2", 7},
            { "A3", 8},{ "B3", 9},{ "C3", 10},{ "D3", 11},
            { "A4", 12},{ "B4", 13},{ "C4", 14},{ "D4", 15},
        };

        public Sequence() 
        {
            _grid = new Grid2D<Set>(3, 8);
        }
        public int NbStep()
        {
            return _grid.Height;
        }

        public List<Set> GetList(int step)
        {
            var list = new List<Set>();
            list.Add(_grid.Get(0, step));
            list.Add(_grid.Get(1, step));
            list.Add(_grid.Get(2, step));

            return list;
        }
        public void Init(string xmlFile, List<Team> teams)
        {
            XmlTextReader reader = new XmlTextReader(xmlFile);

            int index = 0;
            int step = 0;

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element: // Le nœud est un élément.

                        if (reader.Name == "match")
                        {
                            Set set = new();

                            while (reader.MoveToNextAttribute()) // Lire les attributs.
                            {
                                if (reader.Name == "teamA") set.TeamA = teams[Indexs[reader.Value]];
                                if (reader.Name == "teamB") set.TeamB = teams[Indexs[reader.Value]];
                                if (reader.Name == "referee") set.TeamReferee = teams[Indexs[reader.Value]];
                            }

                            //Console.WriteLine($"<<{set.TeamA.TeamName} vs {set.TeamB.TeamName}>> = {set.TeamReferee.TeamName}");

                            _grid.Set(index, step-1, set); // Important le step-1 , parceque le tableau débute à Zero et step est déja 1 quand il renconte l'élément "<step>"

                            index++;
                        }

                        if (reader.Name == "step")
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
