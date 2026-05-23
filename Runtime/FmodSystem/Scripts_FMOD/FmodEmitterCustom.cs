using UnityEngine;
using System.Collections;

public class FmodEmitterCustom : MonoBehaviour
{
    public enum EmitterMode
    {
        None,
        Basic,
        Advanced
    }

    public enum PlayEvent
    {
        None,
        OnEnable,
        OnStart,
        OnMouseEnter
    }

    public enum StopEvent
    {
        None,
        OnDisable,
        OnDestroy
    }

    // Mode
    public EmitterMode mode;

    // Basic Config
    public string eventId;
    public bool is3D = false;
    public bool oneShot = false;

    // Triggers
    public PlayEvent playEvent;
    public StopEvent stopEvent;

    // Advanced Settings
    public float radius = 5f;
    public Color gizmoColor = Color.cyan;

    private FmodCommands fmodCommands;

    void Awake()
    {
        fmodCommands = FmodCommands.Instance;
    }

    void OnEnable()
    {
        if (playEvent == PlayEvent.OnEnable)
            StartCoroutine(PlayNextFrame());
    }

    IEnumerator PlayNextFrame()
    {
        yield return null;

        Play();
    }

    void Start()
    {
        if (playEvent == PlayEvent.OnStart)
            Play();
    }

    void OnDisable()
    {
        if (stopEvent == StopEvent.OnDisable)
            Stop();
    }

    void OnDestroy()
    {
        if (stopEvent == StopEvent.OnDestroy)
            Stop();
    }

    void OnMouseEnter()
    {
        if (playEvent == PlayEvent.OnMouseEnter)
            Play();
    }

    public void Play()
    {
        if (fmodCommands == null)
            return;

        if (oneShot)
        {
            fmodCommands.PlayOneShot(eventId);
            return;
        }

        fmodCommands.PlayLoop(eventId);
    }

    public void Stop(bool fade = true)
    {
        if (fmodCommands == null)
            return;

        fmodCommands.Stop(eventId, fade);
    }

    public void Pause(bool pause)
    {
        if (fmodCommands == null)
            return;

        fmodCommands.Pause(eventId, pause);
    }

    void OnDrawGizmos()
    {
        if (mode != EmitterMode.Advanced)
            return;

        if (!is3D)
            return;

        Gizmos.color = gizmoColor;

        Gizmos.DrawWireSphere(transform.position, radius);
    }
}