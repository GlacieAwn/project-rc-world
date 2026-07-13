using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RCWorld.SceneManagement
{
    // Persistent entry point for requested scene loads. Place one in the bootstrap scene.
    public sealed class SceneLoader : MonoBehaviour, ISceneLoader
    {
        public static SceneLoader Instance { get; private set; }

        [SerializeField] private LoadingScreen loadingScreen;

        public bool IsLoading { get; private set; }
        public float LoadProgress { get; private set; }
        public float RemainingLoadProgress => 1f - LoadProgress;

        public event Action<float> LoadProgressChanged;
        public event Action<SceneLoadRequest> LoadStarted;
        public event Action<SceneLoadRequest> LoadCompleted;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            if (loadingScreen != null)
                loadingScreen.SetVisible(false);
        }

        public Coroutine RequestLoad(SceneLoadRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.SceneName))
            {
                Debug.LogWarning("A scene load was requested without a scene name.", this);
                return null;
            }

            if (IsLoading)
            {
                Debug.LogWarning("A scene load is already in progress.", this);
                return null;
            }

            return StartCoroutine(LoadRoutine(request));
        }

        private IEnumerator LoadRoutine(SceneLoadRequest request)
        {
            IsLoading = true;
            SetProgress(0f);
            loadingScreen?.SetVisible(true);
            LoadStarted?.Invoke(request);

            AsyncOperation operation = SceneManager.LoadSceneAsync(request.SceneName, request.LoadMode);
            if (operation == null)
            {
                FinishLoad(request);
                yield break;
            }

            while (!operation.isDone)
            {
                // Unity reports 0.9 until activation is allowed. Normalize this for UI.
                SetProgress(Mathf.Clamp01(operation.progress / 0.9f));
                yield return null;
            }

            SetProgress(1f);
            FinishLoad(request);
        }

        private void FinishLoad(SceneLoadRequest request)
        {
            loadingScreen?.SetVisible(false);
            IsLoading = false;
            LoadCompleted?.Invoke(request);
        }

        private void SetProgress(float value)
        {
            LoadProgress = value;
            loadingScreen?.SetProgress(value);
            LoadProgressChanged?.Invoke(value);
        }
    }
}
