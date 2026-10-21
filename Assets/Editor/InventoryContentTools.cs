using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FishingEquipmentCatalog))]
public sealed class EquipmentCatalogInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        using (new EditorGUI.DisabledScope(EditorApplication.isPlaying))
            if (GUILayout.Button("Refresh Equipment List (keep existing IDs)"))
                InventoryContentTools.Refresh((FishingEquipmentCatalog)target);
    }
}

public sealed class InventoryContentTools : EditorWindow
{
    private FishingEquipmentDefinition equipment;
    private FishDefinition fish;
    private int quantity = 1;
    private int selectedFish;
    private string message;
    [MenuItem("Tools/Fishing/Inventory Content Tools")]
    private static void Open() => GetWindow<InventoryContentTools>("Inventory Content");
    public static void Refresh(FishingEquipmentCatalog catalog)
    {
        string[] guids = AssetDatabase.FindAssets("t:FishingEquipmentDefinition");
        Array.Sort(guids, StringComparer.Ordinal);
        var definitions = guids.Select(id => AssetDatabase.LoadAssetAtPath<FishingEquipmentDefinition>(AssetDatabase.GUIDToAssetPath(id))).ToArray();
        var ids = new System.Collections.Generic.HashSet<string>(StringComparer.Ordinal);
        for (int i = 0; i < definitions.Length; i++)
        {
            string id = string.IsNullOrWhiteSpace(definitions[i].ItemId) ? guids[i] : definitions[i].ItemId;
            if (!ids.Add(id)) { Debug.LogError("Duplicate equipment ID: " + id); return; }
        }
        for (int i = 0; i < definitions.Length; i++)
        {
            if (!string.IsNullOrWhiteSpace(definitions[i].ItemId)) continue;
            var data = new SerializedObject(definitions[i]);
            data.FindProperty("itemId").stringValue = guids[i];
            data.ApplyModifiedProperties(); AssetDatabase.SaveAssetIfDirty(definitions[i]);
        }
        var serialized = new SerializedObject(catalog);
        var items = serialized.FindProperty("items"); items.arraySize = definitions.Length;
        for (int i = 0; i < definitions.Length; i++) items.GetArrayElementAtIndex(i).objectReferenceValue = definitions[i];
        serialized.ApplyModifiedProperties(); AssetDatabase.SaveAssetIfDirty(catalog);
        Debug.Log("Equipment catalog refreshed. Starting Items and player ownership were not changed.", catalog);
    }
    private void OnGUI()
    {
        EditorGUILayout.HelpBox("Edit definitions/catalogs outside Play Mode. Grant/remove buttons below operate on the active saved profile during Play; catalog membership alone never grants ownership.", MessageType.Info);
        equipment = (FishingEquipmentDefinition)EditorGUILayout.ObjectField("Equipment / Bait", equipment, typeof(FishingEquipmentDefinition), false);
        quantity = Mathf.Max(1, EditorGUILayout.IntField("Quantity", quantity));
        var inventory = PlayerInventory.Instance;
        var owned = inventory != null ? inventory.GetComponent<PlayerEquipment>() : null;
        using (new EditorGUI.DisabledScope(!EditorApplication.isPlaying || owned == null || !owned.IsReady || equipment == null))
        {
            if (GUILayout.Button("Grant selected item"))
                message = owned.TryGrant(equipment.ItemId, quantity, out var grantError) ? "Granted and saved." : grantError;
            if (GUILayout.Button("Remove selected item"))
                message = owned.TryRemove(equipment.ItemId, quantity, out var removeError) ? "Removed and saved." : removeError;
        }
        EditorGUILayout.Space();
        fish = (FishDefinition)EditorGUILayout.ObjectField("Fish species", fish, typeof(FishDefinition), false);
        using (new EditorGUI.DisabledScope(!EditorApplication.isPlaying || inventory == null || !inventory.IsReady))
        {
            using (new EditorGUI.DisabledScope(fish == null))
                if (GUILayout.Button("Grant one test catch"))
                    message = inventory.TryRecordCatch(Guid.NewGuid().ToString("N"), fish, out var catchError) ? "Catch added and saved." : catchError;
            if (inventory != null && inventory.FishCount > 0)
            {
                string[] names = inventory.Fish.Select(item => "#" + item.CatchSequence + " " +
                    (inventory.TryGetDefinition(item, out var definition) ? definition.DisplayName : item.SpeciesId)).ToArray();
                selectedFish = EditorGUILayout.Popup("Caught fish", Mathf.Clamp(selectedFish, 0, names.Length - 1), names);
                if (GUILayout.Button("Remove selected caught fish"))
                    message = inventory.TryRemoveFish(inventory.Fish[selectedFish].InstanceId) ? "Catch removed and saved." : "Could not remove catch.";
            }
        }
        if (!string.IsNullOrEmpty(message)) EditorGUILayout.HelpBox(message, MessageType.None);
    }
}
