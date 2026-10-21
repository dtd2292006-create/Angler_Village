using System;
using System.Collections.Generic;

public static class PlayerSaveSchema
{
    public const string Format = "FishingGame.PlayerData";
    public const int CurrentVersion = 2;
}

[Serializable]
public sealed class PlayerSaveData
{
    public string format;
    public int schemaVersion;
    public EquipmentSaveData equipment;
    public long lastCatchSequence;
    public List<CaughtFishSaveEntry> caughtFish;
}
