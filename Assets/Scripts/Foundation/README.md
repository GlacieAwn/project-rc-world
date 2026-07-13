# RC World foundation guide

This folder contains small runtime services for scenes, saves, and FMOD audio. The **Core Services** object in `SampleScene` already contains `SceneLoader` and `SaveService`; both persist between scene changes.

## Before using scene loading

1. Add every target scene to **File > Build Profiles > Scene List**.
2. Keep `SampleScene` (or another bootstrap scene containing **Core Services**) as the first scene opened by the game.
3. Optionally create a Canvas with a `LoadingScreen` component, then assign it to `SceneLoader`'s **Loading Screen** field. The screen should be a child of **Core Services** if it must survive a single-scene load.

`LoadingScreen`'s **Show Remaining Progress** is enabled by default: a slider starts full and empties as loading finishes. Disable it for the conventional empty-to-full display.

## Scene management

### `SceneLoadRequest`

A serializable description of one transition. In the Inspector it appears as a **Scene Request** field; enter a scene name and choose `Single` or `Additive` loading.

| Member | Use |
| --- | --- |
| `new SceneLoadRequest(sceneName, loadMode)` | Creates a request in code. `loadMode` defaults to `Single`. |
| `SceneName` | Returns the configured scene name. |
| `LoadMode` | Returns `Single` or `Additive`. |

```csharp
using RCWorld.SceneManagement;
using UnityEngine.SceneManagement;

SceneLoadRequest raceRequest = new SceneLoadRequest("RaceTrack01", LoadSceneMode.Single);
```

### `SceneLoader`

This is the central scene-loading service. Do not create another one in a scene that can be reached from the bootstrap scene; duplicates destroy themselves.

| Member | Use |
| --- | --- |
| `Instance` | The currently active persistent loader. Check for `null` if a scene can be played directly in the Editor. |
| `IsLoading` | `true` while a requested async load is running. |
| `LoadProgress` | Normalized loaded amount from `0` to `1`. Unity's internal `0–0.9` progress is converted for UI use. |
| `RemainingLoadProgress` | Amount still to load, from `1` to `0`. |
| `LoadProgressChanged` | Event raised whenever normalized progress changes. Receives progress from `0` to `1`. |
| `LoadStarted` | Event raised after a valid request begins. Receives that request. |
| `LoadCompleted` | Event raised after the async operation completes. Receives that request. |
| `RequestLoad(request)` | Begins loading. Returns the running `Coroutine`, or `null` when the request is invalid or a load is already active. |

`Awake`, `LoadRoutine`, `FinishLoad`, and `SetProgress` are internal lifecycle/implementation functions. Unity calls `Awake`; gameplay code should use `RequestLoad` instead.

```csharp
using RCWorld.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class RaceStartButton : MonoBehaviour
{
    public void StartRace()
    {
        if (SceneLoader.Instance == null)
        {
            Debug.LogWarning("SceneLoader is missing.");
            return;
        }

        SceneLoadRequest request = new SceneLoadRequest("RaceTrack01", LoadSceneMode.Single);
        SceneLoader.Instance.RequestLoad(request);
    }
}
```

To observe loading without owning the loading screen:

```csharp
using RCWorld.SceneManagement;
using UnityEngine;

public sealed class LoadProgressLogger : MonoBehaviour
{
    private void OnEnable()
    {
        if (SceneLoader.Instance != null)
            SceneLoader.Instance.LoadProgressChanged += LogProgress;
    }

    private void OnDisable()
    {
        if (SceneLoader.Instance != null)
            SceneLoader.Instance.LoadProgressChanged -= LogProgress;
    }

    private void LogProgress(float progress)
    {
        Debug.Log($"Load progress: {progress:P0}");
    }
}
```

### `AutomaticSceneChange`

Attach this component to any scene object to make a configured request without writing a new script.

| Member | Use |
| --- | --- |
| `Start()` | Unity lifecycle function. When **Load On Start** is enabled, begins the delay. Do not call it yourself. |
| `RequestSceneChange()` | Public method that sends the Inspector-configured **Scene Request** to `SceneLoader`. It can be called from a UI Button, Timeline signal, animation event, or script. |
| `RequestAfterDelay()` | Internal coroutine called by `Start`; waits for **Delay Seconds**, then calls `RequestSceneChange`. |

For a finish-line trigger, add `AutomaticSceneChange`, set its scene request to `Results`, leave **Load On Start** disabled, and call `RequestSceneChange()` from your trigger script or a UnityEvent.

### `LoadingScreen`

An optional view component. Assign a `CanvasGroup` and/or a UI `Slider`; either field may be left empty.

| Member | Use |
| --- | --- |
| `SetVisible(visible)` | Shows or hides the screen. With a CanvasGroup it changes alpha and input blocking; without one it enables/disables the GameObject. Usually called by `SceneLoader`. |
| `SetProgress(loadedProgress)` | Updates the slider using loaded progress from `0` to `1`. Applies **Show Remaining Progress** when enabled. Usually called by `SceneLoader`. |

```csharp
// Only useful for a custom loading-screen flow. SceneLoader calls both methods itself.
loadingScreen.SetVisible(true);
loadingScreen.SetProgress(0.5f);
```

## Saving

The save service stores plain data classes as individual JSON files in `Application.persistentDataPath/Saves`. It does not serialize GameObjects, MonoBehaviours, or gameplay systems. Use public fields with `JsonUtility`.

### `ISaveData` and `ISaveService`

| Member | Use |
| --- | --- |
| `ISaveData` | Marker interface implemented by every saved data class. |
| `ISaveService` | Interface for code that needs saving without depending on the `SaveService` MonoBehaviour. |
| `Save<T>(key, data)` | Serializes `data` to `<key>.json`; overwrites an existing file. Throws if data or key is invalid. |
| `TryLoad<T>(key, out data)` | Reads a matching file. Returns `false` and gives `null` data when no usable save exists. |
| `LoadOrCreate<T>(key)` | Reads the file, or returns a new `T` when none exists. Requires a parameterless constructor. |
| `Delete(key)` | Removes `<key>.json`. Returns `true` only when a file existed. |

### `SaveService`

| Member | Use |
| --- | --- |
| `Instance` | The persistent save-service instance. |
| `SaveDirectory` | Full platform-specific directory currently used for saves. |
| `Awake()` | Unity lifecycle function: establishes the singleton and creates the save directory. Do not call it yourself. |
| `GetFilePath(key)` | Private helper that validates a key and creates its file path. |

Example data type and usage:

```csharp
using System;
using RCWorld.Saving;
using UnityEngine;

[Serializable]
public sealed class PlayerSettingsData : ISaveData
{
    public float musicVolume = 1f;
    public bool subtitlesEnabled = true;
}

public sealed class SettingsExample : MonoBehaviour
{
    private const string SettingsKey = "player-settings";

    public void SaveSettings()
    {
        PlayerSettingsData data = new PlayerSettingsData();
        data.musicVolume = 0.75f;
        data.subtitlesEnabled = true;
        SaveService.Instance.Save(SettingsKey, data);
    }

    public void LoadSettings()
    {
        PlayerSettingsData data = SaveService.Instance.LoadOrCreate<PlayerSettingsData>(SettingsKey);
        Debug.Log($"Music volume: {data.musicVolume}");
    }
}
```

For a new save category, create another `[Serializable]` class implementing `ISaveData` and give it its own key. `SaveService` needs no changes.

## FMOD audio

Import the official FMOD for Unity integration first. It defines `FMOD_PRESENT`, which enables these components. Assign all `EventReference` values in the Inspector; the code contains no event or bank paths and does not set up banks.

### `FmodOneShotPlayer`

Attach it to an object, assign **Event Reference**, then call `Play()` from a Unity Button or your code. The event plays at the GameObject's current position. Empty event references are ignored.

| Member | Use |
| --- | --- |
| `Play()` | Starts the assigned event once at `transform.position`. |

```csharp
// Drag a FmodOneShotPlayer component into this field in the Inspector.
[SerializeField] private RCWorld.Audio.FmodOneShotPlayer boostSound;

public void UseBoost()
{
    boostSound.Play();
}
```

### `FmodMusicPlayer`

Attach it to a persistent music object and populate **Music Events** with EventReferences from your FMOD `Music/` directory. **Shuffle** chooses a random assigned event and avoids repeating the current track when possible. **Play On Start** starts a track automatically.

| Member | Use |
| --- | --- |
| `Start()` | Unity lifecycle function; calls `PlayNext` when **Play On Start** is enabled. |
| `OnDestroy()` | Unity lifecycle function; stops and releases the current FMOD instance. |
| `PlayNext()` | Starts the next sequential track or a shuffled track, depending on **Shuffle**. |
| `PlayCurrentSongLooped()` | Restarts the selected current event. If none has been selected, starts one. The event's own FMOD loop region controls the loop timing. |
| `StopCurrent()` | Fades out, releases, and clears the current event instance. Safe to call when nothing is playing. |
| `PlayIndex(index)` | Private helper that starts one configured event. |
| `TryGetNextIndex(out index)` | Private helper that finds a valid assigned music event, returning `false` if none exist. |

```csharp
using RCWorld.Audio;
using UnityEngine;

public sealed class MusicControls : MonoBehaviour
{
    [SerializeField] private FmodMusicPlayer musicPlayer;

    public void NextTrack()
    {
        musicPlayer.PlayNext();
    }

    public void LoopCurrentTrack()
    {
        musicPlayer.PlayCurrentSongLooped();
    }

    public void StopMusic()
    {
        musicPlayer.StopCurrent();
    }
}
```

For looping, create the loop region inside the FMOD event. `FmodMusicPlayer` starts a single instance and never manually restarts it while it is playing, preserving the authored loop points.
