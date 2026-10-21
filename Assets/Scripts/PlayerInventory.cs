using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[DefaultExecutionOrder(-1200)]
[DisallowMultipleComponent]
public class PlayerInventory : MonoBehaviour
{
    [SerializeField] private FishCatalog fishCatalog;
    [Header("Runtime Debug")]
    [SerializeField] private int totalFish;
    [SerializeField] private long lastCatchSequence;

    public static PlayerInventory Instance { get; private set; }
    public bool IsPrimary => Instance == this;
    public bool IsReady { get; private set; }
    public int FishCount => state.Count;
    public long LastCatchSequence => state.LastCatchSequence;
    public IReadOnlyList<CaughtFishInstance> Fish => state.Fish;
    public event Action Changed;

    private FishInventory state = new FishInventory();
    internal FishInventory State => state;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        if (transform.parent != null)
        {
            Debug.LogError("PlayerInventory: PlayerData must be a dedicated root object.", this);
            enabled = false;
            return;
        }
        DontDestroyOnLoad(gameObject);
    }

    internal void Initialize(FishInventory loaded)
    {
        if (loaded == null) throw new ArgumentNullException(nameof(loaded));
        state = loaded;
        IsReady = true;
        UpdateDebug();
        Changed?.Invoke();
    }

    public bool TryRecordCatch(string catchId, FishDefinition species, out string message)
    {
        message = null;
        if (!IsPrimary || !IsReady)
        {
            message = "Player inventory is not initialized. Check PlayerSaveController errors.";
            return false;
        }
        FishDefinition registered;
        if (species == null || fishCatalog == null ||
            !fishCatalog.TryGetFish(species.SpeciesId, out registered) || registered != species)
        {
            message = "Fish species is not registered. Refresh FishCatalog and species IDs before Play.";
            return false;
        }
        try
        {
            CaughtFishInstance added;
            if (!state.TryAddCatch(catchId, species.SpeciesId, DateTime.UtcNow.Ticks, out added))
                return false; // Already credited; intentionally no Changed/save event.
        }
        catch (Exception error)
        {
            message = error.Message;
            return false;
        }
        UpdateDebug();
        Changed?.Invoke();
        return true;
    }

    public bool TryGetDefinition(CaughtFishInstance item, out FishDefinition definition)
    {
        definition = null;
        return item != null && fishCatalog != null && fishCatalog.TryGetFish(item.SpeciesId, out definition);
    }

    private void UpdateDebug()
    {
        totalFish = state.Count;
        lastCatchSequence = state.LastCatchSequence;
    }

    [ContextMenu("Log Fish Inventory")]
    public void LogFishInventory()
    {
        StringBuilder text = new StringBuilder("=== FISH INVENTORY ===\n");
        text.AppendLine("Ready=" + IsReady + " Count=" + FishCount + " Last catch=" + LastCatchSequence);
        foreach (CaughtFishInstance item in state.Fish)
        {
            FishDefinition definition;
            string name = TryGetDefinition(item, out definition) ? definition.DisplayName : "Unknown species: " + item.SpeciesId;
            text.AppendLine("#" + item.CatchSequence + " " + name + " | " + item.InstanceId +
                " | caught UTC " + item.CaughtAtUtc.ToString("O"));
        }
        Debug.Log(text.ToString(), this);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
