using UnityEngine;

[CreateAssetMenu(menuName = "Fishing/Guild/Commission")]
public sealed class CommissionDefinition : ScriptableObject
{
    public string id;
    public string title;
    [TextArea] public string description;
    [Tooltip("Empty accepts any species.")] public FishDefinition species;
    [Min(1)] public int quantity = 1;
    public bool requiresFreshCatch;
    [Min(1)] public int minimumLevel = 1;
    [Min(1)] public float durationGameMinutes = 1440;
    [Min(0)] public int rewardCoins = 30;
    [Min(0)] public int rewardXp = 40;
}
