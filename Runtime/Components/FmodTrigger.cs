#if FMOD_PRESENT
using System;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Toca ou para eventos BetterFMOD a partir de trigger, colisao, ciclo de vida e clique da Unity.
/// </summary>
public class FmodTrigger : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private string eventId;
    [SerializeField] private bool loop;
    [SerializeField] private bool followSelf = true;
    [SerializeField] private FmodTriggerMoment playOn = FmodTriggerMoment.TriggerEnter;
    [SerializeField] private FmodTriggerMoment stopOn = FmodTriggerMoment.Disable | FmodTriggerMoment.Destroy;
    [SerializeField] private float fadeOut;

    private FmodHandle handle;

    private void OnEnable()
    {
        Execute(FmodTriggerMoment.Enable);
    }

    private void OnDisable()
    {
        StopIfConfigured(FmodTriggerMoment.Disable);
    }

    private void OnDestroy()
    {
        StopIfConfigured(FmodTriggerMoment.Destroy);
        Execute(FmodTriggerMoment.Destroy);
    }

    private void OnTriggerEnter(Collider other)
    {
        Execute(FmodTriggerMoment.TriggerEnter);
    }

    private void OnTriggerExit(Collider other)
    {
        Execute(FmodTriggerMoment.TriggerExit);
        StopIfConfigured(FmodTriggerMoment.TriggerExit);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Execute(FmodTriggerMoment.Collision);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Execute(FmodTriggerMoment.TriggerEnter);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Execute(FmodTriggerMoment.TriggerExit);
        StopIfConfigured(FmodTriggerMoment.TriggerExit);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Execute(FmodTriggerMoment.Collision);
    }

    private void OnMouseDown()
    {
        Execute(FmodTriggerMoment.Click);
    }

    /// <summary>
    /// Executa as acoes configuradas para clique do ponteiro.
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        Execute(FmodTriggerMoment.Click);
    }

    /// <summary>
    /// Toca o evento configurado.
    /// </summary>
    public FmodHandle Play()
    {
        if (string.IsNullOrWhiteSpace(eventId))
            return FmodHandle.Invalid(eventId);

        FmodEventBuilder builder = Fmod.Event(eventId);

        if (loop)
            builder.Loop();

        if (followSelf)
            builder.FollowTransform(transform);

        handle = builder.Play();

        return handle;
    }

    /// <summary>
    /// Para o evento ativo.
    /// </summary>
    public void Stop()
    {
        if (handle != null && handle.IsValid)
            handle.Stop(fadeOut > 0f, fadeOut);
        else
            Fmod.Stop(eventId, fadeOut > 0f, fadeOut);
    }

    private void Execute(FmodTriggerMoment moment)
    {
        if ((playOn & moment) != 0)
            Play();
    }

    private void StopIfConfigured(FmodTriggerMoment moment)
    {
        if ((stopOn & moment) != 0)
            Stop();
    }
}

/// <summary>
/// Define os momentos da Unity que podem disparar acoes do BetterFMOD.
/// </summary>
[Flags]
public enum FmodTriggerMoment
{
    None = 0,
    TriggerEnter = 1,
    TriggerExit = 2,
    Collision = 4,
    Enable = 8,
    Disable = 16,
    Destroy = 32,
    Click = 64
}
#endif
