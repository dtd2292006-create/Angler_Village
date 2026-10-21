using UnityEngine;

public sealed class ShopService : MonoBehaviour
{
    [SerializeField] private VillageContent content;
    public VillageContent Content => content;
    private PlayerInventory Inventory => PlayerInventory.Instance;
    private PlayerEquipment Equipment => Inventory.GetComponent<PlayerEquipment>();
    private PlayerProgress Progress => PlayerProgress.Current;
    public bool ClaimStarter(out string message)
    {
        message = null;
        if (Progress == null || !Progress.IsReady || !Progress.Certified) { message = "Đến guild nhận chứng nhận trước nhé."; return false; }
        if (Progress.StarterKitClaimed) { message = "Bạn đã nhận bộ khởi đầu."; return false; }
        if (!CanGrant(content.starterLine) || !CanGrant(content.starterBait)) { message = "Bộ khởi đầu chưa được cấu hình đúng."; return false; }
        using (Inventory.GetComponent<PlayerSaveController>().DeferWrites())
        {
            Equipment.TryGrant(content.starterLine.ItemId, 1, out _);
            Equipment.TryGrant(content.starterBait.ItemId, 1, out _);
            Equipment.TryEquip(content.starterLine.ItemId, out _);
            Equipment.TryEquip(content.starterBait.ItemId, out _);
            Progress.ClaimKit(); Progress.Notify();
        }
        message = "Đã nhận và trang bị dây + mồi miễn phí. Đi về biển bên phải để tới hồ.";
        return true;
    }
    private bool CanGrant(FishingEquipmentDefinition definition)
    {
        if (definition == null || !definition.IsUsable || !Equipment.TryGet(definition.ItemId, out var registered) || registered != definition) return false;
        foreach (var item in Equipment.Items) if (item.ItemId == definition.ItemId && item.Quantity == int.MaxValue) return false;
        return true;
    }
    public bool HasEmergencyBait(FishingEquipmentDefinition item, int price)
    {
        if (Progress == null || Equipment == null || item == null || item != content.starterBait ||
            price <= 0 || Progress.Coins >= price) return false;
        foreach (var owned in Equipment.Items)
            if (owned.Kind == FishingEquipmentKind.Bait && owned.Quantity > 0) return false;
        return true;
    }
    public bool BuyEquipment(int index, out string message)
    {
        message = null;
        if (Progress == null || !Progress.Certified || !Progress.StarterKitClaimed) { message = "Nhận bộ khởi đầu miễn phí trước nhé."; return false; }
        if (index < 0 || index >= content.equipment.Length) return false;
        var offer = content.equipment[index];
        bool emergency = offer != null && HasEmergencyBait(offer.item, offer.price);
        int price = emergency ? 0 : offer != null ? offer.price : -1;
        if (offer == null || price < 0 || !CanGrant(offer.item) || Progress.Coins < price)
        { message = "Không đủ xu hoặc vật phẩm chưa sẵn sàng."; return false; }
        // Free basic line is a recovery, not an unlimited stack grant.
        if (price == 0)
            foreach (var item in Equipment.Items) if (item.ItemId == offer.item.ItemId && item.Quantity > 0)
            { message = "Bạn vẫn còn vật phẩm này. Hãy trang bị trong túi đồ."; return false; }
        using (Inventory.GetComponent<PlayerSaveController>().DeferWrites())
        {
            if (!Equipment.TryGrant(offer.item.ItemId, 1, out message)) return false;
            Progress.Spend(price); Progress.Notify();
        }
        message = emergency ? "Đã nhận 1 mồi cứu trợ miễn phí. Trang bị trong túi [I] để tiếp tục câu." :
            "Đã mua " + offer.item.DisplayName + ". Trang bị trong túi đồ [I].";
        return true;
    }
    public int EquipmentSellPrice(string id)
    {
        foreach (var offer in content.equipment) if (offer != null && offer.item != null && offer.item.ItemId == id) return Mathf.Max(0, offer.price / 2);
        return -1;
    }
    public bool SellEquipment(string id, out string message)
    {
        message = null;
        if (Progress == null || !Progress.Certified || !Progress.StarterKitClaimed) return false;
        int price = EquipmentSellPrice(id);
        if (price < 0 || !Equipment.TryGet(id, out var definition) || (long)Progress.Coins + price > int.MaxValue)
        { message = "Vật phẩm không bán được."; return false; }
        if (definition.Kind == FishingEquipmentKind.Rod || definition.Kind == FishingEquipmentKind.Bait)
        {
            long rods = 0;
            foreach (var item in Equipment.Items) if (item.Kind == definition.Kind) rods += item.Quantity;
            if (rods <= 1) { message = "Hãy giữ lại ít nhất một cần và một mồi để tiếp tục câu."; return false; }
        }
        using (Inventory.GetComponent<PlayerSaveController>().DeferWrites())
        {
            if (!Equipment.TryRemove(id, 1, out message)) return false;
            Progress.Reward(price, 0); Progress.Notify();
        }
        message = "Đã bán trang bị: +" + price + " xu."; return true;
    }
    public bool SellFish(string instanceId, out string message)
    {
        message = null;
        if (Progress == null || !Progress.Certified || !Progress.StarterKitClaimed) return false;
        CaughtFishInstance selected = null;
        foreach (var fish in Inventory.Fish) if (fish.InstanceId == instanceId) selected = fish;
        var offer = selected != null ? content.FishPrice(selected.SpeciesId) : null;
        if (offer == null || offer.sellPrice < 0 || (long)Progress.Coins + offer.sellPrice > int.MaxValue)
        { message = "Cửa hàng chưa định giá con cá này."; return false; }
        using (Inventory.GetComponent<PlayerSaveController>().DeferWrites())
        {
            if (!Inventory.TryRemoveFish(instanceId)) return false;
            Progress.Reward(offer.sellPrice, 0); Progress.Notify();
        }
        message = "Đã bán cá: +" + offer.sellPrice + " xu."; return true;
    }
}
