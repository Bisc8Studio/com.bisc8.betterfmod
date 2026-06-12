using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class FMODInstaller
{
    private const string HasSetupKey = "BISC8_FMOD_SETUP_DONE";

    static FMODInstaller()
    {
        EditorApplication.delayCall += CheckSetup;
    }

    static void CheckSetup()
    {
        if (SessionState.GetBool("BISC8_FMOD_POPUP_SHOWN", false))
            return;

        SessionState.SetBool("BISC8_FMOD_POPUP_SHOWN", true);

        if (EditorPrefs.GetBool(HasSetupKey, false))
            return;

        ShowSetupDialog();
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

    static void CreateAssets()
    {
        if (typeof(FMODUnity.RuntimeManager) == null)
        {
            Debug.LogError("[BISC8 FMOD] FMOD not installed.");
            return;
        }

        string folder = "Assets/BISC8/BetterFMOD";
        string path = folder + "/FMODSystem.asset";

        if (!AssetDatabase.IsValidFolder("Assets/BISC8"))
            AssetDatabase.CreateFolder("Assets", "BISC8");

        if (!AssetDatabase.IsValidFolder(folder))
            AssetDatabase.CreateFolder("Assets/BISC8", "BetterFMOD");

        var existing = AssetDatabase.LoadAssetAtPath<FMODSystem>(path);
        if (existing != null)
        {
            Debug.Log("[BISC8 FMOD] Already exists.");
            EditorPrefs.SetBool(HasSetupKey, true);
            return;
        }

        var asset = ScriptableObject.CreateInstance<FMODSystem>();
        AssetDatabase.CreateAsset(asset, path);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorPrefs.SetBool(HasSetupKey, true);
        EditorPrefs.SetString("BISC8_FMOD_PATH", path);

        Debug.Log("[BISC8 FMOD] Setup complete.");
    }
}