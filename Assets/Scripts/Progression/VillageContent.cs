using System;
using UnityEngine;

[Serializable]
public sealed class EquipmentShopOffer
{
    public FishingEquipmentDefinition item;
    [Min(0)] public int price;
}
[Serializable]
public sealed class FishMarketOffer
{
    public FishDefinition fish;
    [Min(0)] public int sellPrice = 8;
}
[CreateAssetMenu(menuName = "Fishing/Village Content")]
public sealed class VillageContent : ScriptableObject
{
    public FishingEquipmentDefinition starterLine;
    public FishingEquipmentDefinition starterBait;
    public CommissionDefinition firstCommission;
    public CommissionDefinition[] commissions = new CommissionDefinition[0];
    public EquipmentShopOffer[] equipment = new EquipmentShopOffer[0];
    public FishMarketOffer[] fish = new FishMarketOffer[0];
    public CommissionDefinition Find(string id)
    {
        if (firstCommission != null && firstCommission.id == id) return firstCommission;
        foreach (var definition in commissions) if (definition != null && definition.id == id) return definition;
        return null;
    }
    public FishMarketOffer FishPrice(string id)
    {
        foreach (var offer in fish) if (offer != null && offer.fish != null && offer.fish.SpeciesId == id) return offer;
        return null;
    }
}
