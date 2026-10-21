using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

// Pure inventory rules. No Unity objects, scene lookups, JSON or disk writes.
public sealed class FishInventory
{
    public const string SaveFormat = PlayerSaveSchema.Format;
    public const int SchemaVersion = PlayerSaveSchema.CurrentVersion;

    private readonly List<CaughtFishInstance> fish = new List<CaughtFishInstance>();
    private readonly HashSet<string> instanceIds = new HashSet<string>(StringComparer.Ordinal);
    private readonly ReadOnlyCollection<CaughtFishInstance> view;
    public IReadOnlyList<CaughtFishInstance> Fish => view;
    public int Count => fish.Count;
    public long LastCatchSequence { get; private set; }

    public FishInventory()
    {
        view = fish.AsReadOnly();
    }

    // Returns false for the same receipt again; never creates a second item for it.
    public bool TryAddCatch(string catchId, string speciesId, long utcTicks, out CaughtFishInstance added)
    {
        added = null;
        string id;
        if (!TryNormalizeId(catchId, out id)) throw new ArgumentException("Invalid catch receipt ID.");
        if (string.IsNullOrWhiteSpace(speciesId)) throw new ArgumentException("Missing species ID.");
        if (utcTicks < DateTime.MinValue.Ticks || utcTicks > DateTime.MaxValue.Ticks)
            throw new ArgumentOutOfRangeException(nameof(utcTicks));
        if (instanceIds.Contains(id)) return false;

        long sequence = checked(LastCatchSequence + 1);
        added = new CaughtFishInstance(id, speciesId, sequence, utcTicks);
        fish.Add(added);
        instanceIds.Add(id);
        LastCatchSequence = sequence;
        return true;
    }

    public PlayerSaveData Export()
    {
        PlayerSaveData data = new PlayerSaveData
        {
            format = SaveFormat,
            schemaVersion = SchemaVersion,
            lastCatchSequence = LastCatchSequence,
            caughtFish = new List<CaughtFishSaveEntry>(fish.Count)
        };
        foreach (CaughtFishInstance item in fish)
            data.caughtFish.Add(new CaughtFishSaveEntry
            {
                instanceId = item.InstanceId,
                speciesId = item.SpeciesId,
                catchSequence = item.CatchSequence,
                caughtAtUtcTicks = item.CaughtAtUtcTicks
            });
        return data;
    }

    public static bool TryRestore(PlayerSaveData data, out FishInventory inventory, out string error)
    {
        inventory = null;
        error = null;
        if (data == null || data.format != SaveFormat)
        {
            error = "Invalid or missing save format marker.";
            return false;
        }
        if (data.schemaVersion != 1 && data.schemaVersion != SchemaVersion)
        {
            error = "Unsupported save schema: " + data.schemaVersion;
            return false;
        }
        if (data.caughtFish == null || data.lastCatchSequence < 0)
        {
            error = "Missing fish list or invalid catch counter.";
            return false;
        }

        FishInventory candidate = new FishInventory();
        HashSet<long> sequences = new HashSet<long>();
        foreach (CaughtFishSaveEntry entry in data.caughtFish)
        {
            string id;
            if (entry == null || !TryNormalizeId(entry.instanceId, out id) ||
                string.IsNullOrWhiteSpace(entry.speciesId) || entry.catchSequence <= 0 ||
                entry.catchSequence > data.lastCatchSequence ||
                entry.caughtAtUtcTicks < DateTime.MinValue.Ticks ||
                entry.caughtAtUtcTicks > DateTime.MaxValue.Ticks)
            {
                error = "Invalid caught fish entry. No partial inventory was loaded.";
                return false;
            }
            if (!candidate.instanceIds.Add(id) || !sequences.Add(entry.catchSequence))
            {
                error = "Duplicate fish ID or catch sequence in save.";
                return false;
            }
            candidate.fish.Add(new CaughtFishInstance(id, entry.speciesId,
                entry.catchSequence, entry.caughtAtUtcTicks));
        }
        candidate.fish.Sort((a, b) => a.CatchSequence.CompareTo(b.CatchSequence));
        candidate.LastCatchSequence = data.lastCatchSequence;
        inventory = candidate;
        return true;
    }

    private static bool TryNormalizeId(string value, out string normalized)
    {
        normalized = null;
        Guid id;
        if (!Guid.TryParseExact(value, "N", out id) || id == Guid.Empty) return false;
        normalized = id.ToString("N");
        return true;
    }
}

