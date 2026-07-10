using System;
using UnityEngine;

public class FmodMultiplayerSettings : MonoBehaviour
{
    [SerializeField] private bool isMultiplayer;
    [SerializeField] private bool autoConfigureFmodButtons = true;
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
        ApplySettings();
    }

    private void OnEnable()
    {
        ApplySettings();
    }

    private void OnValidate()
    {
#if UNITY_EDITOR
        if (!isMultiplayer || Application.isPlaying)
            return;

        UnityEditor.EditorApplication.delayCall += () =>
        {
            if (this != null)
                EnsureAutomaticTransport();
        };
#endif
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

        if (isMultiplayer)
            EnsureAutomaticTransport();

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

    public void ApplySettingsNow()
    {
        ApplySettings();
    }

    private void EnsureAutomaticTransport()
    {
        if (FmodCommands.MultiplayerTransport != null)
            return;

        Type transportType = FindType("FmodUnityNetcodeTransport");
        if (transportType == null || !typeof(Component).IsAssignableFrom(transportType))
            return;

        Component existingTransport = UnityEngine.Object.FindObjectOfType(transportType) as Component;
        if (existingTransport is IFmodMultiplayerTransport existingFmodTransport)
        {
            FmodCommands.MultiplayerTransport = existingFmodTransport;
            return;
        }

        GameObject transportObject = gameObject;
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            transportObject = GameObject.Find("BetterFMOD_NetcodeRelay");
            if (transportObject == null)
            {
                transportObject = new GameObject("BetterFMOD_NetcodeRelay");
                UnityEditor.Undo.RegisterCreatedObjectUndo(transportObject, "Apply FMOD Multiplayer Settings");
            }
        }
#endif

        Type networkObjectType = FindType("Unity.Netcode.NetworkObject");
        if (networkObjectType != null && typeof(Component).IsAssignableFrom(networkObjectType) && transportObject.GetComponent(networkObjectType) == null)
            AddComponent(transportObject, networkObjectType);

        Component transport = transportObject.GetComponent(transportType);
        if (transport == null)
            transport = AddComponent(transportObject, transportType);

        if (transport is IFmodMultiplayerTransport fmodTransport)
            FmodCommands.MultiplayerTransport = fmodTransport;
    }

    private static Component AddComponent(GameObject target, Type componentType)
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
            return UnityEditor.Undo.AddComponent(target, componentType);
#endif

        return target.AddComponent(componentType);
    }

    private static Type FindType(string typeName)
    {
        foreach (System.Reflection.Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type type = assembly.GetType(typeName);
            if (type != null)
                return type;
        }

        return null;
    }
}
