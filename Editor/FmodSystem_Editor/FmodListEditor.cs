using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CreateFmodList))]
public class CreateFmodListEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SerializedProperty typeProp = serializedObject.FindProperty("type");
        SerializedProperty typeNameProp = serializedObject.FindProperty("typeName");
        SerializedProperty eventsProp = serializedObject.FindProperty("events");

        EditorGUILayout.PropertyField(typeProp);

        ListType currentType = (ListType)typeProp.enumValueIndex;

        if (currentType == ListType.Other)
        {
            EditorGUILayout.PropertyField(typeNameProp);
        }

        if (currentType != ListType.None)
        {
            EditorGUILayout.PropertyField(eventsProp, true);
        }

        serializedObject.ApplyModifiedProperties();
    }

    [MenuItem("Assets/Create/BISC8 FMOD/Create List")]
    public static void CreateList()
    {
        CreateFmodList asset = ScriptableObject.CreateInstance<CreateFmodList>();

        const string rootFolder = "Assets/BISC8";
        const string betterFmodFolder = rootFolder + "/BetterFMOD";
        const string folder = betterFmodFolder + "/Lists";

        EnsureFolder("Assets", "BISC8");
        EnsureFolder(rootFolder, "BetterFMOD");
        EnsureFolder(betterFmodFolder, "Lists");

        string path = AssetDatabase.GenerateUniqueAssetPath(
            folder + "/NewFmodList.asset"
        );

        AssetDatabase.CreateAsset(asset, path);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.FocusProjectWindow();

        Selection.activeObject = asset;
    }

    private static void EnsureFolder(string parent, string name)
    {
        string path = parent + "/" + name;
        if (!AssetDatabase.IsValidFolder(path))
            AssetDatabase.CreateFolder(parent, name);
    }
}
