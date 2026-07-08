#if FMOD_PRESENT
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Componente de botao que executa acoes BetterFMOD em Canvas ou objetos 3D no mundo.
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
                action.Execute();
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

    // Campos do comando raiz
    public string soundId;          // Event ID, Keep Key, path de snapshot/bus/VCA
    public bool   fade;             // Stop / StopAll
    public float  floatValue = 1f;  // Volume, Duration, FadeTime, etc.
    public float  floatValue2 = 1f; // Segundo float (FadeTo)
    public string parameter;        // SetParameter / SetParameterLabel / SetGlobalParameter
    public string label;            // SetParameterLabel

    // Modificadores de cascata aplicados ao FmodHandle (so para Play / PlayLoop / StartSnapshot / Kept)
    public List<ButtonCascadeStep> cascade = new();

    public void Execute()
    {
        if (moment == ButtonMoment.None)
            return;

        FmodHandle handle = ExecuteRoot();

        if (handle != null && IsHandleCommand(command) && cascade != null)
        {
            foreach (ButtonCascadeStep step in cascade)
                step?.Apply(handle);
        }
    }

    private FmodHandle ExecuteRoot()
    {
        bool hasId = !string.IsNullOrWhiteSpace(soundId);

        switch (command)
        {
            case ButtonRootCommand.Play:
                return hasId ? Fmod.Play(soundId) : null;
            case ButtonRootCommand.PlayLoop:
                return hasId ? Fmod.PlayLoop(soundId) : null;
            case ButtonRootCommand.Stop:
                if (hasId) Fmod.Stop(soundId, fade, floatValue); return null;
            case ButtonRootCommand.Pause:
                if (hasId) Fmod.Pause(soundId); return null;
            case ButtonRootCommand.Resume:
                if (hasId) Fmod.Resume(soundId); return null;
            case ButtonRootCommand.TogglePause:
                if (hasId) Fmod.TogglePause(soundId); return null;
            case ButtonRootCommand.StopAll:
                Fmod.StopAll(fade, floatValue); return null;
            case ButtonRootCommand.FadeIn:
                if (hasId) Fmod.FadeIn(soundId, floatValue); return null;
            case ButtonRootCommand.FadeOut:
                if (hasId) Fmod.FadeOut(soundId, floatValue); return null;
            case ButtonRootCommand.FadeTo:
                if (hasId) Fmod.FadeTo(soundId, floatValue, floatValue2); return null;
            case ButtonRootCommand.SetVolume:
                if (hasId) Fmod.SetVolume(soundId, floatValue); return null;
            case ButtonRootCommand.SetPitch:
                if (hasId) Fmod.SetPitch(soundId, floatValue); return null;
            case ButtonRootCommand.SetParameter:
                if (hasId) Fmod.SetParameter(soundId, parameter, floatValue); return null;
            case ButtonRootCommand.SetParameterLabel:
                if (hasId) Fmod.SetParameterLabel(soundId, parameter, label); return null;
            case ButtonRootCommand.SetGlobalParameter:
                Fmod.SetGlobalParameter(parameter, floatValue); return null;
            case ButtonRootCommand.StartSnapshot:
                return hasId ? Fmod.StartSnapshot(soundId) : null;
            case ButtonRootCommand.StopSnapshot:
                if (hasId) Fmod.StopSnapshot(soundId); return null;
            case ButtonRootCommand.SetBusVolume:
                if (hasId) Fmod.SetBusVolume(soundId, floatValue); return null;
            case ButtonRootCommand.SetVcaVolume:
                if (hasId) Fmod.SetVcaVolume(soundId, floatValue); return null;
            case ButtonRootCommand.Kept:
                return hasId ? Fmod.Kept(soundId) : null;
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
            case ButtonCascadeModifier.Follow:
                if (target != null) handle.Follow(target);
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
#endif
