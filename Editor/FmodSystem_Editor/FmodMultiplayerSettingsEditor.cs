using FMODUnity;
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

        Settings settings = Settings.Instance;
        if (settings != null)
        {
            Undo.RecordObject(settings, "Apply FMOD Multiplayer Settings");

            SerializedObject serializedSettings = new SerializedObject(settings);
            serializedSettings.FindProperty("HasSourceProject").boolValue = false;
            serializedSettings.FindProperty("BankRefreshCooldown").intValue = -2;
            serializedSettings.FindProperty("ShowBankRefreshWindow").boolValue = false;
            serializedSettings.ApplyModifiedProperties();

            DisableLiveUpdate(settings.DefaultPlatform);
            DisableLiveUpdate(settings.PlayInEditorPlatform);

            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
        }

        ApplySceneButtonSettings(multiplayerSettings);
    }

    private static bool EnsureNoLegacyFmodPluginCopy()
    {
        const string packageFmodPath = "Packages/com.bisc8.betterfmod/Runtime/FmodSystem/Plugins_FMOD/CustomFMOD/FMOD";
        const string legacyFmodPath = "Assets/BISC8/BetterFMOD/FMOD";
        const string legacyMarkerPath = legacyFmodPath + "/FMODUnity.asmdef";

        if (!Directory.Exists(packageFmodPath) || !File.Exists(legacyMarkerPath))
            return true;

        bool remove = EditorUtility.DisplayDialog(
            "BISC8 Better FMOD",
            "A legacy FMOD copy exists in Assets while BetterFMOD also provides FMOD from the package. Remove Assets/BISC8/BetterFMOD/FMOD to avoid duplicate native plugins?",
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

    private static void DisableLiveUpdate(Platform platform)
    {
        if (platform == null)
            return;

        Undo.RecordObject(platform, "Apply FMOD Multiplayer Settings");

        SerializedObject serializedPlatform = new SerializedObject(platform);
        SerializedProperty liveUpdate = serializedPlatform.FindProperty("Properties.LiveUpdate");
        if (liveUpdate != null)
        {
            SerializedProperty value = liveUpdate.FindPropertyRelative("Value");
            if (value.propertyType == SerializedPropertyType.Enum)
                value.enumValueIndex = (int)TriStateBool.Disabled;
            else
                value.intValue = (int)TriStateBool.Disabled;

            liveUpdate.FindPropertyRelative("HasValue").boolValue = true;
        }

        serializedPlatform.ApplyModifiedProperties();
        EditorUtility.SetDirty(platform);
    }
}
