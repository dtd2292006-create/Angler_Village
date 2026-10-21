using UnityEngine;

// Bridges the saved loadout to scene components; does not own inventory or movement.
[DefaultExecutionOrder(80)]
public sealed class FishingEquipmentRig : MonoBehaviour
{
    [SerializeField] private HookController hook;
    [SerializeField] private FishResistanceController resistance;
    [SerializeField] private BaitVisualController bait;
    [SerializeField] private SpriteRenderer rodRenderer;
    [SerializeField] private PlayerFishingController fishing;
    [SerializeField] private PlayerAnimationController animationController;
    private PlayerEquipment equipment;
    private FishingEquipmentDefinition rod;
    private bool castInProgress;
    private bool refreshPending;
    private string activeLineId;
    private string activeBaitId;
    private bool baitUsed;
    private HookCatchController hookCatch;
    public bool CanChange => isActiveAndEnabled && !castInProgress && hook != null && !hook.IsDeployed &&
        (fishing == null || !fishing.IsCasting) &&
        (animationController == null || !animationController.IsCatching);
    private void Start()
    {
        if (PlayerInventory.Instance == null) return;
        equipment = PlayerInventory.Instance.GetComponent<PlayerEquipment>();
        if (equipment == null || !equipment.IsReady) return;
        equipment.RegisterRig(this);
        equipment.Changed += Apply;
        hook.TryAuthorizeCast = AuthorizeCast;
        hookCatch = hook.GetComponent<HookCatchController>();
        if (hookCatch != null) hookCatch.TryPayBiteCost = PayBiteCost;
        hook.LineBroken += ConsumeBrokenLine;
        Apply();
    }
    private void Apply()
    {
        refreshPending = true;
        if (!CanChange || equipment == null) return;
        refreshPending = false;
        rod = equipment.Equipped(FishingEquipmentKind.Rod);
        var line = equipment.Equipped(FishingEquipmentKind.Line);
        var lure = equipment.Equipped(FishingEquipmentKind.Bait);
        hook.SetEquipmentReady(rod != null && line != null && lure != null);
        if (rodRenderer != null) rodRenderer.enabled = rod != null;
        if (rod != null) hook.ApplyRodDefinition(rod);
        if (line != null && resistance != null) resistance.SetLineDefinition(line.Line);
        if (lure != null && bait != null) bait.SetDefinition(lure.Bait);
        if (rod != null && rodRenderer != null) rodRenderer.sprite = rod.RodSprite;
    }
    private bool AuthorizeCast()
    {
        if (!isActiveAndEnabled || equipment == null || !equipment.IsReady) return false;
        var currentRod = equipment.Equipped(FishingEquipmentKind.Rod);
        var line = equipment.Equipped(FishingEquipmentKind.Line);
        var lure = equipment.Equipped(FishingEquipmentKind.Bait);
        if (currentRod == null || line == null || lure == null) return false;
        castInProgress = true;
        activeLineId = line.ItemId;
        activeBaitId = lure.ItemId;
        baitUsed = false;
        return true; // The bait remains owned until a successful bite.
    }
    private bool PayBiteCost()
    {
        if (!castInProgress || baitUsed || equipment == null) return false;
        // Keep the rig valid throughout this fight, even when this is the final bait.
        if (!equipment.TryConsumeEquipped(FishingEquipmentKind.Bait, activeBaitId)) return false;
        baitUsed = true;
        return true;
    }
    private void ConsumeBrokenLine()
    {
        // BreakLine may be requested by multiple systems, but only the first succeeds.
        string lostLine = activeLineId;
        activeLineId = null;
        if (!string.IsNullOrEmpty(lostLine) && equipment != null)
            equipment.TryConsumeEquipped(FishingEquipmentKind.Line, lostLine);
        refreshPending = true;
    }
    private void LateUpdate()
    {
        if (castInProgress && hook != null && !hook.IsDeployed &&
            (fishing == null || !fishing.IsCasting) &&
            (animationController == null || !animationController.IsCatching))
        {
            castInProgress = false;
            activeLineId = null;
            activeBaitId = null;
            baitUsed = false;
            refreshPending = true;
        }
        if (refreshPending && CanChange) Apply();
    }
    private void OnDestroy()
    {
        if (hook != null) { hook.TryAuthorizeCast = null; hook.LineBroken -= ConsumeBrokenLine; }
        if (hookCatch != null) hookCatch.TryPayBiteCost = null;
        if (equipment == null) return;
        equipment.Changed -= Apply;
        equipment.UnregisterRig(this);
    }
}

