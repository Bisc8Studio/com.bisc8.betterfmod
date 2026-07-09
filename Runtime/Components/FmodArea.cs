using UnityEngine;

/// <summary>
/// Aplica snapshot, musica e parametros BetterFMOD enquanto um alvo esta dentro de uma area.
/// </summary>
public class FmodArea : MonoBehaviour
{
    [SerializeField] private string targetTag = "Player";
    [SerializeField] private string snapshot;
    [SerializeField] private string music;
    [SerializeField] private string parameterEvent;
    [SerializeField] private string parameter;
    [SerializeField] private float enterValue = 1f;
    [SerializeField] private float exitValue;
    [SerializeField] private float fadeOut = 1f;

    private FmodHandle snapshotHandle;
    private FmodHandle musicHandle;

    private void OnTriggerEnter(Collider other)
    {
        if (!IsTarget(other))
            return;

        Enter();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsTarget(other))
            return;

        Exit();
    }

    /// <summary>
    /// Aplica o estado de audio configurado para entrada na area.
    /// </summary>
    public void Enter()
    {
        if (!string.IsNullOrWhiteSpace(snapshot))
            snapshotHandle = FmodB8.StartSnapshot(snapshot);

        if (!string.IsNullOrWhiteSpace(music))
            musicHandle = FmodB8.Event(music).Loop().FadeIn(0.25f).Play();

        if (!string.IsNullOrWhiteSpace(parameter))
            FmodB8.SetParameter(parameterEvent, parameter, enterValue);
    }

    /// <summary>
    /// Aplica o estado de audio configurado para saida da area.
    /// </summary>
    public void Exit()
    {
        if (snapshotHandle != null && snapshotHandle.IsValid)
            snapshotHandle.FadeOut(fadeOut);

        if (musicHandle != null && musicHandle.IsValid)
            musicHandle.FadeOut(fadeOut);

        if (!string.IsNullOrWhiteSpace(parameter))
            FmodB8.SetParameter(parameterEvent, parameter, exitValue);
    }

    private bool IsTarget(Collider other)
    {
        return string.IsNullOrWhiteSpace(targetTag) || other.CompareTag(targetTag);
    }
}
