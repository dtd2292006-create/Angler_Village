using System;
using UnityEditor;
using UnityEngine;

// No NUnit or Test Framework dependency. Runs the real pure gameplay classes.
public static class FishingFightSelfCheck
{
    [MenuItem("Tools/Fishing/Run Fight Rule Checks")]
    public static void Run()
    {
        int passed = 0;
        Check("Resistance is present immediately at the farthest distance", () =>
        {
            FishingResistanceState state = State(80f);
            Near(state.Resistance, 80f);
            Require(state.Ratio > 0.75f);
        }, ref passed);

        Check("Pulling and paying back the same distance cannot farm resistance", () =>
        {
            FishingResistanceState state = State(80f);
            for (int i = 0; i < 100; i++)
            {
                state.RefreshDistance(8f);
                Near(state.Resistance, 64f);
                state.RefreshDistance(10f);
                Near(state.Resistance, 80f);
            }
        }, ref passed);

        Check("A successful QTE reduces resistance and net pull reduces it further", () =>
        {
            FishingResistanceState state = State(80f);
            state.AdjustResistance(-28f, 10f);
            Near(state.Resistance, 52f);
            state.RefreshDistance(9.35f);
            Near(state.Resistance, 48.62f);
        }, ref passed);

        Check("Idle time increases resistance even at the maximum payout distance", () =>
        {
            FishingResistanceState state = State(80f);
            state.Step(1f, 10f, false, true, 2f);
            Near(state.Resistance, 82f);
        }, ref passed);

        Check("Overload grace expires without an unavoidable instant break", () =>
        {
            FishingResistanceState state = new FishingResistanceState(100f, 120f, 10f, 0f, 1f, 2.5f, 1f, 3f);
            state.Step(3f, 10f, true, true, 0f);
            Near(state.OverloadSeconds, 0f);
            Require(!state.IsBroken);
            state.Step(2.4f, 10f, false, true, 0f);
            Require(!state.IsBroken);
            state.Step(0.2f, 10f, false, true, 0f);
            Require(state.IsBroken);
        }, ref passed);

        Check("A QTE can rescue overload; time recovers gradually", () =>
        {
            FishingResistanceState state = new FishingResistanceState(100f, 110f, 10f, 0f, 1f, 2.5f, 1f, 0f);
            state.Step(1f, 10f, true, true, 0f);
            Near(state.OverloadSeconds, 1f);
            state.AdjustResistance(-30f, 10f);
            state.Step(0.5f, 10f, true, true, 0f);
            Near(state.OverloadSeconds, 0.5f);
            Require(!state.IsBroken);
        }, ref passed);

        Check("Stronger line changes the band for the same fish", () =>
        {
            Require(FishingFightRules.GetBand(80f / 100f, 0.35f, 0.75f) == FishResistanceBand.Red);
            Require(FishingFightRules.GetBand(80f / 150f, 0.35f, 0.75f) == FishResistanceBand.Medium);
            Require(FishingFightRules.GetBand(80f / 250f, 0.35f, 0.75f) == FishResistanceBand.Green);
        }, ref passed);

        Check("Great is species-dependent and stronger line widens both windows", () =>
        {
            float weakGood, weakGreat, strongGood, strongGreat, speciesGood, speciesGreat;
            FishingFightRules.GetWindowSizes(0.2f, 0.06f, 75f, 80f, 1f, true, out weakGood, out weakGreat);
            FishingFightRules.GetWindowSizes(0.2f, 0.06f, 150f, 80f, 1f, true, out strongGood, out strongGreat);
            FishingFightRules.GetWindowSizes(0.2f, 0.03f, 75f, 80f, 1f, true, out speciesGood, out speciesGreat);
            Require(strongGood > weakGood && strongGreat > weakGreat);
            Require(speciesGreat < weakGreat);
        }, ref passed);

        Check("Good and Great grading is exact and accepts only one answer", () =>
        {
            FishingQteRound round = new FishingQteRound();
            round.Begin(2f, 0.3f, 0.2f, 0.06f);
            round.Advance(0.8f);
            Require(round.Submit() == FishingQteResult.Great);
            Require(round.Submit() == FishingQteResult.None);
            round.Begin(2f, 0.3f, 0.2f, 0.06f);
            round.Advance(0.64f);
            Require(round.Submit() == FishingQteResult.Good);
            round.Begin(2f, 0.3f, 0.2f, 0.06f);
            round.Advance(0.2f);
            Require(round.Submit() == FishingQteResult.Fail);
        }, ref passed);

        Check("No overlapping round, timeout fails once, pause does not advance", () =>
        {
            FishingQteRound round = new FishingQteRound();
            round.Begin(2f, 0.3f, 0.2f, 0.06f);
            bool rejected = false;
            try { round.Begin(2f, 0.3f, 0.2f, 0.06f); }
            catch (InvalidOperationException) { rejected = true; }
            Require(rejected);
            round.Advance(0f);
            Near(round.Progress, 0f);
            Require(round.Advance(2f));
            Require(round.Result == FishingQteResult.Fail);
            Require(!round.Advance(1f));
        }, ref passed);

        Check("Holding reel without moving the fish never drains resistance over time", () =>
        {
            FishingResistanceState state = State(80f);
            state.Step(5f, 10f, true, true, 2f);
            Near(state.Resistance, 80f);
            Near(state.RemainingStrength, 80f);
        }, ref passed);

        Check("At the retrieval point even accumulated penalties give zero resistance", () =>
        {
            FishingResistanceState state = new FishingResistanceState(100f, 80f, 10f, 0.25f, 1f, 2.5f, 1f, 3f);
            state.AdjustResistance(300f, 5f);
            state.RefreshDistance(0.25f);
            Near(state.Resistance, 0f);
            state.Step(1f, 0.25f, false, true, 2f);
            Near(state.Resistance, 0f);
        }, ref passed);

        Check("Same position has no drift except the explicit idle penalty", () =>
        {
            FishingResistanceState state = State(80f);
            state.RefreshDistance(5f);
            Near(state.Resistance, 40f);
            state.Step(1f, 5f, false, true, 2f);
            Near(state.Resistance, 41f);
            state.Step(0f, 5f, false, true, 2f);
            Near(state.Resistance, 41f);
        }, ref passed);

        Check("Stronger progress curve drains faster by distance, not by holding time", () =>
        {
            FishingResistanceState normal = State(80f);
            FishingResistanceState stronger = new FishingResistanceState(100f, 80f, 10f, 0f, 2f, 2.5f, 1f, 3f);
            normal.RefreshDistance(5f);
            stronger.RefreshDistance(5f);
            Near(normal.Resistance, 40f);
            Near(stronger.Resistance, 20f);
        }, ref passed);

        Check("Reel ceiling follows green, medium and red", () =>
        {
            Near(FishingReelSpeedRules.BandCeiling(0.2f, 0.35f, 0.75f, 0.6f, 0.2f), 1f);
            Near(FishingReelSpeedRules.BandCeiling(0.55f, 0.35f, 0.75f, 0.6f, 0.2f), 0.6f);
            Near(FishingReelSpeedRules.BandCeiling(0.8f, 0.35f, 0.75f, 0.6f, 0.2f), 0.2f);
            Near(FishingReelSpeedRules.BandCeiling(2f, 0.35f, 0.75f, 0.6f, 0.2f), 0.2f);
        }, ref passed);

        Check("Reel ceiling is continuous and never speeds up as resistance rises", () =>
        {
            float previous = 1f;
            for (int i = 0; i <= 1200; i++)
            {
                float value = FishingReelSpeedRules.BandCeiling(i / 1000f, 0.35f, 0.75f, 0.6f, 0.2f);
                Require(value <= previous + 0.00001f);
                Require(previous - value < 0.01f);
                Require(value >= 0.1999f && value <= 1f);
                previous = value;
            }
        }, ref passed);

        Check("Recovery cannot bypass red ceiling and penalties do not multiply", () =>
        {
            Near(FishingReelSpeedRules.Combine(0f, 0.2f), 0.2f);
            Near(FishingReelSpeedRules.Combine(1f, 0.2f), 0.2f);
            Near(FishingReelSpeedRules.Combine(9f, 0.2f), 0.1f);
        }, ref passed);

        Check("Encoded load preserves the intended HookController reel multiplier", () =>
        {
            float multiplier = FishingReelSpeedRules.Combine(1f, 0.2f);
            float load = 1f / multiplier - 1f;
            Near(2.5f / (1f + load), 0.5f);
            Near(FishingReelSpeedRules.Combine(0f, 1f), 1f);
        }, ref passed);

        Debug.Log("Fishing fight checks passed: " + passed + "/18. Still test scene/input/Animator integration in Play Mode.");
    }

    private static FishingResistanceState State(float initial)
    {
        return new FishingResistanceState(100f, initial, 10f, 0f, 1f, 2.5f, 1f, 3f);
    }

    private static void Check(string name, Action test, ref int passed)
    {
        try { test(); passed++; }
        catch (Exception error)
        {
            Debug.LogError("Fishing fight check failed: " + name + "\n" + error);
            throw;
        }
    }

    private static void Require(bool value)
    {
        if (!value) throw new Exception("Unexpected gameplay result.");
    }

    private static void Near(float actual, float expected)
    {
        if (Math.Abs(actual - expected) > 0.001f)
            throw new Exception("Expected " + expected + ", actual " + actual);
    }
}
