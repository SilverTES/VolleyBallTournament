using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mugen.Core;
using Mugen.GFX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VolleyBallTournament
{
    internal class Timer : Node
    {
        private double _elapsedTime;
        public bool IsRunning => _isRunning;
        private bool _isRunning;

        public TimeSpan ElapsedTime => TimeSpan.FromSeconds(_elapsedTime);
        public Timer() 
        {
            _elapsedTime = 0;
            _isRunning = false;

            SetSize(360, 120);
            SetPivot(Mugen.Physics.Position.CENTER);
        }
        public void StartTimer()
        {
            _isRunning = true;
        }

        public void StopTimer()
        {
            _isRunning = false;
            _elapsedTime = 0;
        }
        public void ToggleTimer()
        { 
            _isRunning = !_isRunning; 
        }
        public void ResetTimer()
        {
            _elapsedTime = 0;
        }
        public string GetFormattedTime()
        {
            return string.Format("{0:D2}:{1:D2}",//:{2:D2}",// {3:D2}",
                ElapsedTime.Minutes,
                ElapsedTime.Seconds);
                //ElapsedTime.Milliseconds / 10);
                //,ElapsedTime.Milliseconds % 10);
        }
        public override Node Update(GameTime gameTime)
        {
            UpdateRect();

            if (_isRunning)
            {
                _elapsedTime += gameTime.ElapsedGameTime.TotalSeconds;
            }

            return base.Update(gameTime);
        }
        public override Node Draw(SpriteBatch batch, GameTime gameTime, int indexLayer)
        {
            if (indexLayer == (int)Layers.Main)
            {
                batch.FillRectangleCentered(AbsXY + OXY, AbsRect.Size.ToVector2(), Color.Black * .25f, 0);
                //batch.CenterStringXY(Static.FontDigitMonoBG, GetFormattedTime(), AbsXY + OXY, Color.Black);
                batch.CenterStringXY(Static.FontDigitMono, GetFormattedTime(), AbsXY + OXY, Color.Orange);
            }

            return base.Draw(batch, gameTime, indexLayer);
        }
    }
}
