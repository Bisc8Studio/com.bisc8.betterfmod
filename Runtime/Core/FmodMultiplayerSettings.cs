#if FMOD_PRESENT
using UnityEngine;

public class FmodMultiplayerSettings : MonoBehaviour
{
    [SerializeField] private bool isMultiplayer;

    /// <summary>
    /// Retorna verdadeiro quando este componente habilita o modo multiplayer do BetterFMOD.
    /// </summary>
    public bool IsMultiplayer => isMultiplayer;

    /// <summary>
    /// Retorna verdadeiro quando o modo multiplayer do BetterFMOD esta ativo.
    /// </summary>
    public static bool MultiplayerModeEnabled { get; private set; }

    private static FmodMultiplayerSettings activeSettings;

    private void Awake()
    {
        ApplySettings();
    }

    private void OnEnable()
    {
        ApplySettings();
    }

    private void OnDisable()
    {
        if (activeSettings != this)
            return;

        activeSettings = null;
        RefreshActiveSettings();
    }

    private void OnDestroy()
    {
        if (activeSettings != this)
            return;

        activeSettings = null;
        RefreshActiveSettings();
    }

    private void ApplySettings()
    {
        activeSettings = this;
        MultiplayerModeEnabled = isMultiplayer;
    }

    private static void RefreshActiveSettings()
    {
        foreach (FmodMultiplayerSettings settings in FindObjectsByType<FmodMultiplayerSettings>(FindObjectsSortMode.None))
        {
            if (settings == activeSettings || !settings.isActiveAndEnabled)
                continue;

            activeSettings = settings;
            MultiplayerModeEnabled = settings.isMultiplayer;
            return;
        }

        MultiplayerModeEnabled = false;
    }
}
#endif
