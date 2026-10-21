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
            byte[] bytes = new UTF8Encoding(false).GetBytes(JsonUtility.ToJson(inventory.Export(), true));
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

    private static bool TryRead(string path, out FishInventory inventory, out string error, out bool newerSchema)
    {
        inventory = null;
        error = null;
        newerSchema = false;
        try
        {
            string json = File.ReadAllText(path, Encoding.UTF8);
            PlayerSaveData data = JsonUtility.FromJson<PlayerSaveData>(json);
            newerSchema = data != null && data.format == FishInventory.SaveFormat &&
                data.schemaVersion > FishInventory.SchemaVersion;
            return FishInventory.TryRestore(data, out inventory, out error);
        }
        catch (Exception exception)
        {
            error = exception.GetType().Name + ": " + exception.Message;
            return false;
        }
    }
}
