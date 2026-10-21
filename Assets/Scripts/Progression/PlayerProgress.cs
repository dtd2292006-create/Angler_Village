using System;
using UnityEngine;

// Persistent profile state. Guild and shop own their own rules; movement knows nothing about them.
[DisallowMultipleComponent]
public sealed class PlayerProgress : MonoBehaviour
{
    public static PlayerProgress Current => PlayerInventory.Instance != null ? PlayerInventory.Instance.GetComponent<PlayerProgress>() : null;
    private ProgressSaveData state = new ProgressSaveData();
    public bool IsReady { get; private set; }
    public event Action Changed;
    public event Action<string> SpeciesDiscovered;
    public int Coins => state.coins;
    public int TotalXp => state.totalXp;
    public bool WelcomeRead => state.welcomeRead;
    public bool Certified => state.certified;
    public bool StarterKitClaimed => state.starterKitClaimed;
    public bool LakeVisited => state.lakeVisited;
    public bool FirstDelivery => state.firstDelivery;
    public string CurrentScene => state.currentScene;
    public string VillageArrival => state.villageArrival;
    public CommissionSaveData Commission => state.commission != null && !string.IsNullOrEmpty(state.commission.definitionId) ? state.commission.Copy() : null;
    public int Level { get { int level = 1, xp = state.totalXp; while (level < 100 && xp >= LevelCost(level)) { xp -= LevelCost(level); level++; } return level; } }
    public int XpInLevel { get { int xp = state.totalXp; for (int level = 1; level < Level; level++) xp -= LevelCost(level); return xp; } }
    public static int LevelCost(int level) => 60 + (level - 1) * 30;
    internal void Initialize(ProgressSaveData loaded) { state = loaded != null ? loaded.Copy() : new ProgressSaveData(); IsReady = true; }
    public bool KnowsSpecies(string id) => CatchCount(id) > 0;
    public int CatchCount(string id)
    {
        foreach (var entry in state.discoveries) if (entry.speciesId == id) return entry.totalCaught;
        return 0;
    }
    public void Discover(string id)
    {
        if (!IsReady || string.IsNullOrWhiteSpace(id)) return;
        foreach (var entry in state.discoveries)
            if (entry.speciesId == id) { if (entry.totalCaught < int.MaxValue) entry.totalCaught++; Notify(); return; }
        state.discoveries.Add(new FishDiscoverySaveEntry { speciesId = id, totalCaught = 1 });
        SpeciesDiscovered?.Invoke(id); Notify();
    }
    internal void ImportExistingFish(System.Collections.Generic.IReadOnlyList<CaughtFishInstance> fish)
    {
        foreach (var item in fish)
        {
            var entry = state.discoveries.Find(value => value.speciesId == item.SpeciesId);
            if (entry == null) state.discoveries.Add(new FishDiscoverySaveEntry { speciesId = item.SpeciesId, totalCaught = 1 });
            else if (entry.totalCaught < int.MaxValue) entry.totalCaught++;
        }
    }
    public ProgressSaveData Export() => state.Copy();
    internal void Notify() => Changed?.Invoke();
    public void ReadWelcome() { if (state.welcomeRead) return; state.welcomeRead = true; Notify(); }
    internal void Certify() { state.certified = true; state.welcomeRead = true; }
    internal void ClaimKit() { state.starterKitClaimed = true; }
    internal void SetCommission(CommissionSaveData commission) { state.commission = commission; }
    internal void Reward(int coins, int xp) { state.coins = checked(state.coins + coins); state.totalXp = checked(state.totalXp + xp); }
    internal void Spend(int coins) { state.coins = checked(state.coins - coins); }
    internal void Deliver() { state.commission = null; state.completedCommissions++; state.firstDelivery = true; }
    internal void FailCommission() { if (state.commission == null || state.commission.failed) return; state.commission.failed = true; state.failedCommissions++; Notify(); }
    public void SetDestination(string scene, string arrival)
    {
        state.currentScene = scene; state.villageArrival = arrival;
        if (scene == "Lake") state.lakeVisited = true;
        Notify();
    }
}
