using UnityEditor;
using UnityEngine;
using System;
using System.IO;

[InitializeOnLoad]
public static class FMODInstaller
{
    private const string HasSetupKey = "BISC8_FMOD_SETUP_DONE";
    private const string PackagePath = "Packages/com.bisc8.betterfmod";
    private const string PackageFMODRelativePath = "Runtime/FmodSystem/Plugins_FMOD/CustomFMOD/FMOD";
    private const string InstalledFMODPath = "Assets/BISC8/BetterFMOD/FMOD";
    private const string InstalledFMODMarker = InstalledFMODPath + "/FMODUnity.asmdef";

    static FMODInstaller()
    {
        EditorApplication.delayCall += CheckSetup;
    }

    static void CheckSetup()
    {
        if (SessionState.GetBool("BISC8_FMOD_POPUP_SHOWN", false))
            return;

        SessionState.SetBool("BISC8_FMOD_POPUP_SHOWN", true);

        if (EditorPrefs.GetBool(HasSetupKey, false) && IsFMODInstalledOnlyInAssets())
            return;

        ShowSetupDialog();
    }

    [MenuItem("BISC8 FMOD/Install FMOD In Assets")]
    public static void InstallFMODInAssets()
    {
        RunSetup();
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
        DeletePackageFMOD();

        AssetDatabase.Refresh();

        bool installedOnlyInAssets = IsFMODInstalledOnlyInAssets();
        EditorPrefs.SetBool(HasSetupKey, installedOnlyInAssets);

        if (installedOnlyInAssets)
            Debug.Log("[BISC8 FMOD] Setup complete. FMOD installed only in Assets/BISC8/BetterFMOD/FMOD/");
        else
            Debug.LogWarning("[BISC8 FMOD] Setup copied FMOD to Assets, but the package copy still exists. Remove the package FMOD folder to avoid duplicates.");
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
        Directory.CreateDirectory(InstalledFMODPath);
        Directory.CreateDirectory(InstalledFMODPath + "/Resources");
        Directory.CreateDirectory(InstalledFMODPath + "/Cache/Editor");
    }

    static void CopyFMOD()
    {
        string source = GetPackageFMODFullPath();
        string dest = InstalledFMODPath;

        if (!Directory.Exists(source))
        {
            Debug.LogError($"[BISC8 FMOD] Package FMOD folder not found: {source}");
            return;
        }

        CopyDirectory(source, dest);
        Debug.Log("[BISC8 FMOD] FMOD folder synchronized to Assets/BISC8/BetterFMOD/FMOD.");
    }

    static void DeletePackageFMOD()
    {
        string source = GetPackageFMODFullPath();
        string sourceMeta = $"{source}.meta";

        if (!File.Exists(InstalledFMODMarker))
        {
            Debug.LogWarning("[BISC8 FMOD] FMOD was not removed from Packages because the Assets installation was not found.");
            return;
        }

        try
        {
            if (Directory.Exists(source))
                Directory.Delete(source, true);

            if (File.Exists(sourceMeta))
                File.Delete(sourceMeta);
        }
        catch (IOException exception)
        {
            Debug.LogWarning($"[BISC8 FMOD] Could not remove package FMOD folder: {exception.Message}");
        }
        catch (UnauthorizedAccessException exception)
        {
            Debug.LogWarning($"[BISC8 FMOD] Could not remove package FMOD folder: {exception.Message}");
        }

        if (Directory.Exists(source))
            Debug.LogWarning("[BISC8 FMOD] Could not remove FMOD from Packages. Remove it manually to avoid duplicate FMOD installations.");
    }

    static bool IsFMODInstalledOnlyInAssets()
    {
        return File.Exists(InstalledFMODMarker) && !Directory.Exists(GetPackageFMODFullPath());
    }

    static string GetPackageFMODFullPath()
    {
        return Path.Combine(GetPackageRootFullPath(), PackageFMODRelativePath);
    }

    static string GetPackageRootFullPath()
    {
        UnityEditor.PackageManager.PackageInfo packageInfo =
            UnityEditor.PackageManager.PackageInfo.FindForAssetPath(PackagePath);
        if (packageInfo != null && !string.IsNullOrEmpty(packageInfo.resolvedPath))
            return packageInfo.resolvedPath;

        return Path.GetFullPath(PackagePath);
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

            File.Copy(file, target, true);
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
