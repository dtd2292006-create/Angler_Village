using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnglerVillage.UI
{
    public sealed class EquipmentInventoryView : MonoBehaviour
    {
        [SerializeField] private RectTransform content;
        [SerializeField] private EquipmentSlotView slotPrefab;
        [SerializeField] private Text loadoutLabel;
        [SerializeField] private Text emptyLabel;
        [SerializeField] private Text detailName;
        [SerializeField] private Text detailText;
        [SerializeField] private Image detailImage;
        [SerializeField] private Button equipButton;
        [SerializeField] private Text equipLabel;
        private readonly List<EquipmentSlotView> slots = new List<EquipmentSlotView>();
        private PlayerEquipment equipment;
        private bool baitOnly;
        private string selected;
        private void Awake()
        {
            foreach (var text in GetComponentsInChildren<Text>(true)) InventoryUIFont.Ensure(text);
            equipButton.onClick.AddListener(Equip);
        }
        private void OnEnable()
        {
            if (PlayerInventory.Instance != null) equipment = PlayerInventory.Instance.GetComponent<PlayerEquipment>();
            if (equipment != null) equipment.Changed += Refresh;
            Refresh();
        }
        private void OnDisable() { if (equipment != null) equipment.Changed -= Refresh; }
        public void ShowBait(bool value) { baitOnly = value; selected = null; if (isActiveAndEnabled) Refresh(); }
        private void Refresh()
        {
            if (equipment == null || !equipment.IsReady) return;
            loadoutLabel.text = "Đang dùng: " + Name(FishingEquipmentKind.Rod) + " / " +
                Name(FishingEquipmentKind.Line) + " / " + Name(FishingEquipmentKind.Bait);
            int index = 0;
            bool retained = false;
            string first = null;
            foreach (var owned in equipment.Items)
            {
                if ((owned.Kind == FishingEquipmentKind.Bait) != baitOnly) continue;
                if (index == slots.Count) slots.Add(Instantiate(slotPrefab, content));
                var slot = slots[index++];
                slot.gameObject.SetActive(true);
                equipment.TryGet(owned.ItemId, out var definition);
                slot.Bind(owned, definition, equipment.IsEquipped(owned.ItemId), Select);
                if (first == null) first = owned.ItemId;
                if (selected == owned.ItemId) retained = true;
            }
            for (int i = index; i < slots.Count; i++) slots[i].gameObject.SetActive(false);
            emptyLabel.gameObject.SetActive(index == 0);
            Select(retained ? selected : first);
            LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        }
        private string Name(FishingEquipmentKind kind)
        {
            var definition = equipment.Equipped(kind);
            return definition != null ? definition.DisplayName : "Chưa có";
        }
        private void Select(string id)
        {
            selected = id;
            foreach (var slot in slots) slot.SetSelected(id);
            FishingEquipmentDefinition definition = null;
            if (id != null) equipment.TryGet(id, out definition);
            bool current = id != null && equipment.IsEquipped(id);
            equipButton.interactable = definition != null && !current;
            equipLabel.text = current ? "Đang sử dụng" : "Trang bị";
            detailName.text = definition != null ? definition.DisplayName : "Chưa chọn vật phẩm";
            detailImage.sprite = definition != null ? definition.Icon : null;
            detailImage.enabled = detailImage.sprite != null;
            if (definition == null) { detailText.text = "Chọn vật phẩm để xem thông tin."; return; }
            string stats = definition.Kind == FishingEquipmentKind.Rod ?
                "Giới hạn dây: " + definition.MaxLineLength.ToString("0.##") + " đơn vị\nVận tốc quăng tối đa: " + definition.MaxCastSpeed.ToString("0.##") :
                definition.Kind == FishingEquipmentKind.Line ?
                "Trần kháng cự: " + definition.Line.ResistanceCapacity.ToString("0.##") +
                "\nHiệu quả giảm kháng cự: ×" + definition.Line.ProgressDrainMultiplier.ToString("0.##") +
                "\nĐộ dễ QTE: ×" + definition.Line.QteWindowMultiplier.ToString("0.##") : "Dùng để thu hút các loài cá thích loại mồi này.";
            detailText.text = definition.Description + "\n\n" + stats;
        }
        private void Equip()
        {
            if (equipment == null || selected == null) return;
            if (!equipment.TryEquip(selected, out string message) && !string.IsNullOrEmpty(message)) detailText.text = message;
        }
        private void OnDestroy() { if (equipButton != null) equipButton.onClick.RemoveListener(Equip); }
    }
}
