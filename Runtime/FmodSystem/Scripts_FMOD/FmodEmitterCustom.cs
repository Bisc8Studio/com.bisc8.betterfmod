#if FMOD_PRESENT
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

    public EmitterMode mode;

    public string eventId;
    public bool is3D = false;
    public bool oneShot = false;

    public PlayEvent playEvent;
    public StopEvent stopEvent;

    public float radius = 5f;
    public Color gizmoColor = Color.cyan;

    private FmodCommands fmod;

    void Awake()
    {
        fmod = FmodCommands.Instance;
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
        if (fmod == null)
            return;

        if (oneShot)
        {
            if (is3D)
                fmod.PlayOneShot3D(eventId, transform);
            else
                fmod.PlayOneShot(eventId);

            return;
        }

        if (is3D)
            fmod.PlayLoop3D(eventId, transform, radius); // 🔥 radius agora REAL
        else
            fmod.PlayLoop(eventId);
    }

    public void Stop(bool fade = true)
    {
        if (fmod == null)
            return;

        fmod.Stop(eventId, fade);
    }

    public void Pause(bool pause)
    {
        if (fmod == null)
            return;

        fmod.Pause(eventId, pause);
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
#endif