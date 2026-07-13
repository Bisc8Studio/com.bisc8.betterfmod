using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Emissor do FMODB8 configurado por funcoes da cascata.
/// </summary>
[AddComponentMenu("FMODB8/FMODB8 Emmiter")]
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
        As3D = 0,
        Attach = 1,
        Position = 2,
        Velocity = 3,
        Radius = 4,
        Volume = 5,
        Pitch = 6,
        FadeIn = 7,
        Parameter = 8,
        ParameterLabel = 9,
        TimelinePosition = 10,
        FadeOut = 11,
        FadeTo = 12,
        Stop = 13,
        Pause = 14,
        Resume = 15,
        TogglePause = 16,
        Detach = 17,
        Keep = 18
    }

    [System.Serializable]
    public sealed class CascadeStep
    {
        public CascadeFunction function;
        public Transform transform;
        public Vector3 vectorValue;
        public float floatValue = 1f;
        public float floatValue2 = 1f;
        public bool boolValue;
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

        handle = builder.Play();
        ApplyCascade(handle);
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

    private void ApplyCascade(FmodHandle targetHandle)
    {
        if (targetHandle == null || !targetHandle.IsValid || cascade == null)
            return;

        ValidateCascadeDependencies();

        foreach (CascadeStep step in cascade)
        {
            if (step == null)
                continue;

            switch (step.function)
            {
                case CascadeFunction.As3D:
                    targetHandle.As3D();
                    break;
                case CascadeFunction.Attach:
                    targetHandle.FollowTransform(step.transform != null ? step.transform : transform);
                    break;
                case CascadeFunction.Position:
                    targetHandle.Position(step.vectorValue);
                    break;
                case CascadeFunction.Velocity:
                    targetHandle.Velocity(step.vectorValue);
                    break;
                case CascadeFunction.Radius:
                    targetHandle.Radius(Mathf.Max(0.01f, step.floatValue));
                    break;
                case CascadeFunction.Volume:
                    targetHandle.Volume(step.floatValue);
                    break;
                case CascadeFunction.Pitch:
                    targetHandle.Pitch(step.floatValue);
                    break;
                case CascadeFunction.FadeIn:
                    targetHandle.FadeIn(Mathf.Max(0f, step.floatValue));
                    break;
                case CascadeFunction.FadeOut:
                    targetHandle.FadeOut(Mathf.Max(0f, step.floatValue));
                    break;
                case CascadeFunction.FadeTo:
                    targetHandle.FadeTo(step.floatValue, Mathf.Max(0f, step.floatValue2));
                    break;
                case CascadeFunction.Stop:
                    targetHandle.Stop(step.boolValue, Mathf.Max(0f, step.floatValue));
                    break;
                case CascadeFunction.Pause:
                    targetHandle.Pause();
                    break;
                case CascadeFunction.Resume:
                    targetHandle.Resume();
                    break;
                case CascadeFunction.TogglePause:
                    targetHandle.TogglePause();
                    break;
                case CascadeFunction.Parameter:
                    targetHandle.Parameter(step.parameter, step.floatValue);
                    break;
                case CascadeFunction.ParameterLabel:
                    targetHandle.SetParameterLabel(step.parameter, step.label);
                    break;
                case CascadeFunction.TimelinePosition:
                    targetHandle.SetTimelinePosition(Mathf.Max(0, step.intValue));
                    break;
                case CascadeFunction.Detach:
                    targetHandle.Detach();
                    break;
                case CascadeFunction.Keep:
                    targetHandle.Keep(step.parameter);
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
        ValidateCascadeDependencies();
    }

    public void ValidateCascadeDependencies()
    {
        if (cascade == null)
            return;

        bool requires3D = HasFunction(CascadeFunction.As3D)
                       || HasFunction(CascadeFunction.Radius)
                       || HasFunction(CascadeFunction.Attach)
                       || HasFunction(CascadeFunction.Position)
                       || HasFunction(CascadeFunction.Velocity);

        if (requires3D && !HasFunction(CascadeFunction.As3D))
            cascade.Insert(0, new CascadeStep { function = CascadeFunction.As3D });

        bool hasSpatialAnchor = HasFunction(CascadeFunction.Attach)
                             || HasFunction(CascadeFunction.Position);

        if (requires3D && !hasSpatialAnchor)
        {
            cascade.Add(new CascadeStep
            {
                function = CascadeFunction.Attach,
                transform = transform
            });
        }

        foreach (CascadeStep step in cascade)
        {
            if (step == null)
                continue;

            if (step.function == CascadeFunction.Radius)
                step.floatValue = Mathf.Max(0.01f, step.floatValue);
            else if (step.function == CascadeFunction.FadeIn
                  || step.function == CascadeFunction.FadeOut
                  || step.function == CascadeFunction.Stop)
                step.floatValue = Mathf.Max(0f, step.floatValue);
            else if (step.function == CascadeFunction.FadeTo)
                step.floatValue2 = Mathf.Max(0f, step.floatValue2);
            else if (step.function == CascadeFunction.TimelinePosition)
                step.intValue = Mathf.Max(0, step.intValue);

            if (step.function == CascadeFunction.Attach && step.transform == null)
                step.transform = transform;
        }
    }

    private bool HasFunction(CascadeFunction function)
    {
        if (cascade == null)
            return false;

        foreach (CascadeStep step in cascade)
        {
            if (step != null && step.function == function)
                return true;
        }

        return false;
    }
}
