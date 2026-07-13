# FMOD integration

Import the official FMOD for Unity package into this project. The package defines `FMOD_PRESENT`, which activates `FmodOneShotPlayer` and `FmodMusicPlayer`.

Do not add bank paths to these scripts. In the Inspector, assign event references directly:

- `FmodOneShotPlayer`: assign any event to test one-shots.
- `FmodMusicPlayer`: assign music events from the `Music/` directory in your FMOD project. `Shuffle` chooses from this list. Call `PlayCurrentSongLooped` to restart the selected event and use its authored loop region. The player never manually restarts a playing event, so FMOD preserves the authored loop points.
