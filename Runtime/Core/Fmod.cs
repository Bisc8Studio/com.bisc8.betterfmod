#if FMOD_PRESENT
using UnityEngine;

/// <summary>
/// Public BetterFMOD API for playback, parameters, volume, pitch, 3D audio, buses, VCAs, and snapshots.
/// </summary>
public static class Fmod
{
    /// <summary>
    /// Plays an FMOD event and returns a handle for the created instance.
    /// </summary>
    public static FmodHandle Play(string id)
    {
        return FmodCommands.EnsureInstance().Play(id);
    }

    /// <summary>
    /// Plays an FMOD event attached to a transform and returns a handle for the created instance.
    /// </summary>
    public static FmodHandle Play(string id, Transform target)
    {
        return FmodCommands.EnsureInstance().Play(id, target);
    }

    /// <summary>
    /// Plays a loop-style FMOD event and returns a handle for the created instance.
    /// </summary>
    public static FmodHandle PlayLoop(string id)
    {
        return FmodCommands.EnsureInstance().PlayLoop(id);
    }

    /// <summary>
    /// Plays a loop-style FMOD event attached to a transform and returns a handle for the created instance.
    /// </summary>
    public static FmodHandle PlayLoop(string id, Transform target)
    {
        return FmodCommands.EnsureInstance().PlayLoop(id, target);
    }

    /// <summary>
    /// Stops every active instance of an event.
    /// </summary>
    public static void Stop(string id)
    {
        FmodCommands.EnsureInstance().Stop(id);
    }

    /// <summary>
    /// Stops every active instance of an event, optionally fading first.
    /// </summary>
    public static void Stop(string id, bool fade, float fadeTime = 1f)
    {
        FmodCommands.EnsureInstance().Stop(id, fade, fadeTime);
    }

    /// <summary>
    /// Stops every active BetterFMOD instance.
    /// </summary>
    public static void StopAll()
    {
        FmodCommands.EnsureInstance().StopAll();
    }

    /// <summary>
    /// Stops every active BetterFMOD instance, optionally fading first.
    /// </summary>
    public static void StopAll(bool fade, float fadeTime = 1f)
    {
        FmodCommands.EnsureInstance().StopAll(fade, fadeTime);
    }

    /// <summary>
    /// Pauses every active instance of an event.
    /// </summary>
    public static void Pause(string id)
    {
        FmodCommands.EnsureInstance().Pause(id, true);
    }

    /// <summary>
    /// Resumes every active instance of an event.
    /// </summary>
    public static void Resume(string id)
    {
        FmodCommands.EnsureInstance().Resume(id);
    }

    /// <summary>
    /// Toggles pause on every active instance of an event.
    /// </summary>
    public static void TogglePause(string id)
    {
        FmodCommands.EnsureInstance().TogglePause(id);
    }

    /// <summary>
    /// Sets a parameter on every active instance of an event.
    /// </summary>
    public static void SetParameter(string id, string parameter, float value)
    {
        FmodCommands.EnsureInstance().SetParameter(id, parameter, value);
    }

    /// <summary>
    /// Gets a parameter from the newest active instance of an event.
    /// </summary>
    public static float GetParameter(string id, string parameter)
    {
        return FmodCommands.EnsureInstance().GetParameter(id, parameter);
    }

    /// <summary>
    /// Sets an FMOD global parameter.
    /// </summary>
    public static void SetGlobalParameter(string parameter, float value)
    {
        FmodCommands.EnsureInstance().SetGlobalParameter(parameter, value);
    }

    /// <summary>
    /// Gets an FMOD global parameter.
    /// </summary>
    public static float GetGlobalParameter(string parameter)
    {
        return FmodCommands.EnsureInstance().GetGlobalParameter(parameter);
    }

    /// <summary>
    /// Sets a labeled parameter on every active instance of an event.
    /// </summary>
    public static void SetParameterLabel(string id, string parameter, string label)
    {
        FmodCommands.EnsureInstance().SetParameterLabel(id, parameter, label);
    }

    /// <summary>
    /// Sets volume on every active instance of an event.
    /// </summary>
    public static void SetVolume(string id, float volume)
    {
        FmodCommands.EnsureInstance().SetVolume(id, volume);
    }

    /// <summary>
    /// Gets volume from the newest active instance of an event.
    /// </summary>
    public static float GetVolume(string id)
    {
        return FmodCommands.EnsureInstance().GetVolume(id);
    }

    /// <summary>
    /// Fades in every active instance of an event.
    /// </summary>
    public static void FadeIn(string id, float duration)
    {
        FmodCommands.EnsureInstance().FadeIn(id, duration);
    }

    /// <summary>
    /// Fades out and stops every active instance of an event.
    /// </summary>
    public static void FadeOut(string id, float duration)
    {
        FmodCommands.EnsureInstance().FadeOut(id, duration);
    }

    /// <summary>
    /// Fades every active instance of an event to a target volume.
    /// </summary>
    public static void FadeTo(string id, float volume, float duration)
    {
        FmodCommands.EnsureInstance().FadeTo(id, volume, duration);
    }

    /// <summary>
    /// Sets pitch on every active instance of an event.
    /// </summary>
    public static void SetPitch(string id, float pitch)
    {
        FmodCommands.EnsureInstance().SetPitch(id, pitch);
    }

    /// <summary>
    /// Gets pitch from the newest active instance of an event.
    /// </summary>
    public static float GetPitch(string id)
    {
        return FmodCommands.EnsureInstance().GetPitch(id);
    }

    /// <summary>
    /// Returns true when any active instance of an event is playing.
    /// </summary>
    public static bool IsPlaying(string id)
    {
        return FmodCommands.EnsureInstance().IsPlaying(id);
    }

    /// <summary>
    /// Returns true when any active instance of an event is paused.
    /// </summary>
    public static bool IsPaused(string id)
    {
        return FmodCommands.EnsureInstance().IsPaused(id);
    }

    /// <summary>
    /// Returns true when at least one active instance exists for an event.
    /// </summary>
    public static bool Exists(string id)
    {
        return FmodCommands.EnsureInstance().Exists(id);
    }

    /// <summary>
    /// Gets playback state from the newest active instance of an event.
    /// </summary>
    public static FmodPlaybackState GetState(string id)
    {
        return FmodCommands.EnsureInstance().GetBetterState(id);
    }

    /// <summary>
    /// Gets timeline position in milliseconds from the newest active instance of an event.
    /// </summary>
    public static int GetTimelinePosition(string id)
    {
        return FmodCommands.EnsureInstance().GetTimelinePosition(id);
    }

    /// <summary>
    /// Sets timeline position in milliseconds on every active instance of an event.
    /// </summary>
    public static void SetTimelinePosition(string id, int milliseconds)
    {
        FmodCommands.EnsureInstance().SetTimelinePosition(id, milliseconds);
    }

    /// <summary>
    /// Makes every active instance of an event follow a transform.
    /// </summary>
    public static void Follow(string id, Transform target)
    {
        FmodCommands.EnsureInstance().Follow(id, target);
    }

    /// <summary>
    /// Detaches every active instance of an event from a transform.
    /// </summary>
    public static void Detach(string id)
    {
        FmodCommands.EnsureInstance().Detach(id);
    }

    /// <summary>
    /// Sets world position on every active instance of an event.
    /// </summary>
    public static void SetPosition(string id, Vector3 position)
    {
        FmodCommands.EnsureInstance().SetPosition(id, position);
    }

    /// <summary>
    /// Sets velocity on every active instance of an event.
    /// </summary>
    public static void SetVelocity(string id, Vector3 velocity)
    {
        FmodCommands.EnsureInstance().SetVelocity(id, velocity);
    }

    /// <summary>
    /// Sets 3D maximum radius on every active instance of an event.
    /// </summary>
    public static void Radius(string id, float radius)
    {
        FmodCommands.EnsureInstance().Set3DRange(id, radius);
    }

    /// <summary>
    /// Sets an FMOD bus volume.
    /// </summary>
    public static void SetBusVolume(string path, float volume)
    {
        FmodCommands.EnsureInstance().BusManager.SetBusVolume(path, volume);
    }

    /// <summary>
    /// Gets an FMOD bus volume.
    /// </summary>
    public static float GetBusVolume(string path)
    {
        return FmodCommands.EnsureInstance().BusManager.GetBusVolume(path);
    }

    /// <summary>
    /// Pauses or resumes an FMOD bus.
    /// </summary>
    public static void SetBusPaused(string path, bool paused)
    {
        FmodCommands.EnsureInstance().BusManager.SetBusPaused(path, paused);
    }

    /// <summary>
    /// Stops all events routed through an FMOD bus.
    /// </summary>
    public static void StopBus(string path)
    {
        FmodCommands.EnsureInstance().BusManager.StopBus(path);
    }

    /// <summary>
    /// Sets an FMOD VCA volume.
    /// </summary>
    public static void SetVcaVolume(string path, float volume)
    {
        FmodCommands.EnsureInstance().BusManager.SetVcaVolume(path, volume);
    }

    /// <summary>
    /// Sets an FMOD VCA volume.
    /// </summary>
    public static void SetVCAVolume(string path, float volume)
    {
        SetVcaVolume(path, volume);
    }

    /// <summary>
    /// Gets an FMOD VCA volume.
    /// </summary>
    public static float GetVcaVolume(string path)
    {
        return FmodCommands.EnsureInstance().BusManager.GetVcaVolume(path);
    }

    /// <summary>
    /// Gets an FMOD VCA volume.
    /// </summary>
    public static float GetVCAVolume(string path)
    {
        return GetVcaVolume(path);
    }

    /// <summary>
    /// Starts an FMOD snapshot and returns a handle for the created snapshot instance.
    /// </summary>
    public static FmodHandle StartSnapshot(string path)
    {
        return FmodCommands.EnsureInstance().SnapshotManager.StartSnapshot(path);
    }

    /// <summary>
    /// Stops every active instance of an FMOD snapshot.
    /// </summary>
    public static void StopSnapshot(string path)
    {
        FmodCommands.EnsureInstance().SnapshotManager.StopSnapshot(path);
    }
}
#endif
