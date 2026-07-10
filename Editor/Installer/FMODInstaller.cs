using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
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

    internal void ResetSetup()
    {
        setupComplete = false;
        Save(true);
    }
}

[InitializeOnLoad]
public static class FMODInstaller
{
    private const string PackagePath = "Packages/com.bisc8.betterfmod";
    private const string PackageFMODPath = "Runtime/FmodSystem/Plugins_FMOD/CustomFMOD/FMOD";
    private const string InstalledRootPath = "Assets/BISC8/BetterFMOD";
    private const string InstalledFMODPath = InstalledRootPath + "/FMOD";
    private const string InstalledMarkerPath = InstalledFMODPath + "/FMODUnity.asmdef";
    private const string LegacyFMODDefine = "FMOD_PRESENT";
    private const string PopupShownKey = "BISC8_FMOD_POPUP_SHOWN_V2";
    private const string LegacySetupKey = "BISC8_FMOD_SETUP_DONE";

    static FMODInstaller()
    {
        EditorApplication.delayCall += CheckSetup;
    }

    private static void CheckSetup()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            EditorApplication.delayCall += CheckSetup;
            return;
        }

        if (IsSetupComplete())
            return;

        if (SessionState.GetBool(PopupShownKey, false))
            return;

        SessionState.SetBool(PopupShownKey, true);
        ShowSetupDialog();
    }

    [MenuItem("FMOD/BISC8 Better FMOD/Setup", false, 20)]
    public static void RunSetupFromFMODMenu()
    {
        RunSetup();
    }

    private static void ResetSetup()
    {
        FMODInstallerState.instance.ResetSetup();
        EditorPrefs.DeleteKey(LegacySetupKey);
        SessionState.SetBool(PopupShownKey, false);
    }

    [MenuItem("Assets/BISC8 FMOD/Create FMOD List", false, 10)]
    public static void CreateFMODList()
    {
        Type listType = Type.GetType("CreateFmodList, BISC8.BetterFMOD.Runtime");
        if (listType == null || !typeof(ScriptableObject).IsAssignableFrom(listType))
        {
            Debug.LogError("[BISC8 FMOD] CreateFmodList is not available. Check the Unity Console for compilation errors.");
            return;
        }

        const string rootFolder = "Assets/BISC8";
        const string betterFmodFolder = rootFolder + "/BetterFMOD";
        const string listFolder = betterFmodFolder + "/Lists";

        EnsureAssetFolder("Assets", "BISC8");
        EnsureAssetFolder(rootFolder, "BetterFMOD");
        EnsureAssetFolder(betterFmodFolder, "Lists");

        string assetPath = AssetDatabase.GenerateUniqueAssetPath(
            listFolder + "/NewFmodList.asset"
        );

        ScriptableObject list = ScriptableObject.CreateInstance(listType);
        AssetDatabase.CreateAsset(list, assetPath);
        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = list;
        EditorGUIUtility.PingObject(list);
    }

    private static void ShowSetupDialog()
    {
        bool install = EditorUtility.DisplayDialog(
            "BISC8 Better FMOD",
            "FMOD is provided directly by the BetterFMOD package. Run setup compatibility checks?",
            "Run Setup",
            "Not now"
        );

        if (install)
            RunSetup();
    }

    private static void RunSetup()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            Debug.LogWarning("[BISC8 FMOD] Wait for Unity to finish compiling before running setup.");
            return;
        }

        string hiddenSourcePath = GetHiddenFMODSourcePath();
        string activeSourcePath = GetActiveFMODSourcePath();

        if (hiddenSourcePath == null && activeSourcePath == null)
        {
            Debug.LogError("[BISC8 FMOD] FMOD source folder was not found in the package. Package root: " + GetPackageRootPath());
            return;
        }

        try
        {
            if (activeSourcePath != null)
            {
                RemoveLegacyFMODDefine();
                MarkSetupComplete();
                Debug.Log("[BISC8 FMOD] Setup complete. FMOD is provided directly by the BetterFMOD package.");
                return;
            }

            string sourcePath = hiddenSourcePath ?? activeSourcePath;
            MoveFMODToAssets(sourcePath);

            if (!File.Exists(InstalledMarkerPath))
                throw new IOException("FMODUnity.asmdef was not installed in Assets.");

            RemoveLegacyFMODDefine();
            MarkSetupComplete();
            AssetDatabase.Refresh();

            Debug.Log("[BISC8 FMOD] Setup complete. FMOD was moved to Assets/BISC8/BetterFMOD/FMOD.");
        }
        catch (Exception exception)
        {
            Debug.LogError("[BISC8 FMOD] Setup failed: " + exception.Message);
        }
    }

    private static bool IsSetupComplete()
    {
        string activeSourcePath = GetActiveFMODSourcePath();
        if (activeSourcePath != null && File.Exists(Path.Combine(activeSourcePath, "FMODUnity.asmdef")))
        {
            RemoveLegacyFMODDefine();

            if (!FMODInstallerState.instance.SetupComplete)
                MarkSetupComplete();

            return true;
        }

        if (!File.Exists(InstalledMarkerPath))
            return false;

        RemoveLegacyFMODDefine();

        if (FMODInstallerState.instance.SetupComplete)
            return true;

        MarkSetupComplete();
        return true;
    }

    private static void MarkSetupComplete()
    {
        FMODInstallerState.instance.MarkSetupComplete();
        EditorPrefs.SetBool(LegacySetupKey, true);
    }

    private static void EnsureAssetFolder(string parent, string name)
    {
        string path = parent + "/" + name;
        if (!AssetDatabase.IsValidFolder(path))
            AssetDatabase.CreateFolder(parent, name);
    }

    private static void RemoveLegacyFMODDefine()
    {
        BuildTargetGroup targetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
        if (targetGroup == BuildTargetGroup.Unknown)
            return;

        NamedBuildTarget namedTarget = NamedBuildTarget.FromBuildTargetGroup(targetGroup);
        string defines = PlayerSettings.GetScriptingDefineSymbols(namedTarget);
        string[] symbols = defines.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
        string updatedDefines = string.Empty;
        bool removedLegacyDefine = false;

        foreach (string symbol in symbols)
        {
            string trimmedSymbol = symbol.Trim();
            if (trimmedSymbol == LegacyFMODDefine)
            {
                removedLegacyDefine = true;
                continue;
            }

            if (string.IsNullOrWhiteSpace(trimmedSymbol))
                continue;

            updatedDefines = string.IsNullOrWhiteSpace(updatedDefines)
                ? trimmedSymbol
                : updatedDefines + ";" + trimmedSymbol;
        }

        if (removedLegacyDefine)
            PlayerSettings.SetScriptingDefineSymbols(namedTarget, updatedDefines);
    }

    private static string GetHiddenFMODSourcePath()
    {
        string packageRoot = GetPackageRootPath();
        string hiddenSource = Path.Combine(packageRoot, PackageFMODPath + "~");

        return Directory.Exists(hiddenSource) ? hiddenSource : null;
    }

    private static string GetActiveFMODSourcePath()
    {
        string packageRoot = GetPackageRootPath();
        string currentSource = Path.Combine(packageRoot, PackageFMODPath);

        return Directory.Exists(currentSource) ? currentSource : null;
    }

    private static string GetPackageRootPath()
    {
        // Try with package.json which is more reliable than the bare folder path
        var packageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssetPath(
            "Packages/com.bisc8.betterfmod/package.json");

        if (packageInfo == null)
            packageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssetPath(PackagePath);

        if (packageInfo != null && !string.IsNullOrEmpty(packageInfo.resolvedPath))
            return packageInfo.resolvedPath;

        // Scan Library/PackageCache as a fallback (handles git packages)
        string projectRoot = Path.GetFullPath(".");
        string packageCacheDir = Path.Combine(projectRoot, "Library", "PackageCache");
        if (Directory.Exists(packageCacheDir))
        {
            foreach (string dir in Directory.GetDirectories(packageCacheDir, "com.bisc8.betterfmod*"))
                return dir;
        }

        return Path.GetFullPath(PackagePath);
    }

    private static void MoveFMODToAssets(string sourcePath)
    {
        EnsureAssetFolder("Assets", "BISC8");
        EnsureAssetFolder("Assets/BISC8", "BetterFMOD");

        // Always copy (never move) so the package cache source stays intact for future setups
        CopyDirectory(sourcePath, InstalledFMODPath);

        // Copy the .meta file if present alongside the source folder
        string sourceMetaPath = sourcePath + ".meta";
        if (File.Exists(sourceMetaPath))
        {
            string destinationMetaPath = InstalledFMODPath + ".meta";
            File.Copy(sourceMetaPath, destinationMetaPath, true);
        }
    }

    private static void DeleteDirectory(string path)
    {
        foreach (string file in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
            File.SetAttributes(file, FileAttributes.Normal);

        Directory.Delete(path, true);
    }

    private static void CopyDirectory(string sourcePath, string destinationPath)
    {
        Directory.CreateDirectory(destinationPath);

        foreach (string directory in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
        {
            string relativePath = GetRelativePath(sourcePath, directory);
            Directory.CreateDirectory(Path.Combine(destinationPath, relativePath));
        }

        foreach (string file in Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories))
        {
            string relativePath = GetRelativePath(sourcePath, file);
            string destinationFile = Path.Combine(destinationPath, relativePath);

            Directory.CreateDirectory(Path.GetDirectoryName(destinationFile));
            File.Copy(file, destinationFile, true);
        }
    }

    private static string GetRelativePath(string rootPath, string path)
    {
        return path.Substring(rootPath.Length)
            .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }
}
