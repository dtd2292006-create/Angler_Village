using UnityEngine;

[CreateAssetMenu(fileName = "Line_New", menuName = "Fishing/Line Definition")]
public class FishingLineDefinition : ScriptableObject
{
    [SerializeField] private string displayName = "Basic Line";
    [SerializeField, Min(1f)] private float resistanceCapacity = 100f;
    [SerializeField, Min(0.01f)] private float progressDrainMultiplier = 1f;
    [SerializeField, Min(0.1f)] private float qteWindowMultiplier = 1f;

    // Retain old asset/API compatibility only. The new fight does not use this value.
    [SerializeField, HideInInspector] private float strength = 1.5f;
    public float Strength => Mathf.Max(0.01f, strength);

    public string DisplayName => displayName;
    public float ResistanceCapacity => Mathf.Max(1f, resistanceCapacity);
    public float ProgressDrainMultiplier => Mathf.Max(0.01f, progressDrainMultiplier);
    public float QteWindowMultiplier => Mathf.Max(0.1f, qteWindowMultiplier);

    private void OnValidate()
    {
        resistanceCapacity = Mathf.Max(1f, resistanceCapacity);
        progressDrainMultiplier = Mathf.Max(0.01f, progressDrainMultiplier);
        qteWindowMultiplier = Mathf.Max(0.1f, qteWindowMultiplier);
    }
}
