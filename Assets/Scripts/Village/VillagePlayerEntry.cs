using System.Collections;
using UnityEngine;

// Authored spawn markers choose the location; the existing capsule decides the foot contact.
[DefaultExecutionOrder(-100)]
public sealed class VillagePlayerEntry : MonoBehaviour
{
    [SerializeField] private Rigidbody2D player;
    [SerializeField] private Transform houseSpawn;
    [SerializeField] private Transform lakeReturnSpawn;
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private bool alignFeetToGround = true;
    private readonly RaycastHit2D[] floorHits = new RaycastHit2D[16];
    private IEnumerator Start()
    {
        // A temporarily unavailable profile must not select House for a returning player.
        while (PlayerProgress.Current == null || !PlayerProgress.Current.IsReady) yield return null;
        var progress = PlayerProgress.Current;
        string arrival = SceneFlow.ConsumeArrival(gameObject.scene.name, progress.VillageArrival);
        bool fromLake = arrival == "Lake";
        var spawn = fromLake ? lakeReturnSpawn : houseSpawn;
        if (player == null || spawn == null)
        {
            Debug.LogError("VillagePlayerEntry: assign Player and the selected spawn marker.", this);
            yield break;
        }
        if (spawn != null)
        {
            player.position = spawn.position; player.linearVelocity = Vector2.zero;
            Physics2D.SyncTransforms();
            var capsule = player.GetComponent<CapsuleCollider2D>();
            if (alignFeetToGround && capsule != null)
            {
                var filter = new ContactFilter2D(); filter.NoFilter(); filter.useTriggers = false;
                int count = Physics2D.Raycast((Vector2)spawn.position + Vector2.up * 3, Vector2.down, filter, floorHits, 8);
                float highest = float.NegativeInfinity;
                for (int i = 0; i < count; i++)
                    if (floorHits[i].collider != null && !floorHits[i].collider.transform.IsChildOf(player.transform))
                        highest = Mathf.Max(highest, floorHits[i].point.y);
                if (!float.IsNegativeInfinity(highest))
                    player.position = new Vector2(player.position.x, highest - (capsule.bounds.min.y - player.transform.position.y));
            }
            Physics2D.SyncTransforms();
        }
        if (movement != null) movement.SetFacing(!fromLake);
        progress.SetDestination(gameObject.scene.name, arrival);
    }
}
