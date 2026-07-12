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
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Events", EditorStyles.boldLabel);
        if (GUILayout.Button("Sync Stages From FMOD", EditorStyles.miniButton, GUILayout.Width(150f)))
            SyncAllStagesFromFmod(eventsProp);

        if (GUILayout.Button("Apply Stages To FMOD", EditorStyles.miniButton, GUILayout.Width(135f)))
            SyncAllStagesToFmod(eventsProp);
        EditorGUILayout.EndHorizontal();

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

    private void SyncAllStagesFromFmod(SerializedProperty eventsProp)
    {
        for (int i = 0; i < eventsProp.arraySize; i++)
        {
            SerializedProperty entry = eventsProp.GetArrayElementAtIndex(i);
            SerializedProperty reference = entry.FindPropertyRelative("reference");
            SerializedProperty stage = entry.FindPropertyRelative("stageInProject");
            SyncStageFromFmod(reference, stage, true);
        }
    }

    private void SyncAllStagesToFmod(SerializedProperty eventsProp)
    {
        for (int i = 0; i < eventsProp.arraySize; i++)
        {
            SerializedProperty entry = eventsProp.GetArrayElementAtIndex(i);
            SerializedProperty reference = entry.FindPropertyRelative("reference");
            SerializedProperty stage = entry.FindPropertyRelative("stageInProject");
            SyncStageToFmod(reference, (StageInProject)stage.enumValueIndex);
        }
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
                SyncStageFromFmod(reference, stage, true);

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

    private static void SyncStageFromFmod(SerializedProperty reference, SerializedProperty stage, bool force = false)
    {
        EventReference eventReference = reference.GetEventReference();
        if (FmodStageInProjectSync.TryGetStage(eventReference, out StageInProject fmodStage, force)
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
                return "yellow";
            case StageInProject.Done:
                return "green";
            case StageInProject.Implemented:
                return "blue";
            default:
                return "red";
        }
    }

    public static int GetFmodColorIndex(StageInProject stage)
    {
        switch (stage)
        {
            case StageInProject.InProcess:
                return 3;
            case StageInProject.Done:
                return 4;
            case StageInProject.Implemented:
                return 5;
            default:
                return 1;
        }
    }

    public static Color GetSolidColor(StageInProject stage)
    {
        switch (stage)
        {
            case StageInProject.InProcess:
                return new Color(1f, 0.9f, 0f);
            case StageInProject.Done:
                return new Color(0.05f, 0.95f, 0.18f);
            case StageInProject.Implemented:
                return new Color(0f, 0.45f, 1f);
            default:
                return new Color(1f, 0.05f, 0.05f);
        }
    }

    public static Color GetEditorColor(StageInProject stage)
    {
        Color color = GetSolidColor(stage);
        return new Color(
            Mathf.Lerp(EditorGUIUtility.isProSkin ? 0.18f : 1f, color.r, 0.65f),
            Mathf.Lerp(EditorGUIUtility.isProSkin ? 0.18f : 1f, color.g, 0.65f),
            Mathf.Lerp(EditorGUIUtility.isProSkin ? 0.18f : 1f, color.b, 0.65f),
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

        if (normalized.Contains("number:"))
        {
            string numericValue = normalized.Replace("number:", string.Empty).Trim();
            if (int.TryParse(numericValue, out int colorIndex))
            {
                switch (colorIndex)
                {
                    case 1:
                        stage = StageInProject.Undone;
                        return true;
                    case 3:
                        stage = StageInProject.InProcess;
                        return true;
                    case 4:
                        stage = StageInProject.Done;
                        return true;
                    case 5:
                        stage = StageInProject.Implemented;
                        return true;
                }
            }
        }

        if (normalized.Contains("rgb:"))
        {
            string[] parts = normalized.Replace("rgb:", string.Empty).Split(',');
            if (parts.Length >= 3
                && float.TryParse(parts[0], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float r)
                && float.TryParse(parts[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float g)
                && float.TryParse(parts[2], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float b))
            {
                if (b > r && b > g)
                    stage = StageInProject.Implemented;
                else if (g > r && g >= b)
                    stage = StageInProject.Done;
                else if (r > 0.7f && g > 0.5f)
                    stage = StageInProject.InProcess;
                else
                    stage = StageInProject.Undone;

                return true;
            }
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
    private static readonly System.Collections.Generic.HashSet<string> LoggedUnknownColorValues = new();
    private static double nextConnectionLogTime;

    public static bool TrySetStage(EventReference eventReference, StageInProject stage)
    {
        if (!TryGetLookupKeys(eventReference, out string lookupKey, out string eventPath))
            return false;

        string color = StageInProjectColors.GetFmodColorName(stage);
        int colorIndex = StageInProjectColors.GetFmodColorIndex(stage);
        string command = string.Format(
            @"(function(lookupKey, eventPath, color, colorIndex) {{
                var eventRef = studio.project.lookup(lookupKey);
                if (!eventRef && eventPath) eventRef = studio.project.lookup(eventPath);
                if (!eventRef) return false;
                var fields = [""color"", ""colour"", ""eventColor"", ""eventColour"", ""labelColor"", ""labelColour"", ""markerColor"", ""markerColour"", ""displayColor"", ""displayColour""];
                var values = [color, color.charAt(0).toUpperCase() + color.slice(1), colorIndex];
                for (var i = 0; i < fields.length; i++) {{
                    for (var j = 0; j < values.length; j++) {{
                        try {{
                            if (eventRef[fields[i]] !== undefined) {{
                                eventRef[fields[i]] = values[j];
                                return true;
                            }}
                        }} catch (e) {{}}
                    }}
                }}
                try {{
                    if (eventRef.properties && eventRef.properties.color !== undefined) {{
                        for (var k = 0; k < values.length; k++) {{
                            try {{
                                eventRef.properties.color = values[k];
                                return true;
                            }} catch (e) {{}}
                        }}
                    }}
                }} catch (e) {{}}
                return false;
            }})(""{0}"", ""{1}"", ""{2}"", {3});",
            EscapeJs(lookupKey),
            EscapeJs(eventPath),
            EscapeJs(color),
            colorIndex);

        return TrySendScriptCommand(command);
    }

    public static bool TryGetStage(EventReference eventReference, out StageInProject stage, bool force = false)
    {
        stage = StageInProject.Undone;

        if (!TryGetLookupKeys(eventReference, out string lookupKey, out string eventPath))
            return false;

        if (!force
            && NextStudioReadTimeByEvent.TryGetValue(lookupKey, out double nextStudioReadTime)
            && EditorApplication.timeSinceStartup < nextStudioReadTime)
        {
            return false;
        }

        NextStudioReadTimeByEvent[lookupKey] = EditorApplication.timeSinceStartup + 0.5d;

        string command = string.Format(
            @"(function(lookupKey, eventPath) {{
                function stringifyColor(value) {{
                    if (value === undefined || value === null) return """";
                    if (typeof value === ""string"") return value;
                    if (typeof value === ""number"") return ""number:"" + value;
                    if (value.name !== undefined) return String(value.name);
                    if (value.displayName !== undefined) return String(value.displayName);
                    if (value.r !== undefined && value.g !== undefined && value.b !== undefined) return ""rgb:"" + value.r + "","" + value.g + "","" + value.b;
                    return String(value);
                }}

                var eventRef = studio.project.lookup(lookupKey);
                if (!eventRef && eventPath) eventRef = studio.project.lookup(eventPath);
                if (!eventRef) return """";
                var fields = [""color"", ""colour"", ""eventColor"", ""eventColour"", ""labelColor"", ""labelColour"", ""markerColor"", ""markerColour"", ""displayColor"", ""displayColour""];
                for (var i = 0; i < fields.length; i++) {{
                    try {{
                        if (eventRef[fields[i]] !== undefined && eventRef[fields[i]] !== null) {{
                            return stringifyColor(eventRef[fields[i]]);
                        }}
                    }} catch (e) {{}}
                }}
                try {{
                    if (eventRef.properties && eventRef.properties.color !== undefined) return stringifyColor(eventRef.properties.color);
                }} catch (e) {{}}
                return """";
            }})(""{0}"", ""{1}"");",
            EscapeJs(lookupKey),
            EscapeJs(eventPath));

        string color = TryGetScriptOutput(command);
        bool parsed = StageInProjectColors.TryGetStageFromFmodColor(color, out stage);
        if (!parsed && !string.IsNullOrWhiteSpace(color) && LoggedUnknownColorValues.Add(color))
            Debug.LogWarning("BetterFMOD: cor do evento FMOD nao reconhecida para Stage In Project: " + color);

        return parsed;
    }

    private static bool TryGetLookupKeys(EventReference eventReference, out string lookupKey, out string eventPath)
    {
        lookupKey = null;
        eventPath = eventReference.Path;

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
                eventPath = editorEventRef.Path;
                return true;
            }
        }

        return false;
    }

    private static bool TrySendScriptCommand(string command)
    {
        try
        {
            return EditorUtils.SendScriptCommand(command);
        }
        catch
        {
            LogStudioConnectionHint();
            return false;
        }
    }

    private static string TryGetScriptOutput(string command)
    {
        try
        {
            string output = EditorUtils.GetScriptOutput(command);
            if (output == null)
                LogStudioConnectionHint();

            return output;
        }
        catch
        {
            LogStudioConnectionHint();
            return null;
        }
    }

    private static void LogStudioConnectionHint()
    {
        if (EditorApplication.timeSinceStartup < nextConnectionLogTime)
            return;

        nextConnectionLogTime = EditorApplication.timeSinceStartup + 10d;
        Debug.LogWarning("BetterFMOD: nao consegui ler/escrever a cor do evento no FMOD Studio. Verifique se o FMOD Studio esta aberto com scripting habilitado e use 'Sync Stages From FMOD' novamente.");
    }

    private static string EscapeJs(string value)
    {
        return value?.Replace("\\", "\\\\").Replace("\"", "\\\"") ?? string.Empty;
    }
}
