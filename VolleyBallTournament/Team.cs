﻿using Microsoft.Xna.Framework;
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

        int _rank = 0;
        public int Score => _score;
        int _score = 0;
        public int Set => _set;
        int _set = 0;

        int _pointTotal = 0; EasingValue _easePointTotal = new(0);
        int _nbMatchWin = 0;
        int _nbMatchPlayed = 3;
        string _teamName;
        public string TeamName => _teamName;

        public Team(string teamName, Group group)
        {
            SetSize(360, 64);
            _teamName = teamName;
            _group = group;
        }
        public void AddPointTotal(int points)
        { 
            _pointTotal += points; 
            _easePointTotal.SetValue(_pointTotal);
        }
        public void AddPoint(int points)
        {
            _score += points;
        }
        public void SetScore(int score) { _score = score; }
        public void SetSet(int set) { _set = set; } 
        public void SetGroup(Group group)
        { 
            _group = group; 
        }
        public void SetRank(int rank)
        { 
            _rank = rank; 
        }
        public void SetNbMatchWin(int nbMatchWin)
        {
            _nbMatchWin = nbMatchWin;
        }
        public void SetNbMatchPlayed(int nbMatchPlayed)
        {
            _nbMatchPlayed = nbMatchPlayed;
        }
        public void SetTeamName(string teamName)
        { 
            _teamName = teamName; 
        }
        public override Node Update(GameTime gameTime)
        {
            UpdateRect();

            return base.Update(gameTime);
        }
        public override Node Draw(SpriteBatch batch, GameTime gameTime, int indexLayer)
        {
            if (indexLayer == (int)Layers.Main)
            {
                batch.FillRectangle(AbsRectF.Extend(-4f), Color.Black * .5f);
                batch.Rectangle(AbsRectF.Extend(-4f), Color.Gray, 1f);


                batch.LeftMiddleBorderedString(Static.FontMain, $"{_teamName}", AbsRectF.LeftMiddle + Vector2.UnitX * 20, Color.White, Color.Black);

                batch.CenterBorderedStringXY(Static.FontMain, $"{_rank}", AbsRectF.LeftMiddle - Vector2.UnitX * 20, Color.Orange, Color.Black);

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
