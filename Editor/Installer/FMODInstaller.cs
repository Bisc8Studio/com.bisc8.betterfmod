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
    private const string InstalledNativePath = InstalledRootPath + "/FMODNative";
    private const string InstalledNativeMarkerPath = InstalledNativePath + "/platforms/win/lib/x86_64/fmodstudio.dll";
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

        // Keep FMOD C# assemblies visible so the package compiles on first import,
        // but install the hidden native libraries under Assets. Windows locks loaded
        // DLLs and UPM otherwise leaves .del--* folders when refreshing a Git package.
        string activeSourcePath = GetActiveFMODSourcePath();
        if (HasHiddenNativeLibraries(activeSourcePath) && !File.Exists(InstalledNativeMarkerPath))
        {
            RunSetup();
            return;
        }

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

    [MenuItem("FMOD/FMODB8/Remove Outdated", false, 21)]
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

    [MenuItem("Assets/FMODB8/Create Event List", false, 10)]
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
        if (GetHiddenFMODSourcePath() != null && GetActiveFMODSourcePath() == null)
        {
            Debug.LogWarning(
                "[FMODB8] Assets/BISC8/FMODB8/FMOD is the active FMOD installation. " +
                "It cannot be removed while FMOD is stored as a hidden package source.");
            return false;
        }

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
                InstallNativeLibraries(activeSourcePath);

                if (HasHiddenNativeLibraries(activeSourcePath) && !File.Exists(InstalledNativeMarkerPath))
                    throw new IOException("FMOD native libraries were not installed in Assets.");

                RemoveLegacyFMODDefine();
                PromptRemoveLegacyInstalledFMODCopy(false);
                MarkSetupComplete();
                AssetDatabase.Refresh();
                Debug.Log("[FMODB8] Setup complete. FMOD assemblies are active in the package and native libraries are installed in Assets.");
                return;
            }

            string sourcePath = hiddenSourcePath ?? activeSourcePath;
            MoveFMODToAssets(sourcePath);

            if (!File.Exists(InstalledMarkerPath))
                throw new IOException("FMODUnity.asmdef was not installed in Assets.");

            RemoveLegacyFMODDefine();
            MarkSetupComplete();
            AssetDatabase.Refresh();

            Debug.Log("[FMODB8] Setup complete. FMOD was installed in Assets/BISC8/FMODB8/FMOD.");
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

    private static bool HasHiddenNativeLibraries(string activeSourcePath)
    {
        if (string.IsNullOrWhiteSpace(activeSourcePath))
            return false;

        return Directory.Exists(Path.Combine(activeSourcePath, "platforms/win/lib~"))
            || Directory.Exists(Path.Combine(activeSourcePath, "platforms/linux/lib~"));
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
            CopyFileIfChanged(sourceMetaPath, destinationMetaPath);
        }
    }

    private static void InstallNativeLibraries(string activeSourcePath)
    {
        if (string.IsNullOrWhiteSpace(activeSourcePath))
            return;

        EnsureAssetFolder("Assets", "BISC8");
        EnsureAssetFolder("Assets/BISC8", "FMODB8");

        InstallNativeLibraryFolder(activeSourcePath, "win");
        InstallNativeLibraryFolder(activeSourcePath, "linux");
    }

    private static void InstallNativeLibraryFolder(string activeSourcePath, string platform)
    {
        string sourcePath = Path.Combine(activeSourcePath, "platforms", platform, "lib~");
        if (!Directory.Exists(sourcePath))
            return;

        string destinationPath = Path.Combine(InstalledNativePath, "platforms", platform, "lib");
        CopyDirectory(sourcePath, destinationPath);
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
            CopyFileIfChanged(file, destinationFile);
        }
    }

    private static bool CopyFileIfChanged(string sourcePath, string destinationPath)
    {
        if (File.Exists(destinationPath) && FilesHaveSameContent(sourcePath, destinationPath))
            return false;

        File.Copy(sourcePath, destinationPath, true);
        return true;
    }

    private static bool FilesHaveSameContent(string firstPath, string secondPath)
    {
        if (FilesAreByteIdentical(firstPath, secondPath))
            return true;

        if (!IsTextFile(firstPath) || !IsTextFile(secondPath))
            return false;

        string firstContent = NormalizeLineEndings(File.ReadAllText(firstPath));
        string secondContent = NormalizeLineEndings(File.ReadAllText(secondPath));
        return string.Equals(firstContent, secondContent, StringComparison.Ordinal);
    }

    private static bool FilesAreByteIdentical(string firstPath, string secondPath)
    {
        FileInfo firstInfo = new FileInfo(firstPath);
        FileInfo secondInfo = new FileInfo(secondPath);

        if (firstInfo.Length != secondInfo.Length)
            return false;

        const int bufferSize = 81920;
        byte[] firstBuffer = new byte[bufferSize];
        byte[] secondBuffer = new byte[bufferSize];

        using (FileStream firstStream = File.OpenRead(firstPath))
        using (FileStream secondStream = File.OpenRead(secondPath))
        {
            while (true)
            {
                int firstRead = firstStream.Read(firstBuffer, 0, firstBuffer.Length);
                int secondRead = secondStream.Read(secondBuffer, 0, secondBuffer.Length);

                if (firstRead != secondRead)
                    return false;

                if (firstRead == 0)
                    return true;

                for (int index = 0; index < firstRead; index++)
                {
                    if (firstBuffer[index] != secondBuffer[index])
                        return false;
                }
            }
        }
    }

    private static bool IsTextFile(string path)
    {
        switch (Path.GetExtension(path).ToLowerInvariant())
        {
            case ".asmdef":
            case ".asmref":
            case ".asset":
            case ".cginc":
            case ".compute":
            case ".config":
            case ".cs":
            case ".gradle":
            case ".h":
            case ".html":
            case ".java":
            case ".json":
            case ".m":
            case ".md":
            case ".meta":
            case ".mm":
            case ".plist":
            case ".props":
            case ".rsp":
            case ".shader":
            case ".targets":
            case ".txt":
            case ".uxml":
            case ".xml":
                return true;
            default:
                return false;
        }
    }

    private static string NormalizeLineEndings(string content)
    {
        return content.Replace("\r\n", "\n").Replace('\r', '\n');
    }

    private static string GetRelativePath(string rootPath, string path)
    {
        return path.Substring(rootPath.Length)
            .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }
}
