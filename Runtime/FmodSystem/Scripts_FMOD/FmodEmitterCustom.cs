using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Emissor do BetterFMOD configurado por funcoes da cascata.
/// </summary>
public class FmodEmitterCustom : MonoBehaviour
{
    /// <summary>
    /// Define o modo de inspector do emissor.
    /// </summary>
    public enum EmitterMode
    {
        None,
        Basic,
        Advanced
    }

    /// <summary>
    /// Define quando este emissor toca.
    /// </summary>
    public enum PlayEvent
    {
        None,
        OnEnable,
        OnStart,
        OnMouseEnter
    }

    /// <summary>
    /// Define quando este emissor para.
    /// </summary>
    public enum StopEvent
    {
        None,
        OnDisable,
        OnDestroy
    }

    public enum CascadeFunction
    {
        As3D,
        Attach,
        Position,
        Velocity,
        Radius,
        Volume,
        Pitch,
        FadeIn,
        Parameter,
        ParameterLabel,
        TimelinePosition
    }

    [System.Serializable]
    public sealed class CascadeStep
    {
        public CascadeFunction function;
        public Transform transform;
        public Vector3 vectorValue;
        public float floatValue = 1f;
        public int intValue;
        public string parameter;
        public string label;
    }

    public EmitterMode mode;
    public string eventId;
    public bool oneShot;
    public PlayEvent playEvent;
    public StopEvent stopEvent;
    public List<CascadeStep> cascade = new();
    public Color gizmoColor = Color.cyan;

    private FmodHandle handle;

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

    /// <summary>
    /// Toca o evento configurado.
    /// </summary>
    public void Play()
    {
        if (string.IsNullOrWhiteSpace(eventId))
            return;

        FmodEventBuilder builder = FmodB8.Event(eventId);

        if (!oneShot)
            builder.Loop();

        ApplyCascade(builder);
        handle = builder.Play();
    }

    /// <summary>
    /// Para o evento configurado.
    /// </summary>
    public void Stop(bool fade = true)
    {
        if (handle != null && handle.IsValid)
            handle.Stop(fade);
        else
            FmodB8.Stop(eventId, fade);
    }

    /// <summary>
    /// Pausa ou retoma o evento configurado.
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
            FmodB8.Pause(eventId);
        else
            FmodB8.Resume(eventId);
    }

    private void ApplyCascade(FmodEventBuilder builder)
    {
        if (builder == null || cascade == null)
            return;

        foreach (CascadeStep step in cascade)
        {
            if (step == null)
                continue;

            switch (step.function)
            {
                case CascadeFunction.As3D:
                    builder.As3D();
                    break;
                case CascadeFunction.Attach:
                    builder.FollowTransform(step.transform != null ? step.transform : transform);
                    break;
                case CascadeFunction.Position:
                    builder.Position(step.vectorValue);
                    break;
                case CascadeFunction.Velocity:
                    builder.Velocity(step.vectorValue);
                    break;
                case CascadeFunction.Radius:
                    builder.Radius(Mathf.Max(0.01f, step.floatValue));
                    break;
                case CascadeFunction.Volume:
                    builder.Volume(step.floatValue);
                    break;
                case CascadeFunction.Pitch:
                    builder.Pitch(step.floatValue);
                    break;
                case CascadeFunction.FadeIn:
                    builder.FadeIn(Mathf.Max(0f, step.floatValue));
                    break;
                case CascadeFunction.Parameter:
                    builder.Parameter(step.parameter, step.floatValue);
                    break;
                case CascadeFunction.ParameterLabel:
                    builder.ParameterLabel(step.parameter, step.label);
                    break;
                case CascadeFunction.TimelinePosition:
                    builder.TimelinePosition(Mathf.Max(0, step.intValue));
                    break;
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (mode != EmitterMode.Advanced || cascade == null)
            return;

        foreach (CascadeStep step in cascade)
        {
            if (step == null || step.function != CascadeFunction.Radius)
                continue;

            Gizmos.color = gizmoColor;
            Gizmos.DrawWireSphere(transform.position, Mathf.Max(0.01f, step.floatValue));
        }
    }

    private void OnValidate()
    {
        if (cascade == null)
            return;

        foreach (CascadeStep step in cascade)
        {
            if (step == null)
                continue;

            if (step.function == CascadeFunction.Radius)
                step.floatValue = Mathf.Max(0.01f, step.floatValue);
            else if (step.function == CascadeFunction.FadeIn)
                step.floatValue = Mathf.Max(0f, step.floatValue);
            else if (step.function == CascadeFunction.TimelinePosition)
                step.intValue = Mathf.Max(0, step.intValue);
        }
    }
}
