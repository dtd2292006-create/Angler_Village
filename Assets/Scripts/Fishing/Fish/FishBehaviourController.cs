using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(FishMovement))]
[RequireComponent(typeof(FishBaitSensor))]
public class FishBehaviourController : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private Transform mouthPoint;

    [Header("Runtime Debug")]
    [SerializeField]
    private bool approachingBait;

    [SerializeField]
    private bool mouthInRange;

    private FishMovement movement;
    private FishBaitSensor sensor;

    private void Awake()
    {
        movement = GetComponent<FishMovement>();
        sensor = GetComponent<FishBaitSensor>();

        if (mouthPoint == null || !mouthPoint.IsChildOf(transform))
        {
            Debug.LogError(
                "FishBehaviourController: Gan MouthPoint " +
                "nam trong nhanh Visual cua con ca nay.",
                this);

            enabled = false;
        }
    }

    private void Update()
    {
        if (Time.timeScale == 0f)
            return;

        HookController target =
            sensor.isActiveAndEnabled ? sensor.Target : null;

        bool validTarget =
            target != null &&
            target.IsReady &&
            target.IsDeployed &&
            target.IsUnderwater;

        if (validTarget)
        {
            movement.SetPursuitTarget(
                target.transform,
                mouthPoint);
        }
        else
        {
            movement.ClearPursuitTarget();
        }

        approachingBait = validTarget;
        mouthInRange = movement.MouthInRange;
    }

    private void OnDisable()
    {
        if (movement != null)
            movement.ClearPursuitTarget();

        approachingBait = false;
        mouthInRange = false;
    }
}