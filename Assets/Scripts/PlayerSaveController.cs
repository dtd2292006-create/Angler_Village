using System.IO;
using UnityEngine;

[DefaultExecutionOrder(-1100)]
[DisallowMultipleComponent]
[RequireComponent(typeof(PlayerInventory))]
[RequireComponent(typeof(PlayerEquipment))]
public class PlayerSaveController : MonoBehaviour
{
    [SerializeField, Min(1f)] private float retrySeconds = 5f;
    [Header("Runtime Debug")]
    [SerializeField] private bool dirty;
    [SerializeField] private string savePath;
    [SerializeField] private string lastSaveError;

    private PlayerInventory inventory;
    private PlayerEquipment equipment;
    private PlayerWorldClock clock;
    private PlayerProgress progress;
    private int writeDeferrals;
    private double lastSavedMinutes;
    private PlayerSaveFileStore store;
    private float nextRetry;
    private bool ownsProfile;

    private void Awake()
    {
        inventory = GetComponent<PlayerInventory>();
        if (!inventory.IsPrimary || !inventory.isActiveAndEnabled)
        {
            enabled = false;
            return;
        }
        savePath = Path.Combine(Application.persistentDataPath, "Saves", "slot_01", "player.json");
        store = new PlayerSaveFileStore(savePath);
        FishInventory loaded;
        string message;
        PlayerSaveLoadStatus result = store.Load(out loaded, out message);
        if (result == PlayerSaveLoadStatus.Failed)
        {
            lastSaveError = message;
            Debug.LogError(message + "\n" + savePath, this);
            enabled = false;
            return;
        }
        equipment = GetComponent<PlayerEquipment>();
        bool needsEquipmentSeed = store.LoadedEquipment == null;
        if (!EquipmentInventory.TryRestore(store.LoadedEquipment, out var equipmentState, out var equipmentError))
        {
            lastSaveError = equipmentError;
            Debug.LogError(equipmentError, this);
            enabled = false;
            return;
        }
        try { equipment.Initialize(equipmentState, needsEquipmentSeed); }
        catch (System.Exception error)
        {
            lastSaveError = error.Message;
            Debug.LogException(error, this);
            enabled = false;
            return;
        }
        clock = GetComponent<PlayerWorldClock>();
        if (clock == null) clock = gameObject.AddComponent<PlayerWorldClock>();
        clock.Initialize(store.LoadedWorldMinutes);
        lastSavedMinutes = clock.ElapsedMinutes;
        clock.SaveRequested += HandleChanged;
        progress = GetComponent<PlayerProgress>();
        if (progress == null) progress = gameObject.AddComponent<PlayerProgress>();
        progress.Initialize(store.LoadedProgress);
        if (store.LoadedProgress == null) progress.ImportExistingFish(loaded.Fish);
        progress.Changed += HandleChanged;
        inventory.Initialize(loaded);
        ownsProfile = true;
        inventory.Changed += HandleChanged;
        equipment.Changed += HandleChanged;
        if (!string.IsNullOrEmpty(message)) Debug.LogWarning(message, this);
        dirty = result != PlayerSaveLoadStatus.Loaded || needsEquipmentSeed || store.RequiresProfileUpgrade;
        if (dirty) SaveNow();
        Debug.Log("Player data ready. Fish=" + inventory.FishCount + "\nSave: " + savePath, this);
    }

    private void HandleChanged()
    {
        dirty = true;
        if (writeDeferrals == 0) SaveNow();
    }

    public System.IDisposable DeferWrites() { writeDeferrals++; return new WriteScope(this); }
    private sealed class WriteScope : System.IDisposable
    {
        private PlayerSaveController owner;
        public WriteScope(PlayerSaveController value) { owner = value; }
        public void Dispose()
        {
            if (owner == null) return;
            owner.writeDeferrals--;
            if (owner.writeDeferrals == 0 && owner.dirty) owner.SaveNow();
            owner = null;
        }
    }

    public bool SaveNow()
    {
        if (!ownsProfile || !inventory.IsReady || writeDeferrals > 0) return false;
        if (clock != null && clock.ElapsedMinutes != lastSavedMinutes) dirty = true;
        if (!dirty) return true;
        string error;
        if (store.TrySave(inventory.State, equipment.State.Export(), clock.ElapsedMinutes, progress.Export(), out error))
        {
            lastSavedMinutes = clock.ElapsedMinutes;
            dirty = false;
            lastSaveError = null;
            return true;
        }
        lastSaveError = error;
        nextRetry = Time.unscaledTime + retrySeconds;
        Debug.LogError("Player save failed; catches remain in memory and will retry. " + error, this);
        return false;
    }

    private void Update()
    {
        if (dirty && Time.unscaledTime >= nextRetry) SaveNow();
    }

    [ContextMenu("Log Save Location")]
    public void LogSaveLocation()
    {
        Debug.Log(Path.Combine(Application.persistentDataPath, "Saves", "slot_01", "player.json"), this);
    }

    private void OnApplicationPause(bool paused) { if (paused && ownsProfile) SaveNow(); }
    private void OnApplicationQuit() { if (ownsProfile) SaveNow(); }

    private void OnDestroy()
    {
        if (clock != null) clock.SaveRequested -= HandleChanged;
        if (progress != null) progress.Changed -= HandleChanged;
        if (inventory != null) inventory.Changed -= HandleChanged;
        if (equipment != null) equipment.Changed -= HandleChanged;
        if (ownsProfile && inventory != null) SaveNow();
    }
}


