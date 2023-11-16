//@vadym udod

using UnityEngine;

namespace HootyBird.Minimal.Tween
{
    public class VolumeTween : TweenBase
    {
        [HideInInspector]
        public AudioSource audioSource;

        public float from = 0f;
        public float to = 1f;

        public float _value { get; private set; }

        public override void AtProgress(float value, PlaybackDirection direction)
        {
            float currentValue;
            switch (direction)
            {
                case PlaybackDirection.FORWARD:
                    if (value < 1f)
                    {
                        currentValue = Mathf.Lerp(from, to, curve.Evaluate(value));
                    }
                    else
                    {
                        currentValue = Mathf.Lerp(from, to, curve.Evaluate(1f));
                        isPlaying = false;
                    }
                    break;

                default:
                    if (value < 1f)
                    {
                        currentValue = Mathf.Lerp(to, from, curve.Evaluate(value));
                    }
                    else
                    {
                        currentValue = Mathf.Lerp(to, from, curve.Evaluate(1f));
                        isPlaying = false;
                    }
                    break;
            }

            SetVolume(currentValue);
        }

        public void SetVolume(float value)
        {
            if (!audioSource)
            {
                if (isPlaying)
                {
                    isPlaying = false;
                }

                Debug.LogWarning($"AudioSource is missing {gameObject.name}");
            }

            audioSource.volume = _value = value;
        }

        public override void OnReset()
        {
            SetVolume(from);
        }

        public override void OnInitialized()
        {
            audioSource = GetComponent<AudioSource>();
        }
    }
}