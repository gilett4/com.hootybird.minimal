//@vadym udod

using UnityEngine;

namespace HootyBird.Minimal.Tween
{
    public class ScaleTween : TweenBase
    {
        public Vector3 from = Vector3.one;
        public Vector3 to = Vector3.one;

        public Vector3 _value { get; private set; }

        public override void AtProgress(float value, PlaybackDirection direction)
        {
            base.AtProgress(value, direction);

            Vector3 currentValue;
            switch (direction)
            {
                case PlaybackDirection.FORWARD:
                    if (value < 1f)
                    {
                        currentValue = Vector3.LerpUnclamped(from, to, curve.Evaluate(value));
                    }
                    else
                    {
                        currentValue = Vector3.LerpUnclamped(from, to, curve.Evaluate(1f));
                        isPlaying = false;
                    }
                    break;

                default:
                    if (value < 1f)
                    {
                        currentValue = Vector3.LerpUnclamped(to, from, curve.Evaluate(value));
                    }
                    else
                    {
                        currentValue = Vector3.LerpUnclamped(to, from, curve.Evaluate(1f));
                        isPlaying = false;
                    }
                    break;
            }

            SetScale(currentValue);
        }

        public void SetScale(Vector3 scale)
        {
            transform.localScale = _value = scale;
        }

        public override void OnReset()
        {
            SetScale(from);
        }

        public override void OnInitialized() { }
    }
}