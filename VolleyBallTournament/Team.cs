using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mugen.Animation;
using Mugen.Core;
using Mugen.GFX;
using System;


namespace VolleyBallTournament
{
    public class Team : Node  
    {
        public Match Match = null;
        public Court Court = null;
        public ScorePanel ScorePanel = null;
        public Group Group;
        public int Rank = 0;
        public int NewRank = 0;
        public Vector2 NewPosition = Vector2.Zero;
        public int Score = 0;
        public int Set = 0;

        public int TotalPoint = 0; EasingValue _easePointTotal = new(0);
        public int NbMatchWin = 0;
        public int NbMatchPlayed = 3;
        public string TeamName;

        Animate _animate;
        public Team(string teamName, Group group, Match match = null, ScorePanel scorePanel = null, Court court = null)
        {
            SetSize(360, 64);
            TeamName = teamName;
            Group = group;
            Match = match;
            ScorePanel = scorePanel;
            Court = court;

            _animate = new Animate();
            _animate.Add("move");
        }
        public Team AddPointTotal(int points)
        { 
            TotalPoint += points; 
            _easePointTotal.SetValue(TotalPoint);
            return this;
        }
        public Team AddPoint(int points) 
        { 
            Score += points; 
            return this; 
        }
        public void MoveToRank(int rank)
        {
            if (rank == Rank) 
                return;

            NewRank = rank;
        }
        public void MoveToPosition(Vector2 position, int duration = 32)
        {
            if (position == XY) 
                return;

            NewPosition = position;
            _animate.SetMotion("move", Easing.QuadraticEaseOut, _y, NewPosition.Y, duration);
            _animate.Start("move");

        }
        public override Node Update(GameTime gameTime)
        {
            if (_animate.IsPlay != null)
            {
                _y = _animate.Value();
            }

            if (_animate.Off("move"))
            {
                //Misc.Log("End move");
            }

            Score = int.Clamp(Score, 0, 99);
            Set = int.Clamp(Set, 0, 3);

            UpdateRect();

            _animate.NextFrame();

            return base.Update(gameTime);
        }
        public override Node Draw(SpriteBatch batch, GameTime gameTime, int indexLayer)
        {
            if (indexLayer == (int)Layers.Main)
            {
                batch.FillRectangle(AbsRectF.Extend(-4f), Color.DarkSlateBlue * .5f);
                //batch.Rectangle(AbsRectF.Extend(-4f), Color.Black, 3f);


                batch.LeftMiddleString(Static.FontMain, $"{TeamName}", AbsRectF.LeftMiddle + Vector2.UnitX * 20, Color.White);

                batch.CenterStringXY(Static.FontMain, $"{Rank}", AbsRectF.LeftMiddle - Vector2.UnitX * 10, Color.Orange);

                batch.CenterStringXY(Static.FontMain, $"{_easePointTotal.GetValue()}", AbsRectF.RightMiddle + Vector2.UnitX * 10, Color.Yellow);

                for (int i = 0; i < NbMatchPlayed; i++)
                {
                    var pos = new Vector2(AbsRectF.RightMiddle.X + i * 24 - (24 * NbMatchPlayed), AbsRectF.Center.Y);
                    
                    //batch.Circle(pos, 10, 16, Color.White, 2f);
                    batch.FilledCircle(Static.TexCircle, pos, 20, Color.Gray);
                    
                    if ( i < NbMatchWin)
                        batch.FilledCircle(Static.TexCircle, pos, 20, Color.Gold);
                        //batch.Point(pos, 10, Color.Gold);
                }
            }   

            return base.Draw(batch, gameTime, indexLayer);
        }
    }
}
