using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mugen.Core;
using Mugen.GFX;
using System;


namespace VolleyBallTournament
{
    internal class Team : Node  
    {
        public Group Group => _group;
        Group _group;
        public int Rank => _rank;
        int _rank = 0;
        public int Score => _score;
        int _score = 0;
        public int Set => _set;
        int _set = 0;

        public int TotalPoint => _totalPoint;
        int _totalPoint = 0; EasingValue _easePointTotal = new(0);
        public int NbMatchWin => _nbMatchWin;
        int _nbMatchWin = 0;
        public int NbMatchPlayer => _nbMatchPlayed;
        int _nbMatchPlayed = 3;
        public string TeamName => _teamName;
        string _teamName;

        public Team(string teamName, Group group)
        {
            SetSize(360, 64);
            _teamName = teamName;
            _group = group;
        }
        public Team AddPointTotal(int points)
        { 
            _totalPoint += points; 
            _easePointTotal.SetValue(_totalPoint);
            return this;
        }
        public Team AddPoint(int points) { _score += points; return this; }
        public Team SetScore(int score) { _score = score; return this; }
        public Team SetSet(int set) { _set = set; return this; }
        public Team SetGroup(Group group) { _group = group; return this; }
        public Team SetRank(int rank) { _rank = rank; return this; }
        public Team SetNbMatchWin(int nbMatchWin) { _nbMatchWin = nbMatchWin; return this; }
        public Team SetNbMatchPlayed(int nbMatchPlayed) { _nbMatchPlayed = nbMatchPlayed; return this; }
        public Team SetTeamName(string teamName) { _teamName = teamName; return this; }
        public override Node Update(GameTime gameTime)
        {
            _score = int.Clamp(_score, 0, 99);
            _set = int.Clamp(_set, 0, 3);

            UpdateRect();

            return base.Update(gameTime);
        }
        public override Node Draw(SpriteBatch batch, GameTime gameTime, int indexLayer)
        {
            if (indexLayer == (int)Layers.Main)
            {
                batch.FillRectangle(AbsRectF.Extend(-4f), Color.DarkSlateBlue * .5f);
                //batch.Rectangle(AbsRectF.Extend(-4f), Color.Black, 3f);


                batch.LeftMiddleString(Static.FontMain, $"{_teamName}", AbsRectF.LeftMiddle + Vector2.UnitX * 20, Color.White);

                batch.CenterStringXY(Static.FontMain, $"{_rank}", AbsRectF.LeftMiddle - Vector2.UnitX * 10, Color.Orange);

                for (int i = 0; i < _nbMatchPlayed; i++)
                {
                    var pos = new Vector2(AbsRectF.RightMiddle.X + i * 24 - (24 * _nbMatchPlayed), AbsRectF.Center.Y);
                    
                    //batch.Circle(pos, 10, 16, Color.White, 2f);
                    batch.FilledCircle(Static.TexCircle, pos, 20, Color.Gray);
                    
                    if ( i < _nbMatchWin)
                        batch.FilledCircle(Static.TexCircle, pos, 20, Color.Gold);
                        //batch.Point(pos, 10, Color.Gold);
                }
            }   

            return base.Draw(batch, gameTime, indexLayer);
        }
    }
}
