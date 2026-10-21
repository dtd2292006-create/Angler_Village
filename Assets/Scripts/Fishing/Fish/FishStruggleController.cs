using UnityEngine;

public enum FishStrugglePhase
{
    None,
    Recovering,
    Struggling
}

[DisallowMultipleComponent]
[RequireComponent(typeof(HookCatchController))]
public class FishStruggleController : MonoBehaviour, IHookLoadProvider
{
    [Header("Response")]
    [Tooltip("How quickly resistance changes between struggling and recovery.")]
    [SerializeField, Min(0.01f)] private float response = 8f;

    [Header("Reel Speed By Resistance")]
    [Tooltip("Reel speed ceiling halfway through the medium band. 0.6 = 60 percent of base reel speed.")]
    [SerializeField, Range(0.05f, 1f)] private float mediumReelMultiplier = 0.6f;
    [Tooltip("Reel speed ceiling at or above the red threshold. 0.2 = 20 percent.")]
    [SerializeField, Range(0.05f, 1f)] private float redReelMultiplier = 0.2f;

    [Header("Runtime Debug - Read Only During Play")]
    [SerializeField] private FishStrugglePhase phase;
    [SerializeField] private float secondsRemaining;
    [SerializeField] private float currentResistance;
    [SerializeField] private float currentAcceleration;
    [SerializeField] private float currentReelMultiplier = 1f;

    public FishStrugglePhase Phase => phase;
    public float CurrentResistance => currentResistance;
    public float ReelSpeedMultiplier => currentReelMultiplier;

    private HookCatchController catchController;
    private HookController hook;
    private FishCatchable fish;
    private FishDefinition definition;
    private FishResistanceController fight;
    private float struggleBlend;

    private void Awake()
    {
        catchController = GetComponent<HookCatchController>();
        hook = GetComponent<HookController>();
        fight = GetComponent<FishResistanceController>();
    }

    private void OnEnable()
    {
        if (catchController == null) catchController = GetComponent<HookCatchController>();
        if (hook == null) hook = GetComponent<HookController>();
        catchController.AttachedFishChanged += HandleFishChanged;
        HandleFishChanged(catchController.AttachedFish);
    }

    private void HandleFishChanged(FishCatchable attachedFish)
    {
        ResetSession();
        if (attachedFish == null || attachedFish.Definition == null) return;

        fish = attachedFish;
        definition = fish.Definition;
        currentResistance = Mathf.Max(0f, definition.RecoveryResistance);
        BeginPhase(FishStrugglePhase.Recovering);
    }

    public HookLoadSample SampleLoad(float deltaTime)
    {
        if (!isActiveAndEnabled || hook == null || !hook.IsDeployed ||
            fish == null || !fish.isActiveAndEnabled || !fish.IsHooked ||
            catchController.AttachedFish != fish || definition == null)
        {
            ResetSession();
            return default(HookLoadSample);
        }

        float dt = Mathf.Max(0f, deltaTime);
        secondsRemaining -= dt;
        // Keep overshoot so phase duration does not depend on the physics tick.
        while (secondsRemaining <= 0f)
        {
            float overshoot = -secondsRemaining;
            BeginPhase(phase == FishStrugglePhase.Struggling
                ? FishStrugglePhase.Recovering : FishStrugglePhase.Struggling);
            secondsRemaining -= overshoot;
        }

        float blend = 1f - Mathf.Exp(-response * dt);
        struggleBlend = Mathf.Lerp(struggleBlend,
            phase == FishStrugglePhase.Struggling ? 1f : 0f, blend);
        float resistance = Mathf.Lerp(
            Mathf.Max(0f, definition.RecoveryResistance),
            Mathf.Max(definition.RecoveryResistance, definition.StruggleResistance),
            struggleBlend);

        bool hasFight = fight != null && fight.HasFight;
        float fightFactor = hasFight
            ? Mathf.InverseLerp(fight.GreenThreshold, 1f, fight.Ratio) : 1f;
        resistance *= fightFactor;
        float speedCeiling = hasFight
            ? FishingReelSpeedRules.BandCeiling(fight.Ratio, fight.GreenThreshold,
                fight.RedThreshold, mediumReelMultiplier, redReelMultiplier)
            : 1f;
        float effectiveMultiplier = FishingReelSpeedRules.Combine(resistance, speedCeiling);

        // Fish cannot swim against the line once lifted out of the water.
        // The same phase clock continues, but its swimming load fades away.
        currentReelMultiplier = hook.IsUnderwater ? effectiveMultiplier : 1f;
        // HookController uses baseSpeed / (1 + ReelResistance). Encode the final
        // multiplier here, so HookController remains unaware of fish/QTE bands.
        currentResistance = 1f / currentReelMultiplier - 1f;
        currentAcceleration = hook.IsUnderwater
            ? Mathf.Max(0f, definition.StruggleAcceleration) * struggleBlend * fightFactor : 0f;

        Vector2 awayFromRod = hook.RodTip != null
            ? (Vector2)(hook.transform.position - hook.RodTip.position)
            : Vector2.down;
        if (awayFromRod.sqrMagnitude < 0.000001f) awayFromRod = Vector2.down;

        float pullMultiplier = Mathf.Lerp(
            Mathf.Max(0f, definition.RecoveryPullMultiplier),
            Mathf.Max(definition.RecoveryPullMultiplier, definition.StrugglePullMultiplier),
            struggleBlend);
        float linePull = hook.IsUnderwater
            ? Mathf.Max(0f, definition.LinePullStrength) * pullMultiplier : 0f;

        return new HookLoadSample(currentResistance,
            awayFromRod.normalized * currentAcceleration, linePull);
    }

    private void BeginPhase(FishStrugglePhase next)
    {
        phase = next;
        Vector2 range = next == FishStrugglePhase.Struggling
            ? definition.StruggleDurationRange : definition.RecoveryDurationRange;
        float minimum = Mathf.Max(0.1f, range.x);
        secondsRemaining = Random.Range(minimum, Mathf.Max(minimum, range.y));
    }

    private void ResetSession()
    {
        fish = null;
        definition = null;
        phase = FishStrugglePhase.None;
        secondsRemaining = 0f;
        currentResistance = 0f;
        currentAcceleration = 0f;
        currentReelMultiplier = 1f;
        struggleBlend = 0f;
    }

    private void OnValidate()
    {
        mediumReelMultiplier = Mathf.Clamp(mediumReelMultiplier, 0.05f, 1f);
        redReelMultiplier = Mathf.Clamp(redReelMultiplier, 0.05f, mediumReelMultiplier);
        response = Mathf.Max(0.01f, response);
    }

    private void OnDisable()
    {
        if (catchController != null)
            catchController.AttachedFishChanged -= HandleFishChanged;
        ResetSession();
    }
}
