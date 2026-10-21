using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class AutoSaveEditor
{
    // Doi thanh 5.0 neu muon luu moi 5 giay.
    private const double IntervalSeconds = 10.0;

    private const string EnabledKey = "FishingGame.AutoSave.Enabled";
    private const string MenuPath = "Tools/Auto Save/Enabled";

    private static double nextSaveTime;
    private static bool isSaving;
    private static bool warnedAboutUntitledScene;

    static AutoSaveEditor()
    {
        nextSaveTime =
            EditorApplication.timeSinceStartup + IntervalSeconds;

        EditorApplication.update -= Update;
        EditorApplication.update += Update;
    }

    private static bool IsEnabled =>
        EditorPrefs.GetBool(EnabledKey, true);

    private static void Update()
    {
        if (!IsEnabled || isSaving)
            return;

        if (EditorApplication.isPlayingOrWillChangePlaymode ||
            EditorApplication.isCompiling ||
            EditorApplication.isUpdating ||
            BuildPipeline.isBuildingPlayer)
        {
            // Khong luu trong luc Play, import, compile hoac build.
            nextSaveTime =
                EditorApplication.timeSinceStartup + IntervalSeconds;
            return;
        }

        if (EditorApplication.timeSinceStartup < nextSaveTime)
            return;

        nextSaveTime =
            EditorApplication.timeSinceStartup + IntervalSeconds;

        SaveChanges();
    }

    private static void SaveChanges()
    {
        if (isSaving)
            return;

        isSaving = true;

        try
        {
            bool hasUntitledScene = false;

            // Luu tat ca scene dang mo co thay doi.
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);

                if (!scene.isLoaded || !scene.isDirty)
                    continue;

                if (string.IsNullOrEmpty(scene.path))
                {
                    hasUntitledScene = true;
                    continue;
                }

                if (!EditorSceneManager.SaveScene(scene))
                {
                    Debug.LogWarning(
                        "[Auto Save] Khong luu duoc scene: " + scene.path);
                }
            }

            // Luu cac asset da duoc danh dau thay doi:
            // animation, controller, material, prefab asset...
            AssetDatabase.SaveAssets();

            if (hasUntitledScene && !warnedAboutUntitledScene)
            {
                Debug.LogWarning(
                    "[Auto Save] Co scene chua duoc dat ten. " +
                    "Nhan Ctrl+S va luu scene mot lan de bat tu dong luu.");

                warnedAboutUntitledScene = true;
            }
            else if (!hasUntitledScene)
            {
                warnedAboutUntitledScene = false;
            }
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
        finally
        {
            isSaving = false;
        }
    }

    [MenuItem(MenuPath)]
    private static void ToggleEnabled()
    {
        EditorPrefs.SetBool(EnabledKey, !IsEnabled);

        nextSaveTime =
            EditorApplication.timeSinceStartup + IntervalSeconds;

        Debug.Log("[Auto Save] " + (IsEnabled ? "Enabled" : "Disabled"));
    }

    [MenuItem(MenuPath, true)]
    private static bool ValidateEnabled()
    {
        Menu.SetChecked(MenuPath, IsEnabled);
        return true;
    }

    [MenuItem("Tools/Auto Save/Save Now")]
    private static void SaveNow()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode ||
            EditorApplication.isCompiling ||
            EditorApplication.isUpdating ||
            BuildPipeline.isBuildingPlayer)
        {
            Debug.LogWarning(
                "[Auto Save] Hay dung Play va cho Unity xu ly xong.");
            return;
        }

        SaveChanges();
    }
}