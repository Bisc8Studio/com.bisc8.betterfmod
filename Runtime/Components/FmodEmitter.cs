using UnityEngine;

/// <summary>
/// Toca e controla um evento BetterFMOD a partir de um GameObject.
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
    /// Toca o evento BetterFMOD configurado.
    /// </summary>
    public FmodHandle Play()
    {
        if (string.IsNullOrWhiteSpace(eventId))
            return FmodHandle.Invalid(eventId);

        FmodEventBuilder builder = FmodB8.Event(eventId);

        if (loop || !oneShot)
            builder.Loop();

        Transform target = followTarget != null ? followTarget : transform;

        if (target != null)
            builder.FollowTransform(target);

        if (radius > 0f)
            builder.Radius(radius);

        if (fadeIn > 0f)
            builder.FadeIn(fadeIn);

        handle = builder.Play();

        return handle;
    }

    /// <summary>
    /// Para a instancia BetterFMOD ativa.
    /// </summary>
    public void Stop()
    {
        if (handle != null && handle.IsValid)
            handle.Stop(fadeOut > 0f, fadeOut);
    }

    /// <summary>
    /// Pausa a instancia BetterFMOD ativa.
    /// </summary>
    public void Pause()
    {
        handle?.Pause();
    }

    /// <summary>
    /// Retoma a instancia BetterFMOD ativa.
    /// </summary>
    public void Resume()
    {
        handle?.Resume();
    }

    /// <summary>
    /// Define um parametro na instancia BetterFMOD ativa.
    /// </summary>
    public void SetParameter(string parameter, float value)
    {
        handle?.Parameter(parameter, value);
    }
}
