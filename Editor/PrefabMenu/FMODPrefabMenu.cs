using System;
using UnityEditor;
using UnityEngine;

public static class FMODPrefabMenu
{
    private const string PackagePrefabsPath = "Packages/com.bisc8.betterfmod/Runtime/FmodSystem/Prefabs_FMOD";

    [MenuItem("GameObject/BISC8 FMOD/BISC8FMOD System", false, 8)]
    private static void CreateBisc8FmodSystem(MenuCommand command)
    {
        CreatePrefab("BISC8FMOD_System.prefab", command);
    }

    [MenuItem("GameObject/BISC8 FMOD/BISC8FMOD Multiplayer", false, 9)]
    private static void CreateBisc8FmodMultiplayer(MenuCommand command)
    {
        CreatePrefab("BISC8FMOD_Multiplayer.prefab", command, EnsureNetcodeRelayComponents);
    }

    [MenuItem("GameObject/BISC8 FMOD/Fmod Emitter Mng", false, 10)]
    private static void CreateFmodEmitter(MenuCommand command)
    {
        CreatePrefab("FmodEmitter_Mng.prefab", command);
    }

    [MenuItem("GameObject/BISC8 FMOD/Fmod Slider Mng", false, 11)]
    private static void CreateFmodSlider(MenuCommand command)
    {
        CreatePrefab("FmodSlider_Mng.prefab", command);
    }

    private static void CreatePrefab(string prefabFileName, MenuCommand command, Action<GameObject> configure = null)
    {
        GameObject prefab = LoadPrefab(prefabFileName);
        if (prefab == null)
        {
            EditorUtility.DisplayDialog(
                "BISC8 Better FMOD",
                $"Prefab not found: {prefabFileName}",
                "OK"
            );
            return;
        }

        UnityEngine.Object instance = PrefabUtility.InstantiatePrefab(prefab);
        if (instance is not GameObject gameObject)
            return;

        GameObjectUtility.SetParentAndAlign(gameObject, command.context as GameObject);
        configure?.Invoke(gameObject);
        Undo.RegisterCreatedObjectUndo(gameObject, $"Create {prefab.name}");
        Selection.activeGameObject = gameObject;
    }

    private static void EnsureNetcodeRelayComponents(GameObject gameObject)
    {
        Type networkObjectType = FindType("Unity.Netcode.NetworkObject");
        Type transportType = FindType("FmodUnityNetcodeTransport");

        if (networkObjectType == null || transportType == null)
        {
            EditorUtility.DisplayDialog(
                "BISC8 Better FMOD",
                "BISC8FMOD_Multiplayer requires Netcode for GameObjects in this project.",
                "OK"
            );
            return;
        }

        if (gameObject.GetComponent(networkObjectType) == null)
            Undo.AddComponent(gameObject, networkObjectType);

        if (gameObject.GetComponent(transportType) == null)
            Undo.AddComponent(gameObject, transportType);
    }

    private static Type FindType(string typeName)
    {
        foreach (System.Reflection.Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type type = assembly.GetType(typeName);
            if (type != null)
                return type;
        }

        return null;
    }

    private static GameObject LoadPrefab(string prefabFileName)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PackagePrefabsPath}/{prefabFileName}");
        if (prefab != null)
            return prefab;

        return null;
    }
}
