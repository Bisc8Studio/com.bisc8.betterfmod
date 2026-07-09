using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

internal static class FmodEventsGenerator
{
    private const string RelativeOutputPath = "Runtime/Core/FmodEvents.Generated.cs";
    private static readonly Regex InvalidCharacters = new("[^a-zA-Z0-9_]", RegexOptions.Compiled);

    [MenuItem("FMOD/BISC8 Better FMOD/Generate FmodEvents", false, 21)]
    private static void GenerateFromMenu()
    {
        Generate();
    }

    internal static void Generate()
    {
        Dictionary<string, string> events = CollectEvents();
        string outputAssetPath = GetOutputAssetPath();
        string absolutePath = Path.GetFullPath(outputAssetPath);
        string directory = Path.GetDirectoryName(absolutePath);

        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        string content = BuildSource(events);

        if (File.Exists(absolutePath) && File.ReadAllText(absolutePath) == content)
            return;

        File.WriteAllText(absolutePath, content, Encoding.UTF8);
        AssetDatabase.ImportAsset(outputAssetPath);
    }

    private static string GetOutputAssetPath()
    {
        string[] guids = AssetDatabase.FindAssets("FmodEventsGenerator t:Script");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid).Replace("\\", "/");

            if (!path.EndsWith("Editor/FmodEventsGenerator.cs"))
                continue;

            string packageRoot = path.Substring(0, path.Length - "Editor/FmodEventsGenerator.cs".Length).TrimEnd('/');
            return packageRoot + "/" + RelativeOutputPath;
        }

        return RelativeOutputPath;
    }

    private static Dictionary<string, string> CollectEvents()
    {
        Dictionary<string, string> result = new();
        HashSet<string> seenIds = new();
        string[] guids = AssetDatabase.FindAssets("t:CreateFmodList");
        System.Array.Sort(guids);

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            CreateFmodList list = AssetDatabase.LoadAssetAtPath<CreateFmodList>(path);

            if (list == null || list.events == null)
                continue;

            foreach (FMODListEntry entry in list.events)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.id) || !seenIds.Add(entry.id))
                    continue;

                string constantName = CreateUniqueName(entry.id, result);
                result.Add(constantName, entry.id);
            }
        }

        return result;
    }

    private static string CreateUniqueName(string id, Dictionary<string, string> existing)
    {
        string name = InvalidCharacters.Replace(id, "_").Trim('_');

        if (string.IsNullOrWhiteSpace(name))
            name = "Event";

        if (char.IsDigit(name[0]))
            name = "_" + name;

        string[] parts = name.Split('_');
        StringBuilder builder = new();

        foreach (string part in parts)
        {
            if (string.IsNullOrWhiteSpace(part))
                continue;

            builder.Append(char.ToUpperInvariant(part[0]));

            if (part.Length > 1)
                builder.Append(part.Substring(1));
        }

        name = builder.Length == 0 ? "Event" : builder.ToString();
        string unique = name;
        int index = 2;

        while (existing.ContainsKey(unique))
        {
            unique = name + index;
            index++;
        }

        return unique;
    }

    private static string BuildSource(Dictionary<string, string> events)
    {
        StringBuilder builder = new();
        builder.AppendLine("/// <summary>");
        builder.AppendLine("/// Fornece ids de eventos BetterFMOD gerados automaticamente.");
        builder.AppendLine("/// </summary>");
        builder.AppendLine("public static class FmodEvents");
        builder.AppendLine("{");

        foreach (KeyValuePair<string, string> entry in events)
        {
            builder.AppendLine("    /// <summary>");
            builder.AppendLine("    /// Id de evento BetterFMOD: " + EscapeForXml(entry.Value));
            builder.AppendLine("    /// </summary>");
            builder.AppendLine("    public const string " + entry.Key + " = \"" + EscapeForCSharp(entry.Value) + "\";");
        }

        builder.AppendLine("}");
        return builder.ToString();
    }

    private static string EscapeForCSharp(string value)
    {
        return value.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }

    private static string EscapeForXml(string value)
    {
        return value.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
    }
}

internal sealed class FmodEventsAssetPostprocessor : AssetPostprocessor
{
    private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        if (ShouldGenerate(importedAssets) || ShouldGenerate(deletedAssets) || ShouldGenerate(movedAssets) || ShouldGenerate(movedFromAssetPaths))
            EditorApplication.delayCall += FmodEventsGenerator.Generate;
    }

    private static bool ShouldGenerate(string[] paths)
    {
        foreach (string path in paths)
        {
            if (path.EndsWith(".asset"))
                return true;
        }

        return false;
    }
}
