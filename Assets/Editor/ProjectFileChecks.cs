#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

// Read-only validation: no scene loading, saving, profile edits or asset replacement.
public static class ProjectFileChecks
{
    [MenuItem("Tools/Fishing/Validate Project Files")]
    public static void Validate()
    {
        string[] scenes = { "Assets/Scenes/MainMenu.unity", "Assets/Scenes/Village.unity", "Assets/Scenes/Lake.unity" };
        string[] prefabs = { "Assets/Prefabs/UI/VillagePanelUI.prefab", "Assets/Prefabs/UI/VillageInteractionUI.prefab",
            "Assets/Prefabs/UI/JourneyHudUI.prefab", "Assets/Prefabs/UI/FishJournalUI.prefab",
            "Assets/Prefabs/UI/VillageShellUI.prefab", "Assets/Prefabs/GameAudio.prefab" };
        var errors = new List<string>();
        foreach (string path in scenes) CheckFile(path, errors);
        foreach (string path in prefabs) CheckFile(path, errors);
        string[] expectedGuids = { "4b91303bb190ef6a04c11df42659ae22", "832a47476a51ebdc010ae14c5bf92009",
            "aa67f78a8ebef4267852dd5e8a4bc698", "d44f025ff16c6c1f518178d9fa2ca8e4",
            "fbd2fe7e21213b74b5d36a559425e56b", "f2f79adf0de7e82401adb9d6f68cd87d" };
        for (int i = 0; i < prefabs.Length; i++)
            if (AssetDatabase.AssetPathToGUID(prefabs[i]) != expectedGuids[i])
                errors.Add("Wrong or missing prefab meta: " + prefabs[i]);
        foreach (string directory in new[] { "Assets/Scripts", "Assets/Editor", "Assets/Scenes", "Assets/Prefabs" })
        {
            if (!Directory.Exists(directory)) { errors.Add("Missing folder: " + directory); continue; }
            foreach (string file in Directory.GetFiles(directory, "*", SearchOption.AllDirectories))
            {
                if (!file.EndsWith(".cs") && !file.EndsWith(".unity") && !file.EndsWith(".prefab")) continue;
                string text;
                try { text = File.ReadAllText(file); }
                catch (Exception error) { errors.Add(file + ": " + error.Message); continue; }
                if (Regex.IsMatch(text, @"^(<<<<<<<|=======|>>>>>>>)", RegexOptions.Multiline))
                    errors.Add("Unresolved Git conflict: " + file);
                if (file.StartsWith("Assets/Scenes/_Recovery", StringComparison.Ordinal)) continue;
                if (file.EndsWith(".unity") || file.EndsWith(".prefab"))
                    foreach (Match match in Regex.Matches(text, @"m_SourcePrefab:\s*\{[^}]*guid:\s*([a-f0-9]{32})"))
                        if (string.IsNullOrEmpty(AssetDatabase.GUIDToAssetPath(match.Groups[1].Value)))
                            errors.Add("Missing source prefab " + match.Groups[1].Value + " in " + file);
            }
        }
        var enabled = new List<string>();
        foreach (var scene in EditorBuildSettings.scenes) if (scene.enabled) enabled.Add(scene.path);
        if (enabled.Count != scenes.Length) errors.Add("Build Scene List must contain MainMenu, Village and Lake only.");
        else for (int i = 0; i < scenes.Length; i++)
            if (enabled[i] != scenes[i]) errors.Add("Wrong build scene order at index " + i + ": " + enabled[i]);
        if (errors.Count == 0)
            Debug.Log("Project files check PASSED: scenes, Village prefab GUIDs, source prefab references, conflict markers and global build scene list. Play/build still require separate verification.");
        else Debug.LogError("Project files check FAILED:\n" + string.Join("\n", errors));
    }
    private static void CheckFile(string path, List<string> errors)
    {
        if (!File.Exists(path)) errors.Add("Missing asset: " + path);
        if (!File.Exists(path + ".meta")) errors.Add("Missing meta: " + path + ".meta");
    }
}
#endif
