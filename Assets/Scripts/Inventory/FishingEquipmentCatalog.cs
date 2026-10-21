using System;
using UnityEngine;

[Serializable]
public sealed class StartingFishingItem
{
    public FishingEquipmentDefinition definition;
    [Min(1)] public int quantity = 1;
    public bool equip = true;
}

[CreateAssetMenu(menuName = "Fishing/Inventory/Equipment Catalog")]
public sealed class FishingEquipmentCatalog : ScriptableObject
{
    [SerializeField] private FishingEquipmentDefinition[] items = new FishingEquipmentDefinition[0];
    [SerializeField] private StartingFishingItem[] startingItems = new StartingFishingItem[0];
    public FishingEquipmentDefinition[] Items => (FishingEquipmentDefinition[])items.Clone();
    public bool TryGet(string id, out FishingEquipmentDefinition definition)
    {
        definition = null;
        foreach (var item in items)
            if (item != null && item.ItemId == id) { definition = item; return true; }
        return false;
    }
    public void Seed(EquipmentInventory inventory)
    {
        foreach (var start in startingItems)
        {
            if (start == null || start.definition == null || start.quantity < 1 ||
                !TryGet(start.definition.ItemId, out var registered) || registered != start.definition || !registered.IsUsable)
                throw new InvalidOperationException("Invalid starting equipment. Check EquipmentCatalog_Main.");
            inventory.Grant(registered.ItemId, registered.Kind, start.quantity);
            if (start.equip) inventory.Equip(registered.ItemId);
        }
    }
    public bool Validate(out string error)
    {
        var ids = new System.Collections.Generic.HashSet<string>(StringComparer.Ordinal);
        foreach (var item in items)
            if (item == null || !item.IsUsable || !ids.Add(item.ItemId))
            { error = "Equipment catalog has missing, invalid or duplicate IDs."; return false; }
        error = null;
        return true;
    }
}
