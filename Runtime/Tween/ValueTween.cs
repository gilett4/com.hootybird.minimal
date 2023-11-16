//@vadym udod

using System;
using UnityEngine;

namespace HootyBird.Minimal.Tween
{
    public class ValueTween : TweenBase
    {
        public float from = 0f;
        public float to = 1f;

        public UnityEventFloat onValue;
        public Action<float> _onValue;

        public float _value { get; private set; }

        public override void AtProgress(float value, PlaybackDirection direction)
        {
            float currentValue;
            switch (direction)
            {
                case PlaybackDirection.FORWARD:
                    if (value < 1f)
                        currentValue = Mathf.Lerp(from, to, curve.Evaluate(value));
                    else
                    {
                        currentValue = Mathf.Lerp(from, to, curve.Evaluate(1f));
                        isPlaying = false;
                    }
                    break;

                default:
                    if (value < 1f)
                        currentValue = Mathf.Lerp(to, from, curve.Evaluate(value));
                    else
                    {
                        currentValue = Mathf.Lerp(to, from, curve.Evaluate(1f));
                        isPlaying = false;
                    }
                    break;
            }

            InvokeValue(currentValue);
        }

        public void InvokeValue(float value)
        {
            _value = value;

            onValue?.Invoke(value);
            _onValue?.Invoke(value);
        }

        public override void OnReset()
        {
            InvokeValue(0f);
        }

        public override void OnInitialized() { }
    }
}
