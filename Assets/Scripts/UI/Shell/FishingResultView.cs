using AnglerVillage.UI;
using UnityEngine;
using UnityEngine.UI;

// Fishing result presentation only. Does not pause, record fish, or resolve gameplay.
public sealed class FishingResultView : MonoBehaviour
{
    [SerializeField] private HookController hook;
    [SerializeField] private HookCatchController catches;
    [SerializeField] private GameObject card;
    [SerializeField] private CanvasGroup group;
    [SerializeField] private Image fishImage;
    [SerializeField] private Image resultIcon;
    [SerializeField] private Sprite brokenIcon;
    [SerializeField] private Sprite emptyIcon;
    [SerializeField] private Text symbol;
    [SerializeField] private Text heading;
    [SerializeField] private Text description;
    [SerializeField] private Graphic accent;
    [SerializeField, Min(1)] private float visibleSeconds = 4.5f;
    [SerializeField] private Color caughtColor = new Color32(113, 212, 135, 255);
    [SerializeField] private Color lostColor = new Color32(255, 158, 128, 255);
    [SerializeField] private Color emptyColor = new Color32(234, 211, 161, 255);
    private float remaining;
    private bool pendingRetrieval;
    private int caughtFrame = -1;
    private int brokenFrame = -1;
    private void Awake()
    {
        foreach (var text in GetComponentsInChildren<Text>(true)) InventoryUIFont.Ensure(text);
        card.SetActive(false); group.blocksRaycasts = false; group.interactable = false;
    }
    private void OnEnable()
    {
        if (hook != null) { hook.Retrieved += Retrieved; hook.LineBroken += Broken; }
        if (catches != null) catches.FishLanded += Landed;
    }
    private void Retrieved() => pendingRetrieval = true;
    private void Landed(FishCatchable fish)
    {
        caughtFrame = Time.frameCount;
        string name = fish.Definition != null ? fish.Definition.DisplayName : "Cá";
        Show("CÂU THÀNH CÔNG", name + "\n[I] xem túi cá  ·  [J] mở sổ cá", fish.CaughtSprite, "+", caughtColor);
    }
    private void Broken()
    {
        brokenFrame = Time.frameCount;
        Show("ĐỨT DÂY · CÁ ĐÃ THOÁT", "Chưa bắt được cá.\nCó thể quăng lại hoặc trở về làng chuẩn bị.", null, "!", lostColor);
    }
    private void Show(string title, string detail, Sprite sprite, string mark, Color color)
    {
        heading.text = title; heading.color = color; description.text = detail;
        fishImage.sprite = sprite; fishImage.enabled = sprite != null;
        if (resultIcon != null)
        {
            resultIcon.sprite = mark == "!" ? brokenIcon : emptyIcon;
            resultIcon.enabled = sprite == null && resultIcon.sprite != null;
        }
        symbol.text = mark; symbol.gameObject.SetActive(sprite == null && (resultIcon == null || !resultIcon.enabled)); symbol.color = color;
        accent.color = color; remaining = visibleSeconds; group.alpha = 1; card.SetActive(true);
    }
    private void LateUpdate()
    {
        // FishLanded is dispatched inside Retrieved; resolve empty casts after every subscriber ran.
        if (pendingRetrieval)
        {
            pendingRetrieval = false;
            if (caughtFrame != Time.frameCount && brokenFrame != Time.frameCount)
                Show("ĐÃ THU MỒI", "Lượt này chưa có cá.\nThử đổi vị trí hoặc chọn mồi phù hợp hơn.", null, "—", emptyColor);
        }
        bool blocked = GameInputBlock.IsBlocked || Time.timeScale <= 0;
        card.SetActive(remaining > 0 && !blocked);
        if (blocked || remaining <= 0) return;
        remaining -= Time.unscaledDeltaTime;
        group.alpha = Mathf.Clamp01(remaining / 0.3f);
    }
    private void OnDisable()
    {
        if (hook != null) { hook.Retrieved -= Retrieved; hook.LineBroken -= Broken; }
        if (catches != null) catches.FishLanded -= Landed;
        pendingRetrieval = false; remaining = 0;
    }
}
