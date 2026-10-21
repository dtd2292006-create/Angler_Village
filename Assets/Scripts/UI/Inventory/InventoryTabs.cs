using UnityEngine;
using UnityEngine.UI;

namespace AnglerVillage.UI
{
    public sealed class InventoryTabs : MonoBehaviour
    {
        [SerializeField] private GameObject fishPage;
        [SerializeField] private GameObject equipmentPage;
        [SerializeField] private EquipmentInventoryView equipmentView;
        [SerializeField] private Button fishButton;
        [SerializeField] private Button gearButton;
        [SerializeField] private Button baitButton;
        [SerializeField] private FigmaPanelGraphic fishFrame;
        [SerializeField] private FigmaPanelGraphic gearFrame;
        [SerializeField] private FigmaPanelGraphic baitFrame;
        private void Awake()
        {
            fishButton.onClick.AddListener(Fish);
            gearButton.onClick.AddListener(Gear);
            baitButton.onClick.AddListener(Bait);
            Fish();
        }
        private void Fish() => Show(0);
        private void Gear() => Show(1);
        private void Bait() => Show(2);
        private void Show(int tab)
        {
            fishPage.SetActive(tab == 0);
            equipmentView.ShowBait(tab == 2);
            equipmentPage.SetActive(tab != 0);
            fishFrame.SetSelected(tab == 0);
            gearFrame.SetSelected(tab == 1);
            baitFrame.SetSelected(tab == 2);
        }
        private void OnDestroy()
        {
            if (fishButton != null) fishButton.onClick.RemoveListener(Fish);
            if (gearButton != null) gearButton.onClick.RemoveListener(Gear);
            if (baitButton != null) baitButton.onClick.RemoveListener(Bait);
        }
    }
}
