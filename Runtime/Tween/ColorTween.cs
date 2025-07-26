//@vadym udod

using System.Collections.Generic;
using UnityEngine;

namespace HootyBird.Minimal.Tween
{
    public class ColorTween : TweenBase
    {
        public Transform[] customObjects;
        /// <summary>
        /// Do the same for all child objects?
        /// </summary>
        public bool propagate = false;
        public bool changeAlpha = true;
        public Color from = Color.white;
        public Color to = Color.white;

        private List<Graphics> graphics;

        /// <summary>
        /// Current color value
        /// </summary>
        public Color _value { get; private set; }

        public override void AtProgress(float value, PlaybackDirection direction)
        {
            base.AtProgress(value, direction);

            Color currentColor;
            switch (direction)
            {
                case PlaybackDirection.FORWARD:
                    if (value < 1f)
                    {
                        currentColor = Color.Lerp(from, to, curve.Evaluate(value));
                    }
                    else
                    {
                        currentColor = Color.Lerp(from, to, curve.Evaluate(1f));
                        isPlaying = false;
                    }
                    break;

                default:
                    if (value < 1f)
                    {
                        currentColor = Color.Lerp(to, from, curve.Evaluate(value));
                    }
                    else
                    {
                        currentColor = Color.Lerp(to, from, curve.Evaluate(1f));
                        isPlaying = false;
                    }
                    break;
            }

            SetColor(currentColor);
        }

        public void SetColor(Color color)
        {
            _value = color;
            graphics.ForEach(colorGroup => colorGroup.SetColor(color, changeAlpha));
        }

        public override void OnReset() => SetColor(from);

        public override void OnInitialized()
        {
            graphics = DoParse(customObjects, propagate);

            if (graphics.Count > 0)
            {
                _value = graphics[0].color;
            }
        }
    }
}