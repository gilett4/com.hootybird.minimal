//@vadym udod

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace HootyBird.Minimal.Tween
{
    public abstract class TweenBase : MonoBehaviour
    {
        public PlaybackDirection playbackDirection = PlaybackDirection.FORWARD;
        public AnimationCurve curve;
        public bool unscaledTime = false;
        public float playbackTime = .3f;
        public bool playOnAwake = false;
        public RepeatType repeat = RepeatType.NONE;
        public UnityEventFloat onProgress;

        public Action<float> _onProgress;

        /// <summary>
        /// progress value
        /// </summary>
        public float value { get; protected set; }
        public float defaultPlaybackTime { get; protected set; }
        public bool isPlaying { get; protected set; }
        public bool initialized { get; protected set; }

        protected virtual void Awake()
        {
            Initialize();
        }

        protected virtual void Update()
        {
            if (!isPlaying)
            {
                return;
            }

            value = Mathf.Clamp01(value + ((unscaledTime) ? Time.unscaledDeltaTime : Time.deltaTime) / playbackTime);

            Progress(value, playbackDirection);
        }

        public virtual void PlayForward(bool resetValue)
        {
            if (resetValue)
            {
                value = 0f;
            }
            else
            {
                if (value != 0)
                {
                    value = 1f - value;
                }
            }

            isPlaying = true;
            playbackDirection = PlaybackDirection.FORWARD;

            Progress(value, playbackDirection);
        }

        public virtual void PlayBackward(bool resetValue)
        {
            if (resetValue)
            {
                value = 0f;
            }
            else
            {
                if (value != 0f)
                {
                    value = 1f - value;
                }
            }

            isPlaying = true;
            playbackDirection = PlaybackDirection.BACKWARD;

            Progress(value, playbackDirection);
        }

        public virtual void Toggle(bool reset)
        {
            switch (playbackDirection)
            {
                case PlaybackDirection.BACKWARD:
                    PlayForward(reset);
                    break;

                case PlaybackDirection.FORWARD:
                    PlayBackward(reset);
                    break;
            }
        }

        public virtual void StopTween(bool reset)
        {
            isPlaying = false;

            if (reset)
            {
                OnReset();
            }
        }

        public virtual void ResetPlaybackTime()
        {
            playbackTime = defaultPlaybackTime;
        }

        public virtual void AtProgress(Single value)
        {
            AtProgress(value, playbackDirection);
        }

        public virtual void Progress(Single value, PlaybackDirection direction)
        {
            Initialize();

            AtProgress(value, direction);

            if (value >= 1f)
            {
                switch (repeat)
                {
                    case RepeatType.ZERO_TO_ONE:
                        if (direction == PlaybackDirection.FORWARD)
                        {
                            PlayForward(true);
                        }
                        else
                        {
                            PlayBackward(true);
                        }

                        break;

                    case RepeatType.PING_PONG:
                        Toggle(true);

                        break;
                }
            }
        }

        public void Initialize()
        {
            if (initialized)
            {
                return;
            }

            initialized = true;
            defaultPlaybackTime = playbackTime;

            OnInitialized();

            if (playOnAwake)
            {
                switch (playbackDirection)
                {
                    case PlaybackDirection.BACKWARD:
                        PlayBackward(true);

                        break;

                    case PlaybackDirection.FORWARD:
                        PlayForward(true);

                        break;
                }
            }
        }

        public List<Graphics> DoParse(Transform[] customObjects, bool propagate)
        {
            List<Graphics> _graphics = new List<Graphics>();

            if (customObjects.Length != 0)
            {
                foreach (Transform obj in customObjects)
                {
                    _graphics = Parse(obj, propagate);
                }
            }
            else
            {
                _graphics = Parse(transform, propagate);
            }

            return _graphics;
        }

        public List<Graphics> Parse(Transform obj, bool propagate)
        {
            List<Graphics> _graphics = new List<Graphics>();

            _graphics.Add(new Graphics() { 
                spriteRenderer = obj.GetComponent<SpriteRenderer>(), 
                uiGraphics = obj.GetComponent<MaskableGraphic>(), 
                lineRenderer = obj.GetComponent<LineRenderer>(),
                canvasGroup = obj.GetComponent<CanvasGroup>(),
            });

            if (propagate)
            {
                for (int i = 0; i < obj.childCount; i++)
                {
                    Parse(obj.GetChild(i), propagate);
                }
            }

            return _graphics;
        }

        public abstract void OnReset();
        public abstract void OnInitialized();
        public virtual void AtProgress(Single value, PlaybackDirection direction)
        {
            onProgress?.Invoke((direction == PlaybackDirection.FORWARD) ? value : 1f - value);
            _onProgress?.Invoke((direction == PlaybackDirection.FORWARD) ? value : 1f - value);
        }
    }

    public class Graphics
    {
        public CanvasGroup canvasGroup;
        public MaskableGraphic uiGraphics;
        public SpriteRenderer spriteRenderer;
        public LineRenderer lineRenderer;

        public float alpha
        {
            set
            {
                if (canvasGroup)
                {
                    canvasGroup.alpha = value;
                }
                else if (uiGraphics)
                {
                    uiGraphics.color = new Color(uiGraphics.color.r, uiGraphics.color.g, uiGraphics.color.b, value);
                }
                else if (spriteRenderer)
                {
                    spriteRenderer.color = new Color(
                        spriteRenderer.color.r,
                        spriteRenderer.color.g,
                        spriteRenderer.color.b,
                        value);
                }
                else if (lineRenderer)
                {
                    lineRenderer.startColor = lineRenderer.endColor = new Color(
                        lineRenderer.startColor.r,
                        lineRenderer.startColor.g,
                        lineRenderer.startColor.b,
                        value);
                }
            }
            get
            {
                if (canvasGroup) return canvasGroup.alpha;
                else if (uiGraphics) return uiGraphics.color.a;
                else if (spriteRenderer) return spriteRenderer.color.a;
                else if (lineRenderer) return lineRenderer.startColor.a;

                return 0f;
            }
        }

        public Color color
        {
            get
            {
                if (uiGraphics) return uiGraphics.color;
                else if (spriteRenderer) return spriteRenderer.color;
                else if (lineRenderer) return lineRenderer.startColor;

                return Color.clear;
            }
        }

        public void SetColor(Color _color, bool changeAlpha)
        {
            if (uiGraphics)
            {
                uiGraphics.color = changeAlpha ? _color :
                    new Color(_color.r, _color.g, _color.b, uiGraphics.color.a);
            }
            else if (spriteRenderer)
            {
                spriteRenderer.color = changeAlpha ? _color :
                    new Color(_color.r, _color.g, _color.b, spriteRenderer.color.a);
            }
            else if (lineRenderer)
            {
                lineRenderer.startColor = lineRenderer.endColor = changeAlpha ? _color :
                    new Color(_color.r, _color.g, _color.b, lineRenderer.startColor.a);
            }
        }
    }

    public enum PlaybackDirection
    {
        FORWARD,
        BACKWARD,
    }

    public enum RepeatType
    {
        NONE,
        ZERO_TO_ONE,
        PING_PONG,
    }

    [System.Serializable]
    public class UnityEventFloat : UnityEvent<float> {}
}