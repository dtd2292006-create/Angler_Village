using AnglerVillage.UI;
using UnityEngine;

// The authored left-hand exit is a world trigger, independent of pause/menu UI.
[RequireComponent(typeof(BoxCollider2D))]
public sealed class LakeVillageExit : MonoBehaviour
{
    [SerializeField] private SceneFlow flow;
    [SerializeField] private HookController hook;
    private bool travelling;
    private bool retryAfterLeaving;
    private void Awake()
    {
        GetComponent<BoxCollider2D>().isTrigger = true;
        if (flow == null || hook == null)
        { Debug.LogError("LakeVillageExit: assign SceneFlow and Hook.", this); enabled = false; return; }
        flow.Failed += Failed;
    }
    private void OnTriggerEnter2D(Collider2D other) => TryTravel(other);
    private void OnTriggerStay2D(Collider2D other) => TryTravel(other);
    private void TryTravel(Collider2D other)
    {
        if (travelling || retryAfterLeaving || flow.IsLoading || Time.timeScale <= 0 ||
            GameInputBlock.IsBlocked || hook.IsDeployed || other.GetComponentInParent<PlayerMovement>() == null) return;
        travelling = true;
        flow.TravelToVillage();
    }
    private void Failed(string message)
    {
        travelling = false; retryAfterLeaving = true;
        Debug.LogError("Không thể về làng: " + message + " Rời vùng lối ra rồi đi vào lại để thử.", this);
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponentInParent<PlayerMovement>() != null) retryAfterLeaving = false;
    }
    private void OnDestroy() { if (flow != null) flow.Failed -= Failed; }
}
