using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class PlayerEquipment : MonoBehaviour
{
    [SerializeField] private FishingEquipmentCatalog catalog;
    public bool IsReady { get; private set; }
    public event Action Changed;
    private EquipmentInventory state = new EquipmentInventory();
    private FishingEquipmentRig rig;
    public IReadOnlyList<OwnedFishingItem> Items => state.Items;
    internal EquipmentInventory State => state;
    internal void Initialize(EquipmentInventory loaded, bool seed)
    {
        if (catalog == null) throw new InvalidOperationException("PlayerEquipment: assign equipment catalog.");
        if (!catalog.Validate(out var error)) throw new InvalidOperationException(error);
        if (seed) catalog.Seed(loaded);
        state = loaded;
        IsReady = true;
        Changed?.Invoke();
    }
    public bool TryGet(string id, out FishingEquipmentDefinition definition)
    {
        definition = null;
        return catalog != null && catalog.TryGet(id, out definition);
    }
    public FishingEquipmentDefinition Equipped(FishingEquipmentKind kind)
    {
        TryGet(state.EquippedId(kind), out var definition);
        return definition;
    }
    public bool IsEquipped(string id)
    {
        var item = state.Find(id);
        return item != null && state.EquippedId(item.Kind) == id;
    }
    internal void RegisterRig(FishingEquipmentRig value) => rig = value;
    internal void UnregisterRig(FishingEquipmentRig value) { if (rig == value) rig = null; }
    // Shops/rewards grant only registered item types; the catalog alone does not grant ownership.
    public bool TryGrant(string id, int quantity, out string message)
    {
        message = null;
        if (!IsReady || quantity <= 0 || !TryGet(id, out var definition) || !definition.IsUsable)
        { message = "Vật phẩm hoặc số lượng không hợp lệ."; return false; }
        try { state.Grant(id, definition.Kind, quantity); }
        catch (Exception error) { message = error.Message; return false; }
        Changed?.Invoke();
        return true;
    }
    public bool TryEquip(string id, out string message)
    {
        message = null;
        if (!IsReady || (rig != null && !rig.CanChange))
        { message = "Chỉ đổi trang bị khi đã thu mồi và kết thúc động tác."; return false; }
        if (!TryGet(id, out var definition) || !definition.IsUsable || state.Find(id) == null ||
            state.Find(id).Kind != definition.Kind)
        { message = "Vật phẩm không khả dụng."; return false; }
        if (!state.Equip(id)) return false;
        Changed?.Invoke();
        return true;
    }
}
