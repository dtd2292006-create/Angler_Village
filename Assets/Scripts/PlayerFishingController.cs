using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(PlayerInputReader))]
[RequireComponent(typeof(PlayerModeController))]
public class PlayerFishingController : MonoBehaviour
{
    private enum CastPhase { Ready, Charging, Releasing, HookDeployed, Catching }

    [Header("References")]
    [SerializeField] private PlayerAnimationController animationController;
    [SerializeField] private PlayerAnimationEvent animationEvents;
    [SerializeField] private HookController hook;
    [SerializeField] private HookCatchController hookCatch;
    [SerializeField] private PlayerCatchVisual catchVisual;

    [Header("Charge")]
    [SerializeField, Min(0.1f)] private float fullChargeSeconds = 1.5f;
    [Header("Debug")]
    [SerializeField] private bool logCastPower = true;

    public float CurrentCharge01 { get; private set; }
    public float LastCastPower01 { get; private set; }
    public bool IsCasting => phase == CastPhase.Charging || phase == CastPhase.Releasing;

    private PlayerInputReader input;
    private PlayerModeController mode;
    private CastPhase phase;
    private float chargeSeconds;
    private bool releaseReceived;
    private int finishedFrame = -1;

    private void Awake()
    {
        input = GetComponent<PlayerInputReader>();
        mode = GetComponent<PlayerModeController>();
        if (hookCatch == null && hook != null)
            hookCatch = hook.GetComponent<HookCatchController>();

        if (animationController == null || animationEvents == null ||
            hook == null || hookCatch == null || catchVisual == null)
        {
            Debug.LogError("PlayerFishingController: Gan du References, ke ca Hook Catch va Catch Visual.", this);
            enabled = false;
        }
    }

    private void OnEnable()
    {
        if (animationEvents != null)
        {
            animationEvents.CastReleased.AddListener(HandleCastRelease);
            animationEvents.CastFinished.AddListener(HandleCastFinished);
        }
        if (hookCatch != null) hookCatch.FishLanded += HandleFishLanded;
        if (animationController != null)
        {
            animationController.CatchStarted += HandleCatchStarted;
            animationController.CatchFinished += HandleCatchFinished;
        }
    }

    private void OnDisable()
    {
        if (animationEvents != null)
        {
            animationEvents.CastReleased.RemoveListener(HandleCastRelease);
            animationEvents.CastFinished.RemoveListener(HandleCastFinished);
        }
        if (hookCatch != null) hookCatch.FishLanded -= HandleFishLanded;
        if (animationController != null)
        {
            animationController.CatchStarted -= HandleCatchStarted;
            animationController.CatchFinished -= HandleCatchFinished;
        }
        CancelFishingAction();
        if (hook != null) hook.SetVisible(false);
    }

    private void Update()
    {
        bool fishing = mode.isActiveAndEnabled && mode.IsFishing;
        hook.SetVisible(fishing && phase != CastPhase.Catching);

        if (!fishing)
        {
            if (phase != CastPhase.Ready || hook.IsDeployed)
                CancelFishingAction();
            return;
        }

        if (Time.timeScale == 0f) return;
        if (!hook.IsReady || !hookCatch.isActiveAndEnabled)
        {
            if (phase != CastPhase.Ready) CancelFishingAction();
            return;
        }

        switch (phase)
        {
            case CastPhase.Ready:
                if (!animationController.IsCatching &&
                    Time.frameCount > finishedFrame + 1 && input.CastPressed)
                    BeginCast();
                break;

            case CastPhase.Charging:
                if (!input.CastHeld)
                {
                    LastCastPower01 = CurrentCharge01;
                    phase = CastPhase.Releasing;
                    animationController.ReleaseCast();
                }
                else
                {
                    chargeSeconds += Time.deltaTime;
                    CurrentCharge01 = Mathf.Clamp01(
                        chargeSeconds / Mathf.Max(0.1f, fullChargeSeconds));
                }
                break;

            case CastPhase.HookDeployed:
                hook.SetReelHeld(input.CastHeld);
                // Neu co ca, su kien FishLanded da chuyen sang Catching
                // ngay khi thu het day trong FixedUpdate.
                if (!hook.IsDeployed) CompleteFishingAction();
                break;
        }
    }

    private void BeginCast()
    {
        phase = CastPhase.Charging;
        chargeSeconds = 0f;
        CurrentCharge01 = 0f;
        releaseReceived = false;
        hook.SetReelHeld(false);
        mode.SetFishingExitLocked(this, true);
        animationController.BeginCast();
    }

    private void HandleCastRelease()
    {
        if (!isActiveAndEnabled || !mode.isActiveAndEnabled || !mode.IsFishing ||
            mode.CurrentSpot == null || phase != CastPhase.Releasing || releaseReceived)
            return;

        releaseReceived = true;
        bool launched = hook.Launch(LastCastPower01, mode.CurrentSpot.FaceRight);
        if (logCastPower)
            Debug.Log($"Cast: {LastCastPower01:P0}, launched: {launched}", this);
    }

    private void HandleCastFinished()
    {
        if (!isActiveAndEnabled || phase != CastPhase.Releasing || !mode.IsFishing)
            return;
        if (!releaseReceived)
            Debug.LogWarning("Chua nhan OnCastRelease. Kiem tra clip Release.", this);

        animationController.FinishCast();
        if (hook.IsDeployed) phase = CastPhase.HookDeployed;
        else CompleteFishingAction();
    }

    private void HandleFishLanded(FishCatchable fish)
    {
        if (!isActiveAndEnabled || !mode.IsFishing || fish == null) return;

        phase = CastPhase.Catching;
        hook.SetReelHeld(false);
        hook.SetVisible(false);
        mode.SetFishingExitLocked(this, true);
        catchVisual.SetCaughtFish(fish.CaughtSprite);
        animationController.BeginCatch();
        Debug.Log("Bat duoc ca: " + (fish.Definition != null ? fish.Definition.DisplayName : fish.name), this);
    }

    private void HandleCatchStarted()
    {
        if (!isActiveAndEnabled || !mode.IsFishing) return;

        // Ho tro thu Catch bang tay trong Animator ma khong de lai luot quang cu.
        if (phase != CastPhase.Catching)
        {
            hookCatch.CancelAttachment();
            hook.ReturnToRest();
            animationController.CancelCast();
        }
        phase = CastPhase.Catching;
        hook.SetVisible(false);
        mode.SetFishingExitLocked(this, true);
    }

    private void HandleCatchFinished()
    {
        if (phase != CastPhase.Catching) return;
        CompleteFishingAction();
    }

    private void CompleteFishingAction()
    {
        hook.SetReelHeld(false);
        mode.SetFishingExitLocked(this, false);
        phase = CastPhase.Ready;
        CurrentCharge01 = 0f;
        finishedFrame = Time.frameCount;
    }

    private void CancelFishingAction()
    {
        if (hookCatch != null) hookCatch.CancelAttachment();
        if (hook != null) hook.ReturnToRest();
        if (catchVisual != null) catchVisual.ClearCaughtFish();
        if (animationController != null)
        {
            animationController.CancelCast();
            animationController.CancelCatch();
        }
        if (mode != null) mode.SetFishingExitLocked(this, false);
        phase = CastPhase.Ready;
        chargeSeconds = 0f;
        CurrentCharge01 = 0f;
        releaseReceived = false;
        finishedFrame = Time.frameCount;
    }
}
