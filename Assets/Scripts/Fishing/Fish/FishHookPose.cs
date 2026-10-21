using UnityEngine;

// FishCatchable aligns the mouth at 50; FishingLineController draws the line at 100.
[DefaultExecutionOrder(110)]
[DisallowMultipleComponent]
[RequireComponent(typeof(FishCatchable))]
public class FishHookPose : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform visualRoot;
    [SerializeField] private Transform mouthPoint;
    [SerializeField] private SpriteRenderer visualRenderer;
    [Tooltip("Optional. Use the fishing rig's LineRenderer to follow its direction near the hook.")]
    [SerializeField] private LineRenderer fishingLine;

    [Header("Sprite")]
    [Tooltip("Head direction in the original sprite: right = (1,0), left = (-1,0).")]
    [SerializeField] private Vector2 localHeadDirection = Vector2.right;

    [Header("Rotation")]
    [SerializeField, Min(1f)] private float rotationSpeed = 360f;
    [SerializeField, Min(0.01f)] private float rotationSmoothTime = 0.18f;

    [Header("Visual Force Approximation")]
    [SerializeField, Min(0.01f)] private float gravityInfluence = 1f;
    [SerializeField, Min(0f)] private float tensionInfluence = 3f;
    [Tooltip("Slack as a fraction of line length at which the tension contribution becomes zero.")]
    [SerializeField, Range(0.01f, 0.5f)] private float slackFraction = 0.08f;
    [SerializeField, Min(0f)] private float dragInfluence = 0.3f;
    [SerializeField, Min(0f)] private float maxDragContribution = 0.75f;
    [SerializeField, Min(0.01f)] private float velocityResponse = 8f;

    private FishCatchable catchable;
    private HookController trackedHook;
    private Quaternion originalRotation;
    private Vector3 previousHookPosition;
    private Vector2 smoothedVelocity;
    private float angularVelocity;
    private bool poseSaved;
    private bool liftingStarted;

    private void Awake()
    {
        catchable = GetComponent<FishCatchable>();
        if (visualRoot == null || mouthPoint == null || visualRenderer == null ||
            visualRoot == transform || !visualRoot.IsChildOf(transform) ||
            !mouthPoint.IsChildOf(visualRoot) ||
            !visualRenderer.transform.IsChildOf(visualRoot) ||
            localHeadDirection.sqrMagnitude < 0.0001f)
        {
            Debug.LogError("FishHookPose: Assign a child Visual containing MouthPoint and SpriteRenderer, and a nonzero head direction.", this);
            enabled = false;
        }
    }

    private void LateUpdate()
    {
        if (!catchable.IsHooked)
        {
            RestorePose();
            return;
        }

        HookController hook = catchable.AttachedHook;
        if (hook == null) return;

        if (!poseSaved)
        {
            originalRotation = visualRoot.localRotation;
            poseSaved = true;
        }

        if (trackedHook != hook)
        {
            trackedHook = hook;
            previousHookPosition = hook.transform.position;
            smoothedVelocity = Vector2.zero;
            angularVelocity = 0f;
        }

        Vector3 position = hook.transform.position;
        float dt = Time.deltaTime;
        if (dt <= 0f)
        {
            previousHookPosition = position;
            return;
        }

        Vector2 velocity = (Vector2)(position - previousHookPosition) / dt;
        previousHookPosition = position;
        smoothedVelocity = Vector2.Lerp(smoothedVelocity, velocity,
            1f - Mathf.Exp(-velocityResponse * dt));

        // Preserve the swimming pose until the player first reels this fish.
        if (hook.IsReeling) liftingStarted = true;
        if (!liftingStarted) return;

        float tension = EstimateTension(hook);
        Vector2 pullDirection = GetPullDirection(hook);

        // These are visual weights, not forces applied to the hook simulation.
        // Gravity makes the tail hang down; water drag makes it trail behind motion.
        Vector2 drag = Vector2.ClampMagnitude(smoothedVelocity * dragInfluence,
            maxDragContribution);
        Vector2 desiredHead = Vector2.up * gravityInfluence +
            pullDirection * tensionInfluence * tension + drag;
        if (desiredHead.sqrMagnitude < 0.0001f) return;

        Vector3 localHead = new Vector3(localHeadDirection.x, localHeadDirection.y, 0f);
        if (visualRenderer.flipX) localHead.x = -localHead.x;
        if (visualRenderer.flipY) localHead.y = -localHead.y;

        Vector2 worldHead = visualRoot.TransformVector(localHead);
        if (worldHead.sqrMagnitude < 0.0001f) return;

        float currentAngle = visualRoot.eulerAngles.z;
        float targetAngle = currentAngle + Vector2.SignedAngle(worldHead, desiredHead);
        float nextAngle = Mathf.SmoothDampAngle(currentAngle, targetAngle,
            ref angularVelocity, rotationSmoothTime, rotationSpeed, dt);
        float step = Mathf.DeltaAngle(currentAngle, nextAngle);
        visualRoot.rotation = Quaternion.AngleAxis(step, Vector3.forward) * visualRoot.rotation;

        // Rotate around the mouth by compensating the root translation.
        transform.position += position - mouthPoint.position;
    }

    private float EstimateTension(HookController hook)
    {
        if (hook.RodTip == null) return 0f;
        float distance = Vector2.Distance(hook.RodTip.position, hook.transform.position);
        float slack = Mathf.Max(0f, hook.CurrentLineLength - distance);
        float slackRange = Mathf.Max(0.01f, hook.CurrentLineLength * slackFraction);
        return 1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(slack / slackRange));
    }

    private Vector2 GetPullDirection(HookController hook)
    {
        // The current line runs from rod tip (index 0) to hook (last index).
        if (fishingLine != null && fishingLine.enabled && fishingLine.positionCount >= 2)
        {
            int last = fishingLine.positionCount - 1;
            Vector3 end = GetLinePosition(last);
            for (int i = last - 1; i >= 0; i--)
            {
                Vector2 direction = GetLinePosition(i) - end;
                if (direction.sqrMagnitude > 0.000001f)
                    return direction.normalized;
            }
        }

        if (hook.RodTip == null) return Vector2.up;
        Vector2 towardRod = hook.RodTip.position - hook.transform.position;
        return towardRod.sqrMagnitude > 0.000001f ? towardRod.normalized : Vector2.up;
    }

    private Vector3 GetLinePosition(int index)
    {
        Vector3 point = fishingLine.GetPosition(index);
        return fishingLine.useWorldSpace ? point : fishingLine.transform.TransformPoint(point);
    }

    private void RestorePose()
    {
        if (poseSaved && visualRoot != null)
            visualRoot.localRotation = originalRotation;
        poseSaved = false;
        liftingStarted = false;
        trackedHook = null;
        smoothedVelocity = Vector2.zero;
        angularVelocity = 0f;
    }

    private void OnDisable()
    {
        RestorePose();
    }
}
