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
    public class TimerCountDown : Node
    {
        private double _elapsedTime;
        private double _durationTime;

        public bool IsRunning => _isRunning;
        private bool _isRunning;

        private bool _onRemainingTime = false;
        public TimeSpan ElapsedTime => TimeSpan.FromSeconds(_durationTime) - TimeSpan.FromSeconds(_elapsedTime);
        public TimerCountDown(double durationInSeconds = 120) 
        {
            _elapsedTime = 0;
            _durationTime = durationInSeconds;
            _isRunning = false;

            SetSize(480, 160);
            SetPivot(Mugen.Physics.Position.CENTER);
        }
        public bool IsFinish()
        {
            return _durationTime - _elapsedTime <= 0;
        }
        public bool OnRemaingTime(double time)
        {
            //Misc.Log($"{(int)_durationTime - (int)_elapsedTime}");

            if (_onRemainingTime) 
                return false;


            if ((int)_durationTime - (int)_elapsedTime == time)
            {
                Misc.Log("On Remaining Time");
                _onRemainingTime = true;
                return true;
            }

            return false;

        }
        public void SetDuration(double durationInSeconds)
        { 
            _durationTime = durationInSeconds; 
        }
        public void StartTimer()
        {
            _onRemainingTime = false;
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
                batch.FillRectangleCentered(AbsXY + OXY, AbsRect.Size.ToVector2(), Color.Black * .5f, 0);
                //batch.CenterStringXY(Static.FontDigitMonoBG, GetFormattedTime(), AbsXY + OXY, Color.Black);
                batch.CenterStringXY(Static.FontDigitMono, GetFormattedTime(), AbsXY + OXY, Color.OrangeRed);
            }

            return base.Draw(batch, gameTime, indexLayer);
        }
    }
}
