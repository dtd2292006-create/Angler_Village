using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerInputReader))]
[RequireComponent(typeof(PlayerMovement))]
public class PlayerModeController : MonoBehaviour
{
    [SerializeField]
    private PlayerAnimationController animationController;

    [Header("Runtime Debug - xem khi Play")]
    [SerializeField]
    private float debugMoveAxis;

    [SerializeField]
    private bool debugInputEnabled;

    [SerializeField]
    private bool debugFishing;

    [SerializeField]
    private bool debugMovementLocked;

    [SerializeField]
    private bool debugFaceRight;

    [SerializeField]
    private List<Object> debugExitLockOwners =
        new List<Object>();

    public bool IsFishing { get; private set; }
    public FishingSpot CurrentSpot { get; private set; }

    private PlayerInputReader input;
    private PlayerMovement movement;

    private readonly HashSet<Object> exitLocks =
        new HashSet<Object>();

    private void Awake()
    {
        input = GetComponent<PlayerInputReader>();
        movement = GetComponent<PlayerMovement>();

        if (animationController == null)
        {
            Debug.LogError(
                "PlayerModeController: Chua gan Animation Controller.",
                this);

            enabled = false;
        }
    }

    private void Update()
    {
        exitLocks.RemoveWhere(owner => owner == null);

        UpdateDebugFields();

        if (!IsFishing || Time.timeScale == 0f)
            return;

        if (CurrentSpot == null ||
            !CurrentSpot.isActiveAndEnabled)
        {
            ExitFishing();
            return;
        }

        if (exitLocks.Count > 0)
            return;

        float axis = input.MoveAxis;

        bool movingTowardShore = CurrentSpot.FaceRight
            ? axis < 0f
            : axis > 0f;

        if (movingTowardShore)
            ExitFishing();
    }

    private void UpdateDebugFields()
    {
        debugMoveAxis = input != null ? input.MoveAxis : 0f;

        debugInputEnabled =
            input != null && input.isActiveAndEnabled;

        debugFishing = IsFishing;

        debugMovementLocked =
            movement != null && movement.IsMovementLocked;

        debugFaceRight =
            CurrentSpot != null && CurrentSpot.FaceRight;

        debugExitLockOwners.Clear();

        foreach (Object owner in exitLocks)
            debugExitLockOwners.Add(owner);
    }

    public bool TryEnterFishing(FishingSpot spot)
    {
        if (!isActiveAndEnabled ||
            IsFishing ||
            spot == null ||
            spot.StandPoint == null ||
            !movement.isActiveAndEnabled)
        {
            return false;
        }

        if (!movement.TryPlaceAtHorizontalPosition(
                spot.StandPoint.position.x))
        {
            return false;
        }

        exitLocks.Clear();

        CurrentSpot = spot;
        IsFishing = true;

        movement.SetFacing(spot.FaceRight);
        movement.SetMovementLocked(true);

        animationController.SetFishing(true);
        return true;
    }

    public void SetFishingExitLocked(Object owner, bool locked)
    {
        if (owner == null)
            return;

        if (locked)
            exitLocks.Add(owner);
        else
            exitLocks.Remove(owner);
    }

    public void ExitFishing()
    {
        exitLocks.Clear();

        if (!IsFishing)
            return;

        IsFishing = false;
        CurrentSpot = null;

        movement.SetMovementLocked(false);

        if (animationController != null)
            animationController.SetFishing(false);
    }

    private void OnDisable()
    {
        ExitFishing();
    }
}