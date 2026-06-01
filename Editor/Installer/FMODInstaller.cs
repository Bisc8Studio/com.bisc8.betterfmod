using UnityEditor;
using UnityEngine;
using System.IO;

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
            "BISC8 FMOD needs to install FMOD assets into your project.\n\nThis will create the folder structure in Assets/BISC8/BetterFMOD/.",
            "Setup",
            "Not now"
        );

        if (create)
            RunSetup();
    }

    static void RunSetup()
    {
        CreateFolders();
        CopyFMOD();
        CreateFMODFolders();
        CopyPrefabs();

        AssetDatabase.Refresh();

        EditorPrefs.SetBool(HasSetupKey, true);

        Debug.Log("[BISC8 FMOD] Setup complete. FMOD installed in Assets/BISC8/BetterFMOD/FMOD/");
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

    static void CreateFMODFolders()
    {
        if (!AssetDatabase.IsValidFolder("Assets/BISC8/BetterFMOD/FMOD"))
            AssetDatabase.CreateFolder("Assets/BISC8/BetterFMOD", "FMOD");

        if (!AssetDatabase.IsValidFolder("Assets/BISC8/BetterFMOD/FMOD/Resources"))
            AssetDatabase.CreateFolder("Assets/BISC8/BetterFMOD/FMOD", "Resources");

        if (!AssetDatabase.IsValidFolder("Assets/BISC8/BetterFMOD/FMOD/Cache"))
            AssetDatabase.CreateFolder("Assets/BISC8/BetterFMOD/FMOD", "Cache");

        if (!AssetDatabase.IsValidFolder("Assets/BISC8/BetterFMOD/FMOD/Cache/Editor"))
            AssetDatabase.CreateFolder("Assets/BISC8/BetterFMOD/FMOD/Cache", "Editor");
    }

    static void CopyFMOD()
    {
        string source = $"{PackagePath}/Runtime/FmodSystem/Plugins_FMOD/CustomFMOD/FMOD~";
        string dest = "Assets/BISC8/BetterFMOD/FMOD";

        if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>($"{dest}/FMODUnity.asmdef") != null)
        {
            Debug.Log("[BISC8 FMOD] FMOD folder already installed, skipping copy.");
            return;
        }

        CopyDirectory(source, dest);
    }

    static void CopyDirectory(string source, string dest)
    {
        Directory.CreateDirectory(dest);

        foreach (string directory in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
        {
            string relativePath = directory.Substring(source.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            Directory.CreateDirectory(Path.Combine(dest, relativePath));
        }

        foreach (string file in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
        {
            string relativePath = file.Substring(source.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            string target = Path.Combine(dest, relativePath);

            if (File.Exists(target))
                continue;

            File.Copy(file, target);
        }
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
