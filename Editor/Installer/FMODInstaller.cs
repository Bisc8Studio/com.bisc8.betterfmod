using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

[FilePath(StatePath, FilePathAttribute.Location.ProjectFolder)]
internal sealed class FMODInstallerState : ScriptableSingleton<FMODInstallerState>
{
    internal const string StatePath = "UserSettings/FMODB8Installer.asset";

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
    private const string PackagePath = "Packages/com.bisc8.simplefmod";
    private const string PackageFMODPath = "Runtime/FmodSystem/Plugins_FMOD/CustomFMOD/FMOD";
    private const string InstalledRootPath = "Assets/BISC8/FMODB8";
    private const string InstalledFMODPath = InstalledRootPath + "/FMOD";
    private const string InstalledMarkerPath = InstalledFMODPath + "/FMODUnity.asmdef";
    private const string LegacyFMODDefine = "FMOD_PRESENT";
    private const string PopupShownKey = "BISC8_FMOD_POPUP_SHOWN_V2";
    private const string LegacyCopyPopupShownKey = "BISC8_FMOD_LEGACY_COPY_POPUP_SHOWN_V1";
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

        if (PromptRemoveLegacyInstalledFMODCopy(true))
            return;

        if (IsSetupComplete())
            return;

        if (SessionState.GetBool(PopupShownKey, false))
            return;

        if (!SessionState.GetBool(PopupShownKey, false))
        {
            SessionState.SetBool(PopupShownKey, true);
            Debug.LogWarning("[FMODB8] FMOD source was not found. Use FMOD/FMODB8/Setup if this package was imported from an older hidden-FMOD layout.");
        }
    }

    [MenuItem("FMOD/FMODB8/Setup", false, 20)]
    public static void RunSetupFromFMODMenu()
    {
        RunSetup();
    }

    [MenuItem("FMOD/FMODB8/Remove Legacy Assets FMOD Copy", false, 21)]
    public static void RemoveLegacyInstalledFMODCopyFromMenu()
    {
        RemoveLegacyInstalledFMODCopy(true);
    }

    private static void ResetSetup()
    {
        FMODInstallerState.instance.ResetSetup();
        EditorPrefs.DeleteKey(LegacySetupKey);
        SessionState.SetBool(PopupShownKey, false);
    }

    [MenuItem("Assets/FMODB8/Create FMOD List", false, 10)]
    public static void CreateFMODList()
    {
        Type listType = Type.GetType("CreateFmodList, BISC8.FMODB8.Runtime");
        if (listType == null || !typeof(ScriptableObject).IsAssignableFrom(listType))
        {
            Debug.LogError("[FMODB8] CreateFmodList is not available. Check the Unity Console for compilation errors.");
            return;
        }

        const string rootFolder = "Assets/BISC8";
        const string fmodb8Folder = rootFolder + "/FMODB8";
        const string listFolder = fmodb8Folder + "/Lists";

        EnsureAssetFolder("Assets", "BISC8");
        EnsureAssetFolder(rootFolder, "FMODB8");
        EnsureAssetFolder(fmodb8Folder, "Lists");

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

    private static bool PromptRemoveLegacyInstalledFMODCopy(bool respectSessionState)
    {
        string activeSourcePath = GetActiveFMODSourcePath();
        if (activeSourcePath == null || !File.Exists(Path.Combine(activeSourcePath, "FMODUnity.asmdef")))
            return false;

        if (!File.Exists(InstalledMarkerPath))
            return false;

        if (respectSessionState && SessionState.GetBool(LegacyCopyPopupShownKey, false))
            return false;

        SessionState.SetBool(LegacyCopyPopupShownKey, true);

        bool remove = EditorUtility.DisplayDialog(
            "FMODB8",
            "A legacy FMOD copy exists at Assets/BISC8/FMODB8/FMOD while the package also provides FMOD. This causes duplicate native plugin errors. Remove the legacy Assets copy?",
            "Remove Legacy Copy",
            "Not now"
        );

        if (!remove)
            return false;

        RemoveLegacyInstalledFMODCopy(true);
        return true;
    }

    public static bool RemoveLegacyInstalledFMODCopy(bool refresh)
    {
        if (!Directory.Exists(InstalledFMODPath) && !File.Exists(InstalledFMODPath + ".meta"))
        {
            Debug.Log("[FMODB8] No legacy FMOD copy found at Assets/BISC8/FMODB8/FMOD.");
            return true;
        }

        bool removedFolder = DeleteFileOrDirectory(InstalledFMODPath);
        bool removedMeta = DeleteFileOrDirectory(InstalledFMODPath + ".meta");

        if (refresh)
            AssetDatabase.Refresh();

        if (removedFolder && removedMeta)
        {
            Debug.Log("[FMODB8] Removed legacy FMOD copy from Assets/BISC8/FMODB8/FMOD.");
            return true;
        }

        Debug.LogWarning(
            "[FMODB8] Could not fully remove the legacy FMOD copy. Close Unity if Windows is locking a native DLL, then delete Assets/BISC8/FMODB8/FMOD manually.");
        return false;
    }

    private static bool DeleteFileOrDirectory(string path)
    {
        if (!Directory.Exists(path) && !File.Exists(path))
            return true;

        try
        {
            ClearReadOnlyAttributes(path);
            FileUtil.DeleteFileOrDirectory(path);
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogWarning("[FMODB8] Could not remove '" + path + "': " + exception.Message);
            return false;
        }
    }

    private static void ClearReadOnlyAttributes(string path)
    {
        if (File.Exists(path))
        {
            File.SetAttributes(path, FileAttributes.Normal);
            return;
        }

        if (!Directory.Exists(path))
            return;

        foreach (string file in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
            File.SetAttributes(file, FileAttributes.Normal);
    }

    private static void RunSetup()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            Debug.LogWarning("[FMODB8] Wait for Unity to finish compiling before running setup.");
            return;
        }

        string hiddenSourcePath = GetHiddenFMODSourcePath();
        string activeSourcePath = GetActiveFMODSourcePath();

        if (hiddenSourcePath == null && activeSourcePath == null)
        {
            Debug.LogError("[FMODB8] FMOD source folder was not found in the package. Package root: " + GetPackageRootPath());
            return;
        }

        try
        {
            if (activeSourcePath != null)
            {
                RemoveLegacyFMODDefine();
                PromptRemoveLegacyInstalledFMODCopy(false);
                MarkSetupComplete();
                AssetDatabase.Refresh();
                Debug.Log("[FMODB8] Setup complete. FMOD source is active in the FMODB8 package.");
                return;
            }

            string sourcePath = hiddenSourcePath ?? activeSourcePath;
            MoveFMODToAssets(sourcePath);

            if (!File.Exists(InstalledMarkerPath))
                throw new IOException("FMODUnity.asmdef was not installed in Assets.");

            RemoveLegacyFMODDefine();
            MarkSetupComplete();
            AssetDatabase.Refresh();

            Debug.Log("[FMODB8] Setup complete. FMOD was moved to Assets/BISC8/FMODB8/FMOD.");
        }
        catch (Exception exception)
        {
            Debug.LogError("[FMODB8] Setup failed: " + exception.Message);
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
            "Packages/com.bisc8.simplefmod/package.json");

        if (packageInfo == null)
            packageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssetPath(PackagePath);

        if (packageInfo != null && !string.IsNullOrEmpty(packageInfo.resolvedPath))
            return packageInfo.resolvedPath;

        // Scan Library/PackageCache as a fallback (handles git packages)
        string projectRoot = Path.GetFullPath(".");
        string packageCacheDir = Path.Combine(projectRoot, "Library", "PackageCache");
        if (Directory.Exists(packageCacheDir))
        {
            foreach (string dir in Directory.GetDirectories(packageCacheDir, "com.bisc8.simplefmod*"))
                return dir;
        }

        return Path.GetFullPath(PackagePath);
    }

    private static void MoveFMODToAssets(string sourcePath)
    {
        EnsureAssetFolder("Assets", "BISC8");
        EnsureAssetFolder("Assets/BISC8", "FMODB8");

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
