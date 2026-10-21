using System;
using UnityEngine;

[DefaultExecutionOrder(-100)]
[DisallowMultipleComponent]
[RequireComponent(typeof(FishResistanceController))]
public class FishingQteController : MonoBehaviour
{
    [SerializeField] private Vector2 firstQteDelay = new Vector2(0.7f, 1f);
    [Tooltip("Earlier first opportunity when the fish is already in the red band.")]
    [SerializeField] private Vector2 firstRedQteDelay = new Vector2(0.2f, 0.35f);
    [SerializeField, Min(0.1f)] private float minimumInterval = 0.6f;
    [SerializeField, Min(0f)] private float greatTempoReward = 0.15f;
    [SerializeField, Min(0f)] private float redFailTempoPenalty = 0.12f;

    [Header("Fail Penalty")]
    [Tooltip("Seconds during which manual reeling is blocked after a failed or timed-out QTE. Zero disables this penalty.")]
    [SerializeField, Min(0f)] private float failReelLockSeconds = 0.8f;

    [Header("Runtime Debug")]
    [SerializeField] private float cooldown;
    [SerializeField] private float tempoMultiplier = 1f;
    [SerializeField] private FishingQteResult lastResult;
    [SerializeField] private float failReelLockRemaining;

    public bool IsActive => round.IsActive && fight != null && fight.HasFight;
    public bool IsFailReelLocked => failReelLockRemaining > 0f;
    public bool HasFight => fight != null && fight.HasFight;
    public int RoundId { get; private set; }
    public float Progress => round.Progress;
    public float GoodStart => round.GoodStart;
    public float GoodWidth => round.GoodWidth;
    public float GreatStart => round.GreatStart;
    public float GreatWidth => round.GreatWidth;
    public FishingQteResult LastResult => lastResult;
    public event Action<FishingQteResult> Resolved;

    private readonly FishingQteRound round = new FishingQteRound();
    private HookController hook;
    private FishResistanceController fight;
    private bool eligibleLastFrame;
    private bool hasRunRound;
    private FishResistanceBand previousBand;

    private void Awake()
    {
        hook = GetComponent<HookController>();
        fight = GetComponent<FishResistanceController>();
    }

    private void OnEnable()
    {
        if (hook == null) hook = GetComponent<HookController>();
        if (fight == null) fight = GetComponent<FishResistanceController>();
        fight.FightChanged += ResetSession;
        ResetSession();
    }

    private void Update()
    {
        if (Time.timeScale == 0f) return;
        if (!fight.HasFight || !fight.IsUnderwater)
        {
            failReelLockRemaining = 0f;
            CancelRound();
            eligibleLastFrame = false;
            return;
        }

        if (failReelLockRemaining > 0f)
        {
            failReelLockRemaining = Mathf.Max(0f, failReelLockRemaining - Time.deltaTime);
            RefreshReelBlock();
        }

        if (round.IsActive)
        {
            if (round.Advance(Time.deltaTime)) Resolve(FishingQteResult.Fail);
            return;
        }

        if (!fight.QteAllowed)
        {
            eligibleLastFrame = false;
            return;
        }

        if (!eligibleLastFrame)
        {
            cooldown = hasRunRound ? GetInterval() : GetFirstDelay();
            eligibleLastFrame = true;
        }
        else if (fight.Band == FishResistanceBand.Red && previousBand != FishResistanceBand.Red)
            cooldown = Mathf.Min(cooldown, hasRunRound ? GetInterval() : GetFirstDelay());

        previousBand = fight.Band;
        cooldown -= Time.deltaTime;
        if (cooldown <= 0f && !IsFailReelLocked) BeginRound();
    }

    private void BeginRound()
    {
        if (round.IsActive || IsFailReelLocked || !fight.QteAllowed || !fight.IsUnderwater) return;
        FishDefinition fish = fight.Fish;
        bool red = fight.Band == FishResistanceBand.Red;
        float good;
        float great;
        FishingFightRules.GetWindowSizes(fish.QteGoodWidth, fish.QteGreatWidth,
            fight.Line.ResistanceCapacity, fish.InitialFightResistance,
            fight.Line.QteWindowMultiplier, red, out good, out great);

        // Keep the target away from the beginning and prevent wrapped zones.
        float center = UnityEngine.Random.Range(0.3f, 0.8f);
        float start = Mathf.Clamp(center - good * 0.5f, 0.12f, 0.95f - good);
        float duration = Mathf.Clamp(fish.QteSweepSeconds * (red ? 0.8f : 1f), 0.7f, 4f);
        round.Begin(duration, start, good, great);
        hasRunRound = true;
        RoundId++;
        RefreshReelBlock();
    }

    public void Submit()
    {
        if (!IsActive || Time.timeScale == 0f) return;
        FishingQteResult result = round.Submit();
        if (result != FishingQteResult.None) Resolve(result);
    }

    private void Resolve(FishingQteResult result)
    {
        if (result == FishingQteResult.Fail)
            failReelLockRemaining = Mathf.Max(0f, failReelLockSeconds);
        RefreshReelBlock();
        lastResult = result;
        fight.ApplyQteResult(result);

        if (result == FishingQteResult.Great)
            tempoMultiplier = Mathf.Min(1.8f, tempoMultiplier + greatTempoReward);
        else if (result == FishingQteResult.Fail && fight.Band == FishResistanceBand.Red)
            tempoMultiplier = Mathf.Max(0.6f, tempoMultiplier - redFailTempoPenalty);

        // No countdown for the next round exists until this round has resolved.
        cooldown = GetInterval();
        eligibleLastFrame = fight.QteAllowed;
        previousBand = fight.Band;
        Resolved?.Invoke(result);
    }

    private float GetFirstDelay()
    {
        return RandomRange(fight.Band == FishResistanceBand.Red
            ? firstRedQteDelay : firstQteDelay, 0.1f);
    }

    private float GetInterval()
    {
        if (fight.Fish == null) return minimumInterval;
        Vector2 interval = fight.Band == FishResistanceBand.Red
            ? fight.Fish.RedQteInterval : fight.Fish.MediumQteInterval;
        return Mathf.Max(minimumInterval, RandomRange(interval, minimumInterval) * tempoMultiplier);
    }

    private static float RandomRange(Vector2 range, float minimum)
    {
        float low = Mathf.Max(minimum, range.x);
        return UnityEngine.Random.Range(low, Mathf.Max(low, range.y));
    }

    private void RefreshReelBlock()
    {
        if (hook != null)
            hook.SetReelBlocked(this, round.IsActive || IsFailReelLocked);
    }

    private void CancelRound()
    {
        round.Cancel();
        RefreshReelBlock();
    }

    private void ResetSession()
    {
        failReelLockRemaining = 0f;
        CancelRound();
        cooldown = 0f;
        tempoMultiplier = 1f;
        lastResult = FishingQteResult.None;
        eligibleLastFrame = false;
        hasRunRound = false;
    }

    private void OnDisable()
    {
        if (fight != null) fight.FightChanged -= ResetSession;
        ResetSession();
    }
}
