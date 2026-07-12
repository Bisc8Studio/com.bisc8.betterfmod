using System.IO;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FmodMultiplayerSettings))]
public class FmodMultiplayerSettingsEditor : Editor
{
    private SerializedProperty isMultiplayer;
    private SerializedProperty autoConfigureFmodButtons;

    private void OnEnable()
    {
        isMultiplayer = serializedObject.FindProperty("isMultiplayer");
        autoConfigureFmodButtons = serializedObject.FindProperty("autoConfigureFmodButtons");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUI.BeginChangeCheck();

        EditorGUILayout.LabelField("Playback Mode", EditorStyles.miniBoldLabel);
        int selectedMode = isMultiplayer.boolValue ? 1 : 0;
        selectedMode = GUILayout.Toolbar(selectedMode, new[] { "Singleplayer", "Multiplayer" });
        isMultiplayer.boolValue = selectedMode == 1;

        EditorGUILayout.PropertyField(autoConfigureFmodButtons, new GUIContent("Auto Configure FmodButtons"));

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();

            if (isMultiplayer.boolValue)
            {
                ApplyMultiplayerFmodSettings((FmodMultiplayerSettings)target);
            }

            return;
        }

        serializedObject.ApplyModifiedProperties();

        if (isMultiplayer.boolValue && GUILayout.Button("Apply FMOD Multiplayer Settings"))
        {
            ApplyMultiplayerFmodSettings((FmodMultiplayerSettings)target);
        }
    }

    private static void ApplyMultiplayerFmodSettings(FmodMultiplayerSettings multiplayerSettings)
    {
        if (!EnsureNoLegacyFmodPluginCopy())
            return;

        ApplySceneButtonSettings(multiplayerSettings);
    }

    private static bool EnsureNoLegacyFmodPluginCopy()
    {
        const string packageFmodPath = "Packages/com.bisc8.simplefmod/Runtime/FmodSystem/Plugins_FMOD/CustomFMOD/FMOD";
        const string legacyFmodPath = "Assets/BISC8/FMODB8/FMOD";
        const string legacyMarkerPath = legacyFmodPath + "/FMODUnity.asmdef";

        if (!Directory.Exists(packageFmodPath) || !File.Exists(legacyMarkerPath))
            return true;

        bool remove = EditorUtility.DisplayDialog(
            "FMODB8",
            "A legacy FMOD copy exists in Assets while FMODB8 also provides FMOD from the package. Remove Assets/BISC8/FMODB8/FMOD to avoid duplicate native plugins?",
            "Remove Legacy Copy",
            "Cancel"
        );

        if (!remove)
            return false;

        return FMODInstaller.RemoveLegacyInstalledFMODCopy(true);
    }

    private static void ApplySceneButtonSettings(FmodMultiplayerSettings multiplayerSettings)
    {
        bool enableMultiplayer = multiplayerSettings == null || multiplayerSettings.IsMultiplayer;
        bool autoConfigureButtons = multiplayerSettings == null || multiplayerSettings.AutoConfigureFmodButtons;

        foreach (FmodButton button in FindObjectsByType<FmodButton>(FindObjectsSortMode.None))
        {
            if (button == null)
                continue;

            Undo.RecordObject(button, "Apply FMOD Multiplayer Settings");

            if (autoConfigureButtons)
                button.ApplyMultiplayerDefaults(enableMultiplayer);
            else
                button.ValidateActions();

            EditorUtility.SetDirty(button);
        }
    }

}
