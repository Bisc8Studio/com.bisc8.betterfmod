#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FmodButton))]
public class FmodButtonEditor : Editor
{
    SerializedProperty actions;
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
            if (DrawAction(actions.GetArrayElementAtIndex(i), i))
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

    // Returns true when the action was deleted.
    bool DrawAction(SerializedProperty action, int actionIndex)
    {
        SerializedProperty moment     = action.FindPropertyRelative("moment");
        SerializedProperty command    = action.FindPropertyRelative("command");
        SerializedProperty playbackScope = action.FindPropertyRelative("playbackScope");
        SerializedProperty soundId    = action.FindPropertyRelative("soundId");
        SerializedProperty fade       = action.FindPropertyRelative("fade");
        SerializedProperty floatValue  = action.FindPropertyRelative("floatValue");
        SerializedProperty floatValue2 = action.FindPropertyRelative("floatValue2");
        SerializedProperty parameter  = action.FindPropertyRelative("parameter");
        SerializedProperty label      = action.FindPropertyRelative("label");
        SerializedProperty cascade    = action.FindPropertyRelative("cascade");

        ButtonRootCommand cmd = (ButtonRootCommand)command.enumValueIndex;

        string header = moment.enumDisplayNames[moment.enumValueIndex]
                      + "  →  "
                      + command.enumDisplayNames[command.enumValueIndex];

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        // ── header row ──────────────────────────────────────────────────
        EditorGUILayout.BeginHorizontal();
        action.isExpanded = EditorGUILayout.Foldout(action.isExpanded, header, true, EditorStyles.boldLabel);
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

            // ── moment + command ────────────────────────────────────────
            EditorGUILayout.PropertyField(moment,  new GUIContent("Moment"));
            EditorGUILayout.PropertyField(command, new GUIContent("Command"));

            if (FmodButtonAction.IsNetworkableCommand(cmd))
            {
                bool isMultiplayer = (FmodPlaybackScope)playbackScope.enumValueIndex == FmodPlaybackScope.Multiplayer;
                bool newIsMultiplayer = EditorGUILayout.Toggle(new GUIContent("Multiplayer"), isMultiplayer);
                playbackScope.enumValueIndex = newIsMultiplayer
                    ? (int)FmodPlaybackScope.Multiplayer
                    : (int)FmodPlaybackScope.Local;
            }

            EditorGUILayout.Space(2);

            // ── root command fields ─────────────────────────────────────
            DrawRootFields(cmd, soundId, fade, floatValue, floatValue2, parameter, label);

            // ── cascade (only for handle-returning commands) ────────────
            if (FmodButtonAction.IsHandleCommand(cmd))
            {
                EditorGUILayout.Space(4);
                EditorGUILayout.LabelField("Cascade", EditorStyles.miniBoldLabel);

                for (int i = 0; i < cascade.arraySize; i++)
                {
                    if (DrawCascadeStep(cascade, cascade.GetArrayElementAtIndex(i), i))
                    {
                        cascade.DeleteArrayElementAtIndex(i);
                        break;
                    }
                }

                Rect addRect = EditorGUILayout.GetControlRect();
                if (GUI.Button(addRect, "+", EditorStyles.miniButton))
                {
                    pendingActionIndex = actionIndex;
                    ShowAddModifierMenu();
                }
            }

            EditorGUI.indentLevel--;
        }

        EditorGUILayout.EndVertical();
        return false;
    }

    void DrawRootFields(ButtonRootCommand cmd,
        SerializedProperty soundId, SerializedProperty fade,
        SerializedProperty floatValue, SerializedProperty floatValue2,
        SerializedProperty parameter, SerializedProperty label)
    {
        switch (cmd)
        {
            // ── play / control – just need a Sound ID ──
            case ButtonRootCommand.Play:
            case ButtonRootCommand.PlayLoop:
            case ButtonRootCommand.Pause:
            case ButtonRootCommand.Resume:
            case ButtonRootCommand.TogglePause:
                EditorGUILayout.PropertyField(soundId, new GUIContent("Sound ID"));
                break;

            // ── snapshot / bus / VCA paths ──
            case ButtonRootCommand.StartSnapshot:
            case ButtonRootCommand.StopSnapshot:
                EditorGUILayout.PropertyField(soundId, new GUIContent("Path"));
                break;

            case ButtonRootCommand.SetBusVolume:
            case ButtonRootCommand.SetVcaVolume:
                EditorGUILayout.PropertyField(soundId,    new GUIContent("Path"));
                EditorGUILayout.PropertyField(floatValue, new GUIContent("Volume"));
                break;

            // ── stop with optional fade ──
            case ButtonRootCommand.Stop:
                EditorGUILayout.PropertyField(soundId, new GUIContent("Sound ID"));
                EditorGUILayout.PropertyField(fade,    new GUIContent("Fade"));
                if (fade.boolValue)
                    EditorGUILayout.PropertyField(floatValue, new GUIContent("Fade Time"));
                break;

            case ButtonRootCommand.StopAll:
                EditorGUILayout.PropertyField(fade, new GUIContent("Fade"));
                if (fade.boolValue)
                    EditorGUILayout.PropertyField(floatValue, new GUIContent("Fade Time"));
                break;

            // ── fade in / out ──
            case ButtonRootCommand.FadeIn:
            case ButtonRootCommand.FadeOut:
                EditorGUILayout.PropertyField(soundId,    new GUIContent("Sound ID"));
                EditorGUILayout.PropertyField(floatValue, new GUIContent("Duration"));
                break;

            // ── fade to target volume ──
            case ButtonRootCommand.FadeTo:
                EditorGUILayout.PropertyField(soundId,     new GUIContent("Sound ID"));
                EditorGUILayout.PropertyField(floatValue,  new GUIContent("Volume"));
                EditorGUILayout.PropertyField(floatValue2, new GUIContent("Duration"));
                break;

            // ── volume / pitch ──
            case ButtonRootCommand.SetVolume:
                EditorGUILayout.PropertyField(soundId,    new GUIContent("Sound ID"));
                EditorGUILayout.PropertyField(floatValue, new GUIContent("Volume"));
                break;

            case ButtonRootCommand.SetPitch:
                EditorGUILayout.PropertyField(soundId,    new GUIContent("Sound ID"));
                EditorGUILayout.PropertyField(floatValue, new GUIContent("Pitch"));
                break;

            // ── parameters ──
            case ButtonRootCommand.SetParameter:
                EditorGUILayout.PropertyField(soundId,    new GUIContent("Sound ID"));
                EditorGUILayout.PropertyField(parameter,  new GUIContent("Parameter"));
                EditorGUILayout.PropertyField(floatValue, new GUIContent("Value"));
                break;

            case ButtonRootCommand.SetParameterLabel:
                EditorGUILayout.PropertyField(soundId,   new GUIContent("Sound ID"));
                EditorGUILayout.PropertyField(parameter, new GUIContent("Parameter"));
                EditorGUILayout.PropertyField(label,     new GUIContent("Label"));
                break;

            case ButtonRootCommand.SetGlobalParameter:
                EditorGUILayout.PropertyField(parameter,  new GUIContent("Parameter"));
                EditorGUILayout.PropertyField(floatValue, new GUIContent("Value"));
                break;

            // ── kept handle ──
            case ButtonRootCommand.Kept:
                EditorGUILayout.PropertyField(soundId, new GUIContent("Keep Key"));
                break;
        }
    }

    // Returns true when the step was deleted.
    bool DrawCascadeStep(SerializedProperty cascade, SerializedProperty step, int index)
    {
        SerializedProperty modifier   = step.FindPropertyRelative("modifier");
        SerializedProperty strVal     = step.FindPropertyRelative("stringValue");
        SerializedProperty strVal2    = step.FindPropertyRelative("stringValue2");
        SerializedProperty floatVal   = step.FindPropertyRelative("floatValue");
        SerializedProperty floatVal2  = step.FindPropertyRelative("floatValue2");
        SerializedProperty boolVal    = step.FindPropertyRelative("boolValue");
        SerializedProperty intVal     = step.FindPropertyRelative("intValue");
        SerializedProperty vecVal     = step.FindPropertyRelative("vectorValue");
        SerializedProperty targetProp = step.FindPropertyRelative("target");

        ButtonCascadeModifier mod = (ButtonCascadeModifier)modifier.enumValueIndex;

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.PropertyField(modifier, GUIContent.none);

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

        switch (mod)
        {
            // no extra fields
            case ButtonCascadeModifier.As3D:
            case ButtonCascadeModifier.Detach:
            case ButtonCascadeModifier.Pause:
            case ButtonCascadeModifier.Resume:
            case ButtonCascadeModifier.TogglePause:
                break;

            case ButtonCascadeModifier.Volume:
                EditorGUILayout.PropertyField(floatVal, new GUIContent("Volume"));
                break;

            case ButtonCascadeModifier.Pitch:
                EditorGUILayout.PropertyField(floatVal, new GUIContent("Pitch"));
                break;

            case ButtonCascadeModifier.Radius:
                EditorGUILayout.PropertyField(floatVal, new GUIContent("Radius"));
                break;

            case ButtonCascadeModifier.FadeIn:
            case ButtonCascadeModifier.FadeOut:
                EditorGUILayout.PropertyField(floatVal, new GUIContent("Duration"));
                break;

            case ButtonCascadeModifier.FadeTo:
                EditorGUILayout.PropertyField(floatVal,  new GUIContent("Volume"));
                EditorGUILayout.PropertyField(floatVal2, new GUIContent("Duration"));
                break;

            case ButtonCascadeModifier.Stop:
                EditorGUILayout.PropertyField(boolVal, new GUIContent("Fade"));
                if (boolVal.boolValue)
                    EditorGUILayout.PropertyField(floatVal, new GUIContent("Fade Time"));
                break;

            case ButtonCascadeModifier.Parameter:
                EditorGUILayout.PropertyField(strVal,  new GUIContent("Parameter"));
                EditorGUILayout.PropertyField(floatVal, new GUIContent("Value"));
                break;

            case ButtonCascadeModifier.ParameterLabel:
                EditorGUILayout.PropertyField(strVal,  new GUIContent("Parameter"));
                EditorGUILayout.PropertyField(strVal2, new GUIContent("Label"));
                break;

            case ButtonCascadeModifier.TimelinePosition:
                EditorGUILayout.PropertyField(intVal, new GUIContent("Milliseconds"));
                break;

            case ButtonCascadeModifier.Follow:
                EditorGUILayout.PropertyField(targetProp, new GUIContent("Transform"));
                break;

            case ButtonCascadeModifier.Position:
                EditorGUILayout.PropertyField(vecVal, new GUIContent("Position"));
                break;

            case ButtonCascadeModifier.Velocity:
                EditorGUILayout.PropertyField(vecVal, new GUIContent("Velocity"));
                break;

            case ButtonCascadeModifier.Keep:
                EditorGUILayout.PropertyField(strVal, new GUIContent("Key (optional)"));
                break;
        }

        EditorGUILayout.EndVertical();
        return false;
    }

    void AddAction()
    {
        int idx = actions.arraySize;
        actions.InsertArrayElementAtIndex(idx);
        SerializedProperty a = actions.GetArrayElementAtIndex(idx);
        a.FindPropertyRelative("moment").enumValueIndex  = 0;
        a.FindPropertyRelative("command").enumValueIndex = 0;
        a.FindPropertyRelative("playbackScope").enumValueIndex = (int)FmodPlaybackScope.Local;
        a.FindPropertyRelative("playLocally").boolValue  = true;
        a.FindPropertyRelative("soundId").stringValue    = string.Empty;
        a.FindPropertyRelative("fade").boolValue         = false;
        a.FindPropertyRelative("floatValue").floatValue  = 1f;
        a.FindPropertyRelative("floatValue2").floatValue = 1f;
        a.FindPropertyRelative("parameter").stringValue  = string.Empty;
        a.FindPropertyRelative("label").stringValue      = string.Empty;
        a.FindPropertyRelative("cascade").ClearArray();
        a.isExpanded = true;
        serializedObject.ApplyModifiedProperties();
    }

    void ShowAddModifierMenu()
    {
        GenericMenu menu = new GenericMenu();
        foreach (string name in System.Enum.GetNames(typeof(ButtonCascadeModifier)))
            menu.AddItem(new GUIContent(name), false, AddModifierStep, name);
        menu.ShowAsContext();
    }

    void AddModifierStep(object modifierName)
    {
        if (pendingActionIndex < 0 || pendingActionIndex >= actions.arraySize)
            return;

        serializedObject.Update();

        SerializedProperty action  = actions.GetArrayElementAtIndex(pendingActionIndex);
        SerializedProperty cascade = action.FindPropertyRelative("cascade");

        int idx = cascade.arraySize;
        cascade.InsertArrayElementAtIndex(idx);
        SerializedProperty step = cascade.GetArrayElementAtIndex(idx);

        step.FindPropertyRelative("modifier").enumValueIndex =
            (int)System.Enum.Parse(typeof(ButtonCascadeModifier), (string)modifierName);
        step.FindPropertyRelative("stringValue").stringValue  = string.Empty;
        step.FindPropertyRelative("stringValue2").stringValue = string.Empty;
        step.FindPropertyRelative("floatValue").floatValue    = 1f;
        step.FindPropertyRelative("floatValue2").floatValue   = 1f;
        step.FindPropertyRelative("boolValue").boolValue      = false;
        step.FindPropertyRelative("intValue").intValue        = 0;
        step.FindPropertyRelative("vectorValue").vector3Value = Vector3.zero;
        step.FindPropertyRelative("target").objectReferenceValue = null;

        pendingActionIndex = -1;
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
