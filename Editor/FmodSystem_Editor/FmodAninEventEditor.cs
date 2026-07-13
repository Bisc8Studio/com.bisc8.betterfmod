using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FmodAninEvent))]
public class FmodAninEventEditor : Editor
{
    private static readonly string[] Tabs = { "Event", "Emitter", "Condition" };
    private static int selectedTab;

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

        selectedTab = GUILayout.Toolbar(selectedTab, Tabs);
        EditorGUILayout.Space(8);

        switch (selectedTab)
        {
            case 0:
                DrawEventTab();
                break;
            case 1:
                DrawEmitterTab();
                break;
            case 2:
                DrawConditionTab();
                break;
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawEventTab()
    {
        EditorGUILayout.LabelField("Animation Event Defaults", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(defaultEventId, new GUIContent("Event ID"));
        EditorGUILayout.PropertyField(defaultParameter, new GUIContent("Parameter"));
        EditorGUILayout.PropertyField(defaultLabel, new GUIContent("Label"));
        EditorGUILayout.PropertyField(defaultTarget, new GUIContent("Target"));
        EditorGUILayout.PropertyField(defaultRadius, new GUIContent("Radius"));
        EditorGUILayout.PropertyField(defaultFadeTime, new GUIContent("Fade Time"));
        EditorGUILayout.PropertyField(warnWhenMissing, new GUIContent("Warnings"));
    }

    private void DrawEmitterTab()
    {
        EditorGUILayout.LabelField("Emitter", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(defaultEmitter, new GUIContent("Default Emitter"));
    }

    private void DrawConditionTab()
    {
        EditorGUILayout.LabelField("Conditions", EditorStyles.boldLabel);

        if (conditions.arraySize == 0)
            EditorGUILayout.HelpBox("Condition None: all animation events execute.", MessageType.Info);

        for (int i = 0; i < conditions.arraySize; i++)
        {
            SerializedProperty group = conditions.GetArrayElementAtIndex(i);
            SerializedProperty entries = group.FindPropertyRelative("entries");

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Condition " + i, EditorStyles.boldLabel);

            if (GUILayout.Button("-", EditorStyles.miniButton, GUILayout.Width(24)))
            {
                conditions.DeleteArrayElementAtIndex(i);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                break;
            }

            EditorGUILayout.EndHorizontal();

            if (entries.arraySize == 0)
                EditorGUILayout.HelpBox("Empty condition is treated as None.", MessageType.None);

            for (int j = 0; j < entries.arraySize; j++)
            {
                DrawConditionEntry(entries, j);
            }

            if (GUILayout.Button("+ Add Option", EditorStyles.miniButton))
                AddConditionEntry(entries);

            EditorGUILayout.EndVertical();
        }

        if (GUILayout.Button("+ Add Condition", EditorStyles.miniButton))
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
