using UnityEngine;

[DefaultExecutionOrder(-900)]
[DisallowMultipleComponent]
[RequireComponent(typeof(HookCatchController))]
public class FishCatchInventoryReceiver : MonoBehaviour
{
    [SerializeField] private PlayerInventory inventory;
    private HookCatchController hookCatch;

    private void Awake() { hookCatch = GetComponent<HookCatchController>(); }

    private void OnEnable()
    {
        if (hookCatch == null) hookCatch = GetComponent<HookCatchController>();
        hookCatch.FishLanded += HandleFishLanded;
    }

    private void HandleFishLanded(FishCatchable fish)
    {
        PlayerInventory target = inventory != null && inventory.IsPrimary ? inventory : PlayerInventory.Instance;
        if (target == null || fish == null || !fish.IsLanded)
        {
            Debug.LogError("FishCatchInventoryReceiver: missing PlayerInventory or invalid landed fish.", this);
            return;
        }
        string message;
        if (target.TryRecordCatch(hookCatch.LastLandedCatchId, fish.Definition, out message))
            Debug.Log("Da them ca vao tui: " + fish.Definition.DisplayName + " | Tong: " + target.FishCount, this);
        else if (!string.IsNullOrEmpty(message)) Debug.LogError(message, this);
    }

    private void OnDisable()
    {
        if (hookCatch != null) hookCatch.FishLanded -= HandleFishLanded;
    }
}
