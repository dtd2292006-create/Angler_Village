using System.Collections;
using UnityEngine;

// Scene entry points are authored markers; only the assigned floor can align the feet.
[DefaultExecutionOrder(-100)]
public sealed class VillagePlayerEntry : MonoBehaviour
{
    [SerializeField] private Rigidbody2D player;
    [SerializeField] private Transform houseSpawn;
    [SerializeField] private Transform lakeReturnSpawn;
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private bool alignFeetToGround = true;
    [SerializeField] private Collider2D ground;
    private readonly RaycastHit2D[] floorHits = new RaycastHit2D[32];

    private IEnumerator Start()
    {
        while (PlayerProgress.Current == null || !PlayerProgress.Current.IsReady) yield return null;
        var progress = PlayerProgress.Current;
        string arrival = SceneFlow.ConsumeArrival(gameObject.scene.name, progress.VillageArrival);
        bool fromLake = arrival == "Lake";
        Transform spawn = fromLake ? lakeReturnSpawn : houseSpawn;
        if (player == null || spawn == null)
        {
            Debug.LogError("VillagePlayerEntry: assign Player and the selected spawn marker.", this);
            yield break;
        }

        // Calculate once from a consistent pose, before changing either physics or Transform.
        Physics2D.SyncTransforms();
        Vector3 destination = spawn.position;
        var capsule = player.GetComponent<CapsuleCollider2D>();
        if (alignFeetToGround && ground != null && capsule != null)
        {
            float footOffset = capsule.bounds.min.y - player.transform.position.y;
            var filter = new ContactFilter2D();
            filter.NoFilter();
            filter.useTriggers = false;
            int count = Physics2D.Raycast((Vector2)destination + Vector2.up * 3,
                Vector2.down, filter, floorHits, 8);
            for (int i = 0; i < count; i++)
            {
                // Ignore the player, scenery and any collider other than the authored floor.
                if (floorHits[i].collider != ground || floorHits[i].normal.y < 0.5f) continue;
                destination.y = floorHits[i].point.y - footOffset;
                break;
            }
        }

        var interpolation = player.interpolation;
        player.interpolation = RigidbodyInterpolation2D.None;
        // Set both poses. SyncTransforms must not reapply the old house Transform to the body.
        player.transform.position = destination;
        player.position = destination;
        player.linearVelocity = Vector2.zero;
        player.angularVelocity = 0f;
        Physics2D.SyncTransforms();
        player.interpolation = interpolation;
        if (movement != null) movement.SetFacing(!fromLake);
        progress.SetDestination(gameObject.scene.name, arrival);
        Debug.Log("Village entry: arrival=" + arrival + " marker=" + spawn.name +
            " position=" + player.position, this);
    }
}
