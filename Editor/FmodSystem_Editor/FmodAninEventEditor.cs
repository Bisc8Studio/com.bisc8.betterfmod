using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FmodAninEvent))]
public class FmodAninEventEditor : Editor
{
    private SerializedProperty script;
    private SerializedProperty defaultEventId;
    private SerializedProperty defaultEmitterKey;
    private SerializedProperty defaultParameter;
    private SerializedProperty defaultLabel;
    private SerializedProperty defaultRadius;
    private SerializedProperty defaultFadeTime;
    private SerializedProperty warnWhenMissing;
    private SerializedProperty conditions;

    private void OnEnable()
    {
        script = serializedObject.FindProperty("m_Script");
        defaultEventId = serializedObject.FindProperty("defaultEventId");
        defaultEmitterKey = serializedObject.FindProperty("defaultEmitterKey");
        defaultParameter = serializedObject.FindProperty("defaultParameter");
        defaultLabel = serializedObject.FindProperty("defaultLabel");
        defaultRadius = serializedObject.FindProperty("defaultRadius");
        defaultFadeTime = serializedObject.FindProperty("defaultFadeTime");
        warnWhenMissing = serializedObject.FindProperty("warnWhenMissing");
        conditions = serializedObject.FindProperty("conditions");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        GUI.enabled = false;
        EditorGUILayout.PropertyField(script);
        GUI.enabled = true;

        EditorGUILayout.Space(6);
        DrawDefaults();

        EditorGUILayout.Space(8);
        DrawAnimationMethods();

        EditorGUILayout.Space(8);
        DrawConditions();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawDefaults()
    {
        EditorGUILayout.LabelField("Animation Event Defaults", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(defaultEventId, new GUIContent("Default Event ID"));
        EditorGUILayout.PropertyField(defaultEmitterKey, new GUIContent("Default Emitter Key"));
        EditorGUILayout.PropertyField(defaultParameter, new GUIContent("Default Parameter"));
        EditorGUILayout.PropertyField(defaultLabel, new GUIContent("Default Label"));
        EditorGUILayout.PropertyField(defaultRadius, new GUIContent("Default Radius"));
        EditorGUILayout.PropertyField(defaultFadeTime, new GUIContent("Default Fade Time"));
        EditorGUILayout.PropertyField(warnWhenMissing, new GUIContent("Warnings"));
    }

    private void DrawAnimationMethods()
    {
        EditorGUILayout.LabelField("Animation Event Calls", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Use void methods for defaults, or string methods with the Animation Event String field. Emitter methods use only the FMODB8 Emmiter Key.",
            MessageType.Info);
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
            EditorGUILayout.LabelField("Condition " + i + " (AND)", EditorStyles.boldLabel);

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
        SerializedProperty value = entry.FindPropertyRelative("value");

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

        if (selectedType == FmodAninEvent.ConditionType.InScene)
            EditorGUILayout.PropertyField(value, new GUIContent("Scene Name or Build Index"));

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
        entry.FindPropertyRelative("value").stringValue = string.Empty;
    }
}
