//@vadym udod

using UnityEngine;

namespace HootyBird.Minimal.Tween
{
    public class RotationTween : TweenBase
    {
        public Vector3 from;
        public Vector3 to;

        public Vector3 _value { get; private set; }

        public override void PlayForward(bool resetValue)
        {
            if (from.z < 0f && to.z > 0f)
            {
                to.z -= 360f;
            }
            else if (to.z < 0f && from.z > 0f)
            {
                from.z -= 360f;
            }

            base.PlayForward(resetValue);
        }

        public override void AtProgress(float value, PlaybackDirection direction)
        {
            base.AtProgress(value, direction);

            Vector3 currentValue;
            switch (direction)
            {
                case PlaybackDirection.FORWARD:
                    if (value < 1f)
                    {
                        currentValue = Vector3.Lerp(from, to, curve.Evaluate(value));
                    }
                    else
                    {
                        currentValue = Vector3.Lerp(from, to, curve.Evaluate(1f));
                        isPlaying = false;
                    }
                    break;

                default:
                    if (value < 1f)
                    {
                        currentValue = Vector3.Lerp(to, from, curve.Evaluate(value));
                    }
                    else
                    {
                        currentValue = Vector3.Lerp(to, from, curve.Evaluate(1f));
                        isPlaying = false;
                    }
                    break;
            }

            SetRotation(currentValue);
        }

        public void SetRotation(Vector3 rotation)
        {
            transform.localEulerAngles = _value = rotation;
        }

        public override void OnReset()
        {
            SetRotation(from);
        }

        public override void OnInitialized() { }
    }
}