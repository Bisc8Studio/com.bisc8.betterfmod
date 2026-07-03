#if FMOD_PRESENT
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Legacy UI pointer component that executes BetterFMOD commands.
/// </summary>
public class FmodButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private List<FmodButtonAction> actions = new();

    /// <summary>
    /// Executes actions configured for pointer click.
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        Execute(ButtonMoment.OnClick);
    }

    /// <summary>
    /// Executes actions configured for pointer enter.
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        Execute(ButtonMoment.OnEnter);
    }

    /// <summary>
    /// Executes actions configured for pointer exit.
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        Execute(ButtonMoment.OnExit);
    }

    private void Execute(ButtonMoment moment)
    {
        foreach (FmodButtonAction action in actions)
        {
            if (action.moment == moment)
                action.Execute();
        }
    }
}

/// <summary>
/// Represents a legacy UI pointer action that maps to a BetterFMOD command.
/// </summary>
[Serializable]
public class FmodButtonAction
{
    public ButtonMoment moment = ButtonMoment.None;
    public FmodCommandType command = FmodCommandType.PlayOneShot;
    public string soundId;
    public bool fade;

    /// <summary>
    /// Executes this BetterFMOD action.
    /// </summary>
    public void Execute()
    {
        if (moment == ButtonMoment.None || string.IsNullOrWhiteSpace(soundId))
            return;

        switch (command)
        {
            case FmodCommandType.PlayOneShot:
                Fmod.Play(soundId);
                break;
            case FmodCommandType.PlayLoop:
                Fmod.PlayLoop(soundId);
                break;
            case FmodCommandType.Stop:
                Fmod.Stop(soundId, fade);
                break;
            case FmodCommandType.Pause:
                Fmod.TogglePause(soundId);
                break;
        }
    }
}

/// <summary>
/// Defines a legacy UI pointer moment.
/// </summary>
public enum ButtonMoment
{
    None,
    OnEnter,
    OnExit,
    OnClick
}

/// <summary>
/// Defines a legacy BetterFMOD command type.
/// </summary>
public enum FmodCommandType
{
    PlayOneShot,
    PlayLoop,
    Stop,
    Pause
}
#endif
