#if BISC8_BETTERFMOD_PRESENT
using FMOD.Studio;

/// <summary>
/// Representa um estado de playback BetterFMOD sem expor tipos do FMOD Studio na API publica.
/// </summary>
public enum FmodPlaybackState
{
    Stopped,
    Starting,
    Playing,
    Sustaining,
    Stopping
}

internal static class FmodStateUtility
{
    internal static FmodPlaybackState ToBetterState(PLAYBACK_STATE state)
    {
        return state switch
        {
            PLAYBACK_STATE.STARTING => FmodPlaybackState.Starting,
            PLAYBACK_STATE.PLAYING => FmodPlaybackState.Playing,
            PLAYBACK_STATE.SUSTAINING => FmodPlaybackState.Sustaining,
            PLAYBACK_STATE.STOPPING => FmodPlaybackState.Stopping,
            _ => FmodPlaybackState.Stopped
        };
    }
}
#endif
