//@vadym udod

using System.Collections.Generic;
using UnityEngine;

namespace HootyBird.Minimal.Tween
{
    public class AlphaTween : TweenBase
    {
        public Transform[] customObjects;
        public bool propagate = false;
        public float from = 0f;
        public float to = 1f;

        public List<Graphics> graphics { get; private set; }

        /// <summary>
        /// Actual alpha value
        /// </summary>
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

            SetAlpha(currentValue);
        }

        public void SetAlpha(float value)
        {
            _value = value;
            foreach (Graphics group in graphics) group.alpha = value;
        }

        public override void OnReset()
        {
            SetAlpha(from);
        }

        public override void OnInitialized()
        {
            graphics = DoParse(customObjects, propagate);

            if (graphics.Count > 0) _value = graphics[0].alpha;
        }
    }
}