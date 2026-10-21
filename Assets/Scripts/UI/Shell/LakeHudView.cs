using AnglerVillage.UI;
using UnityEngine;
using UnityEngine.UI;

public sealed class LakeHudView : MonoBehaviour
{
    [SerializeField] private GameObject hudRoot;
    [SerializeField] private FishInventoryView inventoryView;
    [SerializeField] private HookController hook;
    [SerializeField] private HookCatchController catchController;
    [SerializeField] private PlayerFishingController fishing;
    [SerializeField] private FishingQteController qte;
    [SerializeField] private Text clockLabel;
    [SerializeField] private Text fishLabel;
    [SerializeField] private Text gearLabel;
    [SerializeField] private Text hint;
    [SerializeField] private GameObject chargePanel;
    [SerializeField] private Slider charge;
    private float summaryTimer;
    private void Awake()
    {
        foreach (Text text in GetComponentsInChildren<Text>(true)) InventoryUIFont.Ensure(text);
    }
    private void Update()
    {
        hudRoot.SetActive(inventoryView == null || !inventoryView.IsOpen);
        summaryTimer -= Time.unscaledDeltaTime;
        var inventory = PlayerInventory.Instance;
        if (inventory != null && summaryTimer <= 0)
        {
            summaryTimer = 0.2f;
            var clock = inventory.GetComponent<PlayerWorldClock>();
            if (clock != null) clockLabel.text = "Ngày " + clock.Day + " · " + clock.Hour.ToString("00") + ":" + clock.Minute.ToString("00");
            fishLabel.text = "Cá trong túi: " + inventory.FishCount;
            var equipment = inventory.GetComponent<PlayerEquipment>();
            if (equipment != null && equipment.IsReady)
            {
                gearLabel.text = Name(equipment, FishingEquipmentKind.Rod) + " / " + Name(equipment, FishingEquipmentKind.Line) + "\n" +
                    Name(equipment, FishingEquipmentKind.Bait);
            }
        }
        chargePanel.SetActive(fishing != null && fishing.IsCasting);
        charge.SetValueWithoutNotify(fishing != null ? fishing.CurrentCharge01 : 0);
        hint.text = hook == null ? "A/D di chuyển · E nói chuyện · I túi đồ · Esc tạm dừng" : qte.IsFailReelLocked ? "Tạm khóa thu dây sau QTE thất bại" :
            qte.IsActive ? "Nhấp chuột khi kim vào vùng trắng hoặc xanh" :
            hook.IsDeployed ? "Giữ chuột trái để thu · Nhả để mồi chìm" :
            !hook.CanLaunch ? "Thiếu trang bị hoặc hết mồi. Mở túi [I]; hết đồ thì về cửa hàng ở làng." : "A/D di chuyển · Giữ/thả chuột để quăng · I túi đồ · Esc tạm dừng";
    }
    private static string Name(PlayerEquipment equipment, FishingEquipmentKind kind)
    {
        var definition = equipment.Equipped(kind);
        if (definition == null) return "Chưa trang bị";
        if (kind != FishingEquipmentKind.Bait) return definition.DisplayName;
        int count = 0;
        foreach (var item in equipment.Items) if (item.ItemId == definition.ItemId) count = item.Quantity;
        return definition.DisplayName + " ×" + count;
    }
}
