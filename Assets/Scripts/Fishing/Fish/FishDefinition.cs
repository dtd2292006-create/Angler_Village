using UnityEngine;

public enum FishSwimStyle
{
    Patrol,
    SwimAndRest,
    Wave
}

[CreateAssetMenu(
    fileName = "Fish_New",
    menuName = "Fishing/Fish Definition")]
public class FishDefinition : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string displayName;
    [SerializeField, HideInInspector] private string speciesId;
    public string SpeciesId => speciesId;

    [Header("Appearance")]
    [SerializeField] private Sprite sprite;
    [SerializeField] private bool spriteFacesRight = true;

    [Header("Preferred Baits")]
    [SerializeField]
    private BaitDefinition[] preferredBaits =
        new BaitDefinition[0];

    [Header("Habitat - Depth From Water Surface")]
    [SerializeField, Min(0f)] private float minDepth = 0.5f;
    [SerializeField, Min(0f)] private float maxDepth = 2f;

    [Header("Swimming")]
    [SerializeField]
    private FishSwimStyle swimStyle = FishSwimStyle.Patrol;

    [SerializeField, Min(0.01f)]
    private float swimSpeed = 0.5f;

    [SerializeField]
    private Vector2 swimDurationRange = new Vector2(2f, 4f);

    [Header("Turning")]
    [Tooltip("Toc do tang van toc, unit/giay binh phuong.")]
    [SerializeField, Min(0.01f)]
    private float acceleration = 0.8f;

    [Tooltip("Toc do giam van toc truoc khi quay dau.")]
    [SerializeField, Min(0.01f)]
    private float deceleration = 1.2f;

    [Tooltip("Thoi gian dung truoc khi boi theo huong moi.")]
    [SerializeField, Min(0f)]
    private float turnPause = 0.15f;

    [Header("Swim And Rest")]
    [SerializeField]
    private Vector2 restDurationRange = new Vector2(0.5f, 1.5f);

    [Header("Wave")]
    [SerializeField, Min(0f)]
    private float waveAmplitude = 0.15f;

    [SerializeField, Min(0.01f)]
    private float waveFrequency = 0.4f;

    [Header("Hooked - Struggle")]
    [Tooltip("Dimensionless resistance while struggling. Reel speed is divided by 1 + resistance.")]
    [SerializeField, Min(0f)] private float struggleResistance = 2f;
    [SerializeField, Min(0f)] private float recoveryResistance = 0.2f;
    [SerializeField] private Vector2 struggleDurationRange = new Vector2(0.8f, 1.4f);
    [SerializeField] private Vector2 recoveryDurationRange = new Vector2(0.8f, 1.5f);
    [Tooltip("Acceleration away from the rod while underwater and struggling, in units/s squared.")]
    [SerializeField, Min(0f)] private float struggleAcceleration = 0.8f;

    [SerializeField, HideInInspector] private float linePullStrength = 1f;
    [SerializeField, HideInInspector] private float recoveryPullMultiplier = 0.6f;
    [SerializeField, HideInInspector] private float strugglePullMultiplier = 1.6f;

    public float LinePullStrength => linePullStrength;
    public float RecoveryPullMultiplier => recoveryPullMultiplier;
    public float StrugglePullMultiplier => strugglePullMultiplier;

    public float StruggleResistance => struggleResistance;
    public float RecoveryResistance => recoveryResistance;
    public Vector2 StruggleDurationRange => struggleDurationRange;
    public Vector2 RecoveryDurationRange => recoveryDurationRange;
    public float StruggleAcceleration => struggleAcceleration;

    [Header("Fight - Resistance")]
    [Tooltip("Remaining strength at the farthest distance of the cast, before QTE/time adjustments.")]
    [SerializeField, Min(0f)] private float initialFightResistance = 80f;
    // Preserved for old assets/API; the distance curve replaces the per-unit model.
    [SerializeField, HideInInspector] private float resistanceDropPerUnit = 8f;
    [Tooltip("1 = linear by distance; above 1 = drops faster when approaching; below 1 = stays high longer.")]
    [SerializeField, Min(0.1f)] private float distanceResistanceExponent = 1f;
    [SerializeField, Min(0f)] private float idleResistanceGainPerSecond = 2f;

    [Header("Fight - QTE Rewards")]
    [SerializeField, Min(0f)] private float goodResistanceDrop = 12f;
    [SerializeField, Min(0f)] private float greatResistanceDrop = 28f;
    [SerializeField, Min(0f)] private float failResistanceGain = 18f;
    [SerializeField, Min(0f)] private float goodBonusReel = 0.15f;
    [SerializeField, Min(0f)] private float greatBonusReel = 0.65f;

    [Header("Fight - Species QTE")]
    [Tooltip("Fraction of one complete turn, before fish/line matchup scaling.")]
    [SerializeField, Range(0.06f, 0.45f)] private float qteGoodWidth = 0.2f;
    [SerializeField, Range(0.012f, 0.18f)] private float qteGreatWidth = 0.06f;
    [SerializeField, Min(0.5f)] private float qteSweepSeconds = 1.8f;
    [SerializeField] private Vector2 mediumQteInterval = new Vector2(2.5f, 4f);
    [SerializeField] private Vector2 redQteInterval = new Vector2(1f, 1.8f);

    public float InitialFightResistance => initialFightResistance;
    public float ResistanceDropPerUnit => resistanceDropPerUnit;
    public float DistanceResistanceExponent => Mathf.Max(0.1f, distanceResistanceExponent);
    public float IdleResistanceGainPerSecond => idleResistanceGainPerSecond;
    public float GoodResistanceDrop => goodResistanceDrop;
    public float GreatResistanceDrop => greatResistanceDrop;
    public float FailResistanceGain => failResistanceGain;
    public float GoodBonusReel => goodBonusReel;
    public float GreatBonusReel => greatBonusReel;
    public float QteGoodWidth => qteGoodWidth;
    public float QteGreatWidth => qteGreatWidth;
    public float QteSweepSeconds => qteSweepSeconds;
    public Vector2 MediumQteInterval => mediumQteInterval;
    public Vector2 RedQteInterval => redQteInterval;

    public string DisplayName => displayName;
    public Sprite Sprite => sprite;
    public bool SpriteFacesRight => spriteFacesRight;

    public float MinDepth => minDepth;
    public float MaxDepth => maxDepth;

    public FishSwimStyle SwimStyle => swimStyle;
    public float SwimSpeed => swimSpeed;
    public Vector2 SwimDurationRange => swimDurationRange;

    public float Acceleration => acceleration;
    public float Deceleration => deceleration;
    public float TurnPause => turnPause;

    public Vector2 RestDurationRange => restDurationRange;
    public float WaveAmplitude => waveAmplitude;
    public float WaveFrequency => waveFrequency;

    public bool PrefersBait(BaitDefinition bait)
    {
        if (bait == null || preferredBaits == null)
            return false;

        foreach (BaitDefinition preferredBait in preferredBaits)
        {
            if (preferredBait == bait)
                return true;
        }

        return false;
    }

    private void OnValidate()
    {
        initialFightResistance = Mathf.Max(0f, initialFightResistance);
        resistanceDropPerUnit = Mathf.Max(0f, resistanceDropPerUnit);
        distanceResistanceExponent = Mathf.Max(0.1f, distanceResistanceExponent);
        idleResistanceGainPerSecond = Mathf.Max(0f, idleResistanceGainPerSecond);
        goodResistanceDrop = Mathf.Max(0f, goodResistanceDrop);
        greatResistanceDrop = Mathf.Max(goodResistanceDrop, greatResistanceDrop);
        failResistanceGain = Mathf.Max(0f, failResistanceGain);
        goodBonusReel = Mathf.Max(0f, goodBonusReel);
        greatBonusReel = Mathf.Max(goodBonusReel, greatBonusReel);
        qteGoodWidth = Mathf.Clamp(qteGoodWidth, 0.06f, 0.45f);
        qteGreatWidth = Mathf.Clamp(qteGreatWidth, 0.012f, Mathf.Min(0.18f, qteGoodWidth * 0.65f));
        qteSweepSeconds = Mathf.Max(0.5f, qteSweepSeconds);
        mediumQteInterval = ValidateRange(mediumQteInterval, 0.6f);
        redQteInterval = ValidateRange(redQteInterval, 0.6f);
        linePullStrength = Mathf.Max(0f, linePullStrength);
        recoveryPullMultiplier = Mathf.Max(0f, recoveryPullMultiplier);
        strugglePullMultiplier = Mathf.Max(recoveryPullMultiplier, strugglePullMultiplier);
        struggleResistance = Mathf.Max(0f, struggleResistance);
        recoveryResistance = Mathf.Clamp(recoveryResistance, 0f, struggleResistance);
        struggleAcceleration = Mathf.Max(0f, struggleAcceleration);
        struggleDurationRange = ValidateRange(struggleDurationRange, 0.1f);
        recoveryDurationRange = ValidateRange(recoveryDurationRange, 0.1f);

        minDepth = Mathf.Max(0f, minDepth);
        maxDepth = Mathf.Max(minDepth, maxDepth);

        swimSpeed = Mathf.Max(0.01f, swimSpeed);
        acceleration = Mathf.Max(0.01f, acceleration);
        deceleration = Mathf.Max(0.01f, deceleration);
        turnPause = Mathf.Max(0f, turnPause);

        swimDurationRange = ValidateRange(
            swimDurationRange, 0.1f);

        restDurationRange = ValidateRange(
            restDurationRange, 0f);
    }

    private static Vector2 ValidateRange(
        Vector2 range, float minimum)
    {
        range.x = Mathf.Max(minimum, range.x);
        range.y = Mathf.Max(range.x, range.y);

        return range;
    }
}