using System;
using System.Collections.Generic;

// Immutable runtime item. A species asset and a caught fish are different objects.
public sealed class CaughtFishInstance
{
    public string InstanceId { get; }
    public string SpeciesId { get; }
    public long CatchSequence { get; }
    public long CaughtAtUtcTicks { get; }
    public DateTime CaughtAtUtc => new DateTime(CaughtAtUtcTicks, DateTimeKind.Utc);

    internal CaughtFishInstance(string instanceId, string speciesId, long sequence, long utcTicks)
    {
        InstanceId = instanceId;
        SpeciesId = speciesId;
        CatchSequence = sequence;
        CaughtAtUtcTicks = utcTicks;
    }
}

// Disk DTOs only; do not expose these as live inventory state.


[Serializable]
public sealed class CaughtFishSaveEntry
{
    public string instanceId;
    public string speciesId;
    public long catchSequence;
    public long caughtAtUtcTicks;
}

