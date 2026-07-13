using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FmodAninEvent))]
public class FmodAninEventEditor : Editor
{
    private SerializedProperty script;
    private SerializedProperty defaultEventId;
    private SerializedProperty defaultParameter;
    private SerializedProperty defaultLabel;
    private SerializedProperty defaultTarget;
    private SerializedProperty defaultRadius;
    private SerializedProperty defaultFadeTime;
    private SerializedProperty warnWhenMissing;
    private SerializedProperty defaultEmitter;
    private SerializedProperty conditions;

    private void OnEnable()
    {
        script = serializedObject.FindProperty("m_Script");
        defaultEventId = serializedObject.FindProperty("defaultEventId");
        defaultParameter = serializedObject.FindProperty("defaultParameter");
        defaultLabel = serializedObject.FindProperty("defaultLabel");
        defaultTarget = serializedObject.FindProperty("defaultTarget");
        defaultRadius = serializedObject.FindProperty("defaultRadius");
        defaultFadeTime = serializedObject.FindProperty("defaultFadeTime");
        warnWhenMissing = serializedObject.FindProperty("warnWhenMissing");
        defaultEmitter = serializedObject.FindProperty("defaultEmitter");
        conditions = serializedObject.FindProperty("conditions");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        GUI.enabled = false;
        EditorGUILayout.PropertyField(script);
        GUI.enabled = true;

        EditorGUILayout.Space(6);
        DrawEventDefaults();

        EditorGUILayout.Space(8);
        DrawEmitterDefaults();

        EditorGUILayout.Space(8);
        DrawConditions();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawEventDefaults()
    {
        EditorGUILayout.LabelField("Animation Event", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(defaultEventId, new GUIContent("Default Event ID"));
        EditorGUILayout.PropertyField(defaultParameter, new GUIContent("Default Parameter"));
        EditorGUILayout.PropertyField(defaultLabel, new GUIContent("Default Label"));
        EditorGUILayout.PropertyField(defaultTarget, new GUIContent("Default Target"));
        EditorGUILayout.PropertyField(defaultRadius, new GUIContent("Default Radius"));
        EditorGUILayout.PropertyField(defaultFadeTime, new GUIContent("Default Fade Time"));
        EditorGUILayout.PropertyField(warnWhenMissing, new GUIContent("Warnings"));
    }

    private void DrawEmitterDefaults()
    {
        EditorGUILayout.LabelField("Emitter", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(defaultEmitter, new GUIContent("Default Emitter"));
    }

    private void DrawConditions()
    {
        EditorGUILayout.LabelField("Conditions", EditorStyles.boldLabel);

        if (conditions.arraySize == 0)
            EditorGUILayout.HelpBox("Condition None: animation events always execute.", MessageType.Info);

        for (int i = 0; i < conditions.arraySize; i++)
        {
            SerializedProperty group = conditions.GetArrayElementAtIndex(i);
            SerializedProperty entries = group.FindPropertyRelative("entries");

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Condition " + i, EditorStyles.boldLabel);

            if (GUILayout.Button("Remove", EditorStyles.miniButton, GUILayout.Width(64)))
            {
                conditions.DeleteArrayElementAtIndex(i);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                break;
            }

            EditorGUILayout.EndHorizontal();

            for (int j = 0; j < entries.arraySize; j++)
                DrawConditionEntry(entries, j);

            if (GUILayout.Button("+ Add OR Option", EditorStyles.miniButton))
                AddConditionEntry(entries);

            EditorGUILayout.EndVertical();
        }

        if (GUILayout.Button("+ Add AND Condition", EditorStyles.miniButton))
            AddConditionGroup();
    }

    private void DrawConditionEntry(SerializedProperty entries, int index)
    {
        SerializedProperty entry = entries.GetArrayElementAtIndex(index);
        SerializedProperty type = entry.FindPropertyRelative("type");
        SerializedProperty invert = entry.FindPropertyRelative("invert");
        SerializedProperty intValue = entry.FindPropertyRelative("intValue");
        SerializedProperty stringValue = entry.FindPropertyRelative("stringValue");
        SerializedProperty gameObject = entry.FindPropertyRelative("gameObject");
        SerializedProperty behaviour = entry.FindPropertyRelative("behaviour");

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(type, GUIContent.none);

        if (GUILayout.Button("-", EditorStyles.miniButton, GUILayout.Width(24)))
        {
            entries.DeleteArrayElementAtIndex(index);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            return;
        }

        EditorGUILayout.EndHorizontal();

        FmodAninEvent.ConditionType selectedType = (FmodAninEvent.ConditionType)type.enumValueIndex;

        switch (selectedType)
        {
            case FmodAninEvent.ConditionType.InSceneBuildIndex:
                EditorGUILayout.PropertyField(intValue, new GUIContent("Scene Build Index"));
                break;
            case FmodAninEvent.ConditionType.InSceneName:
                EditorGUILayout.PropertyField(stringValue, new GUIContent("Scene Name"));
                break;
            case FmodAninEvent.ConditionType.GameObjectActive:
                EditorGUILayout.PropertyField(gameObject, new GUIContent("GameObject"));
                break;
            case FmodAninEvent.ConditionType.BehaviourEnabled:
                EditorGUILayout.PropertyField(behaviour, new GUIContent("Behaviour"));
                break;
        }

        if (selectedType != FmodAninEvent.ConditionType.None)
            EditorGUILayout.PropertyField(invert, new GUIContent("Invert"));

        EditorGUILayout.EndVertical();
    }

    private void AddConditionGroup()
    {
        int index = conditions.arraySize;
        conditions.InsertArrayElementAtIndex(index);

        SerializedProperty group = conditions.GetArrayElementAtIndex(index);
        SerializedProperty entries = group.FindPropertyRelative("entries");
        entries.ClearArray();
        AddConditionEntry(entries);
    }

    private static void AddConditionEntry(SerializedProperty entries)
    {
        int index = entries.arraySize;
        entries.InsertArrayElementAtIndex(index);

        SerializedProperty entry = entries.GetArrayElementAtIndex(index);
        entry.FindPropertyRelative("type").enumValueIndex = (int)FmodAninEvent.ConditionType.None;
        entry.FindPropertyRelative("invert").boolValue = false;
        entry.FindPropertyRelative("intValue").intValue = 0;
        entry.FindPropertyRelative("stringValue").stringValue = string.Empty;
        entry.FindPropertyRelative("gameObject").objectReferenceValue = null;
        entry.FindPropertyRelative("behaviour").objectReferenceValue = null;
    }
}
