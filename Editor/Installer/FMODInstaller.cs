using UnityEditor;
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
    private const string InstalledDefine = "BISC8_BETTERFMOD_INSTALLED";

    private static bool _checked;

    static FMODInstaller()
    {
        EditorApplication.delayCall += CheckSetup;
    }

    static void CheckSetup()
    {
        if (_checked)
            return;

        _checked = true;

        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            return;

        if (EditorPrefs.GetBool(HasSetupKey, false))
            return;

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
            "Install FMOD assets into Assets/BISC8/BetterFMOD?",
            "Setup",
            "Not now"
        );

        if (create)
            RunSetup();
    }

    static void RunSetup()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            return;

        CreateFolders();

        if (!CopyFMOD())
        {
            EditorPrefs.DeleteKey(HasSetupKey);
            return;
        }

        CreateFMODFolders();

        AssetDatabase.Refresh();

        EnableInstalledDefine();

        EditorPrefs.SetBool(HasSetupKey, true);
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

    static bool CopyFMOD()
    {
        string source = GetPackageFMODFullPath();
        string dest = InstalledFMODPath;

        if (!Directory.Exists(source))
        {
            Debug.LogError($"[BISC8 FMOD] Missing source: {source}");
            return false;
        }

        CopyDirectory(source, dest);
        return true;
    }

    static string GetPackageFMODFullPath()
    {
        string root = GetPackageRootFullPath();
        return Path.Combine(root, PackageFMODRelativePath);
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

        foreach (string directory in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
        {
            string relativePath = directory.Substring(source.Length)
                .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            Directory.CreateDirectory(Path.Combine(dest, relativePath));
        }

        foreach (string file in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
        {
            string relativePath = file.Substring(source.Length)
                .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            File.Copy(file, Path.Combine(dest, relativePath), true);
        }
    }

    static void EnableInstalledDefine()
    {
        BuildTargetGroup group = EditorUserBuildSettings.selectedBuildTargetGroup;
        string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);

        if (!defines.Contains(InstalledDefine))
        {
            defines = string.IsNullOrWhiteSpace(defines)
                ? InstalledDefine
                : $"{defines};{InstalledDefine}";

            PlayerSettings.SetScriptingDefineSymbolsForGroup(group, defines);
        }
    }
}