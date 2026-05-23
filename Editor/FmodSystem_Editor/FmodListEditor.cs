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

        string folder = "Packages/com.bisc8.betterfmod/Runtime/FmodSystem/Lists_FMOD";

        if (!AssetDatabase.IsValidFolder(folder))
        {
            Debug.LogError("Folder not found: " + folder);
            return;
        }

        string path = AssetDatabase.GenerateUniqueAssetPath(
            folder + "/NewFmodList.asset"
        );

        AssetDatabase.CreateAsset(asset, path);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.FocusProjectWindow();

        Selection.activeObject = asset;
    }
}