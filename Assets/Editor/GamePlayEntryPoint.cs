using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Editor-only entry point. SceneFlow still owns all in-game scene transitions.
[InitializeOnLoad]
public static class GamePlayEntryPoint
{
    private const string MenuScene = "Assets/Scenes/MainMenu.unity";
    private const string StartMenu = "Tools/Fishing/Play Mode/Start From Main Menu";
    private const string CurrentMenu = "Tools/Fishing/Play Mode/Start From Current Scene";
    private static string PreferenceKey => "AnglerVillage.StartFromMainMenu." + Application.dataPath;
    private static bool StartsFromMenu => EditorPrefs.GetBool(PreferenceKey, true);

    static GamePlayEntryPoint()
    {
        EditorApplication.delayCall += ApplyWhenReady;
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    private static void ApplyWhenReady()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            EditorApplication.delayCall += ApplyWhenReady;
            return;
        }
        Apply();
    }

    private static void Apply()
    {
        if (!StartsFromMenu)
        {
            EditorSceneManager.playModeStartScene = null;
            return;
        }
        var scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(MenuScene);
        if (scene == null)
        {
            Debug.LogError("Cannot set Play Mode entry scene: " + MenuScene);
            return;
        }
        EditorSceneManager.playModeStartScene = scene;
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode) Apply();
    }

    [MenuItem(StartMenu)]
    private static void StartFromMenu()
    {
        EditorPrefs.SetBool(PreferenceKey, true);
        Apply();
    }

    [MenuItem(CurrentMenu)]
    private static void StartFromCurrent()
    {
        EditorPrefs.SetBool(PreferenceKey, false);
        Apply();
    }

    [MenuItem(StartMenu, true)]
    private static bool ValidateMenu()
    {
        Menu.SetChecked(StartMenu, StartsFromMenu);
        return !EditorApplication.isPlayingOrWillChangePlaymode;
    }

    [MenuItem(CurrentMenu, true)]
    private static bool ValidateCurrent()
    {
        Menu.SetChecked(CurrentMenu, !StartsFromMenu);
        return !EditorApplication.isPlayingOrWillChangePlaymode;
    }
}
