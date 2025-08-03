using HootyBird.Minimal.Repositories;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

namespace HootyBird.Minimal.Services
{
    /// <summary>
    /// Service for managing simple audio effects.
    /// </summary>
    public class AudioService : MonoBehaviour
    {
        public static AudioService Instance;

        [SerializeField]
        private AudioRepository audioRepository;
        [SerializeField]
        private AudioMixer mixer;
        [SerializeField]
        private AudioSource audioSource;

        private Dictionary<string, CachedAudioData> currentlyPlayed = new Dictionary<string, CachedAudioData>();
        private Queue<string> toRemove = new Queue<string>();

        protected void Awake()
        {
            if (Instance != null)
            {
                DestroyImmediate(gameObject);
                return;
            }

            Instance = this;
            audioSource = GetComponent<AudioSource>();
        }

        protected void Update()
        {
            if (currentlyPlayed.Count > 0)
            {
                foreach (string clip in currentlyPlayed.Keys.ToList())
                {
                    if (currentlyPlayed[clip].durationLeft - Time.deltaTime > 0f)
                    {
                        currentlyPlayed[clip].durationLeft -= Time.deltaTime;
                    }
                    else
                    {
                        toRemove.Enqueue(clip);
                    }
                }
            }

            while (toRemove.Count > 0)
            {
                currentlyPlayed.Remove(toRemove.Dequeue());
            }
        }

        public static void SetMasterVolume(float value) =>
            Instance?.mixer.SetFloat("MasterVolume", Mathf.Clamp01(1f - value) * -80f);

        public void PlaySfx(string name, float volume = 1f, int clipIndex = -1) =>
            PlaySfx(name, audioSource, volume, clipIndex);

        public void PlaySfx(string name, AudioSource source, float volume = 1f, int clipIndex = -1)
        {
            if (!source)
            {
                Debug.LogWarning("AudioService have no audio source attached.");
                return;
            }

            SerializedAudioData data = audioRepository.GetByName(name);

            if (data == null)
            {
                Debug.LogWarning($"No audio data with name: {name}");
                return;
            }

            AudioClip clipToPlay = data.GetClip(clipIndex);

            if (clipToPlay == null)
            {
                return;
            }

            bool isCurrentlyPlaying = currentlyPlayed.ContainsKey(name);
            bool playClip = !isCurrentlyPlaying || (isCurrentlyPlaying && currentlyPlayed[name].progress > data.repeatThreshold);
            if (playClip)
            {
                if (isCurrentlyPlaying)
                {
                    currentlyPlayed[name].ResetDuration();
                }
                else
                {
                    currentlyPlayed.Add(
                        name,
                        new CachedAudioData() { duration = clipToPlay.length, durationLeft = clipToPlay.length });
                }

                source.PlayOneShot(clipToPlay, volume);
            }
        }

        private class CachedAudioData
        {
            public float duration;
            public float durationLeft;

            public float progress => (duration - durationLeft) / duration;

            public void ResetDuration()
            {
                durationLeft = duration;
            }
        }
    }
}