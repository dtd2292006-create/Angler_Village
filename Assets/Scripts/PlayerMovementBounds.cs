using UnityEngine;

public class PlayerMovementBounds : MonoBehaviour
{
    [SerializeField] private BoxCollider2D allowedArea;
    [SerializeField] private CapsuleCollider2D playerCollider;

    [SerializeField, Min(0f)]
    private float edgePadding = 0.01f;

    private bool warnedAboutWidth;

    public bool IsConfigured =>
        allowedArea != null &&
        playerCollider != null &&
        allowedArea.enabled &&
        playerCollider.enabled &&
        allowedArea.gameObject.activeInHierarchy;

    public float ClampRootX(float desiredRootX, float currentRootX)
    {
        if (!IsConfigured)
            return currentRootX;

        Bounds area = allowedArea.bounds;
        Bounds player = playerCollider.bounds;

        // Tinh ca Offset va kich thuoc collider,
        // khong chi chan diem goc Player.
        float rootX = transform.position.x;
        float leftOffset = player.min.x - rootX;
        float rightOffset = player.max.x - rootX;

        float minimumRootX =
            area.min.x - leftOffset + edgePadding;

        float maximumRootX =
            area.max.x - rightOffset - edgePadding;

        if (minimumRootX > maximumRootX)
        {
            if (!warnedAboutWidth)
            {
                Debug.LogError(
                    "PlayerBounds hep hon Capsule Collider cua Player.",
                    this);

                warnedAboutWidth = true;
            }

            return currentRootX;
        }

        return Mathf.Clamp(
            desiredRootX,
            minimumRootX,
            maximumRootX
        );
    }
}