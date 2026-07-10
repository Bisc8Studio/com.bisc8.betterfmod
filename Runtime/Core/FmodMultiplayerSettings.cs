using UnityEngine;

public class FmodMultiplayerSettings : MonoBehaviour
{
    [SerializeField] private bool isMultiplayer;
    [SerializeField] private bool autoConfigureFmodButtons = true;
    [SerializeField] private bool dontDestroyOnLoad = true;
    [SerializeField, HideInInspector] private bool playLocalWhenTransportMissing;

    /// <summary>
    /// Retorna verdadeiro quando este componente habilita o modo multiplayer do BetterFMOD.
    /// </summary>
    public bool IsMultiplayer => isMultiplayer;

    public bool AutoConfigureFmodButtons => autoConfigureFmodButtons;

    /// <summary>
    /// Retorna verdadeiro quando o modo multiplayer do BetterFMOD esta ativo.
    /// </summary>
    public static bool MultiplayerModeEnabled { get; private set; }

    public static bool PlayLocalWhenTransportMissing { get; private set; }

    private static FmodMultiplayerSettings activeSettings;

    private void Awake()
    {
        if (dontDestroyOnLoad)
        {
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }

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
        PlayLocalWhenTransportMissing = playLocalWhenTransportMissing;

        if (isMultiplayer && autoConfigureFmodButtons)
            ApplyToSceneButtons();
    }

    private static void RefreshActiveSettings()
    {
        foreach (FmodMultiplayerSettings settings in FindObjectsByType<FmodMultiplayerSettings>(FindObjectsSortMode.None))
        {
            if (settings == activeSettings || !settings.isActiveAndEnabled)
                continue;

            activeSettings = settings;
            MultiplayerModeEnabled = settings.isMultiplayer;
            PlayLocalWhenTransportMissing = settings.playLocalWhenTransportMissing;
            return;
        }

        MultiplayerModeEnabled = false;
        PlayLocalWhenTransportMissing = false;
    }

    public void ApplyToSceneButtons()
    {
        foreach (FmodButton button in FindObjectsByType<FmodButton>(FindObjectsSortMode.None))
        {
            if (button == null)
                continue;

            button.ApplyMultiplayerDefaults(isMultiplayer);
        }
    }

}
