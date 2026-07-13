using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

internal static class FmodEventsGenerator
{
    private const string OutputFolder = "Assets/BISC8/FMODB8/Generated";
    private const string OutputAssetPath = OutputFolder + "/FmodEvents.Generated.cs";
    private const string AssemblyReferencePath = OutputFolder + "/BISC8.FMODB8.Generated.asmref";
    private const string AssemblyReferenceContent = "{\n  \"reference\": \"GUID:a684a620fa772e14c926ef1b853f01b6\"\n}\n";
    private static readonly Regex InvalidCharacters = new("[^a-zA-Z0-9_]", RegexOptions.Compiled);

    [MenuItem("FMOD/FMODB8/Generate Events", false, 22)]
    private static void GenerateFromMenu()
    {
        Generate();
    }

    internal static void Generate()
    {
        Dictionary<string, string> events = CollectEvents();
        string absolutePath = Path.GetFullPath(OutputAssetPath);
        string directory = Path.GetDirectoryName(absolutePath);

        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        EnsureAssemblyReference();

        string content = BuildSource(events);

        if (File.Exists(absolutePath) && File.ReadAllText(absolutePath) == content)
            return;

        File.WriteAllText(absolutePath, content, Encoding.UTF8);
        AssetDatabase.ImportAsset(OutputAssetPath);
    }

    private static void EnsureAssemblyReference()
    {
        string absolutePath = Path.GetFullPath(AssemblyReferencePath);
        if (File.Exists(absolutePath) && File.ReadAllText(absolutePath) == AssemblyReferenceContent)
            return;

        File.WriteAllText(absolutePath, AssemblyReferenceContent, Encoding.UTF8);
        AssetDatabase.ImportAsset(AssemblyReferencePath);
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
        builder.AppendLine("/// Fornece ids de eventos FMODB8 gerados automaticamente.");
        builder.AppendLine("/// </summary>");
        builder.AppendLine("public static class FmodEvents");
        builder.AppendLine("{");

        foreach (KeyValuePair<string, string> entry in events)
        {
            builder.AppendLine("    /// <summary>");
            builder.AppendLine("    /// Id de evento FMODB8: " + EscapeForXml(entry.Value));
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
    private static bool generateQueued;

    private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        if (AssetDatabase.IsAssetImportWorkerProcess())
            return;

        if (generateQueued)
            return;

        if (!ShouldGenerate(importedAssets) && !ShouldGenerate(deletedAssets) && !ShouldGenerate(movedAssets) && !ShouldGenerate(movedFromAssetPaths))
            return;

        generateQueued = true;
        EditorApplication.delayCall += GenerateWhenEditorIsReady;
    }

    private static void GenerateWhenEditorIsReady()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            EditorApplication.delayCall += GenerateWhenEditorIsReady;
            return;
        }

        generateQueued = false;
        FmodEventsGenerator.Generate();
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
