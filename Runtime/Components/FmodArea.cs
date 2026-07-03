#if FMOD_PRESENT
using UnityEngine;

/// <summary>
/// Applies BetterFMOD snapshot, music, and parameter changes while a target is inside an area.
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
    /// Applies the configured area enter audio state.
    /// </summary>
    public void Enter()
    {
        if (!string.IsNullOrWhiteSpace(snapshot))
            snapshotHandle = Fmod.StartSnapshot(snapshot);

        if (!string.IsNullOrWhiteSpace(music))
            musicHandle = Fmod.PlayLoop(music).FadeIn(0.25f);

        if (!string.IsNullOrWhiteSpace(parameter))
            Fmod.SetParameter(parameterEvent, parameter, enterValue);
    }

    /// <summary>
    /// Applies the configured area exit audio state.
    /// </summary>
    public void Exit()
    {
        if (snapshotHandle != null && snapshotHandle.IsValid)
            snapshotHandle.FadeOut(fadeOut);

        if (musicHandle != null && musicHandle.IsValid)
            musicHandle.FadeOut(fadeOut);

        if (!string.IsNullOrWhiteSpace(parameter))
            Fmod.SetParameter(parameterEvent, parameter, exitValue);
    }

    private bool IsTarget(Collider other)
    {
        return string.IsNullOrWhiteSpace(targetTag) || other.CompareTag(targetTag);
    }
}
#endif
