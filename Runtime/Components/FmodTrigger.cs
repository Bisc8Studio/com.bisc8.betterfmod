#if FMOD_PRESENT
using System;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Plays or stops BetterFMOD events from Unity trigger, collision, lifecycle, and click events.
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
    /// Executes actions configured for pointer click.
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        Execute(FmodTriggerMoment.Click);
    }

    /// <summary>
    /// Plays the configured event.
    /// </summary>
    public FmodHandle Play()
    {
        if (string.IsNullOrWhiteSpace(eventId))
            return FmodHandle.Invalid(eventId);

        handle = loop ? Fmod.PlayLoop(eventId) : Fmod.Play(eventId);

        if (followSelf)
            handle.Follow(transform);

        return handle;
    }

    /// <summary>
    /// Stops the active event.
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
/// Defines Unity moments that can trigger BetterFMOD actions.
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
