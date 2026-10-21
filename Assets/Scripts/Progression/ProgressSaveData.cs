using System;
using System.Collections.Generic;

[Serializable]
public sealed class ProgressSaveData
{
    public List<FishDiscoverySaveEntry> discoveries = new List<FishDiscoverySaveEntry>();
    public int version = 1;
    public int coins;
    public int totalXp;
    public bool welcomeRead;
    public bool certified;
    public bool starterKitClaimed;
    public bool lakeVisited;
    public bool firstDelivery;
    public int completedCommissions;
    public int failedCommissions;
    public string currentScene = "Village";
    public string villageArrival = "House";
    public CommissionSaveData commission;
    public ProgressSaveData Copy() => new ProgressSaveData {
        discoveries = discoveries.ConvertAll(item => item.Copy()),
        version = version, coins = coins, totalXp = totalXp, welcomeRead = welcomeRead,
        certified = certified, starterKitClaimed = starterKitClaimed, lakeVisited = lakeVisited,
        firstDelivery = firstDelivery, completedCommissions = completedCommissions,
        failedCommissions = failedCommissions, currentScene = currentScene, villageArrival = villageArrival,
        commission = commission == null ? null : commission.Copy() };
    public static bool Validate(ProgressSaveData data, out string error)
    {
        error = null;
        if (data == null || data.version != 1 || data.coins < 0 || data.totalXp < 0 ||
            data.completedCommissions < 0 || data.failedCommissions < 0 ||
            (data.currentScene != "Lake" && data.currentScene != "Village") ||
            (data.villageArrival != "House" && data.villageArrival != "Lake"))
        { error = "Invalid player progress."; return false; }
        if (data.discoveries == null) { error = "Missing fish journal."; return false; }
        var ids = new HashSet<string>(StringComparer.Ordinal);
        foreach (var entry in data.discoveries)
            if (entry == null || string.IsNullOrWhiteSpace(entry.speciesId) || entry.totalCaught < 1 || !ids.Add(entry.speciesId))
            { error = "Invalid fish journal entry."; return false; }
        var c = data.commission;
        // Unity inline serialization can materialize an empty optional class.
        if (c != null && !string.IsNullOrEmpty(c.definitionId) &&
            (c.acceptedSequence < 0 || c.deadlineMinutes < 0 || double.IsNaN(c.deadlineMinutes) || double.IsInfinity(c.deadlineMinutes)))
        { error = "Invalid commission receipt."; return false; }
        return true;
    }
}
[Serializable]
public sealed class CommissionSaveData
{
    public string definitionId;
    public long acceptedSequence;
    public double deadlineMinutes;
    public bool failed;
    public CommissionSaveData Copy() => (CommissionSaveData)MemberwiseClone();
}

[Serializable]
public sealed class FishDiscoverySaveEntry
{
    public string speciesId;
    public int totalCaught;
    public FishDiscoverySaveEntry Copy() => (FishDiscoverySaveEntry)MemberwiseClone();
}
