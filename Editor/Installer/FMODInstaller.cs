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
            "BISC8 FMOD recommends creating the default folder structure now.\n\nCreate folders?",
            "Create",
            "Not now"
        );

        if (create)
            CreateFolders();
    }

    static void CreateFolders()
    {
        if (!AssetDatabase.IsValidFolder("Assets/BISC8"))
            AssetDatabase.CreateFolder("Assets", "BISC8");

        if (!AssetDatabase.IsValidFolder("Assets/BISC8/BetterFMOD"))
            AssetDatabase.CreateFolder("Assets/BISC8", "BetterFMOD");

        if (!AssetDatabase.IsValidFolder("Assets/BISC8/BetterFMOD/Lists"))
            AssetDatabase.CreateFolder("Assets/BISC8/BetterFMOD", "Lists");

        AssetDatabase.Refresh();

        EditorPrefs.SetBool(HasSetupKey, true);

        Debug.Log("[BISC8 FMOD] Setup complete. Folders created at Assets/BISC8/BetterFMOD/");
    }
}