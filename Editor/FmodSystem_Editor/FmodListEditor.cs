using FMODUnity;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CreateFmodList))]
public class CreateFmodListEditor : Editor
{
    private const float EntrySpacing = 4f;

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
            DrawEvents(eventsProp);
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawEvents(SerializedProperty eventsProp)
    {
        EditorGUILayout.Space(4);
        EditorGUILayout.LabelField("Events", EditorStyles.boldLabel);

        for (int i = 0; i < eventsProp.arraySize; i++)
        {
            SerializedProperty entry = eventsProp.GetArrayElementAtIndex(i);
            DrawEventEntry(eventsProp, entry, i);
            EditorGUILayout.Space(EntrySpacing);
        }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("+ Add Event", EditorStyles.miniButton))
        {
            int index = eventsProp.arraySize;
            eventsProp.InsertArrayElementAtIndex(index);

            SerializedProperty entry = eventsProp.GetArrayElementAtIndex(index);
            entry.FindPropertyRelative("id").stringValue = string.Empty;
            entry.FindPropertyRelative("stageInProject").enumValueIndex = (int)StageInProject.Undone;
            SerializedProperty reference = entry.FindPropertyRelative("reference");
            reference.SetEventReference(new FMOD.GUID(), string.Empty);
        }

        GUI.enabled = eventsProp.arraySize > 0;
        if (GUILayout.Button("- Remove Last", EditorStyles.miniButton))
            eventsProp.DeleteArrayElementAtIndex(eventsProp.arraySize - 1);
        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();
    }

    private void DrawEventEntry(SerializedProperty eventsProp, SerializedProperty entry, int index)
    {
        SerializedProperty id = entry.FindPropertyRelative("id");
        SerializedProperty reference = entry.FindPropertyRelative("reference");
        SerializedProperty stage = entry.FindPropertyRelative("stageInProject");

        SyncStageFromFmod(reference, stage);

        StageInProject currentStage = (StageInProject)stage.enumValueIndex;
        Color stageColor = StageInProjectColors.GetEditorColor(currentStage);
        Color oldBackground = GUI.backgroundColor;
        GUI.backgroundColor = stageColor;

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUI.backgroundColor = oldBackground;

        Rect colorRect = GUILayoutUtility.GetLastRect();
        colorRect.width = 5f;
        colorRect.x += 1f;
        colorRect.y += 1f;
        colorRect.height -= 2f;
        EditorGUI.DrawRect(colorRect, StageInProjectColors.GetSolidColor(currentStage));

        EditorGUI.indentLevel++;

        EditorGUILayout.BeginHorizontal();
        entry.isExpanded = EditorGUILayout.Foldout(entry.isExpanded, string.IsNullOrEmpty(id.stringValue) ? "Event" : id.stringValue, true, EditorStyles.boldLabel);

        GUI.enabled = index > 0;
        if (GUILayout.Button("Up", EditorStyles.miniButtonLeft, GUILayout.Width(34)))
            eventsProp.MoveArrayElement(index, index - 1);

        GUI.enabled = index < eventsProp.arraySize - 1;
        if (GUILayout.Button("Down", EditorStyles.miniButtonMid, GUILayout.Width(48)))
            eventsProp.MoveArrayElement(index, index + 1);

        GUI.enabled = true;
        if (GUILayout.Button("-", EditorStyles.miniButtonRight, GUILayout.Width(24)))
        {
            eventsProp.DeleteArrayElementAtIndex(index);
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
            return;
        }
        EditorGUILayout.EndHorizontal();

        if (entry.isExpanded)
        {
            EditorGUILayout.PropertyField(id, new GUIContent("ID"));
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(reference, new GUIContent("Reference"));
            if (EditorGUI.EndChangeCheck())
                SyncStageFromFmod(reference, stage);

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(stage, new GUIContent("Stage In Project"));
            if (EditorGUI.EndChangeCheck())
                SyncStageToFmod(reference, (StageInProject)stage.enumValueIndex);
        }
        else
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(stage, GUIContent.none, GUILayout.MaxWidth(160f));
            if (EditorGUI.EndChangeCheck())
                SyncStageToFmod(reference, (StageInProject)stage.enumValueIndex);

            GUILayout.Label(reference.GetEventReferencePath(), EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();
        }

        EditorGUI.indentLevel--;
        EditorGUILayout.EndVertical();
    }

    private static void SyncStageToFmod(SerializedProperty reference, StageInProject stage)
    {
        EventReference eventReference = reference.GetEventReference();
        FmodStageInProjectSync.TrySetStage(eventReference, stage);
    }

    private static void SyncStageFromFmod(SerializedProperty reference, SerializedProperty stage)
    {
        EventReference eventReference = reference.GetEventReference();
        if (FmodStageInProjectSync.TryGetStage(eventReference, out StageInProject fmodStage)
            && stage.enumValueIndex != (int)fmodStage)
        {
            stage.enumValueIndex = (int)fmodStage;
        }
    }
}

internal static class StageInProjectColors
{
    public static string GetFmodColorName(StageInProject stage)
    {
        switch (stage)
        {
            case StageInProject.InProcess:
                return "Yellow";
            case StageInProject.Done:
                return "Green";
            case StageInProject.Implemented:
                return "Blue";
            default:
                return "Red";
        }
    }

    public static Color GetSolidColor(StageInProject stage)
    {
        switch (stage)
        {
            case StageInProject.InProcess:
                return new Color(1f, 0.82f, 0.08f);
            case StageInProject.Done:
                return new Color(0.24f, 0.72f, 0.32f);
            case StageInProject.Implemented:
                return new Color(0.16f, 0.42f, 0.95f);
            default:
                return new Color(0.92f, 0.18f, 0.18f);
        }
    }

    public static Color GetEditorColor(StageInProject stage)
    {
        Color color = GetSolidColor(stage);
        return new Color(
            Mathf.Lerp(1f, color.r, 0.28f),
            Mathf.Lerp(1f, color.g, 0.28f),
            Mathf.Lerp(1f, color.b, 0.28f),
            1f);
    }

    public static bool TryGetStageFromFmodColor(string value, out StageInProject stage)
    {
        stage = StageInProject.Undone;

        if (string.IsNullOrWhiteSpace(value))
            return false;

        string normalized = value.Trim().ToLowerInvariant();
        if (normalized.Contains("yellow") || normalized.Contains("amarelo") || normalized.Contains("#ffff00") || normalized.Contains("#ffd"))
        {
            stage = StageInProject.InProcess;
            return true;
        }

        if (normalized.Contains("green") || normalized.Contains("verde") || normalized.Contains("#00ff00") || normalized.Contains("#0f0"))
        {
            stage = StageInProject.Done;
            return true;
        }

        if (normalized.Contains("blue") || normalized.Contains("azul") || normalized.Contains("#0000ff") || normalized.Contains("#00f"))
        {
            stage = StageInProject.Implemented;
            return true;
        }

        if (normalized.Contains("red") || normalized.Contains("vermelho") || normalized.Contains("#ff0000") || normalized.Contains("#f00"))
        {
            stage = StageInProject.Undone;
            return true;
        }

        return false;
    }
}

internal static class FmodStageInProjectSync
{
    private static readonly System.Collections.Generic.Dictionary<string, double> NextStudioReadTimeByEvent = new();

    public static bool TrySetStage(EventReference eventReference, StageInProject stage)
    {
        if (!EditorUtils.IsConnectedToStudio())
            return false;

        if (!TryGetLookupKey(eventReference, out string lookupKey))
            return false;

        string color = StageInProjectColors.GetFmodColorName(stage);
        string command = string.Format(
            @"(function(lookupKey, color) {{
                var eventRef = studio.project.lookup(lookupKey);
                if (!eventRef) return false;
                var fields = [""color"", ""colour"", ""eventColor"", ""eventColour"", ""labelColor"", ""labelColour"", ""markerColor"", ""markerColour"", ""displayColor"", ""displayColour""];
                for (var i = 0; i < fields.length; i++) {{
                    try {{
                        if (eventRef[fields[i]] !== undefined) {{
                            eventRef[fields[i]] = color;
                            return true;
                        }}
                    }} catch (e) {{}}
                }}
                try {{
                    eventRef.color = color;
                    return true;
                }} catch (e) {{}}
                return false;
            }})(""{0}"", ""{1}"");",
            EscapeJs(lookupKey),
            EscapeJs(color));

        return EditorUtils.SendScriptCommand(command);
    }

    public static bool TryGetStage(EventReference eventReference, out StageInProject stage)
    {
        stage = StageInProject.Undone;

        if (!EditorUtils.IsConnectedToStudio())
            return false;

        if (!TryGetLookupKey(eventReference, out string lookupKey))
            return false;

        if (NextStudioReadTimeByEvent.TryGetValue(lookupKey, out double nextStudioReadTime)
            && EditorApplication.timeSinceStartup < nextStudioReadTime)
        {
            return false;
        }

        NextStudioReadTimeByEvent[lookupKey] = EditorApplication.timeSinceStartup + 0.5d;

        string command = string.Format(
            @"(function(lookupKey) {{
                var eventRef = studio.project.lookup(lookupKey);
                if (!eventRef) return """";
                var fields = [""color"", ""colour"", ""eventColor"", ""eventColour"", ""labelColor"", ""labelColour"", ""markerColor"", ""markerColour"", ""displayColor"", ""displayColour""];
                for (var i = 0; i < fields.length; i++) {{
                    try {{
                        if (eventRef[fields[i]] !== undefined && eventRef[fields[i]] !== null) {{
                            return String(eventRef[fields[i]]);
                        }}
                    }} catch (e) {{}}
                }}
                return """";
            }})(""{0}"");",
            EscapeJs(lookupKey));

        string color = EditorUtils.GetScriptOutput(command);
        return StageInProjectColors.TryGetStageFromFmodColor(color, out stage);
    }

    private static bool TryGetLookupKey(EventReference eventReference, out string lookupKey)
    {
        lookupKey = null;

        if (!eventReference.Guid.IsNull)
        {
            lookupKey = eventReference.Guid.ToString();
            return true;
        }

        if (!string.IsNullOrWhiteSpace(eventReference.Path))
        {
            EditorEventRef editorEventRef = EventManager.EventFromPath(eventReference.Path);
            if (editorEventRef != null)
            {
                lookupKey = editorEventRef.Guid.ToString();
                return true;
            }
        }

        return false;
    }

    private static string EscapeJs(string value)
    {
        return value?.Replace("\\", "\\\\").Replace("\"", "\\\"") ?? string.Empty;
    }
}
