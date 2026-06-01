using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class FMODInstaller
{
    private const string HasSetupKey = "BISC8_FMOD_SETUP_DONE";
    private const string PackagePath = "Packages/com.bisc8.betterfmod";

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
            "BISC8 FMOD needs to copy its assets to your project.\n\nThis will create the folder structure and copy prefabs to Assets/BISC8/BetterFMOD/.",
            "Setup",
            "Not now"
        );

        if (create)
            RunSetup();
    }

    static void RunSetup()
    {
        CreateFolders();
        CopyPrefabs();

        AssetDatabase.Refresh();

        EditorPrefs.SetBool(HasSetupKey, true);

        Debug.Log("[BISC8 FMOD] Setup complete. Assets copied to Assets/BISC8/BetterFMOD/");
    }

    static void CreateFolders()
    {
        if (!AssetDatabase.IsValidFolder("Assets/BISC8"))
            AssetDatabase.CreateFolder("Assets", "BISC8");

        if (!AssetDatabase.IsValidFolder("Assets/BISC8/BetterFMOD"))
            AssetDatabase.CreateFolder("Assets/BISC8", "BetterFMOD");

        if (!AssetDatabase.IsValidFolder("Assets/BISC8/BetterFMOD/Lists"))
            AssetDatabase.CreateFolder("Assets/BISC8/BetterFMOD", "Lists");
    }

    static void CopyPrefabs()
    {
        string source = $"{PackagePath}/Runtime/FmodSystem/Prefabs_FMOD";
        string dest = "Assets/BISC8/BetterFMOD/Prefabs";

        if (AssetDatabase.IsValidFolder(dest))
        {
            Debug.Log("[BISC8 FMOD] Prefabs folder already exists, skipping copy.");
            return;
        }

        FileUtil.CopyFileOrDirectory(source, dest);
    }
}