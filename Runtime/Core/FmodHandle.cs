#if FMOD_PRESENT
using UnityEngine;

/// <summary>
/// Controls a single BetterFMOD event instance.
/// </summary>
public sealed class FmodHandle
{
    private readonly FmodCommands commands;

    internal FmodHandle(FmodCommands commands, int id, string eventId)
    {
        this.commands = commands;
        Id = id;
        EventId = eventId;
    }

    /// <summary>
    /// Gets an invalid handle for a failed or missing event.
    /// </summary>
    public static FmodHandle Invalid(string eventId = "")
    {
        return new FmodHandle(null, 0, eventId);
    }

    /// <summary>
    /// Gets the unique runtime id for this instance.
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// Gets the BetterFMOD event id or FMOD path used to create this instance.
    /// </summary>
    public string EventId { get; }

    /// <summary>
    /// Gets whether this handle still points to a live FMOD instance.
    /// </summary>
    public bool IsValid => commands != null && commands.TryGetInstance(Id, out _);

    /// <summary>
    /// Stops this event instance immediately.
    /// </summary>
    public FmodHandle Stop()
    {
        commands?.Stop(Id, false, 0f);
        return this;
    }

    /// <summary>
    /// Stops this event instance, optionally fading it out first.
    /// </summary>
    public FmodHandle Stop(bool fade, float fadeTime = 1f)
    {
        commands?.Stop(Id, fade, fadeTime);
        return this;
    }

    /// <summary>
    /// Pauses this event instance.
    /// </summary>
    public FmodHandle Pause()
    {
        commands?.Pause(Id);
        return this;
    }

    /// <summary>
    /// Resumes this event instance.
    /// </summary>
    public FmodHandle Resume()
    {
        commands?.Resume(Id);
        return this;
    }

    /// <summary>
    /// Toggles pause on this event instance.
    /// </summary>
    public FmodHandle TogglePause()
    {
        commands?.TogglePause(Id);
        return this;
    }

    /// <summary>
    /// Sets a parameter on this event instance.
    /// </summary>
    public FmodHandle Parameter(string parameter, float value)
    {
        commands?.SetParameter(Id, parameter, value);
        return this;
    }

    /// <summary>
    /// Sets a parameter on this event instance.
    /// </summary>
    public FmodHandle SetParameter(string parameter, float value)
    {
        return Parameter(parameter, value);
    }

    /// <summary>
    /// Gets a parameter from this event instance.
    /// </summary>
    public float GetParameter(string parameter)
    {
        return commands == null ? 0f : commands.GetParameter(Id, parameter);
    }

    /// <summary>
    /// Sets a labeled parameter on this event instance.
    /// </summary>
    public FmodHandle SetParameterLabel(string parameter, string label)
    {
        commands?.SetParameterLabel(Id, parameter, label);
        return this;
    }

    /// <summary>
    /// Sets the volume on this event instance.
    /// </summary>
    public FmodHandle Volume(float volume)
    {
        commands?.SetVolume(Id, volume);
        return this;
    }

    /// <summary>
    /// Sets the volume on this event instance.
    /// </summary>
    public FmodHandle SetVolume(float volume)
    {
        return Volume(volume);
    }

    /// <summary>
    /// Gets the volume from this event instance.
    /// </summary>
    public float GetVolume()
    {
        return commands == null ? 0f : commands.GetVolume(Id);
    }

    /// <summary>
    /// Fades this event instance in from silence.
    /// </summary>
    public FmodHandle FadeIn(float duration)
    {
        commands?.FadeIn(Id, duration);
        return this;
    }

    /// <summary>
    /// Fades this event instance out and stops it.
    /// </summary>
    public FmodHandle FadeOut(float duration)
    {
        commands?.Stop(Id, true, duration);
        return this;
    }

    /// <summary>
    /// Fades this event instance to a target volume.
    /// </summary>
    public FmodHandle FadeTo(float volume, float duration)
    {
        commands?.FadeTo(Id, volume, duration);
        return this;
    }

    /// <summary>
    /// Sets the pitch on this event instance.
    /// </summary>
    public FmodHandle Pitch(float pitch)
    {
        commands?.SetPitch(Id, pitch);
        return this;
    }

    /// <summary>
    /// Sets the pitch on this event instance.
    /// </summary>
    public FmodHandle SetPitch(float pitch)
    {
        return Pitch(pitch);
    }

    /// <summary>
    /// Gets the pitch from this event instance.
    /// </summary>
    public float GetPitch()
    {
        return commands == null ? 0f : commands.GetPitch(Id);
    }

    /// <summary>
    /// Gets whether this event instance is currently playing.
    /// </summary>
    public bool IsPlaying()
    {
        return commands != null && commands.IsPlaying(Id);
    }

    /// <summary>
    /// Gets whether this event instance is paused.
    /// </summary>
    public bool IsPaused()
    {
        return commands != null && commands.IsPaused(Id);
    }

    /// <summary>
    /// Gets the playback state for this event instance.
    /// </summary>
    public FmodPlaybackState GetState()
    {
        return commands == null ? FmodPlaybackState.Stopped : commands.GetBetterState(Id);
    }

    /// <summary>
    /// Gets the timeline position in milliseconds for this event instance.
    /// </summary>
    public int GetTimelinePosition()
    {
        return commands == null ? 0 : commands.GetTimelinePosition(Id);
    }

    /// <summary>
    /// Sets the timeline position in milliseconds for this event instance.
    /// </summary>
    public FmodHandle SetTimelinePosition(int milliseconds)
    {
        commands?.SetTimelinePosition(Id, milliseconds);
        return this;
    }

    /// <summary>
    /// Makes this event instance follow a transform.
    /// </summary>
    public FmodHandle Follow(Transform target)
    {
        commands?.Follow(Id, target);
        return this;
    }

    /// <summary>
    /// Detaches this event instance from its follow target.
    /// </summary>
    public FmodHandle Detach()
    {
        commands?.Detach(Id);
        return this;
    }

    /// <summary>
    /// Sets the 3D world position for this event instance.
    /// </summary>
    public FmodHandle SetPosition(Vector3 position)
    {
        commands?.SetPosition(Id, position);
        return this;
    }

    /// <summary>
    /// Sets the 3D world position for this event instance.
    /// </summary>
    public FmodHandle Position(Vector3 position)
    {
        return SetPosition(position);
    }

    /// <summary>
    /// Sets the 3D velocity for this event instance.
    /// </summary>
    public FmodHandle SetVelocity(Vector3 velocity)
    {
        commands?.SetVelocity(Id, velocity);
        return this;
    }

    /// <summary>
    /// Sets the 3D velocity for this event instance.
    /// </summary>
    public FmodHandle Velocity(Vector3 velocity)
    {
        return SetVelocity(velocity);
    }

    /// <summary>
    /// Sets the 3D maximum radius for this event instance.
    /// </summary>
    public FmodHandle Radius(float radius)
    {
        commands?.Set3DRange(Id, radius);
        return this;
    }
}
#endif
