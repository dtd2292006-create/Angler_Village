using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInputReader))]
[RequireComponent(typeof(PlayerMovementBounds))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Transform visual;
    [SerializeField, Min(0f)] private float moveSpeed = 3f;

    public bool IsMovementLocked { get; private set; }
    public bool IsWalking { get; private set; }
    public bool FacingRight { get; private set; } = true;

    private Rigidbody2D body;
    private PlayerInputReader input;
    private PlayerMovementBounds movementBounds;
    private Vector3 initialVisualScale;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        input = GetComponent<PlayerInputReader>();
        movementBounds = GetComponent<PlayerMovementBounds>();

        if (visual == null)
        {
            Debug.LogError(
                "PlayerMovement: Chua gan Visual.", this);

            enabled = false;
            return;
        }

        initialVisualScale = visual.localScale;
        FacingRight = initialVisualScale.x >= 0f;
    }

    private void Start()
    {
        if (!movementBounds.IsConfigured)
        {
            Debug.LogError(
                "PlayerMovement: Gan Allowed Area va Player Collider " +
                "trong PlayerMovementBounds.", this);

            enabled = false;
        }
    }

    private void FixedUpdate()
    {
        if (!movementBounds.IsConfigured)
        {
            StopHorizontal();
            return;
        }

        float axis = IsMovementLocked ? 0f : input.MoveAxis;

        if (axis != 0f)
            SetFacing(axis > 0f);

        float currentX = body.position.x;

        float desiredX =
            currentX + axis * moveSpeed * Time.fixedDeltaTime;

        float limitedX =
            movementBounds.ClampRootX(desiredX, currentX);

        float horizontalVelocity =
            (limitedX - currentX) / Time.fixedDeltaTime;

        body.linearVelocity = new Vector2(
            horizontalVelocity,
            body.linearVelocity.y
        );

        IsWalking =
            !IsMovementLocked &&
            Mathf.Abs(horizontalVelocity) > 0.001f;
    }

    public void SetMovementLocked(bool locked)
    {
        IsMovementLocked = locked;

        if (locked)
            StopHorizontal();
    }

    public void SetFacing(bool faceRight)
    {
        FacingRight = faceRight;

        Vector3 scale = initialVisualScale;
        scale.x = Mathf.Abs(initialVisualScale.x) *
                  (faceRight ? 1f : -1f);

        visual.localScale = scale;
    }

    public bool TryPlaceAtHorizontalPosition(float worldX)
    {
        if (!isActiveAndEnabled || !movementBounds.IsConfigured)
            return false;

        float limitedX =
            movementBounds.ClampRootX(worldX, body.position.x);

        if (Mathf.Abs(limitedX - worldX) > 0.001f)
        {
            Debug.LogWarning(
                "Diem dung nam ngoai PlayerBounds hoac qua sat mep.",
                this);

            return false;
        }

        StopHorizontal();

        // Giu nguyen do cao; mat cau van do collider nen quyet dinh.
        body.position = new Vector2(worldX, body.position.y);

        return true;
    }

    private void StopHorizontal()
    {
        IsWalking = false;

        if (body != null)
        {
            body.linearVelocity =
                new Vector2(0f, body.linearVelocity.y);
        }
    }

    private void OnDisable()
    {
        StopHorizontal();
    }
}