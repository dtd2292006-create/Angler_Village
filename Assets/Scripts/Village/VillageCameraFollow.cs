using UnityEngine;

[RequireComponent(typeof(Camera))]
public sealed class VillageCameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private BoxCollider2D cameraBounds;
    [SerializeField] private Vector2 offset = new Vector2(0, 1.4f);
    [SerializeField, Min(0)] private float smoothSeconds = 0.15f;
    private Camera view;
    private Vector3 velocity;
    private void Awake() => view = GetComponent<Camera>();
    private void Start() { if (target != null) transform.position = Limited(); }
    private Vector3 Limited()
    {
        Vector3 position = target.position + (Vector3)offset; position.z = transform.position.z;
        if (cameraBounds == null) return position;
        Bounds bounds = cameraBounds.bounds;
        float halfY = view.orthographicSize, halfX = halfY * view.aspect;
        position.x = bounds.size.x > 2 * halfX ? Mathf.Clamp(position.x, bounds.min.x + halfX, bounds.max.x - halfX) : bounds.center.x;
        position.y = bounds.size.y > 2 * halfY ? Mathf.Clamp(position.y, bounds.min.y + halfY, bounds.max.y - halfY) : bounds.center.y;
        return position;
    }
    private void LateUpdate()
    {
        if (target == null || Time.timeScale <= 0) return;
        transform.position = Vector3.SmoothDamp(transform.position, Limited(), ref velocity, smoothSeconds);
    }
}
