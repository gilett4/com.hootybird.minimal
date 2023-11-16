//@vadym udod

using UnityEngine;

namespace HootyBird.Minimal.Tween
{
    public class PositionTween : TweenBase
    {
        public Vector3 from;
        public Vector3 to;
        public bool local = true;

        private RectTransform rectTransform;

        public Vector3 _value { get; private set; }

        public void FromCurrentPosition()
        {
            if (rectTransform)
                from = rectTransform.anchoredPosition;
            else
                from = transform.localPosition;
        }

        public override void AtProgress(float value, PlaybackDirection direction)
        {
            Vector3 currentValue;
            switch (direction)
            {
                case PlaybackDirection.FORWARD:
                    if (value < 1f)
                        currentValue = Vector3.Lerp(from, to, curve.Evaluate(value));
                    else
                    {
                        currentValue = Vector3.Lerp(from, to, curve.Evaluate(1f));
                        isPlaying = false;
                    }
                    break;

                default:
                    if (value < 1f)
                        currentValue = Vector3.Lerp(to, from, curve.Evaluate(value));
                    else
                    {
                        currentValue = Vector3.Lerp(to, from, curve.Evaluate(1f));
                        isPlaying = false;
                    }
                    break;
            }

            SetPosition(currentValue);
        }

        public void SetPosition(Vector3 position)
        {
            _value = position;

            if (local)
            {
                if (rectTransform)
                    rectTransform.anchoredPosition = position;
                else
                    transform.localPosition = position;
            }
            else
                transform.position = position;
        }

        public override void OnReset()
        {
            SetPosition(from);
        }

        public override void OnInitialized()
        {
            rectTransform = GetComponent<RectTransform>();

            if (local)
            {
                if (rectTransform)
                    _value = rectTransform.anchoredPosition;
                else
                    _value = transform.localPosition;
            }
            else
                _value = transform.position;
        }
    }
}