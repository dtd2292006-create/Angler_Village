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
            // Exact old DTO: no equipment field at all.
            File.WriteAllText(path, JsonUtility.ToJson(new LegacyFishProfile
            {
                format = legacy.format, schemaVersion = 1,
                lastCatchSequence = legacy.lastCatchSequence, caughtFish = legacy.caughtFish
            }));
            var store = new PlayerSaveFileStore(path);
            Require(store.Load(out var restoredFish, out _) == PlayerSaveLoadStatus.Loaded && restoredFish.Count == 1,
                "v1 fish must load unchanged.");
            Require(store.LoadedEquipment == null && store.RequiresProfileUpgrade,
                "v1 profile must be recognizable for starter grant.");
            // A v1 fixture serialized with today's DTO can contain an inline empty object too.
            legacy.equipment = new EquipmentSaveData();
            File.WriteAllText(path, JsonUtility.ToJson(legacy));
            Require(store.Load(out restoredFish, out _) == PlayerSaveLoadStatus.Loaded &&
                store.LoadedEquipment == null && restoredFish.Count == 1,
                "v1 inline placeholders must not suppress the starter kit.");
            // Repair the early v2 bug, but never replenish a deliberately empty, initialized profile.
            legacy.schemaVersion = 2;
            legacy.equipmentInitialized = false;
            File.WriteAllText(path, JsonUtility.ToJson(legacy));
            Require(store.Load(out restoredFish, out _) == PlayerSaveLoadStatus.Loaded &&
                store.LoadedEquipment == null && store.RequiresProfileUpgrade,
                "Early v2 placeholder must be eligible for a one-time starter grant.");
            legacy.equipmentInitialized = true;
            File.WriteAllText(path, JsonUtility.ToJson(legacy));
            Require(store.Load(out restoredFish, out _) == PlayerSaveLoadStatus.Loaded &&
                store.LoadedEquipment != null && store.LoadedEquipment.items.Count == 0 &&
                store.RequiresProfileUpgrade,
                "An empty loadout stays empty while the new progress section is migrated.");
            legacy.equipmentInitialized = false;
            File.WriteAllText(path, JsonUtility.ToJson(legacy));
            Require(store.Load(out restoredFish, out _) == PlayerSaveLoadStatus.Loaded &&
                store.LoadedEquipment == null, "Restore the starter migration fixture.");
            Require(EquipmentInventory.TryRestore(store.LoadedEquipment, out var equipment, out _), "Legacy equipment init failed.");
            catalog.Seed(equipment);
            Require(equipment.Items.Count == 1 && equipment.Items[0].Kind == FishingEquipmentKind.Rod, "New profiles start with only the default rod.");
            Require(store.TrySave(restoredFish, equipment.Export(), out var error), error);
            var secondStore = new PlayerSaveFileStore(path);
            Require(secondStore.Load(out var secondFish, out _) == PlayerSaveLoadStatus.Loaded && secondFish.Count == 1,
                "v2 round trip must preserve fish.");
            Require(secondStore.LoadedEquipment != null && !secondStore.RequiresProfileUpgrade,
                "Starter grant must not run again after reload.");
            // Non-empty early v2 equipment must be preserved, not reset to defaults.
            var oldV2 = secondFish.Export();
            oldV2.equipment = secondStore.LoadedEquipment;
            oldV2.equipmentInitialized = false;
            File.WriteAllText(path, JsonUtility.ToJson(oldV2));
            Require(secondStore.Load(out secondFish, out _) == PlayerSaveLoadStatus.Loaded &&
                secondStore.LoadedEquipment != null && secondStore.RequiresProfileUpgrade,
                "Existing early v2 equipment must survive addition of the initialization marker.");
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
    [Serializable]
    private sealed class LegacyFishProfile
    {
        public string format;
        public int schemaVersion;
        public long lastCatchSequence;
        public System.Collections.Generic.List<CaughtFishSaveEntry> caughtFish;
    }

    private static void Require(bool valid, string message)
    { if (!valid) throw new InvalidOperationException(message); }
}

