using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(BoxCollider2D))]
public class FishSwimmingArea : MonoBehaviour
{
    [Tooltip("WaterArea dung de lay cao do mat nuoc.")]
    [SerializeField]
    private BoxCollider2D waterArea;

    private BoxCollider2D swimmingBounds;

    private void Awake()
    {
        swimmingBounds = GetComponent<BoxCollider2D>();
    }

    public bool TryGetRegion(
        FishDefinition definition,
        SpriteRenderer visual,
        Transform fishRoot,
        out Rect region)
    {
        region = default;

        if (swimmingBounds == null)
            swimmingBounds = GetComponent<BoxCollider2D>();

        if (!isActiveAndEnabled ||
            definition == null ||
            visual == null ||
            visual.sprite == null ||
            fishRoot == null ||
            waterArea == null ||
            !waterArea.enabled ||
            !waterArea.gameObject.activeInHierarchy ||
            !swimmingBounds.enabled ||
            !swimmingBounds.gameObject.activeInHierarchy)
        {
            return false;
        }

        Bounds area = swimmingBounds.bounds;
        Bounds water = waterArea.bounds;
        Bounds fish = visual.bounds;

        Vector3 rootPosition = fishRoot.position;

        // Khoảng hình cá nhô ra so với gốc Fish.
        float leftOffset = fish.min.x - rootPosition.x;
        float rightOffset = fish.max.x - rootPosition.x;
        float bottomOffset = fish.min.y - rootPosition.y;
        float topOffset = fish.max.y - rootPosition.y;

        // Giới hạn trong phần giao giữa vùng bơi và vùng nước.
        float left = Mathf.Max(area.min.x, water.min.x);
        float right = Mathf.Min(area.max.x, water.max.x);
        float bottom = Mathf.Max(area.min.y, water.min.y);
        float top = Mathf.Min(area.max.y, water.max.y);

        float minX = left - leftOffset;
        float maxX = right - rightOffset;

        float surfaceY = water.max.y;

        float minY = Mathf.Max(
            bottom - bottomOffset,
            surfaceY - definition.MaxDepth);

        float maxY = Mathf.Min(
            top - topOffset,
            surfaceY - definition.MinDepth);

        if (minX > maxX || minY > maxY)
            return false;

        region = Rect.MinMaxRect(minX, minY, maxX, maxY);
        return true;
    }
}