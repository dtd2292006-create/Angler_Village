using System;
using UnityEngine;
using UnityEngine.UI;

namespace AnglerVillage.UI
{
    public sealed class EquipmentSlotView : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private FigmaPanelGraphic frame;
        [SerializeField] private Image icon;
        [SerializeField] private Text label;
        private string id;
        private Action<string> clicked;
        private void Awake()
        {
            InventoryUIFont.Ensure(label);
            button.onClick.AddListener(Click);
        }
        public void Bind(OwnedFishingItem owned, FishingEquipmentDefinition definition, bool equipped, Action<string> callback)
        {
            id = owned.ItemId;
            clicked = callback;
            label.text = (definition != null ? definition.DisplayName : "Đồ chưa xác định") +
                (owned.Kind == FishingEquipmentKind.Bait ? " ×" + owned.Quantity : "") +
                (equipped ? " ✓" : "");
            icon.sprite = definition != null ? definition.Icon : null;
            icon.enabled = icon.sprite != null;
            icon.preserveAspect = true;
        }
        public void SetSelected(string selected) => frame.SetSelected(id == selected);
        private void Click() => clicked?.Invoke(id);
        private void OnDestroy() { if (button != null) button.onClick.RemoveListener(Click); }
    }
}
