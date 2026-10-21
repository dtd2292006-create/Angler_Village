using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class FishInventorySelfCheck
{
    [MenuItem("Tools/Fishing/Run Inventory Rule Checks")]
    public static void Run()
    {
        int passed = 0;
        Check("Same receipt is credited once", () =>
        {
            FishInventory inventory = new FishInventory();
            string id = Id();
            CaughtFishInstance item;
            Require(inventory.TryAddCatch(id, "species-a", Ticks(), out item));
            Require(!inventory.TryAddCatch(id, "species-a", Ticks(), out item));
            Require(inventory.Count == 1 && inventory.LastCatchSequence == 1);
        }, ref passed);

        Check("Same species creates separate instances and monotonic order", () =>
        {
            FishInventory inventory = new FishInventory();
            CaughtFishInstance first, second;
            inventory.TryAddCatch(Id(), "species-a", Ticks(), out first);
            inventory.TryAddCatch(Id(), "species-a", Ticks() - TimeSpan.TicksPerDay, out second);
            Require(first.InstanceId != second.InstanceId && inventory.Count == 2);
            Require(first.CatchSequence == 1 && second.CatchSequence == 2);
        }, ref passed);

        Check("Restoring preserves receipts and next sequence", () =>
        {
            FishInventory original = new FishInventory();
            string firstId = Id();
            CaughtFishInstance item;
            original.TryAddCatch(firstId, "species-a", Ticks(), out item);
            FishInventory restored;
            string error;
            Require(FishInventory.TryRestore(original.Export(), out restored, out error), error);
            Require(!restored.TryAddCatch(firstId, "species-a", Ticks(), out item));
            Require(restored.TryAddCatch(Id(), "species-b", Ticks(), out item));
            Require(item.CatchSequence == 2);
        }, ref passed);

        Check("Duplicate records reject the whole save", () =>
        {
            FishInventory original = OneFish();
            PlayerSaveData data = original.Export();
            data.caughtFish.Add(data.caughtFish[0]);
            FishInventory restored;
            string error;
            Require(!FishInventory.TryRestore(data, out restored, out error));
            Require(restored == null);
        }, ref passed);

        Check("Unknown species is kept rather than silently deleted", () =>
        {
            FishInventory restored;
            string error;
            Require(FishInventory.TryRestore(OneFish().Export(), out restored, out error), error);
            Require(restored.Fish[0].SpeciesId == "species-a");
        }, ref passed);

        Check("JSON disk roundtrip keeps IDs, order and UTC ticks", () => WithStore(store =>
        {
            FishInventory original = OneFish();
            Save(store, original);
            FishInventory restored;
            string message;
            Require(store.Load(out restored, out message) == PlayerSaveLoadStatus.Loaded, message);
            Require(restored.Count == 1);
            Require(restored.Fish[0].InstanceId == original.Fish[0].InstanceId);
            Require(restored.Fish[0].CaughtAtUtcTicks == original.Fish[0].CaughtAtUtcTicks);
            Require(restored.LastCatchSequence == original.LastCatchSequence);
        }), ref passed);

        Check("Malformed primary recovers the previous valid backup", () => WithStore(store =>
        {
            FishInventory inventory = OneFish();
            Save(store, inventory);
            CaughtFishInstance item;
            inventory.TryAddCatch(Id(), "species-b", Ticks(), out item);
            Save(store, inventory);
            File.WriteAllText(store.FullPath, "broken JSON");
            FishInventory restored;
            string message;
            Require(store.Load(out restored, out message) == PlayerSaveLoadStatus.RecoveredBackup, message);
            Require(restored.Count == 1);
            Save(store, restored);
            Require(store.Load(out restored, out message) == PlayerSaveLoadStatus.Loaded, message);
            Require(restored.Count == 1);
        }), ref passed);

        Check("Unrecoverable files are not overwritten with empty inventory", () => WithStore(store =>
        {
            File.WriteAllText(store.FullPath, "bad primary");
            File.WriteAllText(store.BackupPath, "bad backup");
            FishInventory restored;
            string message;
            Require(store.Load(out restored, out message) == PlayerSaveLoadStatus.Failed);
            Require(restored == null && File.ReadAllText(store.FullPath) == "bad primary");
        }), ref passed);

        Check("Newer schema is not silently downgraded to an old backup", () => WithStore(store =>
        {
            FishInventory inventory = OneFish();
            Save(store, inventory);
            PlayerSaveData future = inventory.Export();
            future.schemaVersion = FishInventory.SchemaVersion + 1;
            string json = JsonUtility.ToJson(future);
            File.WriteAllText(store.FullPath, json);
            FishInventory restored;
            string message;
            Require(store.Load(out restored, out message) == PlayerSaveLoadStatus.Failed);
            Require(File.ReadAllText(store.FullPath) == json);
        }), ref passed);

        Check("A complete temporary file can recover an interrupted first commit", () => WithStore(store =>
        {
            File.WriteAllText(store.TemporaryPath, JsonUtility.ToJson(OneFish().Export()));
            FishInventory restored;
            string message;
            Require(store.Load(out restored, out message) == PlayerSaveLoadStatus.RecoveredTemporary, message);
            Require(restored.Count == 1);
        }), ref passed);

        Debug.Log("Fish inventory checks passed: " + passed + "/10. Still verify the catch event and scene setup in Play Mode.");
    }

    private static string Id() { return Guid.NewGuid().ToString("N"); }
    private static long Ticks() { return DateTime.UtcNow.Ticks; }
    private static FishInventory OneFish()
    {
        FishInventory inventory = new FishInventory();
        CaughtFishInstance item;
        inventory.TryAddCatch(Id(), "species-a", Ticks(), out item);
        return inventory;
    }
    private static void Save(PlayerSaveFileStore store, FishInventory inventory)
    {
        string error;
        Require(store.TrySave(inventory, out error), error);
    }
    private static void WithStore(Action<PlayerSaveFileStore> test)
    {
        string directory = Path.Combine(Path.GetTempPath(), "FishingInventoryChecks", Id());
        Directory.CreateDirectory(directory);
        try { test(new PlayerSaveFileStore(Path.Combine(directory, "player.json"))); }
        finally { Directory.Delete(directory, true); }
    }
    private static void Require(bool value, string error = null)
    {
        if (!value) throw new Exception(error ?? "Unexpected inventory result.");
    }
    private static void Check(string name, Action test, ref int passed)
    {
        try { test(); passed++; }
        catch (Exception error)
        {
            Debug.LogError("Inventory check failed: " + name + "\n" + error);
            throw;
        }
    }
}
