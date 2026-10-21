using AnglerVillage.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class InventoryUIFontRepair
{
    [InitializeOnLoadMethod]
    private static void Schedule() => EditorApplication.delayCall += RepairAfterImport;

    private static void RepairAfterImport()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            EditorApplication.delayCall += RepairAfterImport;
            return;
        }
        RepairPrefabs();
    }

    public static void RepairPrefabs()
    {
        foreach (string path in new[] {
            "Assets/Prefabs/UI/FishInventoryUI.prefab",
            "Assets/Prefabs/UI/InventoryUI.prefab",
            "Assets/Prefabs/UI/MainMenuUI.prefab",
            "Assets/Prefabs/UI/LakeShellUI.prefab",
            "Assets/Prefabs/UI/FishInventorySlot.prefab",
            "Assets/Prefabs/UI/EquipmentSlot.prefab",
            "Assets/Prefabs/UI/VillageShellUI.prefab",
            "Assets/Prefabs/UI/JourneyHudUI.prefab",
            "Assets/Prefabs/UI/VillagePanelUI.prefab",
            "Assets/Prefabs/UI/JourneyListRow.prefab",
            "Assets/Prefabs/UI/FishJournalUI.prefab" })
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null) continue;
            GameObject contents = PrefabUtility.LoadPrefabContents(path);
            try
            {
                bool changed = false;
                foreach (Text text in contents.GetComponentsInChildren<Text>(true))
                    changed |= InventoryUIFont.Ensure(text);
                if (changed)
                {
                    PrefabUtility.SaveAsPrefabAsset(contents, path);
                    Debug.Log("Inventory UI: repaired missing font references in " + path);
                }
            }
            finally { PrefabUtility.UnloadPrefabContents(contents); }
        }
    }
}


