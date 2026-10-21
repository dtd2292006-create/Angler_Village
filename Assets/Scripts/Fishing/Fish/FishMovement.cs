using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(FishActor))]
[RequireComponent(typeof(FishFacing))]
public class FishMovement : MonoBehaviour
{
    private enum SwimState
    {
        Swimming,
        Braking,
        Waiting
    }

    [Header("References")]
    [SerializeField] private FishSwimmingArea swimmingArea;
    [SerializeField] private SpriteRenderer visualRenderer;

    [Header("Boundary")]
    [SerializeField, Min(0.001f)]
    private float turnMargin = 0.03f;

    [Header("Approach Bait")]
    [SerializeField, Min(0.1f)]
    private float approachSpeedMultiplier = 1.4f;

    [Tooltip("Khoang cach tu mieng toi moi khi dung lai.")]
    [SerializeField, Min(0.01f)]
    private float mouthStopDistance = 0.08f;

    [Tooltip("Nguong ngang de tranh lat hinh lien tuc.")]
    [SerializeField, Min(0.001f)]
    private float facingDeadZone = 0.04f;

    public bool IsApproaching => pursuitTarget != null;
    public bool MouthInRange { get; private set; }

    private FishActor actor;
    private FishFacing facing;
    private FishDefinition definition;

    private Rect region;
    private SwimState state;

    private Vector2 direction;
    private Vector2 nextDirection;
    private Vector2 pursuitVelocity;

    private float currentSpeed;
    private float swimTimer;
    private float waitTimer;
    private float wavePhase;

    private bool restAfterBraking;
    private bool initialized;
    private bool warnedAboutRegion;

    private Transform pursuitTarget;
    private Transform mouthPoint;

    private void Awake()
    {
        actor = GetComponent<FishActor>();
        facing = GetComponent<FishFacing>();

        if (swimmingArea == null || visualRenderer == null)
        {
            Debug.LogError(
                "FishMovement: Chua gan Swimming Area " +
                "hoac Visual Renderer.", this);

            enabled = false;
        }
    }

    private void OnEnable()
    {
        initialized = false;
        warnedAboutRegion = false;
        pursuitVelocity = Vector2.zero;
    }

    private void OnDisable()
    {
        pursuitTarget = null;
        mouthPoint = null;
        pursuitVelocity = Vector2.zero;
        MouthInRange = false;
    }

    public void SetPursuitTarget(Transform target, Transform mouth)
    {
        if (target == null || mouth == null)
        {
            ClearPursuitTarget();
            return;
        }

        if (pursuitTarget == target && mouthPoint == mouth)
            return;

        pursuitTarget = target;
        mouthPoint = mouth;
        MouthInRange = false;

        // Giữ vận tốc ban đầu khi chuyển từ bơi sang tiếp cận.
        pursuitVelocity = initialized
            ? direction * currentSpeed
            : Vector2.zero;
    }

    public void ClearPursuitTarget()
    {
        if (pursuitTarget == null && mouthPoint == null)
            return;

        pursuitTarget = null;
        mouthPoint = null;
        pursuitVelocity = Vector2.zero;

        MouthInRange = false;
        initialized = false;
    }

    private void Update()
    {
        if (Time.timeScale == 0f)
            return;

        FishDefinition selectedDefinition = actor.Definition;

        if (!RefreshRegion(selectedDefinition))
            return;

        if (!initialized || definition != selectedDefinition)
        {
            definition = selectedDefinition;
            InitializeMovement();
        }

        SetPosition(ClampPosition(transform.position));

        float dt = Time.deltaTime;

        if (pursuitTarget != null && mouthPoint != null)
        {
            UpdateApproach(dt);
            return;
        }

        MouthInRange = false;
        UpdateFreeSwimming(dt);
    }

    private void UpdateApproach(float dt)
    {
        Vector2 targetPosition = pursuitTarget.position;

        Vector2 mouthToBait =
            targetPosition - (Vector2)mouthPoint.position;

        // Không đổi hướng khi miệng đã ở sát mồi.
        if (mouthToBait.magnitude > mouthStopDistance &&
            Mathf.Abs(mouthToBait.x) > facingDeadZone)
        {
            facing.SetFacing(
                mouthToBait.x > 0f,
                definition.SpriteFacesRight);
        }

        // Sau khi lật, vị trí miệng và phần hình nhô ra đã thay đổi.
        if (!RefreshRegion(definition))
            return;

        Vector2 rootPosition = transform.position;

        Vector2 mouthOffset =
            (Vector2)mouthPoint.position - rootPosition;

        Vector2 actualMouthDelta =
            targetPosition - (Vector2)mouthPoint.position;

        float mouthDistance = actualMouthDelta.magnitude;

        // Đích của gốc cá sao cho miệng cách mồi một đoạn nhỏ.
        Vector2 desiredMouthPosition = targetPosition;

        if (mouthDistance > 0.0001f)
        {
            desiredMouthPosition -=
                actualMouthDelta / mouthDistance * mouthStopDistance;
        }

        Vector2 desiredRootPosition = ClampPosition(
            desiredMouthPosition - mouthOffset);

        Vector2 toGoal = desiredRootPosition - rootPosition;
        float distance = toGoal.magnitude;

        float maximumSpeed =
            definition.SwimSpeed * approachSpeedMultiplier;

        // Càng gần đích càng giảm tốc.
        float stoppingSpeed = Mathf.Sqrt(
            2f * definition.Deceleration * distance);

        float desiredSpeed = Mathf.Min(
            maximumSpeed, stoppingSpeed);

        Vector2 desiredVelocity = distance > 0.0001f
            ? toGoal / distance * desiredSpeed
            : Vector2.zero;

        float rate = desiredSpeed < pursuitVelocity.magnitude
            ? definition.Deceleration
            : definition.Acceleration;

        pursuitVelocity = Vector2.MoveTowards(
            pursuitVelocity,
            desiredVelocity,
            rate * dt);

        Vector2 step = pursuitVelocity * dt;

        if (distance <= 0.002f)
        {
            pursuitVelocity = Vector2.zero;
            SetPosition(desiredRootPosition);
        }
        else if (step.magnitude >= distance &&
                 Vector2.Dot(step, toGoal) > 0f)
        {
            pursuitVelocity = Vector2.zero;
            SetPosition(desiredRootPosition);
        }
        else
        {
            SetPosition(ClampPosition(rootPosition + step));
        }

        MouthInRange = Vector2.Distance(
            mouthPoint.position,
            pursuitTarget.position) <= mouthStopDistance + 0.02f;
    }

    private void UpdateFreeSwimming(float dt)
    {
        switch (state)
        {
            case SwimState.Swimming:
                UpdateSwimming(dt);
                break;

            case SwimState.Braking:
                currentSpeed = Mathf.MoveTowards(
                    currentSpeed,
                    0f,
                    definition.Deceleration * dt);

                if (currentSpeed <= 0.0001f)
                {
                    currentSpeed = 0f;
                    state = SwimState.Waiting;
                    waitTimer = definition.TurnPause;

                    if (restAfterBraking)
                    {
                        waitTimer += RandomInRange(
                            definition.RestDurationRange);
                    }
                }
                break;

            case SwimState.Waiting:
                waitTimer -= dt;

                if (waitTimer <= 0f)
                    ResumeSwimming();

                break;
        }

        MoveFish(dt);
    }

    private void InitializeMovement()
    {
        initialized = true;
        currentSpeed = 0f;

        direction = RandomDirection();
        wavePhase = Random.Range(0f, Mathf.PI * 2f);

        state = SwimState.Swimming;

        swimTimer = RandomInRange(
            definition.SwimDurationRange) *
            Random.Range(0.3f, 1f);

        ApplyFacing();
    }

    private void UpdateSwimming(float dt)
    {
        currentSpeed = Mathf.MoveTowards(
            currentSpeed,
            definition.SwimSpeed,
            definition.Acceleration * dt);

        float stoppingDistance =
            currentSpeed * currentSpeed /
            (2f * Mathf.Max(0.01f, definition.Deceleration));

        float lookAhead =
            stoppingDistance +
            currentSpeed * dt +
            turnMargin;

        Vector2 position = transform.position;
        Vector2 predicted = position + direction * lookAhead;

        bool nearHorizontalEdge =
            (direction.x < 0f && predicted.x <= region.xMin) ||
            (direction.x > 0f && predicted.x >= region.xMax);

        bool nearVerticalEdge =
            definition.SwimStyle != FishSwimStyle.Wave &&
            ((direction.y < 0f && predicted.y <= region.yMin) ||
             (direction.y > 0f && predicted.y >= region.yMax));

        if (nearHorizontalEdge || nearVerticalEdge)
        {
            Vector2 reflected = direction;

            if (nearHorizontalEdge)
                reflected.x *= -1f;

            if (nearVerticalEdge)
                reflected.y *= -1f;

            RequestTurn(reflected.normalized, false);
            return;
        }

        swimTimer -= dt;

        if (swimTimer <= 0f)
        {
            Vector2 newDirection = RandomDirection();

            newDirection.x =
                -Mathf.Sign(direction.x) *
                Mathf.Abs(newDirection.x);

            RequestTurn(
                newDirection.normalized,
                definition.SwimStyle == FishSwimStyle.SwimAndRest);
        }
    }

    private void RequestTurn(Vector2 requestedDirection, bool shouldRest)
    {
        nextDirection = requestedDirection;
        restAfterBraking = shouldRest;
        state = SwimState.Braking;
    }

    private void ResumeSwimming()
    {
        direction = nextDirection;

        Vector2 position = transform.position;

        if (position.x <= region.xMin + turnMargin)
            direction.x = Mathf.Abs(direction.x);
        else if (position.x >= region.xMax - turnMargin)
            direction.x = -Mathf.Abs(direction.x);

        if (position.y <= region.yMin + turnMargin)
            direction.y = Mathf.Abs(direction.y);
        else if (position.y >= region.yMax - turnMargin)
            direction.y = -Mathf.Abs(direction.y);

        direction.Normalize();

        state = SwimState.Swimming;
        currentSpeed = 0f;

        swimTimer = RandomInRange(definition.SwimDurationRange);

        ApplyFacing();
    }

    private void MoveFish(float dt)
    {
        if (currentSpeed <= 0f)
            return;

        Vector2 position = transform.position;
        Vector2 nextPosition =
            position + direction * currentSpeed * dt;

        if (definition.SwimStyle == FishSwimStyle.Wave)
        {
            float speedRatio = Mathf.Clamp01(
                currentSpeed / Mathf.Max(0.01f, definition.SwimSpeed));

            float previousPhase = wavePhase;

            wavePhase += Mathf.PI * 2f *
                definition.WaveFrequency * speedRatio * dt;

            float amplitude = Mathf.Min(
                definition.WaveAmplitude, region.height * 0.5f);

            nextPosition.y += amplitude *
                (Mathf.Sin(wavePhase) - Mathf.Sin(previousPhase));
        }

        if (RefreshRegion(definition))
            SetPosition(ClampPosition(nextPosition));
    }

    private Vector2 RandomDirection()
    {
        float x = Random.value < 0.5f ? -1f : 1f;

        float y = definition.SwimStyle == FishSwimStyle.Wave
            ? 0f
            : Random.Range(-0.25f, 0.25f);

        return new Vector2(x, y).normalized;
    }

    private void ApplyFacing()
    {
        facing.SetFacing(
            direction.x > 0f,
            definition.SpriteFacesRight);

        if (RefreshRegion(definition))
            SetPosition(ClampPosition(transform.position));
    }

    private bool RefreshRegion(FishDefinition fishDefinition)
    {
        bool valid = swimmingArea.TryGetRegion(
            fishDefinition,
            visualRenderer,
            transform,
            out region);

        if (!valid)
        {
            if (!warnedAboutRegion)
            {
                Debug.LogWarning(
                    "FishMovement: Khong co vung boi hop le.",
                    this);

                warnedAboutRegion = true;
            }

            return false;
        }

        warnedAboutRegion = false;
        return true;
    }

    private Vector2 ClampPosition(Vector2 position)
    {
        position.x = Mathf.Clamp(position.x, region.xMin, region.xMax);
        position.y = Mathf.Clamp(position.y, region.yMin, region.yMax);

        return position;
    }

    private void SetPosition(Vector2 position)
    {
        Vector3 result = transform.position;
        result.x = position.x;
        result.y = position.y;

        transform.position = result;
    }

    private static float RandomInRange(Vector2 range)
    {
        return Random.Range(range.x, range.y);
    }
}