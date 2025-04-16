using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Mugen.Core;
using Mugen.GFX;
using Mugen.GUI;
using Mugen.Input;
using System.Collections.Generic;
using System.Xml.Linq;

namespace VolleyBallTournament
{
    public class PhaseRegister : Node   
    {
        public bool IsLocked = false;
        private string _title = "Enregistrement des équipes";
        private KeyboardState _key;

        Container _div;

        List<GroupRegister> _groupRegisters = [];

        public int NbGroup => _nbGroup;
        private int _nbGroup;
        public int NbTeamPerGroup => _nbTeamPerGroup;
        private int _nbTeamPerGroup;
        public int NbMatch => _nbMatch;
        private int _nbMatch;

        private List<Team> _teams = [];
        private List<Group> _groups = [];
        private List<Match> _matchs = [];

        public RotationManager RotationManager => _rotationManager;
        private RotationManager _rotationManager = new RotationManager();

        private TextBox _currentTextBox = null;

        Game _game;
        public PhaseRegister(Game game, string xmlFile)//, int nbGroup, int nbTeamPerGroup, int nbMatch)
        {
            //_nbGroup = nbGroup;
            //_nbTeamPerGroup = nbTeamPerGroup;
            //_nbMatch = nbMatch;
            _game = game;

            LoadConfigFile(xmlFile);

            SetSize(Screen.Width, Screen.Height);

            _div = new Container(Style.Space.One * 20, Style.Space.One * 20);

            for (int i = 0; i < _nbGroup; i++)
            {
                var groupRegister = new GroupRegister(game, _nbTeamPerGroup, i).AppendTo(this).This<GroupRegister>();
                _div.Insert(groupRegister);
                _groupRegisters.Add(groupRegister);
            }

            _div.SetPosition((Screen.Width - _div.Rect.Width) / 2, (Screen.Height - _div.Rect.Height) / 2);
            _div.Refresh();

            CreateGroups();
            CreateMatchs();

            // Associe les textBox avec les TeamName des joueurs venant d'être créer !
            var textBoxs = GetTexBoxs();
            for (int i = 0; i < textBoxs.Count; i++)
            {
                var textBox = textBoxs[i];
                textBox.OnChange += (t) =>
                {
                    _teams[t.Id].Stats.SetTeamName(textBox.Text);
                };
                textBox.OnFocus += (t) => 
                { 
                    _currentTextBox = t;
                };
            }

        }
        private void LoadConfigFile(string xmlFile)
        {
            XDocument doc = XDocument.Load(xmlFile);
            // Lire la config
            var config = doc.Root.Element("config");
            int nbGroupe = int.Parse(config.Attribute("nbGroupe").Value);
            int nbEquipeParGroupe = int.Parse(config.Attribute("nbEquipeParGroupe").Value);
            int nbTerrain = int.Parse(config.Attribute("nbTerrain").Value);

            _nbGroup = nbGroupe;
            _nbTeamPerGroup = nbEquipeParGroupe;
            _nbMatch = nbTerrain;
        }
        public List<Team> GetTeams() { return _teams; }
        public List<Group> GetGroups() { return _groups; }
        public List<Match> GetMatchs() { return _matchs; }
        private void CreateGroups()
        {
            int teamNumber = 0;
            for (int i = 0; i < _nbGroup; i++)
            {
                var group = new Group($"Groupe {(char)(64 + i + 1)}");
                _groups.Add(group);

                for (int t = 0; t < _nbTeamPerGroup; t++)
                {
                    var team = new Team($"Team {teamNumber + 1}");
                    _teams.Add(team);

                    group.AddTeam(team);

                    teamNumber++;   
                }
            }
        }
        private void CreateMatchs()
        {
            for (int i = 0; i < _nbMatch; i++)
            {
                var teamA = new Team("TeamA");
                var teamB = new Team("TeamB");
                var teamReferee = new Team("TeamR");

                var match = new Match(_game, $"{i + 1}", new MatchConfig(i, 1, 25, teamA, teamB, teamReferee));

                _matchs.Add(match);
            }
        }
        public List<TextBox> GetTexBoxs()
        {
            var texBoxs = new List<TextBox>();
            
            for (int i = 0; i < _groupRegisters.Count; i++)
            {
                var textBoxs = _groupRegisters[i].GroupOf<TextBox>();

                for (int t = 0; t < textBoxs.Count; t++)
                {
                    var textBox = textBoxs[t];
                    texBoxs.Add(textBox);
                }
            }

            return texBoxs;
        }
        public void FocusPrevGroupRegister(TextBox textBox)
        {
            if (textBox == null) return;

            if (!textBox.IsCursorAtHome) return;

            if (_groupRegisters.Count == 0) return;

            var groupRegister = textBox._parent.This<GroupRegister>();

            int idGroupRegister = groupRegister.IdGroupRegister;

            int idPrevGroupRegister = idGroupRegister - 1;

            if (idGroupRegister == 0)
            {
                idPrevGroupRegister = _groupRegisters.Count - 1;
            }

            var prevGroupRegister = _groupRegisters[idPrevGroupRegister];
            // Va dans le group précédent
            var textBoxs = prevGroupRegister.GroupOf<TextBox>();

            int idPrevTextBox = _currentTextBox.Id % NbTeamPerGroup;

            _currentTextBox.SetFocus(false);
            _currentTextBox = textBoxs[idPrevTextBox].SetFocus(true);

        }
        public void FocusNextGroupRegister(TextBox textBox)
        {
            if (textBox == null) return;

            if (!textBox.IsCursorAtEnd) return;

            if (_groupRegisters.Count == 0) return;

            var groupRegister = textBox._parent.This<GroupRegister>();

            int idGroupRegister = groupRegister.IdGroupRegister;

            int idNextGroupRegister = idGroupRegister + 1;

            if (idGroupRegister > _groupRegisters.Count - 2 ) 
            {
                idNextGroupRegister = 0;
            }

            var nextGroupRegister = _groupRegisters[idNextGroupRegister];
            // Va dans le group suivant
            var textBoxs = nextGroupRegister.GroupOf<TextBox>();

            int idNextTextBox = _currentTextBox.Id % NbTeamPerGroup;

            _currentTextBox.SetFocus(false);
            _currentTextBox = textBoxs[idNextTextBox].SetFocus(true);

        }
        public void FocusNextTextBox()
        {   
            var textBoxs = GetTexBoxs();

            if (textBoxs.Count == 0) return;

            bool oneFocus = false;
            // search focused textBox
            for (int i = 0;i < textBoxs.Count;i++)
            {
                var textBox = textBoxs[i];
                    
                if (textBox.IsFocus)
                {
                    if (i < textBoxs.Count - 1)
                    {
                        textBox.SetFocus(false);
                        _currentTextBox = textBoxs[i + 1].SetFocus(true);
                    }
                    else
                    {
                        textBox.SetFocus(false);
                        _currentTextBox = textBoxs[0].SetFocus(true);
                    }
                    oneFocus = true;
                    break;
                }
            }
            if (!oneFocus)
                _currentTextBox = textBoxs[0].SetFocus(true);
        }
        public void FocusPrevTextBox()
        {
            var textBoxs = GetTexBoxs();

            if (textBoxs.Count == 0) return;

            bool oneFocus = false;
            // search focused textBox
            for (int i = 0; i < textBoxs.Count; i++)
            {
                var textBox = textBoxs[i];

                if (textBox.IsFocus)
                {
                    if (i > 0)
                    {
                        textBox.SetFocus(false);
                        _currentTextBox = textBoxs[i - 1].SetFocus(true);
                    }
                    else
                    {
                        textBox.SetFocus(false);
                        _currentTextBox = textBoxs[textBoxs.Count - 1].SetFocus(true);
                    }
                    oneFocus = true;
                    break;
                }
            }
            if (!oneFocus)
                _currentTextBox = textBoxs[0].SetFocus(true);
        }
        public override Node Update(GameTime gameTime)
        {
            UpdateRect();

            if (IsLocked)
            {
                var textBoxs = GroupOf<TextBox>();
                for (int i = 0; i < textBoxs.Count; i++)
                {
                    var textBox = textBoxs[i];
                    textBox.SetFocus(false);
                }
            }
            else
            {
                _key = Static.Key;

                if (!_key.IsKeyDown(Keys.LeftControl)) // Lock les touches si changement d'ecran
                {
                    if (ButtonControl.OnPress("Tab", _key.IsKeyDown(Keys.Tab))) FocusNextTextBox();
                    if (ButtonControl.OnPress("Up", _key.IsKeyDown(Keys.Up))) FocusPrevTextBox();
                    if (ButtonControl.OnPress("Down", _key.IsKeyDown(Keys.Down))) FocusNextTextBox();
                    if (ButtonControl.OnPress("Enter", _key.IsKeyDown(Keys.Enter))) FocusNextTextBox();

                    if (ButtonControl.OnPress("Left", _key.IsKeyDown(Keys.Left))) FocusPrevGroupRegister(_currentTextBox);
                    if (ButtonControl.OnPress("Right", _key.IsKeyDown(Keys.Right))) FocusNextGroupRegister(_currentTextBox);
                }
            }

            UpdateChilds(gameTime);

            return base.Update(gameTime);
        }
        public override Node Draw(SpriteBatch batch, GameTime gameTime, int indexLayer)
        {
            if (indexLayer == (int)Layers.Main)
            {
                batch.Draw(Static.TexBG00, AbsRect, Color.White);
                batch.FillRectangle(AbsRectF, Color.Black * .25f);

                //batch.Grid(AbsXY + Vector2.Zero, Screen.Width, Screen.Height, 40, 40, Color.Black * .5f);
                //for (int i = 0; i < 4; i++)
                //{
                //    batch.FillRectangle(AbsXY + new Vector2(i * 480 + 40, 280), new Vector2(400, 600), Color.DarkSlateBlue * .5f);

                //    batch.CenterStringXY(Static.FontMain, $"Groupe {i+1}", AbsXY + new Vector2(i * 480 + 40 + 200, 280) + Vector2.One * 6, Color.Black *.5f);
                //    batch.CenterStringXY(Static.FontMain, $"Groupe {i+1}", AbsXY + new Vector2(i * 480 + 40 + 200, 280), Color.White);
                //}

            }

            if (indexLayer == (int)Layers.HUD)
            {
                batch.LeftTopString(Static.FontMain, _title, AbsRectF.TopLeft + Vector2.UnitX * 40 + Vector2.One * 6, Color.Black);
                batch.LeftTopString(Static.FontMain, _title, AbsRectF.TopLeft + Vector2.UnitX * 40, Color.White);
            }
            if (indexLayer == (int)Layers.Debug)
            {
                //if (_currentTextBox != null)
                //    batch.CenterStringXY(Static.FontMini, $"{_currentTextBox.Id}", _currentTextBox.AbsRectF.BottomCenter, Color.Red);
            }

            DrawChilds(batch, gameTime, indexLayer);
            return base.Draw(batch, gameTime, indexLayer);
        }
    }
}
