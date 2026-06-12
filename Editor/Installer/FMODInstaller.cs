using UnityEditor;
using UnityEngine;

[FilePath(StatePath, FilePathAttribute.Location.ProjectFolder)]
internal sealed class FMODInstallerState : ScriptableSingleton<FMODInstallerState>
{
    internal const string StatePath = "UserSettings/BISC8BetterFMODInstaller.asset";

    [SerializeField]
    private bool setupComplete;

    internal bool SetupComplete => setupComplete;

    internal void MarkSetupComplete()
    {
        setupComplete = true;
        Save(true);
    }
}

[InitializeOnLoad]
public static class FMODInstaller
{
    private const string LegacyHasSetupKey = "BISC8_FMOD_SETUP_DONE";
    private const string PopupShownKey = "BISC8_FMOD_POPUP_SHOWN";
    private const string SetupFolder = "Assets/BISC8/BetterFMOD";
    private const string SetupAssetPath = SetupFolder + "/FMODSystem.asset";

    static FMODInstaller()
    {
        EditorApplication.delayCall += CheckSetup;
    }

    static void CheckSetup()
    {
        if (IsSetupComplete())
            return;

        if (SessionState.GetBool(PopupShownKey, false))
            return;

        SessionState.SetBool(PopupShownKey, true);
        ShowSetupDialog();
    }

    static bool IsSetupComplete()
    {
        if (FMODInstallerState.instance.SetupComplete)
            return true;

        // Migrate installations completed by older package versions.
        if (EditorPrefs.GetBool(LegacyHasSetupKey, false) ||
            AssetDatabase.LoadAssetAtPath<FMODSystem>(SetupAssetPath) != null)
        {
            MarkSetupComplete();
            return true;
        }

        return false;
    }

    static void ShowSetupDialog()
    {
        bool create = EditorUtility.DisplayDialog(
            "BISC8 Better FMOD",
            "BISC8 FMOD recommends creating the assets at this time.\n\nCreate FMODSystem assets now?",
            "Create Assets",
            "Not now"
        );

        if (create)
            CreateAssets();
    }

    [MenuItem("Tools/BISC8 Better FMOD/Run Setup")]
    static void RunSetupManually()
    {
        CreateAssets();
    }

    static void CreateAssets()
    {
        if (!AssetDatabase.IsValidFolder("Assets/BISC8"))
            AssetDatabase.CreateFolder("Assets", "BISC8");

        if (!AssetDatabase.IsValidFolder(SetupFolder))
            AssetDatabase.CreateFolder("Assets/BISC8", "BetterFMOD");

        var existing = AssetDatabase.LoadAssetAtPath<FMODSystem>(SetupAssetPath);
        if (existing != null)
        {
            Debug.Log("[BISC8 FMOD] Already exists.");
            MarkSetupComplete();
            return;
        }

        var asset = ScriptableObject.CreateInstance<FMODSystem>();
        AssetDatabase.CreateAsset(asset, SetupAssetPath);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        if (AssetDatabase.LoadAssetAtPath<FMODSystem>(SetupAssetPath) == null)
        {
            Debug.LogError("[BISC8 FMOD] Setup failed to create FMODSystem.asset.");
            return;
        }

        MarkSetupComplete();
        EditorPrefs.SetString("BISC8_FMOD_PATH", SetupAssetPath);

        Debug.Log("[BISC8 FMOD] Setup complete.");
    }

    static void MarkSetupComplete()
    {
        FMODInstallerState.instance.MarkSetupComplete();
        EditorPrefs.SetBool(LegacyHasSetupKey, true);
    }
}
