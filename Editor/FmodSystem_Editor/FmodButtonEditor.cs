#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FmodButton))]
public class FmodButtonEditor : Editor
{
    SerializedProperty actions;

    // Tracks which action index the pending "add step" menu refers to.
    private int pendingActionIndex = -1;

    void OnEnable()
    {
        actions = serializedObject.FindProperty("actions");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("FmodButton Actions", EditorStyles.boldLabel);
        EditorGUILayout.Space(4);

        for (int i = 0; i < actions.arraySize; i++)
        {
            bool removed = DrawAction(actions.GetArrayElementAtIndex(i), i);
            if (removed)
            {
                actions.DeleteArrayElementAtIndex(i);
                break;
            }
            EditorGUILayout.Space(2);
        }

        if (GUILayout.Button("+ Add Action", EditorStyles.miniButton))
            AddAction();

        serializedObject.ApplyModifiedProperties();
    }

    // Returns true when the action was deleted (caller must break the loop).
    bool DrawAction(SerializedProperty action, int actionIndex)
    {
        SerializedProperty moment = action.FindPropertyRelative("moment");
        SerializedProperty cascade = action.FindPropertyRelative("cascade");

        string headerLabel = moment.enumDisplayNames[moment.enumValueIndex];

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        // ---- header row ----
        EditorGUILayout.BeginHorizontal();
        action.isExpanded = EditorGUILayout.Foldout(action.isExpanded, headerLabel, true, EditorStyles.boldLabel);
        if (GUILayout.Button("-", EditorStyles.miniButton, GUILayout.Width(22)))
        {
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            return true;
        }
        EditorGUILayout.EndHorizontal();

        if (action.isExpanded)
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(moment, new GUIContent("Moment"));
            EditorGUILayout.Space(4);

            // ---- cascade steps ----
            for (int i = 0; i < cascade.arraySize; i++)
            {
                bool stepRemoved = DrawCascadeStep(cascade, cascade.GetArrayElementAtIndex(i), i);
                if (stepRemoved)
                {
                    cascade.DeleteArrayElementAtIndex(i);
                    break;
                }
            }

            // ---- add step button ----
            Rect addRect = EditorGUILayout.GetControlRect();
            if (GUI.Button(addRect, "+", EditorStyles.miniButton))
            {
                pendingActionIndex = actionIndex;
                ShowAddStepMenu();
            }

            EditorGUI.indentLevel--;
        }

        EditorGUILayout.EndVertical();
        return false;
    }

    // Returns true when the step was deleted (caller must break the loop).
    bool DrawCascadeStep(SerializedProperty cascade, SerializedProperty step, int index)
    {
        SerializedProperty command    = step.FindPropertyRelative("command");
        SerializedProperty soundId    = step.FindPropertyRelative("soundId");
        SerializedProperty fade       = step.FindPropertyRelative("fade");
        SerializedProperty floatValue  = step.FindPropertyRelative("floatValue");
        SerializedProperty floatValue2 = step.FindPropertyRelative("floatValue2");
        SerializedProperty parameter  = step.FindPropertyRelative("parameter");
        SerializedProperty label      = step.FindPropertyRelative("label");

        ButtonCommandType cmd = (ButtonCommandType)command.enumValueIndex;

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.PropertyField(command, GUIContent.none);

        GUI.enabled = index > 0;
        if (GUILayout.Button("Up", EditorStyles.miniButtonLeft, GUILayout.Width(34)))
            cascade.MoveArrayElement(index, index - 1);

        GUI.enabled = index < cascade.arraySize - 1;
        if (GUILayout.Button("Down", EditorStyles.miniButtonMid, GUILayout.Width(48)))
            cascade.MoveArrayElement(index, index + 1);

        GUI.enabled = true;
        if (GUILayout.Button("-", EditorStyles.miniButtonRight, GUILayout.Width(24)))
        {
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            return true;
        }

        EditorGUILayout.EndHorizontal();

        switch (cmd)
        {
            case ButtonCommandType.Play:
            case ButtonCommandType.Pause:
            case ButtonCommandType.Resume:
            case ButtonCommandType.TogglePause:
                EditorGUILayout.PropertyField(soundId, new GUIContent("Sound ID"));
                break;

            case ButtonCommandType.Stop:
                EditorGUILayout.PropertyField(soundId, new GUIContent("Sound ID"));
                EditorGUILayout.PropertyField(fade,    new GUIContent("Fade"));
                if (fade.boolValue)
                    EditorGUILayout.PropertyField(floatValue, new GUIContent("Fade Time"));
                break;

            case ButtonCommandType.StopAll:
                EditorGUILayout.PropertyField(fade, new GUIContent("Fade"));
                if (fade.boolValue)
                    EditorGUILayout.PropertyField(floatValue, new GUIContent("Fade Time"));
                break;

            case ButtonCommandType.FadeIn:
            case ButtonCommandType.FadeOut:
                EditorGUILayout.PropertyField(soundId,    new GUIContent("Sound ID"));
                EditorGUILayout.PropertyField(floatValue, new GUIContent("Duration"));
                break;

            case ButtonCommandType.FadeTo:
                EditorGUILayout.PropertyField(soundId,     new GUIContent("Sound ID"));
                EditorGUILayout.PropertyField(floatValue,  new GUIContent("Volume"));
                EditorGUILayout.PropertyField(floatValue2, new GUIContent("Duration"));
                break;

            case ButtonCommandType.SetVolume:
                EditorGUILayout.PropertyField(soundId,    new GUIContent("Sound ID"));
                EditorGUILayout.PropertyField(floatValue, new GUIContent("Volume"));
                break;

            case ButtonCommandType.SetPitch:
                EditorGUILayout.PropertyField(soundId,    new GUIContent("Sound ID"));
                EditorGUILayout.PropertyField(floatValue, new GUIContent("Pitch"));
                break;

            case ButtonCommandType.SetParameter:
                EditorGUILayout.PropertyField(soundId,    new GUIContent("Sound ID"));
                EditorGUILayout.PropertyField(parameter,  new GUIContent("Parameter"));
                EditorGUILayout.PropertyField(floatValue, new GUIContent("Value"));
                break;

            case ButtonCommandType.SetParameterLabel:
                EditorGUILayout.PropertyField(soundId,   new GUIContent("Sound ID"));
                EditorGUILayout.PropertyField(parameter, new GUIContent("Parameter"));
                EditorGUILayout.PropertyField(label,     new GUIContent("Label"));
                break;

            case ButtonCommandType.SetGlobalParameter:
                EditorGUILayout.PropertyField(parameter,  new GUIContent("Parameter"));
                EditorGUILayout.PropertyField(floatValue, new GUIContent("Value"));
                break;

            case ButtonCommandType.StartSnapshot:
            case ButtonCommandType.StopSnapshot:
                EditorGUILayout.PropertyField(soundId, new GUIContent("Path"));
                break;

            case ButtonCommandType.SetBusVolume:
            case ButtonCommandType.SetVcaVolume:
                EditorGUILayout.PropertyField(soundId,    new GUIContent("Path"));
                EditorGUILayout.PropertyField(floatValue, new GUIContent("Volume"));
                break;
        }

        EditorGUILayout.EndVertical();
        return false;
    }

    void AddAction()
    {
        int idx = actions.arraySize;
        actions.InsertArrayElementAtIndex(idx);
        SerializedProperty newAction = actions.GetArrayElementAtIndex(idx);
        newAction.FindPropertyRelative("moment").enumValueIndex = 0;
        newAction.FindPropertyRelative("cascade").ClearArray();
        newAction.isExpanded = true;
        serializedObject.ApplyModifiedProperties();
    }

    void ShowAddStepMenu()
    {
        GenericMenu menu = new GenericMenu();
        foreach (string name in System.Enum.GetNames(typeof(ButtonCommandType)))
            menu.AddItem(new GUIContent(name), false, AddStep, name);
        menu.ShowAsContext();
    }

    void AddStep(object commandName)
    {
        if (pendingActionIndex < 0 || pendingActionIndex >= actions.arraySize)
            return;

        serializedObject.Update();

        SerializedProperty action  = actions.GetArrayElementAtIndex(pendingActionIndex);
        SerializedProperty cascade = action.FindPropertyRelative("cascade");

        int idx = cascade.arraySize;
        cascade.InsertArrayElementAtIndex(idx);
        SerializedProperty step = cascade.GetArrayElementAtIndex(idx);

        step.FindPropertyRelative("command").enumValueIndex =
            (int)System.Enum.Parse(typeof(ButtonCommandType), (string)commandName);
        step.FindPropertyRelative("soundId").stringValue    = string.Empty;
        step.FindPropertyRelative("fade").boolValue         = false;
        step.FindPropertyRelative("floatValue").floatValue  = 1f;
        step.FindPropertyRelative("floatValue2").floatValue = 1f;
        step.FindPropertyRelative("parameter").stringValue  = string.Empty;
        step.FindPropertyRelative("label").stringValue      = string.Empty;

        pendingActionIndex = -1;
        serializedObject.ApplyModifiedProperties();
    }
}
#endif