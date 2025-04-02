﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Mugen.Core;
using Mugen.GFX;
using Mugen.GUI;
using Mugen.Input;


namespace VolleyBallTournament
{
    internal class ScreenPlay : Node
    {
        private readonly string _tournamentName = "Phase de Poule";
        private readonly Container _divMain;
        private readonly Container _divMatch;
        private readonly Container _divGroup;

        private readonly Match[] _matchs = new Match[3];

        private readonly Group[] _groups = new Group[4];

        private readonly Team[] _teams = new Team[16];

        private readonly Timer _timer;

        public ScreenPlay()
        {
            SetSize(Screen.Width, Screen.Height);

            _divMain = new Container(Style.Space.One * 10, Style.Space.One * 0, Mugen.Physics.Position.VERTICAL);
            _divMatch = new Container(Style.Space.One * 10, new Style.Space(0, 0, 0, 0), Mugen.Physics.Position.HORIZONTAL);
            _divGroup = new Container(Style.Space.One * 10, Style.Space.One * 40, Mugen.Physics.Position.HORIZONTAL);

            int teamNumber = 0;
            for (int i = 0; i < _groups.Length; i++)
            {
                _groups[i] = (Group)new Group($"{i}").AppendTo(this);
                
                for (int t = 0; t < 4; t++)
                {
                    var team = new Team($"Team {teamNumber}", _groups[i]);
                    _teams[teamNumber] = team;

                    _groups[i].AddTeam(team);

                    teamNumber++;
                }

                _divGroup.Insert(_groups[i]);
            }

            int group = 0;
            for (int i = 0; i < _matchs.Length; i++)
            {
                _matchs[i] = (Match)new Match($"{i + 1}", _teams[group * 4 + i], _teams[group * 4 + i + 1]).AppendTo(this);
                
                _divMatch.Insert(_matchs[i]);

                group++;
            }

            _timer = (Timer)new Timer().AppendTo(this);

            _divMain.Insert(_timer);
            _divMain.Insert(_divMatch);
            _divMain.Insert(_divGroup);

            _divMain.SetPosition((Screen.Width - _divMain.Rect.Width) / 2, (Screen.Height - _divMain.Rect.Height) / 2);
            _divMain.Refresh();

        }
        public override Node Update(GameTime gameTime)
        {
            for (int i = 0; i < _matchs.Length; i++)
            {
                var match = _matchs[i];

                var teamA = match.Score.TeamA;
                var teamB = match.Score.TeamB;

                if (ButtonControl.OnePress($"AddPointA{i}", Keyboard.GetState().IsKeyDown((Keys)112 + i * 4)))
                {
                    teamA.AddPoint(1);
                }
                if (ButtonControl.OnePress($"SubPointA{i}", Keyboard.GetState().IsKeyDown((Keys)113 + i * 4)))
                {
                    teamA.AddPoint(-1);
                }
                if (ButtonControl.OnePress($"AddPointB{i}", Keyboard.GetState().IsKeyDown((Keys)114 + i * 4)))
                {
                    teamB.AddPoint(1);
                }
                if (ButtonControl.OnePress($"SubPointB{i}", Keyboard.GetState().IsKeyDown((Keys)115 + i * 4)))
                {
                    teamB.AddPoint(-1);
                }
            }

            if (ButtonControl.OnePress("ToggleTimer", Keyboard.GetState().IsKeyDown(Keys.Space)))
            {
                _timer.ToggleTimer();
                Misc.Log("Toggle Timer");

                for (int i = 0; i < _matchs.Length; i++)
                {
                    _matchs[i].Court.State.Set(_timer.IsRunning ? Court.States.Play : Court.States.Ready);
                }
            }
            if (ButtonControl.OnePress("Stop", Keyboard.GetState().IsKeyDown(Keys.Back)))
            {
                _timer.StopTimer();
                Misc.Log("Stop Timer");
            }

            UpdateChilds(gameTime);

            return base.Update(gameTime);
        }
        public override Node Draw(SpriteBatch batch, GameTime gameTime, int indexLayer)
        {
            batch.GraphicsDevice.Clear(Color.Transparent);

            if (indexLayer == (int)Layers.Main)
            {
                //batch.GraphicsDevice.Clear(Color.DarkSlateBlue);
                batch.Draw(Static.TexBG00, AbsRect, Color.White);
                batch.FillRectangle(AbsRectF, Color.Black * .75f);

                //batch.Grid(Vector2.Zero, Screen.Width, Screen.Height, 40, 40, Color.Gray * .5f, 1f);

                batch.CenterBorderedStringXY(Static.FontMain, $"{_tournamentName}", AbsRectF.TopCenter + Vector2.UnitY * 32, Color.White, Color.Black);
            }

            if (indexLayer == (int)Layers.Debug)
            {
                batch.RightMiddleString(Static.FontMain, $"V{0}.{1}", AbsRectF.BottomRight - Vector2.One * 32, Color.White);
                
                batch.LeftMiddleString(Static.FontMain, "Classement par nombre de victoires + l'écart de point entre le gagant et le perdant", AbsRectF.BottomLeft + Vector2.UnitX * 10 - Vector2.UnitY * 20, Color.White);
            }

            DrawChilds(batch, gameTime, indexLayer);

            return base.Draw(batch, gameTime, indexLayer);
        }
    }
}
