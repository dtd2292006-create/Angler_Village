using UnityEngine;
using UnityEngine.UI;

// Existing HUD component name retained; it now displays fish resistance.
[DisallowMultipleComponent]
[RequireComponent(typeof(CanvasGroup))]
public class FishingTensionView : MonoBehaviour
{
    [SerializeField] private FishResistanceController source;
    [SerializeField] private Slider slider;
    [SerializeField] private Image fillImage;
    [SerializeField] private Color safeColor = new Color(0.2f, 0.9f, 0.4f);
    [SerializeField] private Color warningColor = new Color(1f, 0.8f, 0.15f);
    [SerializeField] private Color dangerColor = new Color(1f, 0.2f, 0.2f);

    private CanvasGroup group;

    private void Awake()
    {
        group = GetComponent<CanvasGroup>();
        group.interactable = false;
        group.blocksRaycasts = false;
        group.alpha = 0f;
        if (source == null || slider == null)
        {
            Debug.LogError("FishingTensionView: Source must now be FishResistanceController; also assign Slider.", this);
            enabled = false;
            return;
        }
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;
        slider.interactable = false;
        Navigation navigation = slider.navigation;
        navigation.mode = Navigation.Mode.None;
        slider.navigation = navigation;
    }

    private void LateUpdate()
    {
        bool visible = source != null && source.HasFight;
        group.alpha = visible ? 1f : 0f;
        if (!visible) return;

        float value = Mathf.Clamp01(source.Ratio);
        slider.SetValueWithoutNotify(value);
        if (fillImage == null) return;
        Color tint = value <= source.GreenThreshold ? safeColor
            : value < source.RedThreshold
                ? Color.Lerp(warningColor, dangerColor,
                    Mathf.InverseLerp(source.GreenThreshold, source.RedThreshold, value))
                : dangerColor;
        if (source.Overload01 > 0f)
            tint = Color.Lerp(tint, Color.white,
                (Mathf.Sin(Time.time * 14f) * 0.5f + 0.5f) * 0.4f * source.Overload01);
        fillImage.color = tint;
    }

    private void OnDisable()
    {
        if (group != null) group.alpha = 0f;
    }
}
