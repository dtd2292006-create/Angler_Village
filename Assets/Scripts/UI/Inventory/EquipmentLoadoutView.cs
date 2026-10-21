using UnityEngine;
using UnityEngine.UI;

namespace AnglerVillage.UI
{
    // Presentation only. Card positions and sizes belong to the prefab.
    public sealed class EquipmentLoadoutView : MonoBehaviour
    {
        [SerializeField] private Image[] icons;
        [SerializeField] private Text[] labels;
        private PlayerEquipment equipment;
        private void OnEnable() { Refresh(); }
        private void Start() { Refresh(); }
        private void Update()
        {
            if (equipment == null && PlayerInventory.Instance != null) Refresh();
        }
        private void Refresh()
        {
            var current = PlayerInventory.Instance != null ? PlayerInventory.Instance.GetComponent<PlayerEquipment>() : null;
            if (current != equipment)
            {
                if (equipment != null) equipment.Changed -= Refresh;
                equipment = current;
                if (equipment != null) equipment.Changed += Refresh;
            }
            var kinds = new[] { FishingEquipmentKind.Rod, FishingEquipmentKind.Line, FishingEquipmentKind.Bait };
            for (int i = 0; i < kinds.Length && i < labels.Length && i < icons.Length; i++)
            {
                var item = equipment != null && equipment.IsReady ? equipment.Equipped(kinds[i]) : null;
                icons[i].enabled = item != null && item.Icon != null;
                icons[i].sprite = item != null ? item.Icon : null;
                labels[i].text = item != null ? item.DisplayName : "Chưa trang bị";
                if (item != null && kinds[i] == FishingEquipmentKind.Bait)
                    foreach (var owned in equipment.Items)
                        if (owned.ItemId == item.ItemId) labels[i].text += " ×" + owned.Quantity;
            }
        }
        private void OnDisable() { if (equipment != null) equipment.Changed -= Refresh; equipment = null; }
    }
}
