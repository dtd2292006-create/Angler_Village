using UnityEngine;

public enum FishingEquipmentKind { Rod, Line, Bait }

[CreateAssetMenu(menuName = "Fishing/Inventory/Equipment Definition")]
public sealed class FishingEquipmentDefinition : ScriptableObject
{
    [SerializeField] private string itemId;
    [SerializeField] private string displayName;
    [SerializeField] private FishingEquipmentKind kind;
    [SerializeField] private Sprite icon;
    [SerializeField, TextArea] private string description;
    [Header("Existing gameplay definitions")]
    [SerializeField] private FishingLineDefinition line;
    [SerializeField] private BaitDefinition bait;
    [Header("Rod — same grip/pivot as the existing rig")]
    [SerializeField] private Sprite rodSprite;
    [SerializeField, Min(0.1f)] private float minCastSpeed = 3;
    [SerializeField, Min(0.1f)] private float maxCastSpeed = 7;
    [SerializeField, Min(0.1f)] private float minLineLength = 2;
    [SerializeField, Min(0.1f)] private float maxLineLength = 7;
    public string ItemId => itemId;
    public string DisplayName => displayName;
    public FishingEquipmentKind Kind => kind;
    public Sprite Icon => icon;
    public string Description => description;
    public FishingLineDefinition Line => line;
    public BaitDefinition Bait => bait;
    public Sprite RodSprite => rodSprite;
    public float MinCastSpeed => Mathf.Max(0.1f, minCastSpeed);
    public float MaxCastSpeed => Mathf.Max(MinCastSpeed, maxCastSpeed);
    public float MinLineLength => Mathf.Max(0.1f, minLineLength);
    public float MaxLineLength => Mathf.Max(MinLineLength, maxLineLength);
    public bool IsUsable => !string.IsNullOrWhiteSpace(itemId) &&
        (kind == FishingEquipmentKind.Rod ? rodSprite != null :
         kind == FishingEquipmentKind.Line ? line != null : bait != null);
}
