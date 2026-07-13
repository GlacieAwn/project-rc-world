#if FMOD_PRESENT
using FMODUnity;
using UnityEngine;

namespace RCWorld.Audio
{
    /// <summary>Inspector-configured FMOD one-shot player for quick gameplay testing.</summary>
    public sealed class FmodOneShotPlayer : MonoBehaviour
    {
        [SerializeField] private EventReference eventReference;

        public void Play()
        {
            if (eventReference.IsNull)
                return;

            RuntimeManager.PlayOneShot(eventReference, transform.position);
        }
    }
}
#endif
