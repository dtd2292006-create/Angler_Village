using System.Collections.Generic;
using UnityEngine;

// Scene population only. Caught fish in the player's save are independent instances.
public sealed class LakeFishRespawn : MonoBehaviour
{
    [SerializeField] private FishCatchable[] startingFish = new FishCatchable[0];
    [SerializeField] private Vector2 respawnSeconds = new Vector2(8, 15);
    private sealed class Slot
    {
        public FishCatchable live;
        public GameObject template;
        public Transform parent;
        public Vector3 position;
        public Quaternion rotation;
        public float remaining = -1;
    }
    private readonly List<Slot> slots = new List<Slot>();
    private void Start()
    {
        var templates = new GameObject("RuntimeFishTemplates");
        templates.transform.SetParent(transform, false);
        templates.SetActive(false);
        var seen = new HashSet<FishCatchable>();
        foreach (var fish in startingFish)
        {
            if (fish == null || !seen.Add(fish)) continue;
            var template = Instantiate(fish.gameObject, fish.transform.position, fish.transform.rotation, templates.transform);
            template.name = fish.gameObject.name;
            slots.Add(new Slot { live = fish, template = template, parent = fish.transform.parent,
                position = fish.transform.position, rotation = fish.transform.rotation });
        }
    }
    private void Update()
    {
        if (Time.timeScale <= 0) return;
        foreach (var slot in slots)
        {
            if (slot.live != null && !slot.live.IsLanded) continue;
            if (slot.remaining < 0) slot.remaining = Random.Range(Mathf.Max(1, respawnSeconds.x), Mathf.Max(respawnSeconds.x, respawnSeconds.y));
            slot.remaining -= Time.deltaTime;
            if (slot.remaining > 0) continue;
            if (slot.live != null) Destroy(slot.live.gameObject);
            var spawned = Instantiate(slot.template, slot.position, slot.rotation, slot.parent);
            spawned.name = slot.template.name;
            slot.live = spawned.GetComponent<FishCatchable>();
            slot.remaining = -1;
        }
    }
}
