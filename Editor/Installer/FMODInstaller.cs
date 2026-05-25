using UnityEditor;
using UnityEngine;
using System.IO;

[InitializeOnLoad]
public static class FMODInstaller
{
    static FMODInstaller()
    {
        EditorApplication.delayCall += CheckFMOD;
    }

    static void CheckFMOD()
    {
        if (System.Type.GetType("FMODUnity.RuntimeManager, FMODUnity") != null)
            return;

        bool install = EditorUtility.DisplayDialog(
            "FMOD Required",
            "BISC8 Better FMOD requires FMOD for Unity.\n\nInstall automatically?",
            "Install",
            "Cancel"
        );

        if (install)
        {
            InstallFMOD();
        }
    }

    static void InstallFMOD()
    {
        string manifestPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "Packages/manifest.json"
        );

        if (!File.Exists(manifestPath))
        {
            Debug.LogError("manifest.json not found.");
            return;
        }

        string json = File.ReadAllText(manifestPath);

        if (json.Contains("com.bisc8.customfmod"))
        {
            Debug.Log("BISC8 FMOD already installed.");
            return;
        }

        string dependency =
            "\"com.bisc8.customfmod\": \"https://github.com/Bisc8Studio/com.bisc8.customfmod.git\",";

        json = json.Replace(
            "\"dependencies\": {",
            "\"dependencies\": {\n    " + dependency
        );

        File.WriteAllText(manifestPath, json);

        Debug.Log("Installing BISC8 FMOD...");

        AssetDatabase.Refresh();
    }
}