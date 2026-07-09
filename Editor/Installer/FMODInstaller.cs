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

    [SerializeField]
    private string installedPackageVersion;

    [SerializeField]
    private string installedPackageRootPath;

    internal bool SetupComplete => setupComplete;

    internal string InstalledPackageVersion => installedPackageVersion;

    internal string InstalledPackageRootPath => installedPackageRootPath;

    internal void MarkSetupComplete(string packageVersion, string packageRootPath)
    {
        setupComplete = true;
        installedPackageVersion = packageVersion;
        installedPackageRootPath = packageRootPath;
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
    private const string FMODDefine = "FMOD_PRESENT";
    private const string PopupShownKey = "BISC8_FMOD_POPUP_SHOWN_V2";
    private const string LegacySetupKey = "BISC8_FMOD_SETUP_DONE";
    private const string UnknownPackageVersion = "unknown";

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

        if (IsSetupCurrent())
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
            "Copy or update FMOD in Assets/BISC8/BetterFMOD/FMOD?",
            "Setup / Update FMOD",
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
            string sourcePath = hiddenSourcePath ?? activeSourcePath;
            bool synchronizedAllFiles = SyncFMODToAssets(sourcePath);

            if (!File.Exists(InstalledMarkerPath))
                throw new IOException("FMODUnity.asmdef was not installed in Assets.");

            EnsureFMODDefine();

            if (synchronizedAllFiles)
                MarkSetupComplete(GetPackageVersion(), GetPackageRootPath());
            else
            {
                FMODInstallerState.instance.ResetSetup();
                EditorPrefs.DeleteKey(LegacySetupKey);
            }

            AssetDatabase.Refresh();

            if (synchronizedAllFiles)
            {
                Debug.Log("[BISC8 FMOD] Setup complete. FMOD files were synchronized to Assets/BISC8/BetterFMOD/FMOD.");
            }
            else
            {
                Debug.LogWarning(
                    "[BISC8 FMOD] Setup partially complete. Editable FMOD files were synchronized, but one or more locked files could not be updated yet.");
            }
        }
        catch (Exception exception)
        {
            Debug.LogError("[BISC8 FMOD] Setup failed: " + exception.Message);
        }
    }

    [MenuItem("FMOD/BISC8 Better FMOD/Force Reinstall", false, 21)]
    public static void ForceReinstallFromFMODMenu()
    {
        ResetSetup();
        RunSetup();
    }

    private static void ResetSetup()
    {
        FMODInstallerState.instance.ResetSetup();
        EditorPrefs.DeleteKey(LegacySetupKey);
        SessionState.SetBool(PopupShownKey, false);
    }

    private static bool TryDeleteFile(string path)
    {
        try
        {
            File.SetAttributes(path, FileAttributes.Normal);
            File.Delete(path);
            return true;
        }
        catch (UnauthorizedAccessException exception)
        {
            LogLockedInstallWarning(path, exception);
            return false;
        }
        catch (IOException exception)
        {
            LogLockedInstallWarning(path, exception);
            return false;
        }
    }

    private static void LogLockedInstallWarning(string path, Exception exception)
    {
        Debug.LogWarning(
            "[BISC8 FMOD] Could not update or remove '" + path + "' because Unity or the OS is still using it. " +
            "Other FMOD files will still be updated. Restart Unity only if this specific native library must be replaced. Details: " + exception.Message);
    }

    private static bool IsSetupCurrent()
    {
        if (!File.Exists(InstalledMarkerPath))
            return false;

        EnsureFMODDefine();

        string packageVersion = GetPackageVersion();
        string packageRootPath = GetPackageRootPath();
        if (FMODInstallerState.instance.SetupComplete &&
            FMODInstallerState.instance.InstalledPackageVersion == packageVersion &&
            FMODInstallerState.instance.InstalledPackageRootPath == packageRootPath)
        {
            return true;
        }

        RunSetup();
        return true;
    }

    private static void MarkSetupComplete(string packageVersion, string packageRootPath)
    {
        FMODInstallerState.instance.MarkSetupComplete(packageVersion, packageRootPath);
        EditorPrefs.SetBool(LegacySetupKey, true);
    }

    private static void EnsureAssetFolder(string parent, string name)
    {
        string path = parent + "/" + name;
        if (!AssetDatabase.IsValidFolder(path))
            AssetDatabase.CreateFolder(parent, name);
    }

    private static void EnsureFMODDefine()
    {
        BuildTargetGroup targetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
        if (targetGroup == BuildTargetGroup.Unknown)
            return;

        NamedBuildTarget namedTarget = NamedBuildTarget.FromBuildTargetGroup(targetGroup);
        string defines = PlayerSettings.GetScriptingDefineSymbols(namedTarget);
        string[] symbols = defines.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (string symbol in symbols)
        {
            if (symbol.Trim() == FMODDefine)
                return;
        }

        string updatedDefines = string.IsNullOrWhiteSpace(defines)
            ? FMODDefine
            : defines.TrimEnd(';') + ";" + FMODDefine;

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

    private static string GetPackageVersion()
    {
        var packageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssetPath(
            "Packages/com.bisc8.betterfmod/package.json");

        if (packageInfo == null)
            packageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssetPath(PackagePath);

        if (packageInfo != null && !string.IsNullOrEmpty(packageInfo.version))
            return packageInfo.version;

        string packageJsonPath = Path.Combine(GetPackageRootPath(), "package.json");
        if (!File.Exists(packageJsonPath))
            return UnknownPackageVersion;

        string packageJson = File.ReadAllText(packageJsonPath);
        const string versionToken = "\"version\"";
        int versionIndex = packageJson.IndexOf(versionToken, StringComparison.Ordinal);
        if (versionIndex < 0)
            return UnknownPackageVersion;

        int colonIndex = packageJson.IndexOf(':', versionIndex);
        int firstQuoteIndex = packageJson.IndexOf('"', colonIndex + 1);
        int secondQuoteIndex = packageJson.IndexOf('"', firstQuoteIndex + 1);

        if (colonIndex < 0 || firstQuoteIndex < 0 || secondQuoteIndex < 0)
            return UnknownPackageVersion;

        return packageJson.Substring(firstQuoteIndex + 1, secondQuoteIndex - firstQuoteIndex - 1);
    }

    private static bool SyncFMODToAssets(string sourcePath)
    {
        EnsureAssetFolder("Assets", "BISC8");
        EnsureAssetFolder("Assets/BISC8", "BetterFMOD");

        bool copiedAllFiles = CopyDirectory(sourcePath, InstalledFMODPath);
        bool deletedStaleFiles = DeleteFilesMissingFromSource(sourcePath, InstalledFMODPath);
        bool deletedEmptyDirectories = DeleteEmptyDirectories(InstalledFMODPath);

        // Copy the .meta file if present alongside the source folder
        string sourceMetaPath = sourcePath + ".meta";
        if (File.Exists(sourceMetaPath))
        {
            string destinationMetaPath = InstalledFMODPath + ".meta";
            copiedAllFiles = CopyFileIfChanged(sourceMetaPath, destinationMetaPath) && copiedAllFiles;
        }

        return copiedAllFiles && deletedStaleFiles && deletedEmptyDirectories;
    }

    private static bool CopyDirectory(string sourcePath, string destinationPath)
    {
        bool copiedAllFiles = true;
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
            copiedAllFiles = CopyFileIfChanged(file, destinationFile) && copiedAllFiles;
        }

        return copiedAllFiles;
    }

    private static bool CopyFileIfChanged(string sourceFile, string destinationFile)
    {
        if (File.Exists(destinationFile) && FilesAreEqual(sourceFile, destinationFile))
            return true;

        try
        {
            if (File.Exists(destinationFile))
                File.SetAttributes(destinationFile, FileAttributes.Normal);

            File.Copy(sourceFile, destinationFile, true);
            return true;
        }
        catch (UnauthorizedAccessException exception)
        {
            LogLockedInstallWarning(destinationFile, exception);
            return false;
        }
        catch (IOException exception)
        {
            LogLockedInstallWarning(destinationFile, exception);
            return false;
        }
    }

    private static bool FilesAreEqual(string firstPath, string secondPath)
    {
        try
        {
            FileInfo firstInfo = new FileInfo(firstPath);
            FileInfo secondInfo = new FileInfo(secondPath);

            if (firstInfo.Length != secondInfo.Length)
                return false;

            byte[] firstBytes = File.ReadAllBytes(firstPath);
            byte[] secondBytes = File.ReadAllBytes(secondPath);

            for (int i = 0; i < firstBytes.Length; i++)
            {
                if (firstBytes[i] != secondBytes[i])
                    return false;
            }

            return true;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
        catch (IOException)
        {
            return false;
        }
    }

    private static bool DeleteFilesMissingFromSource(string sourcePath, string destinationPath)
    {
        if (!Directory.Exists(destinationPath))
            return true;

        bool deletedAllFiles = true;
        foreach (string destinationFile in Directory.GetFiles(destinationPath, "*", SearchOption.AllDirectories))
        {
            string relativePath = GetRelativePath(destinationPath, destinationFile);
            string sourceFile = Path.Combine(sourcePath, relativePath);

            if (!File.Exists(sourceFile))
                deletedAllFiles = TryDeleteFile(destinationFile) && deletedAllFiles;
        }

        return deletedAllFiles;
    }

    private static bool DeleteEmptyDirectories(string path)
    {
        if (!Directory.Exists(path))
            return true;

        bool deletedAllDirectories = true;
        string[] directories = Directory.GetDirectories(path, "*", SearchOption.AllDirectories);
        Array.Sort(directories, (first, second) => second.Length.CompareTo(first.Length));

        foreach (string directory in directories)
        {
            if (Directory.GetFiles(directory).Length > 0 || Directory.GetDirectories(directory).Length > 0)
                continue;

            try
            {
                Directory.Delete(directory, false);
            }
            catch (UnauthorizedAccessException exception)
            {
                LogLockedInstallWarning(directory, exception);
                deletedAllDirectories = false;
            }
            catch (IOException exception)
            {
                LogLockedInstallWarning(directory, exception);
                deletedAllDirectories = false;
            }
        }

        return deletedAllDirectories;
    }

    private static string GetRelativePath(string rootPath, string path)
    {
        return path.Substring(rootPath.Length)
            .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }
}
