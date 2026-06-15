using System;
using System.IO;
using UnityEditor;
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
}

[InitializeOnLoad]
public static class FMODInstaller
{
    private const string PackagePath = "Packages/com.bisc8.betterfmod";
    private const string PackageFMODPath = "Runtime/FmodSystem/Plugins_FMOD/CustomFMOD/FMOD";
    private const string InstalledRootPath = "Assets/BISC8/BetterFMOD";
    private const string InstalledFMODPath = InstalledRootPath + "/FMOD";
    private const string InstalledMarkerPath = InstalledRootPath + "/.installed";
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
            "Install and configure FMOD in Assets/BISC8/BetterFMOD?",
            "Setup",
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
            Debug.LogError("[BISC8 FMOD] FMOD source folder was not found in the package.");
            return;
        }

        try
        {
            if (hiddenSourcePath != null)
            {
                Directory.CreateDirectory(InstalledRootPath);
                CopyDirectory(hiddenSourcePath, InstalledFMODPath);
                File.WriteAllText(InstalledMarkerPath, "installed");
            }

            AssetDatabase.Refresh();
            MarkSetupComplete();
            OpenFMODSetupWizard();

            Debug.Log("[BISC8 FMOD] Setup complete.");
        }
        catch (Exception exception)
        {
            Debug.LogError("[BISC8 FMOD] Setup failed: " + exception.Message);
        }
    }

    private static bool IsSetupComplete()
    {
        bool installationPresent =
            Directory.Exists(InstalledFMODPath) ||
            GetActiveFMODSourcePath() != null;

        if (!installationPresent)
            return false;

        if (FMODInstallerState.instance.SetupComplete)
            return true;

        if (!File.Exists(InstalledMarkerPath))
            return false;

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

    private static void OpenFMODSetupWizard()
    {
        Type wizardType = Type.GetType("FMODUnity.SetupWizardWindow, FMODUnityEditor");
        var showAssistant = wizardType?.GetMethod(
            "ShowAssistant",
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static
        );

        if (showAssistant == null)
        {
            Debug.LogWarning("[BISC8 FMOD] FMOD Setup Wizard is not available yet. Wait for Unity to finish importing and run setup again.");
            return;
        }

        showAssistant.Invoke(null, null);
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
        var packageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssetPath(PackagePath);

        if (packageInfo != null && !string.IsNullOrEmpty(packageInfo.resolvedPath))
            return packageInfo.resolvedPath;

        return Path.GetFullPath(PackagePath);
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
