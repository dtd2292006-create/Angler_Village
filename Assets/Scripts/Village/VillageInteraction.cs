using AnglerVillage.UI;
using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public sealed class VillageInteraction : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Transform waiter;
    [SerializeField] private Transform shopkeeper;
    [SerializeField] private Transform lakeEntrance;
    [SerializeField] private VillagePanelView panels;
    [SerializeField] private SceneFlow flow;
    [SerializeField] private Text prompt;
    [SerializeField] private Button interact;
    [SerializeField, Min(0.1f)] private float talkDistance = 1.6f;
    [SerializeField, Min(0.1f)] private float entranceDistance = 0.65f;
    private int closest = -1;
    private bool enteredPortal;
    private void Awake() { interact.onClick.AddListener(Interact); flow.Failed += TravelFailed; }
    private void TravelFailed(string message) { enteredPortal = false; panels.ShowTravelError(message); }
    private void OnDestroy() { if (flow != null) flow.Failed -= TravelFailed; }
    private void Update()
    {
        if (player == null || GameInputBlock.IsBlocked || Time.timeScale <= 0 || flow.IsLoading)
        { interact.gameObject.SetActive(false); return; }
        closest = -1; float best = talkDistance;
        if (waiter != null && Mathf.Abs(player.position.x - waiter.position.x) < best)
        { closest = 0; best = Mathf.Abs(player.position.x - waiter.position.x); }
        if (shopkeeper != null && Mathf.Abs(player.position.x - shopkeeper.position.x) < best) closest = 1;
        bool atPortal = lakeEntrance != null && Mathf.Abs(player.position.x - lakeEntrance.position.x) <= entranceDistance;
        if (atPortal)
        {
            closest = 2;
            var progress = PlayerProgress.Current;
            if (progress != null && progress.Certified && progress.StarterKitClaimed && !enteredPortal)
            { enteredPortal = true; flow.TravelToLake(); }
        }
        else enteredPortal = false;
        interact.gameObject.SetActive(closest >= 0);
        prompt.text = atPortal && PlayerProgress.Current != null && PlayerProgress.Current.StarterKitClaimed ? "Đường ra hồ" : closest == 0 ? "[E] Nói chuyện · Guild" : closest == 1 ? "[E] Nói chuyện · Cửa hàng" : "Đến guild và nhận bộ dây/mồi trước khi ra hồ";
#if ENABLE_INPUT_SYSTEM
        bool pressed = Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
        bool pressed = Input.GetKeyDown(KeyCode.E);
#else
        bool pressed = false;
#endif
        if (pressed) Interact();
    }
    private void Interact()
    {
        if (GameInputBlock.IsBlocked || Time.timeScale <= 0 || flow.IsLoading) return;
        if (closest == 0) panels.OpenGuild(); else if (closest == 1) panels.OpenShop();
        else if (closest == 2) panels.ExplainLockedEntrance();
    }
}
