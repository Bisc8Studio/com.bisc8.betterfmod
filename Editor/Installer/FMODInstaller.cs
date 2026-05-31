using UnityEditor;
using UnityEditor.SceneManagement;
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
            "BISC8 Better FMOD Setup\n\n" +
            "This setup will:\n\n" +
            "• Create the BISC8 folder structure\n" +
            "• Create Lists_FMOD folder\n" +
            "• Add FmodSystem to Scene 0\n\n" +
            "Continue?",
            "Create",
            "Not now"
        );

        if (!create)
            return;

        CreateFolders();
        CreateFmodSystemInScene0();

        EditorPrefs.SetBool(HasSetupKey, true);

        Debug.Log("[BISC8 FMOD] Setup complete.");
    }

    static void CreateFolders()
    {
        if (!AssetDatabase.IsValidFolder("Assets/BISC8"))
            AssetDatabase.CreateFolder("Assets", "BISC8");

        if (!AssetDatabase.IsValidFolder("Assets/BISC8/BetterFMOD"))
            AssetDatabase.CreateFolder("Assets/BISC8", "BetterFMOD");

        if (!AssetDatabase.IsValidFolder("Assets/BISC8/BetterFMOD/Lists_FMOD"))
            AssetDatabase.CreateFolder("Assets/BISC8/BetterFMOD", "Lists_FMOD");

        AssetDatabase.Refresh();
    }

    static void CreateFmodSystemInScene0()
    {
        if (EditorBuildSettings.scenes.Length == 0)
        {
            Debug.LogWarning("[BISC8 FMOD] No scenes found in Build Settings.");
            return;
        }

        string scenePath = EditorBuildSettings.scenes[0].path;

        var scene = EditorSceneManager.OpenScene(
            scenePath,
            OpenSceneMode.Single
        );

        if (GameObject.Find("FmodSystem") != null)
        {
            Debug.Log("[BISC8 FMOD] FmodSystem already exists in Scene 0.");
            return;
        }

        string[] guids = AssetDatabase.FindAssets("FmodSystem t:Prefab");

        if (guids.Length == 0)
        {
            Debug.LogWarning("[BISC8 FMOD] FmodSystem prefab not found.");
            return;
        }

        string prefabPath = AssetDatabase.GUIDToAssetPath(guids[0]);

        GameObject prefab =
            AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

        if (prefab == null)
        {
            Debug.LogWarning("[BISC8 FMOD] Could not load FmodSystem prefab.");
            return;
        }

        PrefabUtility.InstantiatePrefab(prefab, scene);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Debug.Log("[BISC8 FMOD] FmodSystem added to Scene 0.");
    }
}