#if FMOD_PRESENT
using UnityEngine;

/// <summary>
/// Plays and controls a BetterFMOD event from a GameObject.
/// </summary>
public class FmodEmitter : MonoBehaviour
{
    [SerializeField] private string eventId;
    [SerializeField] private bool loop;
    [SerializeField] private bool oneShot = true;
    [SerializeField] private bool playOnAwake;
    [SerializeField] private bool stopOnDisable = true;
    [SerializeField] private bool stopOnDestroy = true;
    [SerializeField] private Transform followTarget;
    [SerializeField] private float radius = 5f;
    [SerializeField] private float fadeIn;
    [SerializeField] private float fadeOut;

    private FmodHandle handle;

    private void Awake()
    {
        if (playOnAwake)
            Play();
    }

    private void OnDisable()
    {
        if (stopOnDisable)
            Stop();
    }

    private void OnDestroy()
    {
        if (stopOnDestroy)
            Stop();
    }

    /// <summary>
    /// Plays the configured BetterFMOD event.
    /// </summary>
    public FmodHandle Play()
    {
        if (string.IsNullOrWhiteSpace(eventId))
            return FmodHandle.Invalid(eventId);

        Transform target = followTarget != null ? followTarget : transform;
        handle = loop || !oneShot ? Fmod.PlayLoop(eventId) : Fmod.Play(eventId);

        if (target != null)
            handle.Follow(target);

        if (radius > 0f)
            handle.Radius(radius);

        if (fadeIn > 0f)
            handle.FadeIn(fadeIn);

        return handle;
    }

    /// <summary>
    /// Stops the active BetterFMOD event instance.
    /// </summary>
    public void Stop()
    {
        if (handle != null && handle.IsValid)
            handle.Stop(fadeOut > 0f, fadeOut);
    }

    /// <summary>
    /// Pauses the active BetterFMOD event instance.
    /// </summary>
    public void Pause()
    {
        handle?.Pause();
    }

    /// <summary>
    /// Resumes the active BetterFMOD event instance.
    /// </summary>
    public void Resume()
    {
        handle?.Resume();
    }

    /// <summary>
    /// Sets a parameter on the active BetterFMOD event instance.
    /// </summary>
    public void SetParameter(string parameter, float value)
    {
        handle?.Parameter(parameter, value);
    }
}
#endif
