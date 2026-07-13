using System;
using System.Collections;
using UnityEngine;

namespace RCWorld.SceneManagement
{
    public interface ISceneLoader
    {
        bool IsLoading { get; }
        float LoadProgress { get; }
        float RemainingLoadProgress { get; }

        event Action<float> LoadProgressChanged;
        event Action<SceneLoadRequest> LoadStarted;
        event Action<SceneLoadRequest> LoadCompleted;

        Coroutine RequestLoad(SceneLoadRequest request);
    }
}
