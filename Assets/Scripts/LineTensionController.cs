using System;
using UnityEngine;

public enum FishingLineBreakReason
{
    None,
    TensionLimit,
    Overpowered
}

[DisallowMultipleComponent]
[RequireComponent(typeof(HookCatchController))]
public class LineTensionController : MonoBehaviour
{
    [Header("Equipment")]
    [SerializeField] private FishingLineDefinition lineDefinition;

    [Header("Tension")]
    [Tooltip("Load/strength below this ratio can be reeled without building tension.")]
    [SerializeField, Range(0.1f, 1f)] private float safeLoadRatio = 0.65f;
    [SerializeField, Min(0.01f)] private float buildRate = 0.6f;
    [SerializeField, Min(0f)] private float recoveryWhileReeling = 0.12f;
    [SerializeField, Min(0f)] private float recoveryOnRelease = 0.35f;
    [Tooltip("Only a nearly taut line transfers this simplified fishing load.")]
    [SerializeField, Min(0f)] private float tautTolerance = 0.08f;

    [Header("Overpowered Fish")]
    [SerializeField, Min(1f)] private float hardBreakRatio = 1.8f;
    [SerializeField, Min(0.05f)] private float hardBreakHoldSeconds = 0.6f;

    [Header("Runtime Debug")]
    [SerializeField, Range(0f, 1f)] private float tension01;
    [SerializeField] private float loadRatio;
    [SerializeField] private float overloadSeconds;
    [SerializeField] private FishingLineBreakReason lastBreakReason;

    public float Tension01 => tension01;
    public float LoadRatio => loadRatio;
    public FishingLineDefinition LineDefinition => lineDefinition;
    public FishingLineBreakReason LastBreakReason => lastBreakReason;
    public bool HasActiveFish => isActiveAndEnabled && lineDefinition != null &&
        hook != null && hook.IsDeployed && catchController != null &&
        catchController.HasFish;

    public event Action<FishDefinition, FishingLineBreakReason> LineBroken;

    private HookController hook;
    private HookCatchController catchController;

    private void Awake()
    {
        hook = GetComponent<HookController>();
        catchController = GetComponent<HookCatchController>();
        if (lineDefinition == null)
        {
            Debug.LogError("LineTensionController: Assign a Line Definition before Play.", this);
            enabled = false;
        }
    }

    private void OnEnable()
    {
        if (hook == null) hook = GetComponent<HookController>();
        if (catchController == null) catchController = GetComponent<HookCatchController>();
        hook.PhysicsLoadSampled += Tick;
        hook.ReturnedToRest += ClearSession;
        hook.Retrieved += ClearSession;
        catchController.AttachedFishChanged += HandleFishChanged;
        HandleFishChanged(catchController.AttachedFish);
    }

    // Use this later from the equipment system. Swapping mid-cast is disallowed.
    public bool SetLineDefinition(FishingLineDefinition definition)
    {
        if (definition == null || (hook != null && hook.IsDeployed)) return false;
        lineDefinition = definition;
        ClearSession();
        return true;
    }

    private void HandleFishChanged(FishCatchable fish)
    {
        ClearSession();
        if (fish != null) lastBreakReason = FishingLineBreakReason.None;
    }

    private void Tick(float deltaTime)
    {
        if (!HasActiveFish || !hook.IsUnderwater || hook.RodTip == null)
        {
            ClearSession();
            return;
        }

        float dt = Mathf.Max(0f, deltaTime);
        if (dt <= 0f) return;

        loadRatio = Mathf.Max(0f, hook.CurrentLoad.LinePullStrength) / lineDefinition.Strength;
        float distance = Vector2.Distance(hook.RodTip.position, hook.transform.position);
        bool taut = distance >= Mathf.Max(0f, hook.CurrentLineLength - tautTolerance);

        if (!hook.IsReeling || !taut)
        {
            tension01 = Mathf.MoveTowards(tension01, 0f, recoveryOnRelease * dt);
            return;
        }

        // Extreme fish strength is a separate equipment limit, not a QTE penalty.
        // Compare intrinsic strength, so a recovery phase cannot bypass that limit.
        // Accumulate time actually pulling; brief releases do not reset this timer.
        FishDefinition fishDefinition = catchController.AttachedFish.Definition;
        float strengthRatio = fishDefinition != null
            ? fishDefinition.LinePullStrength / lineDefinition.Strength : 0f;
        if (strengthRatio >= hardBreakRatio)
            overloadSeconds += dt;

        if (overloadSeconds >= hardBreakHoldSeconds)
        {
            BreakLine(FishingLineBreakReason.Overpowered);
            return;
        }

        float excess = loadRatio - safeLoadRatio;
        if (excess > 0f)
            tension01 = Mathf.Clamp01(tension01 + excess * buildRate * dt);
        else
            tension01 = Mathf.MoveTowards(tension01, 0f, recoveryWhileReeling * dt);

        if (tension01 >= 1f) BreakLine(FishingLineBreakReason.TensionLimit);
    }

    private void BreakLine(FishingLineBreakReason reason)
    {
        FishCatchable fish = catchController.AttachedFish;
        FishDefinition definition = fish != null ? fish.Definition : null;
        if (!hook.BreakLine()) return;

        // ReturnToRest clears the active session synchronously; keep the result after it.
        lastBreakReason = reason;
        Debug.Log("Day cau bi dut: " + reason, this);
        LineBroken?.Invoke(definition, reason);
    }

    private void ClearSession()
    {
        tension01 = 0f;
        loadRatio = 0f;
        overloadSeconds = 0f;
    }

    private void OnDisable()
    {
        if (hook != null)
        {
            hook.PhysicsLoadSampled -= Tick;
            hook.ReturnedToRest -= ClearSession;
            hook.Retrieved -= ClearSession;
        }
        if (catchController != null)
            catchController.AttachedFishChanged -= HandleFishChanged;
        ClearSession();
    }
}
