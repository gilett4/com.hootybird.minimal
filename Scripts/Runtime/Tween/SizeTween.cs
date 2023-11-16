//@vadym udod

using UnityEngine;

namespace HootyBird.Minimal.Tween
{
    public class SizeTween : TweenBase
    {
        public Vector2 from;
        public Vector2 to;

        private RectTransform rectTransform;
        private SpriteRenderer spriteRenderer;

        public Vector2 _value { get; private set; }

        public override void AtProgress(float value, PlaybackDirection direction)
        {
            Vector2 currentValue;
            switch (direction)
            {
                case PlaybackDirection.FORWARD:
                    if (value < 1f)
                        currentValue = Vector2.Lerp(from, to, curve.Evaluate(value));
                    else
                    {
                        currentValue = Vector2.Lerp(from, to, curve.Evaluate(1f));
                        isPlaying = false;
                    }
                    break;

                default:
                    if (value < 1f)
                        currentValue = Vector2.Lerp(to, from, curve.Evaluate(value));
                    else
                    {
                        currentValue = Vector2.Lerp(to, from, curve.Evaluate(1f));
                        isPlaying = false;
                    }
                    break;
            }

            SetSize(currentValue);
        }

        public void SetSize(Vector2 size)
        {
            _value = size;

            if (rectTransform)
                rectTransform.sizeDelta = size;
            else if (spriteRenderer)
                spriteRenderer.size = size;
        }

        public override void OnReset()
        {
            SetSize(from);
        }

        public override void OnInitialized()
        {
            rectTransform = GetComponent<RectTransform>();
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }
}