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
        if (GUILayout.Button("Get To FMOD", EditorStyles.miniButton, GUILayout.Width(150f)))
            SyncAllStagesFromFmod(eventsProp);

        if (GUILayout.Button("Send To FMOD", EditorStyles.miniButton, GUILayout.Width(135f)))
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
            FmodStageInProjectSync.LearnColorFromFmod(reference.GetEventReference(), true);
        }

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

        StageInProject currentStage = (StageInProject)stage.enumValueIndex;
        Color stageColor = StageInProjectColors.GetEditorColor(currentStage);
        Color oldBackground = GUI.backgroundColor;
        GUI.backgroundColor = stageColor;

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUI.backgroundColor = oldBackground;

        EditorGUI.indentLevel++;

        bool removeRequested = false;
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
            removeRequested = true;
        }
        EditorGUILayout.EndHorizontal();

        if (!removeRequested)
        {
            if (entry.isExpanded)
            {
                EditorGUILayout.PropertyField(id, new GUIContent("ID"));
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(reference, new GUIContent("Reference"));
                EditorGUI.EndChangeCheck();

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(stage, new GUIContent("Stage In Project"));
                if (EditorGUI.EndChangeCheck() && FmodStageInProjectSync.CanUseStudio(false))
                    SyncStageToFmod(reference, (StageInProject)stage.enumValueIndex);
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(stage, GUIContent.none, GUILayout.MaxWidth(160f));
                if (EditorGUI.EndChangeCheck() && FmodStageInProjectSync.CanUseStudio(false))
                    SyncStageToFmod(reference, (StageInProject)stage.enumValueIndex);

                GUILayout.Label(reference.GetEventReferencePath(), EditorStyles.miniLabel);
                EditorGUILayout.EndHorizontal();
            }
        }

        EditorGUI.indentLevel--;
        EditorGUILayout.EndVertical();

        if (Event.current.type == EventType.Repaint)
        {
            Rect colorRect = GUILayoutUtility.GetLastRect();
            colorRect.width = 5f;
            colorRect.x += 1f;
            colorRect.y += 1f;
            colorRect.height -= 2f;
            EditorGUI.DrawRect(colorRect, StageInProjectColors.GetSolidColor(currentStage));
        }

        if (removeRequested)
            eventsProp.DeleteArrayElementAtIndex(index);
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
    private static readonly System.Collections.Generic.Dictionary<string, FmodColorBinding> ColorBindingByEvent = new();
    private static readonly System.Collections.Generic.Dictionary<StageInProject, FmodColorWriteValue> WriteValueByStage = new();
    private static readonly System.Collections.Generic.HashSet<string> LoggedUnknownColorValues = new();
    private static double nextConnectionLogTime;

    public static bool CanUseStudio(bool showDialog = true)
    {
        bool connected = false;

        try
        {
            connected = EditorUtils.IsConnectedToStudio();
        }
        catch
        {
            connected = false;
        }

        if (!connected && showDialog)
        {
            EditorUtility.DisplayDialog(
                "FMODB8",
                "Nao consegui conectar no FMOD Studio ainda. Abra o FMOD Studio com o projeto carregado e tente de novo. Se acabou de abrir, espere alguns segundos e clique novamente.",
                "OK");
        }

        return connected;
    }

    public static bool TrySetStage(EventReference eventReference, StageInProject stage)
    {
        if (!TryGetLookupKeys(eventReference, out string lookupKey, out string eventPath))
            return false;

        if (!ColorBindingByEvent.TryGetValue(lookupKey, out FmodColorBinding binding))
        {
            LearnColorFromFmod(eventReference, true);
            ColorBindingByEvent.TryGetValue(lookupKey, out binding);
        }

        if (string.IsNullOrEmpty(binding.Field))
        {
            Debug.LogWarning("FMODB8: nao consegui descobrir o campo de cor deste evento no FMOD. Send ignorado para evitar limpar a cor.");
            return false;
        }

        if (!TryGetWriteValue(stage, binding.ValueKind, out FmodColorWriteValue writeValue))
        {
            Debug.LogWarning("FMODB8: ainda nao sei o valor real do FMOD para '" + stage + "' no formato '" + binding.ValueKind + "'. Use Get To FMOD em pelo menos um evento com essa cor, ou altere esse stage no FMOD uma vez, antes de mandar pela Unity.");
            return false;
        }

        string command = string.Format(
            @"(function(lookupKey, eventPath, ownerName, field, valueKind, stringValue, numberValue, r, g, b) {{
                function capture(value) {{
                    if (value === undefined || value === null) return {{ kind: ""null"", value: null }};
                    if (typeof value === ""number"") return {{ kind: ""number"", value: value }};
                    if (typeof value === ""string"") return {{ kind: ""string"", value: value }};
                    if (value && typeof value === ""object"" && value.r !== undefined && value.g !== undefined && value.b !== undefined) {{
                        return {{ kind: ""rgb"", value: {{ r: value.r, g: value.g, b: value.b }} }};
                    }}
                    return {{ kind: ""object"", value: value }};
                }}

                function restore(owner, previous) {{
                    if (previous.kind === ""rgb"") {{
                        var current = owner[field];
                        if (current && current.r !== undefined && current.g !== undefined && current.b !== undefined) {{
                            current.r = previous.value.r;
                            current.g = previous.value.g;
                            current.b = previous.value.b;
                            owner[field] = current;
                            return;
                        }}
                    }}

                    owner[field] = previous.value;
                }}

                function assign(owner) {{
                    if (!owner || owner[field] === undefined || owner[field] === null) return false;

                    if (valueKind === ""number"") {{
                        owner[field] = numberValue;
                        return true;
                    }}

                    if (valueKind === ""string"") {{
                        owner[field] = stringValue;
                        return true;
                    }}

                    if (valueKind === ""rgb"") {{
                        var current = owner[field];
                        if (current.r !== undefined && current.g !== undefined && current.b !== undefined) {{
                            current.r = r;
                            current.g = g;
                            current.b = b;
                            owner[field] = current;
                            return true;
                        }}
                    }}

                    return false;
                }}

                function verify(owner) {{
                    var current = owner[field];

                    if (valueKind === ""number"") return current === numberValue;

                    if (valueKind === ""string"") {{
                        return String(current) === String(stringValue);
                    }}

                    if (valueKind === ""rgb"") {{
                        return current
                            && current.r !== undefined
                            && Math.abs(current.r - r) < 0.001
                            && Math.abs(current.g - g) < 0.001
                            && Math.abs(current.b - b) < 0.001;
                    }}

                    return false;
                }}

                var eventRef = studio.project.lookup(lookupKey);
                if (!eventRef && eventPath) eventRef = studio.project.lookup(eventPath);
                if (!eventRef) return false;
                var owner = ownerName === ""properties"" ? eventRef.properties : eventRef;
                if (!owner || owner[field] === undefined || owner[field] === null) return false;

                var previous = capture(owner[field]);

                try {{
                    if (!assign(owner)) return false;
                    if (verify(owner)) return true;
                    restore(owner, previous);
                    return false;
                }} catch (e) {{
                    try {{
                        restore(owner, previous);
                    }} catch (restoreError) {{}}
                    return false;
                }}
            }})(""{0}"", ""{1}"", ""{2}"", ""{3}"", ""{4}"", ""{5}"", {6}, {7}, {8}, {9});",
            EscapeJs(lookupKey),
            EscapeJs(eventPath),
            EscapeJs(binding.Owner),
            EscapeJs(binding.Field),
            EscapeJs(binding.ValueKind),
            EscapeJs(writeValue.StringValue),
            writeValue.NumberValue.ToString(System.Globalization.CultureInfo.InvariantCulture),
            writeValue.R.ToString(System.Globalization.CultureInfo.InvariantCulture),
            writeValue.G.ToString(System.Globalization.CultureInfo.InvariantCulture),
            writeValue.B.ToString(System.Globalization.CultureInfo.InvariantCulture));

        bool sent = TrySendScriptCommand(command);
        if (!sent)
            Debug.LogWarning("FMODB8: Send To FMOD falhou e tentei preservar a cor original. Use 'Get To FMOD' novamente; se ainda ler a cor, nada foi perdido.");

        return sent;
    }

    public static bool LearnColorFromFmod(EventReference eventReference, bool force = false)
    {
        return TryGetStage(eventReference, out _, force);
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
                function stringifyColor(ownerName, field, value) {{
                    if (value === undefined || value === null) return """";
                    if (typeof value === ""string"") return ownerName + ""|"" + field + ""|string|"" + value;
                    if (typeof value === ""number"") return ownerName + ""|"" + field + ""|number|"" + value;
                    if (value.r !== undefined && value.g !== undefined && value.b !== undefined) return ownerName + ""|"" + field + ""|rgb|"" + value.r + "","" + value.g + "","" + value.b;
                    if (value.name !== undefined) return ownerName + ""|"" + field + ""|string|"" + String(value.name);
                    if (value.displayName !== undefined) return ownerName + ""|"" + field + ""|string|"" + String(value.displayName);
                    return ownerName + ""|"" + field + ""|string|"" + String(value);
                }}

                var eventRef = studio.project.lookup(lookupKey);
                if (!eventRef && eventPath) eventRef = studio.project.lookup(eventPath);
                if (!eventRef) return """";
                var fields = [""color"", ""colour"", ""eventColor"", ""eventColour"", ""labelColor"", ""labelColour"", ""markerColor"", ""markerColour"", ""displayColor"", ""displayColour""];
                for (var i = 0; i < fields.length; i++) {{
                    try {{
                        if (eventRef[fields[i]] !== undefined && eventRef[fields[i]] !== null) {{
                            return stringifyColor(""event"", fields[i], eventRef[fields[i]]);
                        }}
                    }} catch (e) {{}}
                }}
                try {{
                    if (eventRef.properties && eventRef.properties.color !== undefined && eventRef.properties.color !== null) return stringifyColor(""properties"", ""color"", eventRef.properties.color);
                }} catch (e) {{}}
                return """";
            }})(""{0}"", ""{1}"");",
            EscapeJs(lookupKey),
            EscapeJs(eventPath));

        string color = TryGetScriptOutput(command);
        if (TryParseColorBinding(color, out FmodColorBinding binding, out string colorValue))
        {
            ColorBindingByEvent[lookupKey] = binding;
        }

        bool parsed = StageInProjectColors.TryGetStageFromFmodColor(colorValue, out stage);
        if (parsed && binding.IsValid)
            LearnWriteValue(stage, binding, colorValue);

        if (!parsed && !string.IsNullOrWhiteSpace(colorValue) && LoggedUnknownColorValues.Add(colorValue))
            Debug.LogWarning("FMODB8: cor do evento FMOD nao reconhecida para Stage In Project: " + colorValue);

        return parsed;
    }

    private static bool TryParseColorBinding(string rawValue, out FmodColorBinding binding, out string colorValue)
    {
        binding = default;
        colorValue = rawValue;

        if (string.IsNullOrWhiteSpace(rawValue))
            return false;

        string[] parts = rawValue.Split(new[] { '|' }, 4);
        if (parts.Length != 4)
            return false;

        binding = new FmodColorBinding
        {
            Owner = parts[0],
            Field = parts[1],
            ValueKind = parts[2]
        };

        colorValue = parts[2] + ":" + parts[3];
        return true;
    }

    private static void LearnWriteValue(StageInProject stage, FmodColorBinding binding, string colorValue)
    {
        FmodColorWriteValue value = new FmodColorWriteValue
        {
            ValueKind = binding.ValueKind
        };

        if (binding.ValueKind == "number")
        {
            string rawNumber = colorValue.Replace("number:", string.Empty).Trim();
            if (!float.TryParse(rawNumber, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float numberValue))
                return;

            value.NumberValue = numberValue;
        }
        else if (binding.ValueKind == "rgb")
        {
            string[] parts = colorValue.Replace("rgb:", string.Empty).Split(',');
            if (parts.Length < 3
                || !float.TryParse(parts[0], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float r)
                || !float.TryParse(parts[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float g)
                || !float.TryParse(parts[2], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float b))
            {
                return;
            }

            value.R = r;
            value.G = g;
            value.B = b;
        }
        else
        {
            int separatorIndex = colorValue.IndexOf(':');
            value.StringValue = separatorIndex >= 0 ? colorValue.Substring(separatorIndex + 1) : colorValue;
        }

        WriteValueByStage[stage] = value;
    }

    private static bool TryGetWriteValue(StageInProject stage, string valueKind, out FmodColorWriteValue value)
    {
        if (WriteValueByStage.TryGetValue(stage, out value) && value.ValueKind == valueKind)
            return true;

        if (valueKind == "rgb")
        {
            Color color = StageInProjectColors.GetSolidColor(stage);
            value = new FmodColorWriteValue
            {
                ValueKind = "rgb",
                R = color.r,
                G = color.g,
                B = color.b
            };
            return true;
        }

        value = default;
        return false;
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
        Debug.LogWarning("FMODB8: nao consegui ler/escrever a cor do evento no FMOD Studio. Verifique se o FMOD Studio esta aberto com scripting habilitado e use 'Get To FMOD' novamente.");
    }

    private static string EscapeJs(string value)
    {
        return value?.Replace("\\", "\\\\").Replace("\"", "\\\"") ?? string.Empty;
    }

    private struct FmodColorBinding
    {
        public string Owner;
        public string Field;
        public string ValueKind;

        public bool IsValid => !string.IsNullOrEmpty(Field) && !string.IsNullOrEmpty(ValueKind);
    }

    private struct FmodColorWriteValue
    {
        public string ValueKind;
        public string StringValue;
        public float NumberValue;
        public float R;
        public float G;
        public float B;
    }
}
