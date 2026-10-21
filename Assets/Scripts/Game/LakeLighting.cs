using UnityEngine;
using UnityEngine.Rendering.Universal;

public sealed class LakeLighting : MonoBehaviour
{
    [SerializeField] private Light2D globalLight;
    [SerializeField] private Color dayColor = Color.white;
    [SerializeField] private Color nightColor = new Color(0.55f, 0.65f, 0.9f, 1);
    [SerializeField, Range(0.1f, 1)] private float nightIntensity = 0.55f;
    private float baseIntensity;
    private void Awake() { if (globalLight != null) baseIntensity = globalLight.intensity; }
    private void Update()
    {
        if (globalLight == null || PlayerInventory.Instance == null) return;
        var clock = PlayerInventory.Instance.GetComponent<PlayerWorldClock>();
        if (clock == null) return;
        float hour = (float)(clock.ElapsedMinutes % 1440) / 60;
        float daylight = Mathf.SmoothStep(0, 1, Mathf.Clamp01(Mathf.Sin((hour - 6) * Mathf.PI / 12)));
        globalLight.color = Color.Lerp(nightColor, dayColor, daylight);
        globalLight.intensity = baseIntensity * Mathf.Lerp(nightIntensity, 1, daylight);
    }
}
