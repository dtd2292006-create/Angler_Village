using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class FishInventoryEditorTools
{
    public static void RefreshCatalog(FishCatalog catalog)
    {
        string[] guids = AssetDatabase.FindAssets("t:FishDefinition");
        Array.Sort(guids, StringComparer.Ordinal);
        var definitions = new System.Collections.Generic.List<FishDefinition>();
        var seen = new System.Collections.Generic.HashSet<FishDefinition>();
        // Keep existing journal display order; append newly authored species.
        foreach (var fish in catalog.Definitions)
            if (fish != null && seen.Add(fish)) definitions.Add(fish);
        foreach (var guid in guids)
        {
            var fish = AssetDatabase.LoadAssetAtPath<FishDefinition>(AssetDatabase.GUIDToAssetPath(guid));
            if (fish != null && seen.Add(fish)) definitions.Add(fish);
        }
        var ids = new System.Collections.Generic.HashSet<string>(StringComparer.Ordinal);
        foreach (var fish in definitions)
        {
            string id = string.IsNullOrWhiteSpace(fish.SpeciesId) ? AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(fish)) : fish.SpeciesId;
            if (!ids.Add(id)) { Debug.LogError("Duplicate species ID: " + id + ". Give a new species its own ID before refreshing.", fish); return; }
        }
        SerializedObject catalogData = new SerializedObject(catalog);
        SerializedProperty list = catalogData.FindProperty("fishDefinitions");
        list.arraySize = definitions.Count;
        for (int i = 0; i < definitions.Count; i++)
        {
            var fish = definitions[i];
            if (string.IsNullOrWhiteSpace(fish.SpeciesId))
            {
                var fishData = new SerializedObject(fish);
                fishData.FindProperty("speciesId").stringValue = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(fish));
                fishData.ApplyModifiedProperties(); AssetDatabase.SaveAssetIfDirty(fish);
            }
            list.GetArrayElementAtIndex(i).objectReferenceValue = fish;
        }
        catalogData.ApplyModifiedProperties(); catalog.RebuildIndex();
        EditorUtility.SetDirty(catalog); AssetDatabase.SaveAssetIfDirty(catalog);
        Debug.Log("Fish catalog refreshed: " + definitions.Count + " species. Existing IDs/order preserved; keep .meta files.", catalog);
    }
}

[CustomEditor(typeof(FishCatalog))]
public class FishCatalogInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        using (new EditorGUI.DisabledScope(EditorApplication.isPlaying))
        {
            if (GUILayout.Button("Refresh Fish List + Stable IDs"))
                FishInventoryEditorTools.RefreshCatalog((FishCatalog)target);
        }
    }
}

public class FishInventorySetupWindow : EditorWindow
{
    private HookCatchController hook;
    private FishCatalog catalog;

    [MenuItem("Tools/Fishing/Setup Fish Inventory")]
    private static void Open()
    {
        GetWindow<FishInventorySetupWindow>("Fish Inventory Setup");
    }

    private void OnGUI()
    {
        EditorGUILayout.HelpBox("Creates a dedicated PlayerData root with inventory/save, refreshes fish IDs, and connects the selected scene Hook. No UI or Animator changes.", MessageType.Info);
        hook = (HookCatchController)EditorGUILayout.ObjectField("Scene Hook", hook, typeof(HookCatchController), true);
        catalog = (FishCatalog)EditorGUILayout.ObjectField("Fish Catalog", catalog, typeof(FishCatalog), false);
        bool valid = hook != null && catalog != null && !EditorApplication.isPlaying && !EditorUtility.IsPersistent(hook);
        using (new EditorGUI.DisabledScope(!valid))
        {
            if (GUILayout.Button("Set up Fish Inventory")) Setup();
        }
    }

    private void Setup()
    {
        PlayerInventory[] existing = UnityEngine.Object.FindObjectsByType<PlayerInventory>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (existing.Length > 1)
        {
            Debug.LogError("More than one PlayerInventory exists in open scenes. Keep one dedicated PlayerData root before setup.");
            return;
        }
        if (existing.Length == 1 && existing[0].transform.parent != null)
        {
            Debug.LogError("Existing PlayerInventory must be on a dedicated root object, not a child.", existing[0]);
            return;
        }

        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Setup fish inventory");
        FishInventoryEditorTools.RefreshCatalog(catalog);
        PlayerInventory inventory;
        if (existing.Length == 0)
        {
            GameObject root = new GameObject("PlayerData");
            Undo.RegisterCreatedObjectUndo(root, "Create PlayerData");
            inventory = Undo.AddComponent<PlayerInventory>(root);
        }
        else inventory = existing[0];

        SetReference(inventory, "fishCatalog", catalog);
        PlayerSaveController save = Ensure<PlayerSaveController>(inventory.gameObject);
        FishCatchInventoryReceiver receiver = Ensure<FishCatchInventoryReceiver>(hook.gameObject);
        SetReference(receiver, "inventory", inventory);
        Undo.RecordObject(inventory, "Enable inventory");
        inventory.enabled = true;
        EditorSceneManager.MarkSceneDirty(inventory.gameObject.scene);
        EditorSceneManager.MarkSceneDirty(hook.gameObject.scene);
        Undo.CollapseUndoOperations(group);
        Selection.activeGameObject = inventory.gameObject;
        Debug.Log("Fish inventory setup complete. Save the scene, then Play. Use PlayerInventory > Log Fish Inventory to inspect catches.", save);
    }

    private static T Ensure<T>(GameObject target) where T : Behaviour
    {
        T component = target.GetComponent<T>();
        if (component == null) component = Undo.AddComponent<T>(target);
        Undo.RecordObject(component, "Enable inventory component");
        component.enabled = true;
        return component;
    }

    private static void SetReference(UnityEngine.Object target, string field, UnityEngine.Object value)
    {
        SerializedObject serialized = new SerializedObject(target);
        serialized.FindProperty(field).objectReferenceValue = value;
        serialized.ApplyModifiedProperties();
    }
}

