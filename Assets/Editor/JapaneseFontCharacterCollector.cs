#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class JapaneseFontCharacterCollector
{
    private static readonly string[] TargetExtensions =
    {
        ".txt", ".csv", ".json"
    };

    [MenuItem("Tools/Font/Collect Japanese Characters")]
    public static void Collect()
    {
        var rootPath = Path.Combine(Application.dataPath, "GameText");
        var outputPath = Path.Combine(rootPath, "FontCharacters.txt");

        if (!Directory.Exists(rootPath))
        {
            Debug.LogError($"Folder not found: {rootPath}");
            return;
        }

        var characters = new SortedSet<char>();

        foreach (var file in Directory.GetFiles(rootPath, "*.*", SearchOption.AllDirectories))
        {
            var ext = Path.GetExtension(file).ToLowerInvariant();

            if (!System.Array.Exists(TargetExtensions, x => x == ext))
                continue;

            var text = File.ReadAllText(file, Encoding.UTF8);

            foreach (var ch in text)
            {
                if (!char.IsControl(ch))
                {
                    characters.Add(ch);
                }
            }
        }

        var builder = new StringBuilder();

        foreach (var ch in characters)
        {
            builder.Append(ch);
        }

        File.WriteAllText(outputPath, builder.ToString(), Encoding.UTF8);

        AssetDatabase.Refresh();

        Debug.Log($"Collected {characters.Count} characters: {outputPath}");
    }
}
#endif