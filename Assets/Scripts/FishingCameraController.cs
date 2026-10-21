using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Camera))]
public class FishingCameraController : MonoBehaviour
{
    [Header("Targets")]
    [SerializeField] private Transform playerTarget;
    [SerializeField] private Transform hookTarget;


    [Header("Areas")]
    [SerializeField] private BoxCollider2D cameraBounds;
    [SerializeField] private Collider2D waterArea;

    [Header("Zoom")]
    [FormerlySerializedAs("viewSize")]
    [SerializeField, Min(0.1f)]
    private float landViewSize = 3f;

    [SerializeField, Min(0.1f)]
    private float underwaterViewSize = 1.8f;

    [SerializeField, Min(0f)]
    private float zoomSmoothTime = 0.35f;

    [Header("Framing")]
    [SerializeField]
    private Vector2 playerOffset = new Vector2(0f, 0.8f);

    [SerializeField]
    private Vector2 hookOffset = Vector2.zero;

    [Header("Movement")]
    [SerializeField, Min(0f)]
    private float smoothTime = 0.25f;

    [Header("Water Transition")]
    [Tooltip("Moi phai o trong nuoc lien tuc bao lau de doi camera.")]
    [SerializeField, Min(0f)]
    private float enterWaterDelay = 0.08f;

    [Tooltip("Moi phai ra khoi nuoc lien tuc bao lau de ve camera bo.")]
    [SerializeField, Min(0f)]
    private float leaveWaterDelay = 0.2f;

    private Camera sceneCamera;
    private HookController hookController;

    private Vector2 followVelocity;
    private float zoomVelocity;
    private float transitionTimer;

    private bool followingHook;
    private bool initialized;
    private bool warnedAboutSmallBounds;

    private void Awake()
    {
        sceneCamera = GetComponent<Camera>();

        if (playerTarget == null ||
            cameraBounds == null ||
            waterArea == null)
        {
            Debug.LogError(
                "FishingCameraController: Gan Player Target, " +
                "Camera Bounds va Water Area.",
                this);

            enabled = false;
            return;
        }

        if (hookTarget != null)
            hookController = hookTarget.GetComponent<HookController>();

        sceneCamera.orthographic = true;
    }

    private void OnEnable()
    {
        initialized = false;
        followingHook = false;

        followVelocity = Vector2.zero;
        zoomVelocity = 0f;
        transitionTimer = 0f;
    }

    private void LateUpdate()
    {
        if (sceneCamera == null ||
            playerTarget == null ||
            cameraBounds == null ||
            !cameraBounds.enabled ||
            !cameraBounds.gameObject.activeInHierarchy)
        {
            return;
        }

        if (initialized && Time.timeScale == 0f)
            return;

        Bounds limits = cameraBounds.bounds;

        float aspect = Mathf.Max(0.01f, sceneCamera.aspect);

        float maximumSize = Mathf.Min(
            limits.extents.y,
            limits.extents.x / aspect);

        if (maximumSize <= 0f)
            return;

        UpdateFollowMode();

        float requestedSize = followingHook
            ? underwaterViewSize
            : landViewSize;

        requestedSize = Mathf.Max(0.1f, requestedSize);

        float targetSize = Mathf.Min(
            requestedSize, maximumSize);

        if (!initialized || zoomSmoothTime <= 0f)
        {
            sceneCamera.orthographicSize = targetSize;
            zoomVelocity = 0f;
        }
        else
        {
            float nextSize = Mathf.SmoothDamp(
                sceneCamera.orthographicSize,
                targetSize,
                ref zoomVelocity,
                zoomSmoothTime,
                Mathf.Infinity,
                Time.deltaTime);

            // Bảo đảm khung nhìn luôn vừa CameraBound,
            // kể cả khi bounds hoặc tỷ lệ màn hình thay đổi.
            sceneCamera.orthographicSize = Mathf.Min(
                nextSize, maximumSize);

            if (nextSize > maximumSize)
                zoomVelocity = 0f;
        }

        if (requestedSize > maximumSize &&
            !warnedAboutSmallBounds)
        {
            Debug.LogWarning(
                "CameraBound nho hon khung nhin yeu cau. " +
                "Camera da giam Size de khong lo ra ngoai.",
                this);

            warnedAboutSmallBounds = true;
        }

        Transform target = followingHook
            ? hookTarget
            : playerTarget;

        Vector2 offset = followingHook
            ? hookOffset
            : playerOffset;

        Vector2 desiredPosition =
            (Vector2)target.position + offset;

        desiredPosition = ClampToBounds(
            desiredPosition, limits);

        Vector2 nextPosition;

        if (!initialized || smoothTime <= 0f)
        {
            nextPosition = desiredPosition;
            followVelocity = Vector2.zero;
        }
        else
        {
            nextPosition = Vector2.SmoothDamp(
                (Vector2)transform.position,
                desiredPosition,
                ref followVelocity,
                smoothTime,
                Mathf.Infinity,
                Time.deltaTime);
        }

        Vector2 clampedPosition = ClampToBounds(
            nextPosition, limits);

        if (!Mathf.Approximately(
                nextPosition.x, clampedPosition.x))
        {
            followVelocity.x = 0f;
        }

        if (!Mathf.Approximately(
                nextPosition.y, clampedPosition.y))
        {
            followVelocity.y = 0f;
        }

        transform.position = new Vector3(
            clampedPosition.x,
            clampedPosition.y,
            transform.position.z);

        initialized = true;
    }

    private void UpdateFollowMode()
    {
        // Mồi đã thu về hoặc bị ẩn:
        // chuyển về chế độ trên bờ, không cần chờ.
        if (!IsHookAvailable())
        {
            followingHook = false;
            transitionTimer = 0f;
            return;
        }

        bool inWater =
            waterArea != null &&
            waterArea.enabled &&
            waterArea.gameObject.activeInHierarchy &&
            waterArea.OverlapPoint(hookTarget.position);

        if (inWater == followingHook)
        {
            transitionTimer = 0f;
            return;
        }

        transitionTimer += Time.deltaTime;

        float requiredDelay = inWater
            ? enterWaterDelay
            : leaveWaterDelay;

        if (transitionTimer >= requiredDelay)
        {
            followingHook = inWater;
            transitionTimer = 0f;
        }
    }

    private bool IsHookAvailable()
    {
        return hookTarget != null &&
               hookTarget.gameObject.activeInHierarchy &&
               hookController != null &&
               hookController.IsReady &&
               hookController.IsDeployed;
    }

    private Vector2 ClampToBounds(
        Vector2 position, Bounds limits)
    {
        float halfHeight = sceneCamera.orthographicSize;

        float halfWidth =
            halfHeight * Mathf.Max(0.01f, sceneCamera.aspect);

        float minX = limits.min.x + halfWidth;
        float maxX = limits.max.x - halfWidth;

        float minY = limits.min.y + halfHeight;
        float maxY = limits.max.y - halfHeight;

        position.x = minX >= maxX
            ? limits.center.x
            : Mathf.Clamp(position.x, minX, maxX);

        position.y = minY >= maxY
            ? limits.center.y
            : Mathf.Clamp(position.y, minY, maxY);

        return position;
    }
}