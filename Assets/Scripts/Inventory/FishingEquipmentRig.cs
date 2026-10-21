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
    public bool CanChange => isActiveAndEnabled && hook != null && !hook.IsDeployed &&
        (fishing == null || !fishing.IsCasting) &&
        (animationController == null || !animationController.IsCatching);
    private void Start()
    {
        if (PlayerInventory.Instance == null) return;
        equipment = PlayerInventory.Instance.GetComponent<PlayerEquipment>();
        if (equipment == null || !equipment.IsReady) return;
        equipment.RegisterRig(this);
        equipment.Changed += Apply;
        Apply();
    }
    private void Apply()
    {
        if (!CanChange || equipment == null) return;
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
    private void OnDestroy()
    {
        if (equipment == null) return;
        equipment.Changed -= Apply;
        equipment.UnregisterRig(this);
    }
}

