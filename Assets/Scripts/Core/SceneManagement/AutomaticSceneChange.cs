using System.Collections;
using UnityEngine;

namespace RCWorld.SceneManagement
{
    /// <summary>Requests a configured scene transition after a delay or when invoked by another system.</summary>
    public sealed class AutomaticSceneChange : MonoBehaviour
    {
        [SerializeField] private SceneLoadRequest sceneRequest;
        [SerializeField] private bool loadOnStart;
        [SerializeField] [Min(0f)] private float delaySeconds;

        private void Start()
        {
            if (loadOnStart)
                StartCoroutine(RequestAfterDelay());
        }

        public void RequestSceneChange()
        {
            if (SceneLoader.Instance == null)
            {
                Debug.LogWarning("No SceneLoader exists. Add one to the bootstrap scene.", this);
                return;
            }

            SceneLoader.Instance.RequestLoad(sceneRequest);
        }

        private IEnumerator RequestAfterDelay()
        {
            if (delaySeconds > 0f)
                yield return new WaitForSeconds(delaySeconds);

            RequestSceneChange();
        }
    }
}
