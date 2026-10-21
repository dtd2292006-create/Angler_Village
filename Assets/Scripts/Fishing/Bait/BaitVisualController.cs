using UnityEngine;

[DisallowMultipleComponent]
public class BaitVisualController : MonoBehaviour
{
    [Header("Data")]
    [SerializeField]
    private BaitDefinition definition;

    [Header("Renderers")]
    [SerializeField]
    private SpriteRenderer baitRenderer;

    [SerializeField]
    private SpriteRenderer innerCircleRenderer;

    [SerializeField]
    private SpriteRenderer outerCircleRenderer;

    public BaitDefinition Definition => definition;

    private void Awake()
    {
        ApplyDefinition();
    }

    private void OnValidate()
    {
        ApplyDefinition();
    }

    private void LateUpdate()
    {
        bool visible =
            baitRenderer != null &&
            baitRenderer.enabled &&
            baitRenderer.gameObject.activeInHierarchy;

        if (innerCircleRenderer != null)
            innerCircleRenderer.enabled = visible;

        if (outerCircleRenderer != null)
            outerCircleRenderer.enabled = visible;
    }

    public void SetDefinition(BaitDefinition newDefinition)
    {
        definition = newDefinition;
        ApplyDefinition();
    }

    [ContextMenu("Apply Definition")]
    public void ApplyDefinition()
    {
        if (outerCircleRenderer != null)
            outerCircleRenderer.color = Color.white;

        if (definition == null)
            return;

        if (baitRenderer != null)
            baitRenderer.sprite = definition.Sprite;

        if (innerCircleRenderer != null)
            innerCircleRenderer.color = definition.CircleColor;
    }

    private void OnDisable()
    {
        if (innerCircleRenderer != null)
            innerCircleRenderer.enabled = false;

        if (outerCircleRenderer != null)
            outerCircleRenderer.enabled = false;
    }
}