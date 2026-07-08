#if FMOD_PRESENT
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Componente de botao que executa cascatas de comandos BetterFMOD em Canvas ou objetos 3D no mundo.
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
/// Acao de botao: define um momento de disparo e uma cascata de comandos BetterFMOD.
/// </summary>
[Serializable]
public class FmodButtonAction
{
    public ButtonMoment moment = ButtonMoment.None;
    public List<ButtonCascadeStep> cascade = new();

    /// <summary>
    /// Executa todos os passos da cascata desta acao.
    /// </summary>
    public void Execute()
    {
        if (moment == ButtonMoment.None || cascade == null)
            return;

        foreach (ButtonCascadeStep step in cascade)
            step?.Execute();
    }
}

/// <summary>
/// Um passo da cascata de comandos do FmodButton.
/// </summary>
[Serializable]
public class ButtonCascadeStep
{
    public ButtonCommandType command = ButtonCommandType.Play;
    public string soundId;
    public bool fade;
    public float floatValue = 1f;
    public float floatValue2 = 1f;
    public string parameter;
    public string label;

    /// <summary>
    /// Executa este passo usando a API do BetterFMOD.
    /// </summary>
    public void Execute()
    {
        switch (command)
        {
            case ButtonCommandType.Play:
                if (!string.IsNullOrWhiteSpace(soundId)) Fmod.Play(soundId);
                break;
            case ButtonCommandType.Stop:
                if (!string.IsNullOrWhiteSpace(soundId)) Fmod.Stop(soundId, fade, floatValue);
                break;
            case ButtonCommandType.Pause:
                if (!string.IsNullOrWhiteSpace(soundId)) Fmod.Pause(soundId);
                break;
            case ButtonCommandType.Resume:
                if (!string.IsNullOrWhiteSpace(soundId)) Fmod.Resume(soundId);
                break;
            case ButtonCommandType.TogglePause:
                if (!string.IsNullOrWhiteSpace(soundId)) Fmod.TogglePause(soundId);
                break;
            case ButtonCommandType.StopAll:
                Fmod.StopAll(fade, floatValue);
                break;
            case ButtonCommandType.FadeIn:
                if (!string.IsNullOrWhiteSpace(soundId)) Fmod.FadeIn(soundId, floatValue);
                break;
            case ButtonCommandType.FadeOut:
                if (!string.IsNullOrWhiteSpace(soundId)) Fmod.FadeOut(soundId, floatValue);
                break;
            case ButtonCommandType.FadeTo:
                if (!string.IsNullOrWhiteSpace(soundId)) Fmod.FadeTo(soundId, floatValue, floatValue2);
                break;
            case ButtonCommandType.SetVolume:
                if (!string.IsNullOrWhiteSpace(soundId)) Fmod.SetVolume(soundId, floatValue);
                break;
            case ButtonCommandType.SetPitch:
                if (!string.IsNullOrWhiteSpace(soundId)) Fmod.SetPitch(soundId, floatValue);
                break;
            case ButtonCommandType.SetParameter:
                if (!string.IsNullOrWhiteSpace(soundId)) Fmod.SetParameter(soundId, parameter, floatValue);
                break;
            case ButtonCommandType.SetParameterLabel:
                if (!string.IsNullOrWhiteSpace(soundId)) Fmod.SetParameterLabel(soundId, parameter, label);
                break;
            case ButtonCommandType.SetGlobalParameter:
                Fmod.SetGlobalParameter(parameter, floatValue);
                break;
            case ButtonCommandType.StartSnapshot:
                if (!string.IsNullOrWhiteSpace(soundId)) Fmod.StartSnapshot(soundId);
                break;
            case ButtonCommandType.StopSnapshot:
                if (!string.IsNullOrWhiteSpace(soundId)) Fmod.StopSnapshot(soundId);
                break;
            case ButtonCommandType.SetBusVolume:
                if (!string.IsNullOrWhiteSpace(soundId)) Fmod.SetBusVolume(soundId, floatValue);
                break;
            case ButtonCommandType.SetVcaVolume:
                if (!string.IsNullOrWhiteSpace(soundId)) Fmod.SetVcaVolume(soundId, floatValue);
                break;
        }
    }
}

/// <summary>
/// Define o momento que dispara uma acao do FmodButton.
/// </summary>
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

/// <summary>
/// Tipos de comandos BetterFMOD disponíveis em um passo de cascata do FmodButton.
/// </summary>
public enum ButtonCommandType
{
    Play,
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
    SetVcaVolume
}
#endif
