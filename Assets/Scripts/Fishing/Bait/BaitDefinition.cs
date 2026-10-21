using UnityEngine;

[CreateAssetMenu(
    fileName = "Bait_New",
    menuName = "Fishing/Bait Definition")]
public class BaitDefinition : ScriptableObject
{
    [Header("Identity")]
    [SerializeField]
    private string displayName;

    [Header("Appearance")]
    [SerializeField]
    private Sprite sprite;

    [SerializeField]
    private Color circleColor = Color.white;

    public string DisplayName => displayName;
    public Sprite Sprite => sprite;
    public Color CircleColor => circleColor;
}