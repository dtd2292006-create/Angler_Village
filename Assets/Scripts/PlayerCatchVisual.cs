using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Animator))]
public class PlayerCatchVisual : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private SpriteRenderer heldFishRenderer;

    [Header("Preview")]
    [Tooltip("Ca mau de kiem tra animation khi chua co he thong bat ca.")]
    [SerializeField]
    private Sprite previewFish;

    private Sprite pendingFish;

    private void Awake()
    {
        if (heldFishRenderer == null)
        {
            Debug.LogError(
                "PlayerCatchVisual: Chua gan Held Fish Renderer.",
                this);

            enabled = false;
            return;
        }

        HideFish();
    }

    // Hệ thống bắt cá sẽ gọi trước khi kích hoạt animation Catch.
    public void SetCaughtFish(Sprite fishSprite)
    {
        pendingFish = fishSprite;
    }

    // Animation Event gọi khi cần hiện cá trên tay.
    public void OnCatchShowFish()
    {
        if (heldFishRenderer == null)
            return;

        Sprite fish = pendingFish != null
            ? pendingFish
            : previewFish;

        if (fish == null)
        {
            HideFish();

            Debug.LogWarning(
                "PlayerCatchVisual: Chua co Sprite ca de hien.",
                this);

            return;
        }

        heldFishRenderer.sprite = fish;
        heldFishRenderer.enabled = true;
    }

    // Animation Event gọi khi kết thúc phần nâng cá.
    public void OnCatchHideFish()
    {
        HideFish();
        pendingFish = null;
    }

    public void ClearCaughtFish()
    {
        HideFish();
        pendingFish = null;
    }

    private void HideFish()
    {
        if (heldFishRenderer != null)
            heldFishRenderer.enabled = false;
    }

    private void OnDisable()
    {
        ClearCaughtFish();
    }
}