using System;

public enum FishResistanceBand { Green, Medium, Red }
public enum FishingQteResult { None, Good, Great, Fail }

// Pure gameplay state: no scene objects, input, rendering or Unity dependency.
public sealed class FishingResistanceState
{
    public float Capacity { get; private set; }
    public float InitialResistance { get; private set; }
    public float RemainingStrength { get; private set; }
    public float Distance01 { get; private set; }
    public float DistanceFactor { get; private set; }
    public float Resistance { get; private set; }
    public float Ratio => Resistance / Capacity;
    public float OverloadSeconds { get; private set; }
    public float ElapsedSeconds { get; private set; }
    public bool IsBroken { get; private set; }

    private readonly float maximumDistance;
    private readonly float catchDistance;
    private readonly float distanceExponent;
    private readonly float overloadDuration;
    private readonly float overloadRecovery;
    private readonly float graceSeconds;

    public FishingResistanceState(float capacity, float initialResistance,
        float maximumDistance, float catchDistance, float distanceExponent,
        float overloadDuration, float overloadRecovery, float graceSeconds)
    {
        Capacity = Math.Max(1f, capacity);
        InitialResistance = Math.Max(0f, initialResistance);
        RemainingStrength = InitialResistance;
        this.catchDistance = Math.Max(0f, catchDistance);
        this.maximumDistance = Math.Max(this.catchDistance + 0.01f, maximumDistance);
        this.distanceExponent = Math.Max(0.1f, distanceExponent);
        this.overloadDuration = Math.Max(0.1f, overloadDuration);
        this.overloadRecovery = Math.Max(0f, overloadRecovery);
        this.graceSeconds = Math.Max(0f, graceSeconds);
        RefreshDistance(this.maximumDistance);
    }

    public void RefreshDistance(float distance)
    {
        // Use a fixed maximum for the cast, not the shortening current line length.
        Distance01 = FishingFightRules.Clamp(
            (Math.Max(0f, distance) - catchDistance) / (maximumDistance - catchDistance), 0f, 1f);
        DistanceFactor = (float)Math.Pow(Distance01, distanceExponent);
        // Progress is reversible and always reaches zero at the retrieval point.
        Resistance = RemainingStrength * DistanceFactor;
    }

    public void AdjustResistance(float amount, float distance)
    {
        // Good/Great/Fail change persistent strength. Distance scales the visible effect.
        RemainingStrength = Math.Max(0f, RemainingStrength + amount);
        RefreshDistance(distance);
    }

    public void Step(float dt, float distance, bool reeling, bool underwater,
        float idleGainPerSecond)
    {
        if (IsBroken || dt <= 0f) return;
        float previousElapsed = ElapsedSeconds;
        ElapsedSeconds += dt;
        if (underwater && !reeling)
            RemainingStrength += Math.Max(0f, idleGainPerSecond) * dt;
        RefreshDistance(distance);

        if (!underwater)
        {
            OverloadSeconds = Math.Max(0f, OverloadSeconds - overloadRecovery * dt);
            return;
        }

        float dangerDt = Math.Max(0f, ElapsedSeconds - Math.Max(previousElapsed, graceSeconds));
        if (Ratio >= 1f)
            OverloadSeconds += dangerDt;
        else
            OverloadSeconds = Math.Max(0f, OverloadSeconds - overloadRecovery * dt);

        IsBroken = OverloadSeconds >= overloadDuration;
    }
}

public sealed class FishingQteRound
{
    public bool IsActive { get; private set; }
    public float Progress { get; private set; }
    public float GoodStart { get; private set; }
    public float GoodWidth { get; private set; }
    public float GreatStart { get; private set; }
    public float GreatWidth { get; private set; }
    public FishingQteResult Result { get; private set; }
    private float duration;
    private float elapsed;

    public void Begin(float sweepSeconds, float goodStart, float goodWidth, float greatWidth)
    {
        if (IsActive) throw new InvalidOperationException("A QTE is already active.");
        duration = Math.Max(0.1f, sweepSeconds);
        GoodWidth = FishingFightRules.Clamp(goodWidth, 0.01f, 0.8f);
        GoodStart = FishingFightRules.Clamp(goodStart, 0f, 1f - GoodWidth);
        GreatWidth = FishingFightRules.Clamp(greatWidth, 0.005f, GoodWidth * 0.8f);
        GreatStart = GoodStart + (GoodWidth - GreatWidth) * 0.5f;
        Progress = 0f;
        elapsed = 0f;
        Result = FishingQteResult.None;
        IsActive = true;
    }

    public bool Advance(float dt)
    {
        if (!IsActive || dt <= 0f) return false;
        elapsed += dt;
        Progress = FishingFightRules.Clamp(elapsed / duration, 0f, 1f);
        if (elapsed < duration) return false;
        Finish(FishingQteResult.Fail);
        return true;
    }

    public FishingQteResult Submit()
    {
        if (!IsActive) return FishingQteResult.None;
        FishingQteResult result = Progress >= GreatStart && Progress <= GreatStart + GreatWidth
            ? FishingQteResult.Great
            : Progress >= GoodStart && Progress <= GoodStart + GoodWidth
                ? FishingQteResult.Good : FishingQteResult.Fail;
        Finish(result);
        return result;
    }

    private void Finish(FishingQteResult result)
    {
        IsActive = false;
        Result = result;
    }

    public void Cancel()
    {
        IsActive = false;
        Result = FishingQteResult.None;
    }
}

public static class FishingFightRules
{
    public static float Clamp(float value, float min, float max)
    {
        return Math.Max(min, Math.Min(max, value));
    }

    public static FishResistanceBand GetBand(float ratio, float green, float red)
    {
        return ratio <= green ? FishResistanceBand.Green
            : ratio >= red ? FishResistanceBand.Red : FishResistanceBand.Medium;
    }

    public static void GetWindowSizes(float speciesGood, float speciesGreat,
        float capacity, float initialResistance, float lineWindowMultiplier,
        bool redBand, out float good, out float great)
    {
        float matchup = Clamp(capacity / Math.Max(1f, initialResistance), 0.45f, 1.8f);
        float scale = matchup * Math.Max(0.1f, lineWindowMultiplier) * (redBand ? 0.8f : 1f);
        good = Clamp(speciesGood * scale, 0.06f, 0.45f);
        great = Clamp(speciesGreat * scale, 0.012f, Math.Min(0.18f, good * 0.65f));
    }
}
