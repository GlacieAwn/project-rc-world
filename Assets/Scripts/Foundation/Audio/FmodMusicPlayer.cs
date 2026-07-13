#if FMOD_PRESENT
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

namespace RCWorld.Audio
{
    /// <summary>
    /// Plays Inspector-assigned events from the FMOD Music directory. No event paths or bank
    /// names are stored in code. For looped playback, author a loop region in the FMOD event;
    /// this class deliberately lets FMOD handle that region instead of restarting audio itself.
    /// </summary>
    public sealed class FmodMusicPlayer : MonoBehaviour
    {
        [Tooltip("Assign music EventReferences from the Music directory in your FMOD banks.")]
        [SerializeField] private EventReference[] musicEvents;
        [SerializeField] private bool shuffle = true;
        [SerializeField] private bool playOnStart = true;

        private EventInstance currentInstance;
        private int currentIndex = -1;

        private void Start()
        {
            if (playOnStart)
                PlayNext();
        }

        private void OnDestroy()
        {
            StopCurrent();
        }

        public void PlayNext()
        {
            if (!TryGetNextIndex(out int nextIndex))
                return;

            PlayIndex(nextIndex);
        }

        public void PlayCurrentSongLooped()
        {
            if (currentIndex < 0)
            {
                PlayNext();
                return;
            }

            PlayIndex(currentIndex);
        }

        public void StopCurrent()
        {
            if (!currentInstance.isValid())
                return;

            currentInstance.stop(STOP_MODE.ALLOWFADEOUT);
            currentInstance.release();
            currentInstance.clearHandle();
        }

        private void PlayIndex(int index)
        {
            StopCurrent();
            currentIndex = index;
            currentInstance = RuntimeManager.CreateInstance(musicEvents[index]);
            currentInstance.start();

            // Loop regions are authored in FMOD Studio. No code-level restart is used, which
            // preserves the event's authored loop points. Non-looping events simply finish.
        }

        private bool TryGetNextIndex(out int index)
        {
            index = -1;
            if (musicEvents == null)
            {
                Debug.LogWarning("Assign at least one music EventReference.", this);
                return false;
            }

            List<int> validIndices = new List<int>();
            for (int i = 0; i < musicEvents.Length; i++)
            {
                if (!musicEvents[i].IsNull)
                    validIndices.Add(i);
            }

            if (validIndices.Count == 0)
            {
                Debug.LogWarning("Assign at least one music EventReference.", this);
                return false;
            }

            if (!shuffle)
            {
                index = validIndices[(validIndices.IndexOf(currentIndex) + 1 + validIndices.Count) % validIndices.Count];
                return true;
            }

            index = validIndices[Random.Range(0, validIndices.Count)];
            if (validIndices.Count > 1 && index == currentIndex)
                index = validIndices[(validIndices.IndexOf(index) + 1) % validIndices.Count];
            return true;
        }
    }
}
#endif
