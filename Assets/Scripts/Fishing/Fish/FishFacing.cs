using UnityEngine;

[DisallowMultipleComponent]
public class FishFacing : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private Transform visualRoot;

    [SerializeField]
    private SpriteRenderer visualRenderer;

    private Vector3 originalScale;
    private bool initialized;

    private void Awake()
    {
        Initialize();
    }

    private bool Initialize()
    {
        if (initialized)
            return true;

        if (visualRoot == null || visualRenderer == null)
        {
            Debug.LogError(
                "FishFacing: Chua gan Visual Root hoac Visual Renderer.",
                this);

            return false;
        }

        if (visualRoot == transform)
        {
            Debug.LogError(
                "FishFacing: Visual Root phai la object Visual con, " +
                "khong phai object goc ca.",
                this);

            return false;
        }

        if (!visualRoot.IsChildOf(transform) ||
            !visualRenderer.transform.IsChildOf(visualRoot))
        {
            Debug.LogError(
                "FishFacing: Kiem tra cau truc Visual cua ca.",
                this);

            return false;
        }

        originalScale = visualRoot.localScale;

        if (Mathf.Abs(originalScale.x) < 0.0001f)
        {
            Debug.LogError(
                "FishFacing: Scale X cua Visual khong duoc bang 0.",
                this);

            return false;
        }

        initialized = true;
        return true;
    }

    public void SetFacing(bool faceRight, bool spriteFacesRight)
    {
        if (!Initialize())
            return;

        // Không lật ảnh lần thứ hai bằng SpriteRenderer.
        visualRenderer.flipX = false;

        bool mirror = faceRight != spriteFacesRight;

        Vector3 scale = originalScale;
        scale.x = Mathf.Abs(originalScale.x) *
                  (mirror ? -1f : 1f);

        visualRoot.localScale = scale;
    }
}