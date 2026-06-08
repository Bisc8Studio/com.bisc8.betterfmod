using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using System;
using System.IO;

[InitializeOnLoad]
public static class FMODInstaller
{
    private const string HasSetupKey = "BISC8_FMOD_SETUP_DONE";

    private const string PackagePath = "Packages/com.bisc8.betterfmod";
    private const string PackageFMODRelativePath = "Runtime/FmodSystem/Plugins_FMOD/CustomFMOD/FMOD~";

    private const string InstalledFMODPath = "Assets/BISC8/BetterFMOD/FMOD";
    private const string InstalledMarkerFile = "Assets/BISC8/BetterFMOD/.installed";

    private const string InstalledDefine = "BISC8_BETTERFMOD_INSTALLED";

    static FMODInstaller()
    {
        EditorApplication.delayCall += CheckSetup;
    }

    static void CheckSetup()
    {
        if (EditorPrefs.GetBool(HasSetupKey, false))
            return;

        if (File.Exists(InstalledMarkerFile))
        {
            EditorPrefs.SetBool(HasSetupKey, true);
            return;
        }

        ShowSetupDialog();
    }

    [MenuItem("FMOD/BISC8/Install FMOD In Assets")]
    public static void InstallFMODInAssets()
    {
        RunSetup();
    }

    static void ShowSetupDialog()
    {
        bool create = EditorUtility.DisplayDialog(
            "BISC8 Better FMOD",
            "BISC8 FMOD needs to install FMOD assets into your project.\n\nThis will create the folder structure in Assets/BISC8/BetterFMOD/FMOD.",
            "Setup",
            "Not now"
        );

        if (create)
            RunSetup();
    }

    static void RunSetup()
    {
        CreateFolders();
        CreateFMODFolders();
        CopyFMOD();

        AssetDatabase.Refresh();

        EnableInstalledDefine();
        MarkInstalled();

        EditorPrefs.SetBool(HasSetupKey, true);

        Debug.Log("[BISC8 FMOD] Setup complete.");
    }

    static void MarkInstalled()
    {
        Directory.CreateDirectory("Assets/BISC8/BetterFMOD");
        File.WriteAllText(InstalledMarkerFile, "installed");
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
        Directory.CreateDirectory(Path.Combine(InstalledFMODPath, "Resources"));
        Directory.CreateDirectory(Path.Combine(InstalledFMODPath, "Cache/Editor"));
    }

    static void CopyFMOD()
    {
        string source = GetPackageFMODFullPath();
        string dest = InstalledFMODPath;

        if (!Directory.Exists(source))
        {
            Debug.LogError("[BISC8 FMOD] Package FMOD folder not found: " + source);
            return;
        }

        CopyDirectory(source, dest);
    }

    static string GetPackageFMODFullPath()
    {
        return Path.Combine(GetPackageRootFullPath(), PackageFMODRelativePath);
    }

    static string GetPackageRootFullPath()
    {
        var packageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssetPath(PackagePath);

        if (packageInfo != null && !string.IsNullOrEmpty(packageInfo.resolvedPath))
            return packageInfo.resolvedPath;

        return Path.GetFullPath(PackagePath);
    }

    static void CopyDirectory(string source, string dest)
    {
        Directory.CreateDirectory(dest);

        foreach (string dir in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
        {
            string relative = dir.Substring(source.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            Directory.CreateDirectory(Path.Combine(dest, relative));
        }

        foreach (string file in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
        {
            string relative = file.Substring(source.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            string target = Path.Combine(dest, relative);

            File.Copy(file, target, true);
        }
    }

    static void EnableInstalledDefine()
    {
        BuildTargetGroup group = EditorUserBuildSettings.selectedBuildTargetGroup;
        NamedBuildTarget named = NamedBuildTarget.FromBuildTargetGroup(group);

        string defines = PlayerSettings.GetScriptingDefineSymbols(named);

        if (defines.Contains(InstalledDefine))
            return;

        defines = string.IsNullOrWhiteSpace(defines)
            ? InstalledDefine
            : defines + ";" + InstalledDefine;

        PlayerSettings.SetScriptingDefineSymbols(named, defines);
    }
}