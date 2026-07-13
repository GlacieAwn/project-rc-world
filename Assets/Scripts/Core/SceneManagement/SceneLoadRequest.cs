using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RCWorld.SceneManagement
{
    // A small, inspector-friendly description of a scene transition.
    // The scene name must be included in Build Settings.
    [Serializable]
    public sealed class SceneLoadRequest
    {
        [Tooltip("Scene name or path, as configured in Build Settings.")]
        [SerializeField] private string sceneName;
        [SerializeField] private LoadSceneMode loadMode = LoadSceneMode.Single;

        public string SceneName => sceneName;
        public LoadSceneMode LoadMode => loadMode;

        public SceneLoadRequest(string sceneName, LoadSceneMode loadMode = LoadSceneMode.Single)
        {
            this.sceneName = sceneName;
            this.loadMode = loadMode;
        }
    }
}
