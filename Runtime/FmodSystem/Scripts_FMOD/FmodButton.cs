using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Componente de botao que executa acoes FMODB8 em Canvas ou objetos 3D no mundo.
/// </summary>
public class FmodButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private List<FmodButtonAction> actions = new();

    public void OnPointerClick(PointerEventData eventData) => Execute(ButtonMoment.OnClickCanvas);
    public void OnPointerEnter(PointerEventData eventData) => Execute(ButtonMoment.OnEnterCanvas);
    public void OnPointerExit(PointerEventData eventData) => Execute(ButtonMoment.OnExitCanvas);
    private void OnMouseDown() => Execute(ButtonMoment.OnClickWorld);
    private void OnMouseEnter() => Execute(ButtonMoment.OnEnterWorld);
    private void OnMouseExit() => Execute(ButtonMoment.OnExitWorld);

    private void Execute(ButtonMoment moment)
    {
        foreach (FmodButtonAction action in actions)
        {
            if (action != null && action.moment == moment)
                action.Execute(transform);
        }
    }

    private void OnValidate()
    {
        ValidateActions();
    }

    public void ValidateActions()
    {
        if (actions == null)
            return;

        foreach (FmodButtonAction action in actions)
            action?.ValidateDependencies(transform);
    }

    public void ApplyMultiplayerDefaults(bool enableMultiplayer)
    {
        if (actions == null)
            return;

        foreach (FmodButtonAction action in actions)
        {
            if (action == null)
                continue;

            if (FmodButtonAction.IsNetworkableCommand(action.command))
                action.playbackScope = enableMultiplayer ? FmodPlaybackScope.Multiplayer : FmodPlaybackScope.Local;

            action.ValidateDependencies(transform);
        }
    }
}

/// <summary>
/// Acao de um FmodButton: momento de disparo, comando raiz e cascata de modificadores no handle.
/// </summary>
[Serializable]
public class FmodButtonAction
{
    public ButtonMoment moment  = ButtonMoment.None;
    public ButtonRootCommand command = ButtonRootCommand.Play;
    public FmodPlaybackScope playbackScope = FmodPlaybackScope.Local;

    // Campos do comando raiz
    public string soundId;          // Event ID, Keep Key, path de snapshot/bus/VCA
    public bool   fade;             // Stop / StopAll
    public float  floatValue = 1f;  // Volume, Duration, FadeTime, etc.
    public float  floatValue2 = 1f; // Segundo float (FadeTo)
    public string parameter;        // SetParameter / SetParameterLabel / SetGlobalParameter
    public string label;            // SetParameterLabel

    // Modificadores de cascata aplicados ao FmodHandle (so para Play / PlayLoop / StartSnapshot / Kept)
    public List<ButtonCascadeStep> cascade = new();

    public void Execute(Transform source = null)
    {
        if (moment == ButtonMoment.None)
            return;

        ValidateDependencies(source);

        if (ShouldDispatchMultiplayer())
        {
            FmodCommands.EnsureInstance().DispatchButtonAction(this, source);
            return;
        }

        ExecuteLocal(source);
    }

    internal FmodHandle ExecuteLocal(Transform source = null)
    {
        FmodHandle handle = ExecuteRoot();

        if (handle != null && IsHandleCommand(command) && cascade != null)
        {
            foreach (ButtonCascadeStep step in cascade)
                step?.Apply(handle, source);
        }

        return handle;
    }

    private FmodHandle ExecuteRoot()
    {
        bool hasId = !string.IsNullOrWhiteSpace(soundId);

        switch (command)
        {
            case ButtonRootCommand.Play:
                return hasId ? FmodB8.Play(soundId) : null;
            case ButtonRootCommand.PlayLoop:
                return hasId ? FmodB8.PlayLoop(soundId) : null;
            case ButtonRootCommand.Stop:
                if (hasId) FmodB8.Stop(soundId, fade, floatValue); return null;
            case ButtonRootCommand.Pause:
                if (hasId) FmodB8.Pause(soundId); return null;
            case ButtonRootCommand.Resume:
                if (hasId) FmodB8.Resume(soundId); return null;
            case ButtonRootCommand.TogglePause:
                if (hasId) FmodB8.TogglePause(soundId); return null;
            case ButtonRootCommand.StopAll:
                FmodB8.StopAll(fade, floatValue); return null;
            case ButtonRootCommand.FadeIn:
                if (hasId) FmodB8.FadeIn(soundId, floatValue); return null;
            case ButtonRootCommand.FadeOut:
                if (hasId) FmodB8.FadeOut(soundId, floatValue); return null;
            case ButtonRootCommand.FadeTo:
                if (hasId) FmodB8.FadeTo(soundId, floatValue, floatValue2); return null;
            case ButtonRootCommand.SetVolume:
                if (hasId) FmodB8.SetVolume(soundId, floatValue); return null;
            case ButtonRootCommand.SetPitch:
                if (hasId) FmodB8.SetPitch(soundId, floatValue); return null;
            case ButtonRootCommand.SetParameter:
                if (hasId) FmodB8.SetParameter(soundId, parameter, floatValue); return null;
            case ButtonRootCommand.SetParameterLabel:
                if (hasId) FmodB8.SetParameterLabel(soundId, parameter, label); return null;
            case ButtonRootCommand.SetGlobalParameter:
                FmodB8.SetGlobalParameter(parameter, floatValue); return null;
            case ButtonRootCommand.StartSnapshot:
                return hasId ? FmodB8.StartSnapshot(soundId) : null;
            case ButtonRootCommand.StopSnapshot:
                if (hasId) FmodB8.StopSnapshot(soundId); return null;
            case ButtonRootCommand.SetBusVolume:
                if (hasId) FmodB8.SetBusVolume(soundId, floatValue); return null;
            case ButtonRootCommand.SetVcaVolume:
                if (hasId) FmodB8.SetVcaVolume(soundId, floatValue); return null;
            case ButtonRootCommand.Kept:
                return hasId ? FmodB8.Kept(soundId) : null;
            default:
                return null;
        }
    }

    /// <summary>
    /// Retorna verdadeiro quando o comando raiz retorna um FmodHandle e aceita modificadores de cascata.
    /// </summary>
    public static bool IsHandleCommand(ButtonRootCommand cmd)
        => cmd == ButtonRootCommand.Play
        || cmd == ButtonRootCommand.PlayLoop
        || cmd == ButtonRootCommand.StartSnapshot
        || cmd == ButtonRootCommand.Kept;

    public static bool IsNetworkableCommand(ButtonRootCommand cmd)
        => cmd == ButtonRootCommand.Play
        || cmd == ButtonRootCommand.PlayLoop
        || cmd == ButtonRootCommand.StartSnapshot;

    public void ValidateDependencies(Transform defaultTarget)
    {
        if (!IsHandleCommand(command))
            return;

        cascade ??= new List<ButtonCascadeStep>();

        bool requires3D = HasModifier(ButtonCascadeModifier.As3D)
                       || HasModifier(ButtonCascadeModifier.Radius)
                       || HasModifier(ButtonCascadeModifier.Follow)
                       || HasModifier(ButtonCascadeModifier.Position)
                       || HasModifier(ButtonCascadeModifier.Velocity);

        if (requires3D && !HasModifier(ButtonCascadeModifier.As3D))
            cascade.Insert(0, new ButtonCascadeStep { modifier = ButtonCascadeModifier.As3D });

        bool hasSpatialAnchor = HasModifier(ButtonCascadeModifier.Follow)
                             || HasModifier(ButtonCascadeModifier.Position);

        if (requires3D && !hasSpatialAnchor)
        {
            cascade.Add(new ButtonCascadeStep
            {
                modifier = ButtonCascadeModifier.Follow,
                target = defaultTarget
            });
        }

        foreach (ButtonCascadeStep step in cascade)
        {
            if (step != null && step.modifier == ButtonCascadeModifier.Follow && step.target == null)
                step.target = defaultTarget;
        }
    }

    public FmodButtonActionPayload ToPayload(Transform source = null)
    {
        ValidateDependencies(source);

        FmodButtonActionPayload payload = new FmodButtonActionPayload
        {
            command = command,
            soundId = soundId,
            fade = fade,
            floatValue = floatValue,
            floatValue2 = floatValue2,
            parameter = parameter,
            label = label,
            hasSourcePosition = source != null,
            sourcePosition = source != null ? source.position : Vector3.zero
        };

        if (cascade == null)
            return payload;

        foreach (ButtonCascadeStep step in cascade)
        {
            if (step == null)
                continue;

            payload.cascade.Add(step.ToPayload(source));
        }

        return payload;
    }

    private bool ShouldDispatchMultiplayer()
    {
        return playbackScope == FmodPlaybackScope.Multiplayer
            && FmodMultiplayerSettings.MultiplayerModeEnabled
            && IsNetworkableCommand(command);
    }

    private bool HasModifier(ButtonCascadeModifier modifier)
    {
        if (cascade == null)
            return false;

        foreach (ButtonCascadeStep step in cascade)
        {
            if (step != null && step.modifier == modifier)
                return true;
        }

        return false;
    }
}

/// <summary>
/// Modificador encadeado ao FmodHandle retornado pelo comando raiz.
/// Espelha os metodos de FmodHandle: Volume, Pitch, Radius, FadeIn, Parameter, Keep, etc.
/// </summary>
[Serializable]
public class ButtonCascadeStep
{
    public ButtonCascadeModifier modifier = ButtonCascadeModifier.Volume;

    public string    stringValue;          // Keep key, nome do parametro
    public string    stringValue2;         // Label do parametro (ParameterLabel)
    public float     floatValue  = 1f;     // Volume, Pitch, Radius, Duration
    public float     floatValue2 = 1f;     // Segundo float (FadeTo: volume + duration)
    public bool      boolValue;            // Fade (Stop)
    public int       intValue;             // Timeline position
    public Vector3   vectorValue;          // Position / Velocity
    public Transform target;               // Follow

    public void Apply(FmodHandle handle, Transform defaultTarget = null)
    {
        if (handle == null || !handle.IsValid)
            return;

        switch (modifier)
        {
            case ButtonCascadeModifier.As3D:
                handle.As3D();
                break;
            case ButtonCascadeModifier.Volume:
                handle.Volume(floatValue);
                break;
            case ButtonCascadeModifier.Pitch:
                handle.Pitch(floatValue);
                break;
            case ButtonCascadeModifier.Radius:
                handle.Radius(Mathf.Max(0.01f, floatValue));
                break;
            case ButtonCascadeModifier.FadeIn:
                handle.FadeIn(floatValue);
                break;
            case ButtonCascadeModifier.FadeOut:
                handle.FadeOut(floatValue);
                break;
            case ButtonCascadeModifier.FadeTo:
                handle.FadeTo(floatValue, floatValue2);
                break;
            case ButtonCascadeModifier.Stop:
                handle.Stop(boolValue, floatValue);
                break;
            case ButtonCascadeModifier.Pause:
                handle.Pause();
                break;
            case ButtonCascadeModifier.Resume:
                handle.Resume();
                break;
            case ButtonCascadeModifier.TogglePause:
                handle.TogglePause();
                break;
            case ButtonCascadeModifier.Parameter:
                handle.Parameter(stringValue, floatValue);
                break;
            case ButtonCascadeModifier.ParameterLabel:
                handle.SetParameterLabel(stringValue, stringValue2);
                break;
            case ButtonCascadeModifier.TimelinePosition:
                handle.SetTimelinePosition(Mathf.Max(0, intValue));
                break;
            case ButtonCascadeModifier.Follow:
                Transform resolvedTarget = target != null ? target : defaultTarget;
                if (resolvedTarget != null) handle.Follow(resolvedTarget);
                break;
            case ButtonCascadeModifier.Detach:
                handle.Detach();
                break;
            case ButtonCascadeModifier.Position:
                handle.Position(vectorValue);
                break;
            case ButtonCascadeModifier.Velocity:
                handle.Velocity(vectorValue);
                break;
            case ButtonCascadeModifier.Keep:
                handle.Keep(stringValue);
                break;
        }
    }

    internal ButtonCascadeStepPayload ToPayload(Transform source = null)
    {
        ButtonCascadeStepPayload payload = new ButtonCascadeStepPayload
        {
            modifier = modifier,
            stringValue = stringValue,
            stringValue2 = stringValue2,
            floatValue = floatValue,
            floatValue2 = floatValue2,
            boolValue = boolValue,
            intValue = intValue,
            vectorValue = vectorValue
        };

        if (modifier == ButtonCascadeModifier.Follow)
        {
            Transform resolvedTarget = target != null ? target : source;
            if (resolvedTarget != null)
            {
                payload.modifier = ButtonCascadeModifier.Position;
                payload.vectorValue = resolvedTarget.position;
            }
        }

        return payload;
    }
}

[Serializable]
public class FmodButtonActionPayload
{
    public ButtonRootCommand command = ButtonRootCommand.Play;
    public string soundId;
    public bool fade;
    public float floatValue = 1f;
    public float floatValue2 = 1f;
    public string parameter;
    public string label;
    public bool hasSourcePosition;
    public Vector3 sourcePosition;
    public List<ButtonCascadeStepPayload> cascade = new();

    public FmodHandle ExecuteLocal()
    {
        ValidateDependencies();

        FmodButtonAction action = new FmodButtonAction
        {
            command = command,
            soundId = soundId,
            fade = fade,
            floatValue = floatValue,
            floatValue2 = floatValue2,
            parameter = parameter,
            label = label
        };

        FmodHandle handle = action.ExecuteLocal();

        if (handle != null && FmodButtonAction.IsHandleCommand(command) && cascade != null)
        {
            foreach (ButtonCascadeStepPayload step in cascade)
                step?.Apply(handle);

            if (hasSourcePosition && !HasSpatialAnchor())
                handle.Position(sourcePosition);
        }

        return handle;
    }

    public void ValidateDependencies()
    {
        if (!FmodButtonAction.IsHandleCommand(command))
            return;

        cascade ??= new List<ButtonCascadeStepPayload>();

        bool requires3D = hasSourcePosition;
        bool hasAs3D = false;
        bool hasSpatialAnchor = false;

        foreach (ButtonCascadeStepPayload step in cascade)
        {
            if (step == null)
                continue;

            if (step.modifier == ButtonCascadeModifier.As3D)
                hasAs3D = true;

            if (step.modifier == ButtonCascadeModifier.Radius
             || step.modifier == ButtonCascadeModifier.Follow
             || step.modifier == ButtonCascadeModifier.Position
             || step.modifier == ButtonCascadeModifier.Velocity)
                requires3D = true;

            if (step.modifier == ButtonCascadeModifier.Follow || step.modifier == ButtonCascadeModifier.Position)
                hasSpatialAnchor = true;
        }

        if (requires3D && !hasAs3D)
            cascade.Insert(0, new ButtonCascadeStepPayload { modifier = ButtonCascadeModifier.As3D });

        if (requires3D && !hasSpatialAnchor && hasSourcePosition)
        {
            cascade.Add(new ButtonCascadeStepPayload
            {
                modifier = ButtonCascadeModifier.Position,
                vectorValue = sourcePosition
            });
        }
    }

    private bool HasSpatialAnchor()
    {
        if (cascade == null)
            return false;

        foreach (ButtonCascadeStepPayload step in cascade)
        {
            if (step == null)
                continue;

            if (step.modifier == ButtonCascadeModifier.Position || step.modifier == ButtonCascadeModifier.Follow)
                return true;
        }

        return false;
    }
}

[Serializable]
public class ButtonCascadeStepPayload
{
    public ButtonCascadeModifier modifier = ButtonCascadeModifier.Volume;
    public string stringValue;
    public string stringValue2;
    public float floatValue = 1f;
    public float floatValue2 = 1f;
    public bool boolValue;
    public int intValue;
    public Vector3 vectorValue;

    public void Apply(FmodHandle handle)
    {
        if (handle == null || !handle.IsValid)
            return;

        switch (modifier)
        {
            case ButtonCascadeModifier.As3D:
                handle.As3D();
                break;
            case ButtonCascadeModifier.Volume:
                handle.Volume(floatValue);
                break;
            case ButtonCascadeModifier.Pitch:
                handle.Pitch(floatValue);
                break;
            case ButtonCascadeModifier.Radius:
                handle.Radius(Mathf.Max(0.01f, floatValue));
                break;
            case ButtonCascadeModifier.FadeIn:
                handle.FadeIn(floatValue);
                break;
            case ButtonCascadeModifier.FadeOut:
                handle.FadeOut(floatValue);
                break;
            case ButtonCascadeModifier.FadeTo:
                handle.FadeTo(floatValue, floatValue2);
                break;
            case ButtonCascadeModifier.Stop:
                handle.Stop(boolValue, floatValue);
                break;
            case ButtonCascadeModifier.Pause:
                handle.Pause();
                break;
            case ButtonCascadeModifier.Resume:
                handle.Resume();
                break;
            case ButtonCascadeModifier.TogglePause:
                handle.TogglePause();
                break;
            case ButtonCascadeModifier.Parameter:
                handle.Parameter(stringValue, floatValue);
                break;
            case ButtonCascadeModifier.ParameterLabel:
                handle.SetParameterLabel(stringValue, stringValue2);
                break;
            case ButtonCascadeModifier.TimelinePosition:
                handle.SetTimelinePosition(Mathf.Max(0, intValue));
                break;
            case ButtonCascadeModifier.Detach:
                handle.Detach();
                break;
            case ButtonCascadeModifier.Position:
            case ButtonCascadeModifier.Follow:
                handle.Position(vectorValue);
                break;
            case ButtonCascadeModifier.Velocity:
                handle.Velocity(vectorValue);
                break;
            case ButtonCascadeModifier.Keep:
                handle.Keep(stringValue);
                break;
        }
    }
}

/// <summary>Define o momento que dispara uma acao do FmodButton.</summary>
public enum ButtonMoment
{
    None,
    OnEnterCanvas,
    OnExitCanvas,
    OnClickCanvas,
    OnEnterWorld,
    OnExitWorld,
    OnClickWorld
}

/// <summary>Comando raiz de uma acao do FmodButton (o que e executado primeiro).</summary>
public enum ButtonRootCommand
{
    Play,
    PlayLoop,
    Stop,
    Pause,
    Resume,
    TogglePause,
    StopAll,
    FadeIn,
    FadeOut,
    FadeTo,
    SetVolume,
    SetPitch,
    SetParameter,
    SetParameterLabel,
    SetGlobalParameter,
    StartSnapshot,
    StopSnapshot,
    SetBusVolume,
    SetVcaVolume,
    Kept
}

public enum FmodPlaybackScope
{
    Local,
    Multiplayer
}

/// <summary>Modificador de cascata aplicado ao FmodHandle apos o comando raiz.</summary>
public enum ButtonCascadeModifier
{
    As3D,
    Volume,
    Pitch,
    Radius,
    FadeIn,
    FadeOut,
    FadeTo,
    Stop,
    Pause,
    Resume,
    TogglePause,
    Parameter,
    ParameterLabel,
    TimelinePosition,
    Follow,
    Detach,
    Position,
    Velocity,
    Keep
}
