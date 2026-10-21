using System;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(HookCatchController))]
public class FishResistanceController : MonoBehaviour
{
    [Header("Equipment")]
    [SerializeField] private FishingLineDefinition lineDefinition;

    [Header("Bands")]
    [SerializeField, Range(0.05f, 0.6f)] private float greenThreshold = 0.35f;
    [SerializeField, Range(0.6f, 0.95f)] private float redThreshold = 0.75f;

    [Header("Break - Resistance At Or Above Capacity")]
    [SerializeField, Min(0.1f)] private float overloadDuration = 2.5f;
    [SerializeField, Min(0f)] private float overloadRecoveryPerSecond = 1f;
    [SerializeField, Min(0f)] private float openingGraceSeconds = 3f;

    [Header("Runtime Debug")]
    [SerializeField] private float resistance;
    [SerializeField] private float resistanceRatio;
    [SerializeField] private float remainingStrength;
    [SerializeField] private float distanceFactor;
    [SerializeField] private float overloadSeconds;
    [SerializeField] private FishResistanceBand band;

    public bool HasFight => isActiveAndEnabled && state != null && hook != null &&
        hook.IsDeployed && fish != null && fish.IsHooked;
    public float Resistance => HasFight ? state.Resistance : 0f;
    public float Ratio => HasFight ? state.Ratio : 0f;
    public float Overload01 => HasFight ? Mathf.Clamp01(state.OverloadSeconds / overloadDuration) : 0f;
    public float GreenThreshold => greenThreshold;
    public float RedThreshold => redThreshold;
    public FishResistanceBand Band => FishingFightRules.GetBand(Ratio, greenThreshold, redThreshold);
    public FishDefinition Fish => HasFight ? fish.Definition : null;
    public FishingLineDefinition Line => lineDefinition;
    public bool QteAllowed => HasFight && Fish != null &&
        state.InitialResistance / state.Capacity > greenThreshold && Band != FishResistanceBand.Green;
    public bool IsUnderwater => HasFight && hook.IsUnderwater;
    public event Action FightChanged;
    public event Action<FishDefinition> LineBroken;

    private HookController hook;
    private HookCatchController catchController;
    private FishCatchable fish;
    private FishingResistanceState state;

    private void Awake()
    {
        hook = GetComponent<HookController>();
        catchController = GetComponent<HookCatchController>();
        if (lineDefinition == null)
        {
            Debug.LogError("FishResistanceController: Assign a Line Definition before Play.", this);
            enabled = false;
        }
    }

    private void OnEnable()
    {
        if (hook == null) hook = GetComponent<HookController>();
        if (catchController == null) catchController = GetComponent<HookCatchController>();
        catchController.AttachedFishChanged += HandleFishChanged;
        hook.PhysicsLoadSampled += StepFight;
        hook.PhysicsStepCompleted += RefreshProgress;
        hook.ReturnedToRest += ClearFight;
        hook.Retrieved += ClearFight;
        HandleFishChanged(catchController.AttachedFish);
    }

    public bool SetLineDefinition(FishingLineDefinition definition)
    {
        if (definition == null || (hook != null && hook.IsDeployed)) return false;
        lineDefinition = definition;
        return true;
    }

    private float DistanceToRod()
    {
        return hook.RodTip != null
            ? Vector2.Distance(hook.transform.position, hook.RodTip.position) : 0f;
    }

    private void HandleFishChanged(FishCatchable attachedFish)
    {
        fish = null;
        state = null;
        if (attachedFish != null && attachedFish.Definition != null && lineDefinition != null)
        {
            fish = attachedFish;
            state = new FishingResistanceState(lineDefinition.ResistanceCapacity,
                fish.Definition.InitialFightResistance, hook.CastLineLimit, hook.CatchDistance,
                fish.Definition.DistanceResistanceExponent * lineDefinition.ProgressDrainMultiplier,
                overloadDuration, overloadRecoveryPerSecond, openingGraceSeconds);
            state.RefreshDistance(DistanceToRod());
        }
        UpdateDebug();
        FightChanged?.Invoke();
    }

    private void StepFight(float dt)
    {
        if (!HasFight) return;
        state.Step(dt, DistanceToRod(), hook.IsReeling, hook.IsUnderwater,
            fish.Definition.IdleResistanceGainPerSecond);
        UpdateDebug();
        if (!state.IsBroken) return;

        FishDefinition escapedSpecies = fish.Definition;
        if (hook.BreakLine())
        {
            Debug.Log("Day dut: ca khang cu qua tran qua lau.", this);
            LineBroken?.Invoke(escapedSpecies);
        }
    }

    private void RefreshProgress(float dt)
    {
        if (!HasFight) return;
        state.RefreshDistance(DistanceToRod());
        UpdateDebug();
    }

    public void ApplyQteResult(FishingQteResult result)
    {
        if (!HasFight) return;
        FishDefinition definition = fish.Definition;
        if (result == FishingQteResult.Good)
        {
            state.AdjustResistance(-definition.GoodResistanceDrop, DistanceToRod());
            hook.QueueBonusReel(definition.GoodBonusReel);
        }
        else if (result == FishingQteResult.Great)
        {
            state.AdjustResistance(-definition.GreatResistanceDrop, DistanceToRod());
            hook.QueueBonusReel(definition.GreatBonusReel);
        }
        else if (result == FishingQteResult.Fail)
            state.AdjustResistance(definition.FailResistanceGain, DistanceToRod());
        UpdateDebug();
    }

    private void ClearFight()
    {
        HandleFishChanged(null);
    }

    private void UpdateDebug()
    {
        resistance = Resistance;
        resistanceRatio = Ratio;
        remainingStrength = HasFight ? state.RemainingStrength : 0f;
        distanceFactor = HasFight ? state.DistanceFactor : 0f;
        overloadSeconds = HasFight ? state.OverloadSeconds : 0f;
        band = Band;
    }

    private void OnDisable()
    {
        if (catchController != null) catchController.AttachedFishChanged -= HandleFishChanged;
        if (hook != null)
        {
            hook.PhysicsLoadSampled -= StepFight;
            hook.PhysicsStepCompleted -= RefreshProgress;
            hook.ReturnedToRest -= ClearFight;
            hook.Retrieved -= ClearFight;
        }
        ClearFight();
    }
}
