using System;
using UnityEngine;
using UnityEngine.UI;

namespace AnglerVillage.UI
{
    public sealed class FishInventorySlotView : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private FigmaPanelGraphic frame;
        [SerializeField] private Image icon;
        [SerializeField] private Text label;
        public string ItemId { get; private set; }
        private Action<string> clicked;
        private void Awake()
        {
            InventoryUIFont.Ensure(label);
            UIButtonAudio.Ensure(button.gameObject);
            button.onClick.AddListener(Click);
        }
        public void Bind(CaughtFishInstance item, FishDefinition definition, Action<string> onClick)
        {
            ItemId = item.InstanceId;
            clicked = onClick;
            label.text = definition != null ? definition.DisplayName : "Cá chưa xác định";
            icon.sprite = definition != null ? definition.Sprite : null;
            icon.enabled = icon.sprite != null;
            icon.preserveAspect = true;
        }
        public void Select(bool value) => frame.SetSelected(value);
        private void Click() => clicked?.Invoke(ItemId);
        private void OnDestroy() => button.onClick.RemoveListener(Click);
    }
}

