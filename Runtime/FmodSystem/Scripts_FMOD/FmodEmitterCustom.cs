#if FMOD_PRESENT
using System.Collections;
using UnityEngine;

/// <summary>
/// Legacy BetterFMOD emitter component kept for compatibility with existing scenes.
/// </summary>
public class FmodEmitterCustom : MonoBehaviour
{
    /// <summary>
    /// Defines the legacy emitter inspector mode.
    /// </summary>
    public enum EmitterMode
    {
        None,
        Basic,
        Advanced
    }

    /// <summary>
    /// Defines when this legacy emitter plays.
    /// </summary>
    public enum PlayEvent
    {
        None,
        OnEnable,
        OnStart,
        OnMouseEnter
    }

    /// <summary>
    /// Defines when this legacy emitter stops.
    /// </summary>
    public enum StopEvent
    {
        None,
        OnDisable,
        OnDestroy
    }

    public EmitterMode mode;
    public string eventId;
    public bool is3D;
    public bool oneShot;
    public PlayEvent playEvent;
    public StopEvent stopEvent;
    public float radius = 5f;
    public Color gizmoColor = Color.cyan;

    private FmodHandle handle;
    private float appliedRadius = -1f;

    private void OnEnable()
    {
        if (playEvent == PlayEvent.OnEnable)
            StartCoroutine(PlayNextFrame());
    }

    private IEnumerator PlayNextFrame()
    {
        yield return null;
        Play();
    }

    private void Start()
    {
        if (playEvent == PlayEvent.OnStart)
            Play();
    }

    private void OnDisable()
    {
        if (stopEvent == StopEvent.OnDisable)
            Stop();
    }

    private void OnDestroy()
    {
        if (stopEvent == StopEvent.OnDestroy)
            Stop();
    }

    private void OnMouseEnter()
    {
        if (playEvent == PlayEvent.OnMouseEnter)
            Play();
    }

    private void Update()
    {
        if (!is3D || oneShot)
            return;

        ApplyRadiusToPlayingEvent();
    }

    /// <summary>
    /// Plays the configured event.
    /// </summary>
    public void Play()
    {
        if (string.IsNullOrWhiteSpace(eventId))
            return;

        if (oneShot)
        {
            if (is3D)
                Fmod.Play(eventId, transform).Radius(radius);
            else
                Fmod.Play(eventId);

            return;
        }

        handle = is3D
            ? Fmod.PlayLoop(eventId, transform).Radius(radius)
            : Fmod.PlayLoop(eventId);
    }

    /// <summary>
    /// Stops the configured event.
    /// </summary>
    public void Stop(bool fade = true)
    {
        if (handle != null && handle.IsValid)
            handle.Stop(fade);
        else
            Fmod.Stop(eventId, fade);

        appliedRadius = -1f;
    }

    /// <summary>
    /// Pauses or resumes the configured event.
    /// </summary>
    public void Pause(bool pause)
    {
        if (handle != null && handle.IsValid)
        {
            if (pause)
                handle.Pause();
            else
                handle.Resume();

            return;
        }

        if (pause)
            Fmod.Pause(eventId);
        else
            Fmod.Resume(eventId);
    }

    private void ApplyRadiusToPlayingEvent()
    {
        float validRadius = Mathf.Max(0.01f, radius);

        if (Mathf.Approximately(appliedRadius, validRadius))
            return;

        if (handle != null && handle.IsValid)
            handle.Radius(validRadius);
        else
            Fmod.Radius(eventId, validRadius);

        appliedRadius = validRadius;
    }

    private void OnDrawGizmos()
    {
        if (mode != EmitterMode.Advanced || !is3D)
            return;

        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, Mathf.Max(0.01f, radius));
    }

    private void OnValidate()
    {
        radius = Mathf.Max(0.01f, radius);
    }
}
#endif
