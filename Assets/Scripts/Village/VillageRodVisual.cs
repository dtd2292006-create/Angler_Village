using UnityEngine;

// Preserve the authored Idle/Walk socket animation; only resolve the equipped rod's image.
public sealed class VillageRodVisual : MonoBehaviour
{
    [SerializeField] private SpriteRenderer rod;
    private void LateUpdate()
    {
        if (PlayerInventory.Instance == null || rod == null) return;
        var equipment = PlayerInventory.Instance.GetComponent<PlayerEquipment>();
        if (equipment == null || !equipment.IsReady) return;
        var definition = equipment.Equipped(FishingEquipmentKind.Rod);
        rod.enabled = definition != null;
        if (definition != null) rod.sprite = definition.RodSprite;
    }
}
