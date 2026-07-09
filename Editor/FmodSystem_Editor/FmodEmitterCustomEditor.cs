using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FmodEmitterCustom))]
public class FmodEmitterCustomEditor : Editor
{
    SerializedProperty mode;
    SerializedProperty eventId;
    SerializedProperty oneShot;
    SerializedProperty playEvent;
    SerializedProperty stopEvent;
    SerializedProperty cascade;
    SerializedProperty gizmoColor;

    void OnEnable()
    {
        mode = serializedObject.FindProperty("mode");
        eventId = serializedObject.FindProperty("eventId");
        oneShot = serializedObject.FindProperty("oneShot");
        playEvent = serializedObject.FindProperty("playEvent");
        stopEvent = serializedObject.FindProperty("stopEvent");
        cascade = serializedObject.FindProperty("cascade");
        gizmoColor = serializedObject.FindProperty("gizmoColor");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(mode);

        if ((FmodEmitterCustom.EmitterMode)mode.enumValueIndex == FmodEmitterCustom.EmitterMode.None)
        {
            serializedObject.ApplyModifiedProperties();
            return;
        }

        EditorGUILayout.Space();

        if ((FmodEmitterCustom.EmitterMode)mode.enumValueIndex == FmodEmitterCustom.EmitterMode.Basic)
        {
            DrawBasic();
        }
        else if ((FmodEmitterCustom.EmitterMode)mode.enumValueIndex == FmodEmitterCustom.EmitterMode.Advanced)
        {
            DrawBasic();
            EditorGUILayout.Space();
            DrawAdvanced();
        }

        EditorGUILayout.Space();
        DrawTriggers();

        EditorGUILayout.Space();

        serializedObject.ApplyModifiedProperties();
    }

    void DrawBasic()
    {
        EditorGUILayout.LabelField("Basic Config", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(eventId);
        EditorGUILayout.PropertyField(oneShot);
    }

    void DrawAdvanced()
    {
        EditorGUILayout.LabelField("Cascade", EditorStyles.boldLabel);

        for (int i = 0; i < cascade.arraySize; i++)
        {
            SerializedProperty step = cascade.GetArrayElementAtIndex(i);
            DrawCascadeStep(step, i);
        }

        Rect buttonRect = EditorGUILayout.GetControlRect();
        if (GUI.Button(buttonRect, "+", EditorStyles.miniButton))
            ShowAddMenu();

        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(gizmoColor);
    }

    void DrawCascadeStep(SerializedProperty step, int index)
    {
        SerializedProperty function = step.FindPropertyRelative("function");
        SerializedProperty transform = step.FindPropertyRelative("transform");
        SerializedProperty vectorValue = step.FindPropertyRelative("vectorValue");
        SerializedProperty floatValue = step.FindPropertyRelative("floatValue");
        SerializedProperty intValue = step.FindPropertyRelative("intValue");
        SerializedProperty parameter = step.FindPropertyRelative("parameter");
        SerializedProperty label = step.FindPropertyRelative("label");

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(function, GUIContent.none);

        GUI.enabled = index > 0;
        if (GUILayout.Button("Up", EditorStyles.miniButtonLeft, GUILayout.Width(34)))
            cascade.MoveArrayElement(index, index - 1);

        GUI.enabled = index < cascade.arraySize - 1;
        if (GUILayout.Button("Down", EditorStyles.miniButtonMid, GUILayout.Width(48)))
            cascade.MoveArrayElement(index, index + 1);

        GUI.enabled = true;
        if (GUILayout.Button("-", EditorStyles.miniButtonRight, GUILayout.Width(24)))
        {
            cascade.DeleteArrayElementAtIndex(index);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            return;
        }

        EditorGUILayout.EndHorizontal();

        FmodEmitterCustom.CascadeFunction value = (FmodEmitterCustom.CascadeFunction)function.enumValueIndex;

        switch (value)
        {
            case FmodEmitterCustom.CascadeFunction.Attach:
                EditorGUILayout.PropertyField(transform, new GUIContent("Transform"));
                break;
            case FmodEmitterCustom.CascadeFunction.Position:
            case FmodEmitterCustom.CascadeFunction.Velocity:
                EditorGUILayout.PropertyField(vectorValue, new GUIContent("Value"));
                break;
            case FmodEmitterCustom.CascadeFunction.Radius:
            case FmodEmitterCustom.CascadeFunction.Volume:
            case FmodEmitterCustom.CascadeFunction.Pitch:
            case FmodEmitterCustom.CascadeFunction.FadeIn:
                EditorGUILayout.PropertyField(floatValue, new GUIContent("Value"));
                break;
            case FmodEmitterCustom.CascadeFunction.Parameter:
                EditorGUILayout.PropertyField(parameter, new GUIContent("Parameter"));
                EditorGUILayout.PropertyField(floatValue, new GUIContent("Value"));
                break;
            case FmodEmitterCustom.CascadeFunction.ParameterLabel:
                EditorGUILayout.PropertyField(parameter, new GUIContent("Parameter"));
                EditorGUILayout.PropertyField(label, new GUIContent("Label"));
                break;
            case FmodEmitterCustom.CascadeFunction.TimelinePosition:
                EditorGUILayout.PropertyField(intValue, new GUIContent("Milliseconds"));
                break;
        }

        EditorGUILayout.EndVertical();
    }

    void ShowAddMenu()
    {
        GenericMenu menu = new GenericMenu();
        string[] names = System.Enum.GetNames(typeof(FmodEmitterCustom.CascadeFunction));

        foreach (string name in names)
            menu.AddItem(new GUIContent(name), false, AddCascadeStep, name);

        menu.ShowAsContext();
    }

    void AddCascadeStep(object functionName)
    {
        serializedObject.Update();
        int index = cascade.arraySize;
        cascade.InsertArrayElementAtIndex(index);

        SerializedProperty step = cascade.GetArrayElementAtIndex(index);
        step.FindPropertyRelative("function").enumValueIndex = (int)System.Enum.Parse(typeof(FmodEmitterCustom.CascadeFunction), (string)functionName);
        step.FindPropertyRelative("transform").objectReferenceValue = null;
        step.FindPropertyRelative("vectorValue").vector3Value = Vector3.zero;
        step.FindPropertyRelative("floatValue").floatValue = 1f;
        step.FindPropertyRelative("intValue").intValue = 0;
        step.FindPropertyRelative("parameter").stringValue = string.Empty;
        step.FindPropertyRelative("label").stringValue = string.Empty;

        serializedObject.ApplyModifiedProperties();
    }

    void DrawTriggers()
    {
        EditorGUILayout.LabelField("Triggers", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(playEvent);
        EditorGUILayout.PropertyField(stopEvent);
    }
}
