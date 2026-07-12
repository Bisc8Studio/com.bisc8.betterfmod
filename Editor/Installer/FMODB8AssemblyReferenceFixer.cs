using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public static class FMODB8AssemblyReferenceFixer
{
    private const string RuntimeAssemblyName = "BISC8.FMODB8.Runtime";
    private const string MenuPath = "FMOD/FMODB8/Fix Project Assembly References";

    [MenuItem(MenuPath, false, 22)]
    public static void FixProjectAssemblyReferences()
    {
        string[] guids = AssetDatabase.FindAssets("t:AssemblyDefinitionAsset", new[] { "Assets" });
        int changedCount = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                continue;

            string json = File.ReadAllText(path);
            if (json.Contains("\"" + RuntimeAssemblyName + "\""))
                continue;

            string updatedJson = AddRuntimeReference(json);
            if (updatedJson == json)
                continue;

            File.WriteAllText(path, updatedJson);
            changedCount++;
        }

        if (changedCount > 0)
        {
            AssetDatabase.Refresh();
            Debug.Log("[FMODB8] Added " + RuntimeAssemblyName + " reference to " + changedCount + " project assembly definition(s).");
            return;
        }

        Debug.Log("[FMODB8] No project assembly definition needed a " + RuntimeAssemblyName + " reference.");
    }

    private static string AddRuntimeReference(string json)
    {
        Match referencesMatch = Regex.Match(json, "\"references\"\\s*:\\s*\\[(?<content>[\\s\\S]*?)\\]");
        if (!referencesMatch.Success)
            return json;

        string content = referencesMatch.Groups["content"].Value;
        string replacement = string.IsNullOrWhiteSpace(content)
            ? "\"references\": [\n    \"" + RuntimeAssemblyName + "\"\n  ]"
            : "\"references\": [" + content.TrimEnd() + ",\n    \"" + RuntimeAssemblyName + "\"\n  ]";

        return json.Substring(0, referencesMatch.Index)
            + replacement
            + json.Substring(referencesMatch.Index + referencesMatch.Length);
    }
}
