#if FMOD_PRESENT
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Componente legado de ponteiro de UI que executa comandos BetterFMOD.
/// </summary>
public class FmodButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private List<FmodButtonAction> actions = new();

    /// <summary>
    /// Executa as acoes configuradas para clique do ponteiro.
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        Execute(ButtonMoment.OnClick);
    }

    /// <summary>
    /// Executa as acoes configuradas para entrada do ponteiro.
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        Execute(ButtonMoment.OnEnter);
    }

    /// <summary>
    /// Executa as acoes configuradas para saida do ponteiro.
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
/// Representa uma acao legada de ponteiro de UI mapeada para um comando BetterFMOD.
/// </summary>
[Serializable]
public class FmodButtonAction
{
    public ButtonMoment moment = ButtonMoment.None;
    public FmodCommandType command = FmodCommandType.PlayOneShot;
    public string soundId;
    public bool fade;

    /// <summary>
    /// Executa esta acao BetterFMOD.
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
/// Define um momento legado de ponteiro de UI.
/// </summary>
public enum ButtonMoment
{
    None,
    OnEnter,
    OnExit,
    OnClick
}

/// <summary>
/// Define um tipo legado de comando BetterFMOD.
/// </summary>
public enum FmodCommandType
{
    PlayOneShot,
    PlayLoop,
    Stop,
    Pause
}
#endif
