using System;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimationController : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private PlayerMovement movement;

    [SerializeField]
    private HookController hook;

    public bool IsCatching { get; private set; }
    public event Action CatchStarted;
    public event Action CatchFinished;

    private static readonly int CatchParameter = Animator.StringToHash("Catch");
    private Animator animator;

    private static readonly int WalkingParameter =
        Animator.StringToHash("IsWalking");

    private static readonly int FishingParameter =
        Animator.StringToHash("IsFishing");

    private static readonly int CastParameter =
        Animator.StringToHash("Cast");

    private static readonly int CastHeldParameter =
        Animator.StringToHash("IsCastHeld");

    private static readonly int CastFinishedParameter =
        Animator.StringToHash("CastFinished");

    private static readonly int ReelingParameter =
        Animator.StringToHash("IsReeling");

    private void Awake()
    {
        animator = GetComponent<Animator>();

        if (movement == null || hook == null)
        {
            Debug.LogError(
                "PlayerAnimationController: " +
                "Chua gan Movement hoac Hook.",
                this);

            enabled = false;
        }
    }

    private void Update()
    {
        animator.SetBool(
            WalkingParameter,
            movement.isActiveAndEnabled &&
            movement.IsWalking);

        bool shouldReel =
            !IsCatching &&
            animator.GetBool(FishingParameter) &&
            hook.isActiveAndEnabled &&
            hook.IsDeployed &&
            hook.IsReeling;

        animator.SetBool(
            ReelingParameter,
            shouldReel);
    }

    public void SetFishing(bool isFishing)
    {
        animator.SetBool(FishingParameter, isFishing);

        if (!isFishing)
        {
            animator.SetBool(ReelingParameter, false);
            CancelCast();
        }
    }

    public void BeginCast()
    {
        animator.ResetTrigger(CastParameter);
        animator.ResetTrigger(CastFinishedParameter);

        animator.SetBool(ReelingParameter, false);
        animator.SetBool(CastHeldParameter, true);

        animator.SetTrigger(CastParameter);
    }

    public void ReleaseCast()
    {
        animator.SetBool(CastHeldParameter, false);
    }

    public void FinishCast()
    {
        animator.SetBool(CastHeldParameter, false);
        animator.SetTrigger(CastFinishedParameter);
    }

    public void CancelCast()
    {
        animator.SetBool(CastHeldParameter, false);
        animator.SetBool(ReelingParameter, false);

        animator.ResetTrigger(CastParameter);
        animator.ResetTrigger(CastFinishedParameter);
    }

    public void BeginCatch()
    {
        CancelCast();
        IsCatching = true;
        animator.ResetTrigger(CatchParameter);
        animator.SetTrigger(CatchParameter);
    }

    public void NotifyCatchEntered()
    {
        IsCatching = true;
        CatchStarted?.Invoke();
    }

    public void NotifyCatchExited()
    {
        IsCatching = false;
        CatchFinished?.Invoke();
    }

    public void CancelCatch()
    {
        IsCatching = false;
        animator.ResetTrigger(CatchParameter);
    }

    private void OnDisable()
    {
        IsCatching = false;
        if (animator != null)
        {
            animator.SetBool(WalkingParameter, false);
            animator.SetBool(ReelingParameter, false);
        }
    }
}