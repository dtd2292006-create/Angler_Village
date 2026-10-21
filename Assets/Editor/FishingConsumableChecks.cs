#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public static class FishingConsumableChecks
{
    [MenuItem("Tools/Fishing/Validate Consumables")]
    public static void Validate()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        { Debug.LogWarning("Exit Play Mode before running consumable checks."); return; }
        var fixture = new GameObject("Consumable check fixture");
        fixture.hideFlags = HideFlags.HideAndDontSave;
        try
        {
            var catalog = AssetDatabase.LoadAssetAtPath<FishingEquipmentCatalog>(
                "Assets/Data/Equipment/EquipmentCatalog_Main.asset");
            Require(catalog != null, "Missing equipment catalog.");
            var content = AssetDatabase.LoadAssetAtPath<VillageContent>(
                "Assets/Data/Village/VillageContent_Main.asset");
            Require(content != null && content.starterLine != null && content.starterBait != null,
                "Missing starter recovery items.");
            var state = new EquipmentInventory();
            catalog.Seed(state);
            string bait = content.starterBait.ItemId, line = content.starterLine.ItemId;
            state.Grant(bait, FishingEquipmentKind.Bait, 2);
            state.Grant(line, FishingEquipmentKind.Line, 1);
            state.Equip(bait); state.Equip(line);
            var equipment = fixture.AddComponent<PlayerEquipment>();
            var serialized = new SerializedObject(equipment);
            serialized.FindProperty("catalog").objectReferenceValue = catalog;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            typeof(PlayerEquipment).GetMethod("Initialize", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(equipment, new object[] { state, false });
            int changes = 0;
            equipment.Changed += () => changes++;
            Require(!Consume(equipment, FishingEquipmentKind.Line, bait), "Reject wrong category.");
            Require(!Consume(equipment, FishingEquipmentKind.Bait, "missing"), "Reject stale ID.");
            Require(changes == 0, "Rejected consumption must not publish changes.");
            var rig = fixture.AddComponent<FishingEquipmentRig>();
            typeof(FishingEquipmentRig).GetField("equipment", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(rig, equipment);
            Require(CallRig(rig, "AuthorizeCast") && CallRig(rig, "AuthorizeCast"), "Empty casts remain possible.");
            Require(state.Find(bait).Quantity == 2 && changes == 0, "Casting without a bite must not consume bait.");
            Require(CallRig(rig, "PayBiteCost"), "Successful bite consumes first bait.");
            Require(!CallRig(rig, "PayBiteCost"), "Repeated bite callback must not double-charge.");
            Require(state.Find(bait).Quantity == 1 && state.EquippedId(FishingEquipmentKind.Bait) == bait,
                "Retain equipped bait while a spare remains.");
            Require(CallRig(rig, "AuthorizeCast"), "Next cast with final bait.");
            Require(state.Find(bait).Quantity == 1, "Final bait remains owned while waiting for a fish.");
            Require(CallRig(rig, "PayBiteCost"), "Successful bite consumes final bait.");
            Require(!CallRig(rig, "AuthorizeCast"), "Reject a new cast when no bait remains.");
            Require(state.Find(bait) == null && state.EquippedId(FishingEquipmentKind.Bait) == null,
                "Clear empty bait stack and loadout.");
            Require(!Consume(equipment, FishingEquipmentKind.Bait, bait), "No negative stock.");
            Require(state.Find(line).Quantity == 1, "Using bait must not consume line.");
            Require(Consume(equipment, FishingEquipmentKind.Line, line), "Broken line use.");
            Require(!Consume(equipment, FishingEquipmentKind.Line, line), "No repeated line loss.");
            Require(changes == 3, "Exactly one event per successful consumption.");
            var json = JsonUtility.ToJson(state.Export());
            Require(EquipmentInventory.TryRestore(JsonUtility.FromJson<EquipmentSaveData>(json),
                out var restored, out var error), error);
            Require(restored.Find(bait) == null && restored.Find(line) == null &&
                restored.EquippedId(FishingEquipmentKind.Bait) == null &&
                restored.EquippedId(FishingEquipmentKind.Line) == null,
                "Reload must preserve exhausted stacks without replenishment.");
            Debug.Log("Consumables checks passed: quantities, category guards, change events and depleted save round trip.");
        }
        catch (Exception error) { Debug.LogException(error); }
        finally { UnityEngine.Object.DestroyImmediate(fixture); }
    }
    private static bool CallRig(FishingEquipmentRig rig, string method)
    {
        return (bool)typeof(FishingEquipmentRig).GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(rig, null);
    }
    private static bool Consume(PlayerEquipment equipment, FishingEquipmentKind kind, string id)
    {
        return (bool)typeof(PlayerEquipment).GetMethod("TryConsumeEquipped", BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(equipment, new object[] { kind, id });
    }
    private static void Require(bool value, string message)
    { if (!value) throw new InvalidOperationException(message); }
}
#endif
