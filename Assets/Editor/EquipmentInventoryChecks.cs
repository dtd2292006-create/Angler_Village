using System;
using System.IO;
using AnglerVillage.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class EquipmentInventoryChecks
{
    [MenuItem("Tools/Fishing/Validate Equipment Inventory")]
    private static void Validate()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        string directory = Path.Combine(Path.GetTempPath(), "AnglerVillage_EquipmentCheck_" + Guid.NewGuid().ToString("N"));
        try
        {
            var catalog = AssetDatabase.LoadAssetAtPath<FishingEquipmentCatalog>("Assets/Data/Equipment/EquipmentCatalog_Main.asset");
            Require(catalog != null && catalog.Validate(out _), "Equipment catalog must have valid unique IDs.");
            // A real v1 profile is migrated without inventing or dropping a caught fish.
            Directory.CreateDirectory(directory);
            string path = Path.Combine(directory, "player.json");
            var fish = new FishInventory();
            fish.TryAddCatch(Guid.NewGuid().ToString("N"), "test-species", DateTime.UtcNow.Ticks, out _);
            var legacy = fish.Export();
            legacy.schemaVersion = 1;
            legacy.equipment = null;
            File.WriteAllText(path, JsonUtility.ToJson(legacy));
            var store = new PlayerSaveFileStore(path);
            Require(store.Load(out var restoredFish, out _) == PlayerSaveLoadStatus.Loaded && restoredFish.Count == 1,
                "v1 fish must load unchanged.");
            Require(store.LoadedEquipment == null, "v1 profile must be recognizable for starter grant.");
            Require(EquipmentInventory.TryRestore(store.LoadedEquipment, out var equipment, out _), "Legacy equipment init failed.");
            catalog.Seed(equipment);
            Require(equipment.Items.Count == 3, "Expected the current rod, line and bait as starter items.");
            Require(store.TrySave(restoredFish, equipment.Export(), out var error), error);
            var secondStore = new PlayerSaveFileStore(path);
            Require(secondStore.Load(out var secondFish, out _) == PlayerSaveLoadStatus.Loaded && secondFish.Count == 1,
                "v2 round trip must preserve fish.");
            Require(secondStore.LoadedEquipment != null, "Starter grant must not run again after reload.");
            Require(EquipmentInventory.TryRestore(secondStore.LoadedEquipment, out var reloaded, out _), "Loadout round trip failed.");
            foreach (var owned in equipment.Items)
                Require(reloaded.Find(owned.ItemId).Quantity == owned.Quantity && reloaded.EquippedId(owned.Kind) == owned.ItemId,
                    "Reload must preserve quantities and equipped IDs.");
            var malformed = equipment.Export();
            malformed.baitId = malformed.rodId;
            Require(!EquipmentInventory.TryRestore(malformed, out _, out _), "Wrong-category equipment must be rejected.");
            malformed = equipment.Export();
            malformed.items.Add(malformed.items[0]);
            Require(!EquipmentInventory.TryRestore(malformed, out _, out _), "Duplicate stacks must be rejected.");
            Require(!equipment.Equip("not-owned"), "Unowned equipment must not be equipped.");
            // A failed/truncated primary must recover fish and equipment together from backup.
            Require(secondStore.TrySave(secondFish, reloaded.Export(), out error), error);
            File.WriteAllText(path, "{broken");
            var recovery = new PlayerSaveFileStore(path);
            Require(recovery.Load(out var recoveredFish, out _) == PlayerSaveLoadStatus.RecoveredBackup && recoveredFish.Count == 1 &&
                recovery.LoadedEquipment != null, "Backup must recover fish and equipment together.");
            InventoryUIFontRepair.RepairPrefabs();
            var canvas = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/InventoryUI.prefab");
            var slots = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/EquipmentSlot.prefab");
            Require(canvas != null && slots != null && canvas.GetComponentInChildren<InventoryTabs>(true) != null &&
                canvas.GetComponentInChildren<EquipmentInventoryView>(true) != null, "Tabbed inventory prefab is incomplete.");
            foreach (var text in slots.GetComponentsInChildren<Text>(true)) Require(text.font != null, "Equipment label missing font.");
            foreach (var view in canvas.GetComponentsInChildren<EquipmentInventoryView>(true))
            {
                var data = new SerializedObject(view);
                foreach (string field in new[] { "content", "slotPrefab", "loadoutLabel", "emptyLabel", "detailName", "detailText", "detailImage", "equipButton", "equipLabel" })
                    Require(data.FindProperty(field).objectReferenceValue != null, "Missing equipment UI reference: " + field);
            }
            Debug.Log("Equipment migration, ownership, backup and prefab checks passed. Play-test all three tabs next.");
        }
        catch (Exception error) { Debug.LogException(error); }
        finally { if (Directory.Exists(directory)) Directory.Delete(directory, true); }
    }
    private static void Require(bool valid, string message)
    { if (!valid) throw new InvalidOperationException(message); }
}
