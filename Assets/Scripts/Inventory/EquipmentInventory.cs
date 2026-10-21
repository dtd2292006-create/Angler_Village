using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

[Serializable]
public sealed class EquipmentSaveData
{
    public int version = 1;
    public List<EquipmentStackData> items = new List<EquipmentStackData>();
    public string rodId;
    public string lineId;
    public string baitId;
}
[Serializable]
public sealed class EquipmentStackData
{
    public string itemId;
    public int kind;
    public int quantity;
}
public sealed class OwnedFishingItem
{
    public string ItemId { get; }
    public FishingEquipmentKind Kind { get; }
    public int Quantity { get; internal set; }
    internal OwnedFishingItem(string id, FishingEquipmentKind kind, int quantity)
    { ItemId = id; Kind = kind; Quantity = quantity; }
}

// Pure ownership/loadout rules. No UI, scene objects, file IO or fishing states.
public sealed class EquipmentInventory
{
    private readonly List<OwnedFishingItem> items = new List<OwnedFishingItem>();
    public ReadOnlyCollection<OwnedFishingItem> Items => items.AsReadOnly();
    private readonly string[] equipped = new string[3];
    public string EquippedId(FishingEquipmentKind kind) => equipped[(int)kind];
    public OwnedFishingItem Find(string id) => items.Find(item => item.ItemId == id);
    public void Grant(string id, FishingEquipmentKind kind, int quantity)
    {
        if (string.IsNullOrWhiteSpace(id) || (int)kind < 0 || (int)kind > 2 || quantity <= 0)
            throw new ArgumentException("Invalid equipment grant.");
        var item = Find(id);
        if (item != null)
        {
            if (item.Kind != kind) throw new InvalidOperationException("Equipment ID changed category.");
            item.Quantity = checked(item.Quantity + quantity);
        }
        else items.Add(new OwnedFishingItem(id, kind, quantity));
    }
    public bool Equip(string id)
    {
        var item = Find(id);
        if (item == null || item.Quantity <= 0 || equipped[(int)item.Kind] == id) return false;
        equipped[(int)item.Kind] = id;
        return true;
    }
    public EquipmentSaveData Export()
    {
        var data = new EquipmentSaveData { rodId = equipped[0], lineId = equipped[1], baitId = equipped[2] };
        foreach (var item in items) data.items.Add(new EquipmentStackData
            { itemId = item.ItemId, kind = (int)item.Kind, quantity = item.Quantity });
        return data;
    }
    public static bool TryRestore(EquipmentSaveData data, out EquipmentInventory inventory, out string error)
    {
        inventory = new EquipmentInventory();
        error = null;
        if (data == null) return true; // Legacy fish-only save; caller grants starter equipment once.
        if (data.version != 1 || data.items == null)
        { inventory = null; error = "Unsupported or malformed equipment save."; return false; }
        foreach (var item in data.items)
        {
            if (item == null || string.IsNullOrWhiteSpace(item.itemId) || item.kind < 0 || item.kind > 2 ||
                item.quantity <= 0 || inventory.Find(item.itemId) != null)
            { inventory = null; error = "Invalid or duplicate equipment stack."; return false; }
            inventory.Grant(item.itemId, (FishingEquipmentKind)item.kind, item.quantity);
        }
        string[] ids = { data.rodId, data.lineId, data.baitId };
        for (int i = 0; i < ids.Length; i++)
        {
            if (string.IsNullOrEmpty(ids[i])) continue;
            var owned = inventory.Find(ids[i]);
            if (owned == null || (int)owned.Kind != i)
            { inventory = null; error = "Equipped item is not owned or has the wrong category."; return false; }
            inventory.Equip(ids[i]);
        }
        return true;
    }
}
