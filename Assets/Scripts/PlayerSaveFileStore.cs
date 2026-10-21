using System;
using System.IO;
using System.Text;
using UnityEngine;

public enum PlayerSaveLoadStatus { NewSave, Loaded, RecoveredBackup, RecoveredTemporary, Failed }

// Local profile storage. Inventory rules never read/write files themselves.
public sealed class PlayerSaveFileStore
{
    public string FullPath { get; }
    public string BackupPath => FullPath + ".bak";
    public string TemporaryPath => FullPath + ".tmp";
    public EquipmentSaveData LoadedEquipment { get; private set; }
    public bool RequiresProfileUpgrade { get; private set; }
    public double LoadedWorldMinutes { get; private set; } = 480;

    public PlayerSaveFileStore(string fullPath)
    {
        if (string.IsNullOrEmpty(fullPath) || !Path.IsPathRooted(fullPath))
            throw new ArgumentException("Save path must be absolute.");
        FullPath = Path.GetFullPath(fullPath);
    }

    public PlayerSaveLoadStatus Load(out FishInventory inventory, out string message)
    {
        inventory = null;
        message = null;
        LoadedEquipment = null;
        RequiresProfileUpgrade = false;
        LoadedWorldMinutes = 480;
        bool primaryExists = File.Exists(FullPath);
        bool backupExists = File.Exists(BackupPath);
        bool tempExists = File.Exists(TemporaryPath);
        if (!primaryExists && !backupExists && !tempExists)
        {
            inventory = new FishInventory();
            return PlayerSaveLoadStatus.NewSave;
        }

        string primaryError = "missing";
        string backupError = "missing";
        string tempError = "missing";
        bool newerSchema;
        if (primaryExists)
        {
            if (TryRead(FullPath, out inventory, out primaryError, out newerSchema))
                return PlayerSaveLoadStatus.Loaded;
            if (newerSchema)
            {
                message = primaryError + " Existing files were not changed.";
                return PlayerSaveLoadStatus.Failed;
            }
        }
        if (backupExists)
        {
            if (TryRead(BackupPath, out inventory, out backupError, out newerSchema))
            {
                message = "Recovered inventory from backup. Primary save: " + primaryError;
                return PlayerSaveLoadStatus.RecoveredBackup;
            }
            if (newerSchema)
            {
                message = backupError + " Existing files were not changed.";
                return PlayerSaveLoadStatus.Failed;
            }
        }
        if (tempExists)
        {
            if (TryRead(TemporaryPath, out inventory, out tempError, out newerSchema))
            {
                message = "Recovered a fully readable temporary save after an interrupted write.";
                return PlayerSaveLoadStatus.RecoveredTemporary;
            }
        }
        inventory = null;
        message = "Cannot load player data. Primary: " + primaryError + "; backup: " +
            backupError + "; temporary: " + tempError + ". Existing files were not changed.";
        return PlayerSaveLoadStatus.Failed;
    }

    public bool TrySave(FishInventory inventory, out string error)
    {
        return TrySave(inventory, LoadedEquipment, out error);
    }

    public bool TrySave(FishInventory inventory, EquipmentSaveData equipment, out string error)
    {
        return TrySave(inventory, equipment, LoadedWorldMinutes, out error);
    }

    public bool TrySave(FishInventory inventory, EquipmentSaveData equipment, double worldMinutes, out string error)
    {
        error = null;
        try
        {
            if (inventory == null) throw new ArgumentNullException(nameof(inventory));
            bool primaryValid = false;
            if (File.Exists(FullPath))
            {
                FishInventory previous;
                string previousError;
                bool newerSchema;
                primaryValid = TryRead(FullPath, out previous, out previousError, out newerSchema);
                if (newerSchema) throw new InvalidDataException(previousError);
            }

            Directory.CreateDirectory(Path.GetDirectoryName(FullPath));
            PlayerSaveData data = inventory.Export();
            if (double.IsNaN(worldMinutes) || double.IsInfinity(worldMinutes) || worldMinutes < 0)
                throw new InvalidDataException("Invalid world clock.");
            data.worldTimeInitialized = true;
            data.worldMinutes = worldMinutes;
            data.equipment = equipment;
            data.equipmentInitialized = equipment != null;
            if (!EquipmentInventory.TryRestore(equipment, out var checkedEquipment, out var equipmentError))
                throw new InvalidDataException(equipmentError);
            byte[] bytes = new UTF8Encoding(false).GetBytes(JsonUtility.ToJson(data, true));
            using (FileStream stream = new FileStream(TemporaryPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                stream.Write(bytes, 0, bytes.Length);
                stream.Flush(true);
            }

            if (File.Exists(FullPath))
            {
                // Never replace a good backup with a malformed primary after recovery.
                if (primaryValid) File.Copy(FullPath, BackupPath, true);
                File.Replace(TemporaryPath, FullPath, null);
            }
            else File.Move(TemporaryPath, FullPath);

            if (!File.Exists(BackupPath)) File.Copy(FullPath, BackupPath, false);
            return true;
        }
        catch (Exception exception)
        {
            error = exception.GetType().Name + ": " + exception.Message;
            return false;
        }
    }

    private static bool IsLegacyPlaceholder(EquipmentSaveData data)
    {
        if (data == null) return true;
        // Never interpret malformed loadouts or future equipment versions as an empty starter profile.
        bool defaultVersion = data.version == 0 || data.version == 1;
        bool emptyItems = data.items != null ? data.items.Count == 0 : data.version == 0;
        return defaultVersion && emptyItems && string.IsNullOrEmpty(data.rodId) &&
            string.IsNullOrEmpty(data.lineId) && string.IsNullOrEmpty(data.baitId);
    }

    private bool TryRead(string path, out FishInventory inventory, out string error, out bool newerSchema)
    {
        inventory = null;
        error = null;
        newerSchema = false;
        try
        {
            string json = File.ReadAllText(path, Encoding.UTF8);
            PlayerSaveData data = JsonUtility.FromJson<PlayerSaveData>(json);
            newerSchema = data != null && data.format == PlayerSaveSchema.Format &&
                data.schemaVersion > PlayerSaveSchema.CurrentVersion;
            if (!FishInventory.TryRestore(data, out inventory, out error)) return false;
            if (data.worldTimeInitialized && (double.IsNaN(data.worldMinutes) || double.IsInfinity(data.worldMinutes) || data.worldMinutes < 0))
            { inventory = null; error = "Invalid world clock."; return false; }
            LoadedWorldMinutes = data.worldTimeInitialized ? data.worldMinutes : 480;
            // v1 never had equipment. Ignore any inline placeholder JsonUtility constructs.
            // Early v2 builds could save that placeholder without granting the starter kit.
            if (data.schemaVersion == 1 ||
                (!data.equipmentInitialized && IsLegacyPlaceholder(data.equipment)))
            {
                LoadedEquipment = null;
                RequiresProfileUpgrade = true;
                return true;
            }
            if (data.equipmentInitialized && data.equipment == null)
            {
                inventory = null;
                error = "Initialized equipment data is missing.";
                return false;
            }
            if (!EquipmentInventory.TryRestore(data.equipment, out var checkedEquipment, out error))
            {
                inventory = null;
                return false;
            }
            LoadedEquipment = data.equipment;
            RequiresProfileUpgrade = !data.equipmentInitialized;
            return true;
        }
        catch (Exception exception)
        {
            error = exception.GetType().Name + ": " + exception.Message;
            return false;
        }
    }
}


