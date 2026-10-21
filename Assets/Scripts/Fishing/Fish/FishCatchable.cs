using UnityEngine;

[DefaultExecutionOrder(50)]
[DisallowMultipleComponent]
[RequireComponent(typeof(FishActor))]
[RequireComponent(typeof(FishMovement))]
[RequireComponent(typeof(FishBaitSensor))]
[RequireComponent(typeof(FishBehaviourController))]
public class FishCatchable : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform mouthPoint;
    [SerializeField] private SpriteRenderer visualRenderer;

    [Header("Bite")]
    [SerializeField, Min(0.01f)] private float biteDistance = 0.12f;
    [SerializeField, Min(0f)] private float biteDelay = 0.25f;

    public HookController AttachedHook => attachedHook;
    public bool IsHooked { get; private set; }
    public bool IsLanded { get; private set; }
    public Sprite CaughtSprite => visualRenderer != null ? visualRenderer.sprite : null;
    public FishDefinition Definition => actor != null ? actor.Definition : null;
    public bool CanBeHooked => isActiveAndEnabled && !IsHooked && !IsLanded &&
        mouthPoint != null && visualRenderer != null && visualRenderer.sprite != null;

    private FishActor actor;
    private FishMovement movement;
    private FishBaitSensor sensor;
    private FishBehaviourController behaviour;
    private HookCatchController owner;
    private HookController attachedHook;
    private HookController candidate;
    private float biteTimer;
    private bool savedMovementEnabled;
    private bool savedSensorEnabled;
    private bool savedBehaviourEnabled;

    private void Awake()
    {
        actor = GetComponent<FishActor>();
        movement = GetComponent<FishMovement>();
        sensor = GetComponent<FishBaitSensor>();
        behaviour = GetComponent<FishBehaviourController>();

        if (mouthPoint == null || !mouthPoint.IsChildOf(transform) ||
            visualRenderer == null)
        {
            Debug.LogError("FishCatchable: Gan MouthPoint va Visual Renderer cua con ca nay.", this);
            enabled = false;
        }
    }

    private void LateUpdate()
    {
        if (Time.timeScale == 0f || IsLanded) return;

        if (IsHooked)
        {
            if (attachedHook == null || !attachedHook.IsReady || !attachedHook.IsDeployed)
            {
                if (owner != null) owner.CancelAttachment();
                else ReleaseFromHook();
                return;
            }

            AlignMouth();
            return;
        }

        HookController target = sensor.isActiveAndEnabled ? sensor.Target : null;
        if (target == null || !target.CanFishBite)
        {
            ResetCandidate();
            return;
        }

        if (candidate != target)
        {
            candidate = target;
            biteTimer = 0f;
        }

        if (Vector2.Distance(mouthPoint.position, target.transform.position) > biteDistance)
        {
            biteTimer = 0f;
            return;
        }

        HookCatchController catchController = target.GetComponent<HookCatchController>();
        if (catchController == null || !catchController.CanAcceptFish)
        {
            ResetCandidate();
            return;
        }

        biteTimer += Time.deltaTime;
        if (biteTimer >= biteDelay)
            catchController.TryAttach(this);
    }

    public bool AttachTo(HookCatchController newOwner, HookController hook)
    {
        if (!CanBeHooked || newOwner == null || hook == null) return false;

        owner = newOwner;
        attachedHook = hook;
        IsHooked = true;
        ResetCandidate();

        savedMovementEnabled = movement.enabled;
        savedSensorEnabled = sensor.enabled;
        savedBehaviourEnabled = behaviour.enabled;

        // Tat ra quyet dinh truoc, sau do tat bo di chuyen va cam bien.
        behaviour.enabled = false;
        movement.enabled = false;
        sensor.enabled = false;

        AlignMouth();
        return true;
    }

    private void AlignMouth()
    {
        Vector3 offset = attachedHook.transform.position - mouthPoint.position;
        transform.position += offset;
    }

    public void ReleaseFromHook()
    {
        if (!IsHooked) return;
        IsHooked = false;
        owner = null;
        attachedHook = null;
        ResetCandidate();

        movement.enabled = savedMovementEnabled;
        sensor.enabled = savedSensorEnabled;
        behaviour.enabled = savedBehaviourEnabled;
    }

    public void MarkLanded()
    {
        IsHooked = false;
        IsLanded = true;
        owner = null;
        attachedHook = null;
        ResetCandidate();
        gameObject.SetActive(false);
    }

    private void ResetCandidate()
    {
        candidate = null;
        biteTimer = 0f;
    }

    private void OnDisable()
    {
        if (IsHooked)
        {
            HookCatchController oldOwner = owner;
            ReleaseFromHook();
            if (oldOwner != null) oldOwner.ForgetFish(this);
        }
        ResetCandidate();
    }
}
