#if FMOD_PRESENT
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Componente de botao que executa comandos BetterFMOD em Canvas ou objetos 3D no mundo.
/// </summary>
public class FmodButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private List<FmodButtonAction> actions = new();

    /// <summary>
    /// Executa as acoes configuradas para clique em UI Canvas.
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        Execute(ButtonMoment.OnClickCanvas);
    }

    /// <summary>
    /// Executa as acoes configuradas para entrada do ponteiro em UI Canvas.
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        Execute(ButtonMoment.OnEnterCanvas);
    }

    /// <summary>
    /// Executa as acoes configuradas para saida do ponteiro em UI Canvas.
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        Execute(ButtonMoment.OnExitCanvas);
    }

    /// <summary>
    /// Executa as acoes configuradas para clique em objeto 3D no mundo.
    /// </summary>
    private void OnMouseDown()
    {
        Execute(ButtonMoment.OnClickWorld);
    }

    /// <summary>
    /// Executa as acoes configuradas para entrada do mouse em objeto 3D no mundo.
    /// </summary>
    private void OnMouseEnter()
    {
        Execute(ButtonMoment.OnEnterWorld);
    }

    /// <summary>
    /// Executa as acoes configuradas para saida do mouse em objeto 3D no mundo.
    /// </summary>
    private void OnMouseExit()
    {
        Execute(ButtonMoment.OnExitWorld);
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
/// Define o momento que dispara uma acao BetterFMOD no Canvas ou no mundo 3D.
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
