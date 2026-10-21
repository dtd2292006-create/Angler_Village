using System;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(HookController))]
public class HookCatchController : MonoBehaviour
{
    public FishCatchable AttachedFish { get; private set; }
    public bool HasFish => AttachedFish != null;
    public string LastLandedCatchId { get; private set; }
    public bool CanAcceptFish => isActiveAndEnabled &&
        hook != null && hook.CanFishBite && !HasFish && !baitConsumedThisCast;

    public event Action<FishCatchable> FishLanded;
    public event Action<FishCatchable> AttachedFishChanged;

    // Pay only after a fish has successfully attached, before publishing the bite.
    public Func<bool> TryPayBiteCost { private get; set; }
    private bool baitConsumedThisCast;
    private HookController hook;

    private void Awake()
    {
        hook = GetComponent<HookController>();
    }

    private void OnEnable()
    {
        if (hook == null) hook = GetComponent<HookController>();
        hook.Retrieved += HandleRetrieved;
        hook.ReturnedToRest += CancelAttachment;
    }

    public bool TryAttach(FishCatchable fish)
    {
        if (!CanAcceptFish || fish == null || !fish.CanBeHooked)
            return false;

        // Giu cho truoc khi tat sensor: chi mot con duoc chap nhan.
        AttachedFish = fish;
        if (!fish.AttachTo(this, hook))
        {
            AttachedFish = null;
            return false;
        }

        if (TryPayBiteCost != null && !TryPayBiteCost())
        {
            AttachedFish = null;
            fish.ReleaseFromHook();
            return false;
        }
        baitConsumedThisCast = true;
        hook.SetBaitConsumed(true);
        AttachedFishChanged?.Invoke(fish);
        return true;
    }

    private void HandleRetrieved()
    {
        FishCatchable fish = AttachedFish;
        AttachedFish = null;
        baitConsumedThisCast = false;
        AttachedFishChanged?.Invoke(null);
        if (fish == null) return;

        // Tat con ca trong ho; khong huy prefab/du lieu loai.
        fish.MarkLanded();
        LastLandedCatchId = Guid.NewGuid().ToString("N");
        FishLanded?.Invoke(fish);
    }

    public void CancelAttachment()
    {
        FishCatchable fish = AttachedFish;
        AttachedFish = null;
        AttachedFishChanged?.Invoke(null);
        if (fish != null) fish.ReleaseFromHook();
        if (hook != null)
        {
            if (!hook.IsDeployed) baitConsumedThisCast = false;
            hook.SetBaitConsumed(baitConsumedThisCast);
        }
    }

    public void ForgetFish(FishCatchable fish)
    {
        if (AttachedFish != fish) return;
        AttachedFish = null;
        AttachedFishChanged?.Invoke(null);
        if (hook != null)
        {
            if (!hook.IsDeployed) baitConsumedThisCast = false;
            hook.SetBaitConsumed(baitConsumedThisCast);
        }
    }

    private void OnDisable()
    {
        if (hook != null)
        {
            hook.Retrieved -= HandleRetrieved;
            hook.ReturnedToRest -= CancelAttachment;
        }
        CancelAttachment();
    }
}
