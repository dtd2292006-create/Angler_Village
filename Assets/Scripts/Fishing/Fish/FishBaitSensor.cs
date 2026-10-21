using System;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(FishActor))]
public class FishBaitSensor : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HookController hook;
    [SerializeField] private BaitVisualController baitVisual;
    [SerializeField] private Transform detectionOrigin;

    [Header("Detection")]
    [SerializeField, Min(0.01f)] private float detectionRadius = 1.5f;
    [SerializeField, Min(0f)] private float loseRangePadding = 0.25f;
    [SerializeField] private bool requirePreferredBait = true;
    [SerializeField, Min(0.02f)] private float checkInterval = 0.1f;

    [Header("Runtime Debug")]
    [SerializeField] private bool baitInRange;
    [SerializeField] private bool baitIsPreferred;
    [SerializeField] private bool hasTarget;

    public bool HasTarget => hasTarget;
    public HookController Target => hasTarget ? hook : null;
    public event Action<HookController> TargetChanged;

    private FishActor actor;
    private float checkTimer;
    private BaitDefinition detectedBait;

    private void Awake()
    {
        actor = GetComponent<FishActor>();
        if (hook == null || baitVisual == null)
        {
            Debug.LogError("FishBaitSensor: Chua gan Hook hoac Bait Visual.", this);
            enabled = false;
        }
    }

    private void OnEnable()
    {
        checkTimer = UnityEngine.Random.Range(0f, checkInterval);
    }

    private void Update()
    {
        if (Time.timeScale == 0f) return;
        // Mat moi ngay khi con khac can, khong doi het chu ky kiem tra.
        if (hasTarget && (hook == null || !hook.CanFishBite))
        {
            baitInRange = false;
            baitIsPreferred = false;
            SetTarget(false, null);
        }

        checkTimer -= Time.deltaTime;
        if (checkTimer > 0f) return;
        checkTimer = Mathf.Max(0.02f, checkInterval);
        CheckBait();
    }

    private void CheckBait()
    {
        baitInRange = false;
        baitIsPreferred = false;

        if (hook == null || !hook.CanFishBite ||
            baitVisual == null || !baitVisual.isActiveAndEnabled ||
            baitVisual.Definition == null || actor.Definition == null)
        {
            SetTarget(false, null);
            return;
        }

        BaitDefinition currentBait = baitVisual.Definition;
        baitIsPreferred = actor.Definition.PrefersBait(currentBait);

        Vector2 origin = detectionOrigin != null
            ? (Vector2)detectionOrigin.position : (Vector2)transform.position;

        float radius = detectionRadius;
        if (hasTarget && detectedBait == currentBait) radius += loseRangePadding;

        float squaredDistance = ((Vector2)hook.transform.position - origin).sqrMagnitude;
        baitInRange = squaredDistance <= radius * radius;
        SetTarget(baitInRange && (!requirePreferredBait || baitIsPreferred), currentBait);
    }

    private void SetTarget(bool accepted, BaitDefinition bait)
    {
        BaitDefinition nextBait = accepted ? bait : null;
        bool changed = hasTarget != accepted || detectedBait != nextBait;
        hasTarget = accepted;
        detectedBait = nextBait;
        if (changed) TargetChanged?.Invoke(accepted ? hook : null);
    }

    private void OnDisable()
    {
        baitInRange = false;
        baitIsPreferred = false;
        SetTarget(false, null);
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 origin = detectionOrigin != null ? detectionOrigin.position : transform.position;
        Gizmos.color = hasTarget ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(origin, detectionRadius);
        if (hasTarget && hook != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(origin, hook.transform.position);
        }
    }
}
