using System;
using UnityEngine;

[DisallowMultipleComponent]
public class HookController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform rodTip;
    [SerializeField] private SpriteRenderer baitVisual;
    [SerializeField] private BoxCollider2D hookBound;
    [SerializeField] private BoxCollider2D waterArea;

    [Header("External Load")]
    [Tooltip("Optional component implementing IHookLoadProvider, normally FishStruggleController on this Hook.")]
    [SerializeField] private MonoBehaviour loadProviderComponent;

    [Header("Cast")]
    [SerializeField, Min(0.1f)]
    private float minCastSpeed = 3f;

    [SerializeField, Min(0.1f)]
    private float maxCastSpeed = 7f;

    [SerializeField, Range(5f, 80f)]
    private float castAngle = 35f;

    [Header("Line Length")]
    [SerializeField, Min(0.1f)]
    private float minLineLength = 2f;

    [SerializeField, Min(0.1f)]
    private float maxLineLength = 7f;

    [SerializeField, Min(0.01f)]
    private float hangingDistance = 0.25f;

    [Header("Reel And Release")]
    [Tooltip("Toc do rut ngan day khi giu chuot.")]
    [SerializeField, Min(0.01f)]
    private float reelSpeed = 2.5f;

    [Tooltip("Toc do nha lai day khi tha chuot.")]
    [SerializeField, Min(0.01f)]
    private float payoutSpeed = 2f;

    [Header("Motion")]
    [SerializeField, Min(0f)]
    private float airGravity = 9.81f;

    [SerializeField, Min(0f)]
    private float waterGravity = 2f;

    [SerializeField, Min(0f)]
    private float airDrag = 0.15f;

    [SerializeField, Min(0f)]
    private float waterDrag = 1.5f;

    [Header("Bounds")]
    [SerializeField, Min(0f)]
    private float edgePadding = 0.02f;

    public bool IsDeployed { get; private set; }
    public bool IsFlying => IsDeployed && !IsUnderwater;
    public bool IsReeling { get; private set; }
    public bool IsUnderwater { get; private set; }

    public Transform RodTip => rodTip;
    public float CatchDistance => hangingDistance + 0.05f;

    // Chiều dài dây đang được sử dụng.
    public float CurrentLineLength { get; private set; }

    // Giới hạn của lượt quăng, không bị giảm khi thu dây.
    public float CastLineLimit { get; private set; }

    public bool IsReady =>
        isActiveAndEnabled &&
        rodTip != null &&
        baitVisual != null &&
        hookBound != null &&
        hookBound.enabled &&
        hookBound.gameObject.activeInHierarchy &&
        waterArea != null &&
        waterArea.enabled &&
        waterArea.gameObject.activeInHierarchy;

    private bool equipmentReady = true;
    public bool CanLaunch => IsReady && equipmentReady && !IsDeployed;
    public bool IsRigVisible => rigVisible && equipmentReady && isActiveAndEnabled;
    public bool HasBait => equipmentReady && !baitConsumed;
    public bool CanFishBite => IsReady && IsDeployed && IsUnderwater && HasBait;

    public HookLoadSample CurrentLoad => currentLoad;

    // Observers can evaluate line safety before this tick moves/retrieves the hook.
    public event Action<float> PhysicsLoadSampled;
    public event Action<float> PhysicsStepCompleted;
    // The equipment rig validates ownership without consuming bait at launch.
    public Func<bool> TryAuthorizeCast { private get; set; }
    public event Action LineBroken;
    public event Action Retrieved;
    public event Action ReturnedToRest;

    private IHookLoadProvider loadProvider;
    private HookLoadSample currentLoad;

    private bool rigVisible;
    private bool baitConsumed;
    private Vector2 velocity;
    private bool reelHeld;
    private readonly System.Collections.Generic.HashSet<UnityEngine.Object> reelBlockOwners =
        new System.Collections.Generic.HashSet<UnityEngine.Object>();
    private float pendingBonusReel;
    private bool bonusReelThisTick;

    private void Awake()
    {
        if (loadProviderComponent == null)
        {
            foreach (MonoBehaviour component in GetComponents<MonoBehaviour>())
            {
                if (component is IHookLoadProvider)
                {
                    loadProviderComponent = component;
                    break;
                }
            }
        }
        loadProvider = loadProviderComponent as IHookLoadProvider;
        if (loadProviderComponent != null && loadProvider == null)
            Debug.LogError("HookController: External Load must implement IHookLoadProvider.", this);

        if (!IsReady)
        {
            Debug.LogError(
                "HookController: Chua gan du References.",
                this);

            enabled = false;
            return;
        }

        ReturnToRest();
    }

    public void SetEquipmentReady(bool ready)
    {
        if (IsDeployed) return;
        equipmentReady = ready;
        UpdateBaitVisibility();
    }

    public void SetVisible(bool visible)
    {
        rigVisible = visible;
        UpdateBaitVisibility();
    }

    public void SetBaitConsumed(bool consumed)
    {
        baitConsumed = consumed;
        UpdateBaitVisibility();
    }

    private void UpdateBaitVisibility()
    {
        if (baitVisual != null)
            baitVisual.enabled = IsRigVisible && HasBait;
    }

    public void SetReelHeld(bool held)
    {
        reelHeld = held;
    }

    public void SetReelBlocked(UnityEngine.Object owner, bool blocked)
    {
        if (owner == null) return;
        if (blocked)
        {
            reelBlockOwners.Add(owner);
            IsReeling = false;
        }
        else reelBlockOwners.Remove(owner);
    }

    private static bool IsInactiveBlockOwner(UnityEngine.Object owner)
    {
        if (owner == null) return true;
        Behaviour behaviour = owner as Behaviour;
        return behaviour != null && !behaviour.isActiveAndEnabled;
    }

    public void QueueBonusReel(float distance)
    {
        if (IsDeployed) pendingBonusReel += Mathf.Max(0f, distance);
    }

    public bool ApplyRodDefinition(FishingEquipmentDefinition definition)
    {
        if (definition == null || definition.Kind != FishingEquipmentKind.Rod || IsDeployed) return false;
        minCastSpeed = definition.MinCastSpeed;
        maxCastSpeed = definition.MaxCastSpeed;
        minLineLength = definition.MinLineLength;
        maxLineLength = definition.MaxLineLength;
        return true;
    }

    public bool Launch(float power01, bool faceRight)
    {
        if (!IsReady || !equipmentReady || IsDeployed)
            return false;

        Vector2 origin = rodTip.position;
        Bounds bounds = hookBound.bounds;

        if (origin.x < bounds.min.x + edgePadding ||
            origin.x > bounds.max.x - edgePadding ||
            origin.y < bounds.min.y + edgePadding ||
            origin.y > bounds.max.y - edgePadding)
        {
            Debug.LogWarning(
                "RodTip phai nam ben trong HookBound.",
                this);

            return false;
        }

        if (TryAuthorizeCast != null && !TryAuthorizeCast()) return false;

        power01 = Mathf.Clamp01(power01);

        float speed = Mathf.Lerp(
            minCastSpeed,
            Mathf.Max(minCastSpeed, maxCastSpeed),
            power01);

        float shortestLength = Mathf.Max(
            minLineLength, hangingDistance + 0.1f);

        CastLineLimit = Mathf.Lerp(
            shortestLength,
            Mathf.Max(shortestLength, maxLineLength),
            power01);

        CurrentLineLength = CastLineLimit;

        float radians = castAngle * Mathf.Deg2Rad;
        float direction = faceRight ? 1f : -1f;

        velocity = new Vector2(
            Mathf.Cos(radians) * speed * direction,
            Mathf.Sin(radians) * speed);

        transform.position = rodTip.position;

        reelHeld = false;
        IsReeling = false;
        IsDeployed = true;
        IsUnderwater = waterArea.OverlapPoint(origin);

        baitConsumed = false;
        SetVisible(true);
        return true;
    }

    private void FixedUpdate()
    {
        if (!IsDeployed || Time.timeScale == 0f)
            return;

        if (!IsReady)
        {
            ReturnToRest();
            return;
        }

        const int substeps = 4;
        float step = Time.fixedDeltaTime / substeps;

        reelBlockOwners.RemoveWhere(IsInactiveBlockOwner);
        IsReeling = reelHeld && reelBlockOwners.Count == 0;
        currentLoad = loadProviderComponent != null && loadProviderComponent.isActiveAndEnabled && loadProvider != null
            ? loadProvider.SampleLoad(Time.fixedDeltaTime)
            : default(HookLoadSample);

        PhysicsLoadSampled?.Invoke(Time.fixedDeltaTime);
        if (!IsDeployed) return;

        bonusReelThisTick = pendingBonusReel > 0f;
        if (bonusReelThisTick)
        {
            CurrentLineLength = Mathf.Max(hangingDistance, CurrentLineLength - pendingBonusReel);
            pendingBonusReel = 0f;
        }

        for (int i = 0; i < substeps; i++)
        {
            SimulateStep(step);

            if (!IsDeployed)
                break;
        }
        if (IsDeployed) PhysicsStepCompleted?.Invoke(Time.fixedDeltaTime);
    }

    private void SimulateStep(float dt)
    {
        Vector2 anchor = rodTip.position;
        Vector2 position = transform.position;

        IsUnderwater = waterArea.OverlapPoint(position);

        float gravity = IsUnderwater
            ? waterGravity
            : airGravity;

        float drag = IsUnderwater
            ? waterDrag
            : airDrag;

        // HookController remains the sole owner of position and velocity.
        Vector2 fishAcceleration = IsUnderwater ? currentLoad.Acceleration : Vector2.zero;
        velocity += (Vector2.down * gravity + fishAcceleration) * dt;
        velocity *= Mathf.Exp(-drag * dt);

        if (IsReeling)
        {
            // Giữ chuột: rút ngắn dây hiện tại.
            CurrentLineLength = Mathf.MoveTowards(
                CurrentLineLength,
                hangingDistance,
                reelSpeed / (1f + Mathf.Max(0f, currentLoad.ReelResistance)) * dt);
        }
        else if (!bonusReelThisTick)
        {
            // Thả chuột: nhả lại dây tới giới hạn lượt quăng.
            CurrentLineLength = Mathf.MoveTowards(
                CurrentLineLength,
                CastLineLimit,
                payoutSpeed * dt);
        }

        position += velocity * dt;

        ConstrainToBounds(ref position);

        Vector2 offset = position - anchor;
        float distance = offset.magnitude;

        if (distance > CurrentLineLength &&
            distance > 0.0001f)
        {
            Vector2 direction = offset / distance;

            position = anchor +
                       direction * CurrentLineLength;

            float outwardSpeed = Vector2.Dot(
                velocity, direction);

            if (outwardSpeed > 0f)
            {
                velocity -= direction * outwardSpeed;
            }
        }

        Vector3 result = transform.position;
        result.x = position.x;
        result.y = position.y;

        transform.position = result;

        IsUnderwater = waterArea.OverlapPoint(position);

        // Chỉ kết thúc khi thực sự đang thu và mồi đã về cần.
        if ((IsReeling || bonusReelThisTick) &&
            CurrentLineLength <= hangingDistance + 0.01f &&
            Vector2.Distance(position, anchor) <=
                CatchDistance)
        {
            ResetMotion();
            Retrieved?.Invoke();
        }
    }

    private void ConstrainToBounds(ref Vector2 position)
    {
        Bounds bounds = hookBound.bounds;

        float minX = bounds.min.x + edgePadding;
        float maxX = bounds.max.x - edgePadding;
        float minY = bounds.min.y + edgePadding;
        float maxY = bounds.max.y - edgePadding;

        if (position.x < minX)
        {
            position.x = minX;

            if (velocity.x < 0f)
                velocity.x = 0f;
        }
        else if (position.x > maxX)
        {
            position.x = maxX;

            if (velocity.x > 0f)
                velocity.x = 0f;
        }

        if (position.y < minY)
        {
            position.y = minY;

            if (velocity.y < 0f)
                velocity.y = 0f;
        }
        else if (position.y > maxY)
        {
            position.y = maxY;

            if (velocity.y > 0f)
                velocity.y = 0f;
        }
    }

    private void LateUpdate()
    {
        if (!IsDeployed && rodTip != null)
        {
            transform.position =
                rodTip.position +
                Vector3.down * hangingDistance;
        }
    }

    public bool BreakLine()
    {
        if (!IsDeployed) return false;
        // Cancellation releases the fish; this must never raise Retrieved.
        ReturnToRest();
        LineBroken?.Invoke();
        return true;
    }

    public void ReturnToRest()
    {
        ResetMotion();
        ReturnedToRest?.Invoke();
    }

    private void ResetMotion()
    {
        baitConsumed = false;
        IsDeployed = false;
        IsReeling = false;
        IsUnderwater = false;

        reelHeld = false;
        reelBlockOwners.Clear();
        pendingBonusReel = 0f;
        bonusReelThisTick = false;
        velocity = Vector2.zero;
        currentLoad = default(HookLoadSample);

        CurrentLineLength = hangingDistance;
        CastLineLimit = hangingDistance;
        UpdateBaitVisibility();

        if (rodTip != null)
        {
            transform.position =
                rodTip.position +
                Vector3.down * hangingDistance;
        }
    }

    private void OnDisable()
    {
        ReturnToRest();
        SetVisible(false);
    }
}

