using Microsoft.Xna.Framework;
using Mugen.Animation;

namespace VolleyBallTournament
{
    public class EasingValue
    {
        float _value { get; set; }
        Animate _animate = new();
        public EasingValue(float initValue = 0f)
        {
            _value = initValue;
            _animate.Add("easing");
        }
        public float SetValue(float newValue, float duration = 32f)
        {
            float prevValue = _value;
            _value = newValue;

            _animate.SetMotion("easing", Easing.QuadraticEaseOut, new Tweening(prevValue, _value, duration));
            _animate.Start("easing");

            return _value;
        }
        public float GetValue()
        {
            if (_animate.IsPlay())
            {
                _value = (int)_animate.Value();
            }

            _animate.NextFrame();

            return _value;
        }

        //public void Update(GameTime gameTime)
        //{

        //}
    }
}
