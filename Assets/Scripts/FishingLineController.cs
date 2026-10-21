using UnityEngine;

[DefaultExecutionOrder(100)]
[DisallowMultipleComponent]
[RequireComponent(typeof(LineRenderer))]
public class FishingLineController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HookController hook;

    [Header("Shape")]
    [SerializeField, Range(4, 64)]
    private int pointCount = 24;

    [SerializeField, Min(0f)]
    private float maxSag = 0.5f;

    [SerializeField, Min(0f)]
    private float sagResponse = 8f;

    private LineRenderer line;
    private Vector3[] points;
    private float currentSag;

    private void Awake()
    {
        line = GetComponent<LineRenderer>();
        line.useWorldSpace = true;

        AllocatePoints();
    }

    private void AllocatePoints()
    {
        int count = Mathf.Clamp(pointCount, 4, 64);

        points = new Vector3[count];
        line.positionCount = count;
    }

    private void LateUpdate()
    {
        bool visible =
            hook != null &&
            hook.isActiveAndEnabled &&
            hook.RodTip != null &&
            hook.IsRigVisible;

        line.enabled = visible;

        if (!visible)
        {
            currentSag = 0f;
            return;
        }

        if (points.Length != Mathf.Clamp(pointCount, 4, 64))
            AllocatePoints();

        Vector3 start = hook.RodTip.position;
        Vector3 end = hook.transform.position;

        if (!hook.IsDeployed)
        {
            // Đoạn dây đang treo thẳng.
            currentSag = 0f;

            for (int i = 0; i < points.Length; i++)
            {
                float t = i / (float)(points.Length - 1);
                points[i] = Vector3.Lerp(start, end, t);
            }

            line.SetPositions(points);
            return;
        }

        float directDistance = Vector3.Distance(start, end);

        float slack = Mathf.Max(
            0f, hook.CurrentLineLength - directDistance);

        // Độ võng mang tính thể hiện hình ảnh.
        // Quyền giới hạn chuyển động thuộc HookController.
        float targetSag = Mathf.Min(
            maxSag, slack * 0.5f);

        if (directDistance >= hook.CurrentLineLength * 0.98f)
        {
            // Dây căng: duỗi ngay để không hiện dây dài quá mức.
            currentSag = 0f;
        }
        else
        {
            float blend = 1f - Mathf.Exp(
                -sagResponse * Time.deltaTime);

            currentSag = Mathf.Lerp(
                currentSag, targetSag, blend);
        }

        for (int i = 0; i < points.Length; i++)
        {
            float t = i / (float)(points.Length - 1);

            Vector3 point = Vector3.Lerp(start, end, t);
            point.y -= 4f * currentSag * t * (1f - t);

            points[i] = point;
        }

        line.SetPositions(points);
    }

    private void OnDisable()
    {
        if (line != null)
            line.enabled = false;
    }
}