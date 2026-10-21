using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnglerVillage.UI
{
    public sealed class FishInventoryView : MonoBehaviour
    {
        [Header("Editable layout references")]
        [SerializeField] private GameObject window;
        [SerializeField] private Button openButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private ScrollRect scroll;
        [SerializeField] private RectTransform content;
        [SerializeField] private FishInventorySlotView slotPrefab;
        [SerializeField] private Text countLabel;
        [SerializeField] private Text emptyLabel;
        [SerializeField] private Image detailImage;
        [SerializeField] private Text detailName;
        [SerializeField] private Text detailText;
        [Header("Do not interrupt a fishing action")]
        [SerializeField] private HookController hook;
        [SerializeField] private PlayerFishingController fishing;
        [SerializeField] private PlayerAnimationController animationController;
        public bool IsOpen => window != null && window.activeSelf;
        private PlayerInventory inventory;
        private string selectedId;
        private readonly List<FishInventorySlotView> slots = new List<FishInventorySlotView>();
        private void Awake()
        {
            window.SetActive(false);
            openButton.onClick.AddListener(Toggle);
            closeButton.onClick.AddListener(Close);
        }
        public void Toggle()
        {
            if (IsOpen) Close(); else Open();
        }
        public void Open()
        {
            if (IsOpen || Time.timeScale <= 0 ||
                (hook != null && hook.IsDeployed) ||
                (fishing != null && fishing.IsCasting) ||
                (animationController != null && animationController.IsCatching)) return;
            inventory = PlayerInventory.Instance;
            if (inventory == null || !inventory.IsReady)
            {
                Debug.LogWarning("Túi cá chưa sẵn sàng. Kiểm tra PlayerData / PlayerSaveController.", this);
                return;
            }
            inventory.Changed += Refresh;
            GameInputBlock.Set(this, true);
            window.SetActive(true);
            Refresh();
        }
        public void Close()
        {
            if (inventory != null) inventory.Changed -= Refresh;
            GameInputBlock.Set(this, false);
            if (window != null) window.SetActive(false);
        }
        private void Refresh()
        {
            if (inventory == null) return;
            var fish = inventory.Fish;
            countLabel.text = "CÁ ĐÃ CÂU: " + fish.Count;
            emptyLabel.gameObject.SetActive(fish.Count == 0);
            // Reuse slots; update only when inventory changes, never every frame.
            for (int i = 0; i < fish.Count; i++)
            {
                if (i == slots.Count) slots.Add(Instantiate(slotPrefab, content));
                var item = fish[fish.Count - 1 - i];
                inventory.TryGetDefinition(item, out FishDefinition definition);
                slots[i].gameObject.SetActive(true);
                slots[i].Bind(item, definition, Select);
            }
            for (int i = fish.Count; i < slots.Count; i++) slots[i].gameObject.SetActive(false);
            bool retained = false;
            foreach (var item in fish) if (item.InstanceId == selectedId) retained = true;
            if (!retained) selectedId = fish.Count > 0 ? fish[fish.Count - 1].InstanceId : null;
            Select(selectedId);
            LayoutRebuilder.ForceRebuildLayoutImmediate(content);
            if (!retained) scroll.verticalNormalizedPosition = 1f;
        }
        private void Select(string id)
        {
            selectedId = id;
            foreach (var slot in slots) slot.Select(id != null && slot.ItemId == id);
            CaughtFishInstance selected = null;
            foreach (var item in inventory.Fish) if (item.InstanceId == id) { selected = item; break; }
            FishDefinition definition = null;
            if (selected != null) inventory.TryGetDefinition(selected, out definition);
            detailName.text = selected == null ? "Chưa có cá" : definition != null ? definition.DisplayName : "Cá chưa xác định";
            detailImage.sprite = definition != null ? definition.Sprite : null;
            detailImage.enabled = detailImage.sprite != null;
            detailText.text = selected == null ? "Câu cá để bổ sung vào túi.\nChọn một con cá để xem thông tin." :
                "Lượt bắt: #" + selected.CatchSequence + "\n" +
                (definition != null ? "Độ sâu sinh sống: " + definition.MinDepth.ToString("0.##") + " – " +
                    definition.MaxDepth.ToString("0.##") + " đơn vị game\n\n" : "Không tìm thấy dữ liệu loài cá.\n\n") +
                "Mỗi con cá được lưu riêng. Cá vẫn được giữ khi ủy thác hết hạn.";
        }
        private void OnDisable() => Close();
        private void OnDestroy()
        {
            openButton.onClick.RemoveListener(Toggle);
            closeButton.onClick.RemoveListener(Close);
        }
    }
}
