using UnityEngine;
using UnityEngine.UI;

namespace RCWorld.SceneManagement
{
    /// <summary>Optional UI view for SceneLoader. Assign only the controls your screen uses.</summary>
    public sealed class LoadingScreen : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Slider progressBar;
        [SerializeField] private bool showRemainingProgress = true;

        public void SetVisible(bool visible)
        {
            if (canvasGroup == null)
            {
                gameObject.SetActive(visible);
                return;
            }

            if (visible)
                canvasGroup.alpha = 1f;
            else
                canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = visible;
            canvasGroup.interactable = visible;
        }

        public void SetProgress(float loadedProgress)
        {
            if (progressBar == null)
                return;

            if (showRemainingProgress)
            {
                progressBar.value = 1f - loadedProgress;
                return;
            }

            progressBar.value = loadedProgress;
        }
    }
}
