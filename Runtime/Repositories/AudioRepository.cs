using System;
using System.Collections.Generic;
using UnityEngine;

namespace HootyBird.Minimal.Repositories
{
    [CreateAssetMenu(fileName = "AudioRepository", menuName = "Repositories/Create AudioData Repository")]
    public class AudioRepository : ScriptableObject
    {
        public List<SerializedAudioData> data;

        public SerializedAudioData GetByName(string name)
        {
            return data.Find(x => x.id == name);
        }
    }

    [Serializable]
    public class SerializedAudioData
    {
        public string id;
        [Range(0f, 1f)]
        public float repeatThreshold = .2f;
        public List<AudioClip> clips;

        public AudioClip GetClip(int clipIndex)
        {
            if (clipIndex == -1)
            {
                return clips.Count == 0 ? null : clips[UnityEngine.Random.Range(0, clips.Count)];
            }

            return clips[clipIndex];
        }
    }
}