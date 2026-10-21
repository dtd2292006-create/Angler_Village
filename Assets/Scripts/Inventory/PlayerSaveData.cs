using System;
using System.Collections.Generic;

public static class PlayerSaveSchema
{
    public const string Format = "FishingGame.PlayerData";
    public const int CurrentVersion = 3;
}

[Serializable]
public sealed class PlayerSaveData
{
    public string format;
    public int schemaVersion;
    // Inline Unity serialization cannot use null as the sole initialization marker.
    public bool worldTimeInitialized;
    public double worldMinutes;
    public bool progressInitialized;
    public ProgressSaveData progress;
    public bool equipmentInitialized;
    public EquipmentSaveData equipment;
    public long lastCatchSequence;
    public List<CaughtFishSaveEntry> caughtFish;
}

