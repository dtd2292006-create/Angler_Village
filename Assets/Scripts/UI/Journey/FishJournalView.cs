using System.Collections.Generic;
using AnglerVillage.UI;
using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[DefaultExecutionOrder(-600)]
public sealed class FishJournalView : MonoBehaviour
{
    [SerializeField] private GameObject window;
    [SerializeField] private Button open;
    [SerializeField] private Button close;
    [SerializeField] private Text count;
    [SerializeField] private Text detailName;
    [SerializeField] private Text detail;
    [SerializeField] private Image portrait;
    [SerializeField] private Text toast;
    [SerializeField] private Graphic toastBacking;
    [SerializeField] private RectTransform content;
    [SerializeField] private ScrollRect scroll;
    [SerializeField] private FishJournalSlotView rowPrefab;
    private PlayerProgress progress;
    private readonly List<FishJournalSlotView> rows = new List<FishJournalSlotView>();
    private float toastRemaining;
    private bool IsOpen => window.activeSelf;
    private void Awake()
    {
        foreach (var text in GetComponentsInChildren<Text>(true)) InventoryUIFont.Ensure(text);
        window.SetActive(false); toast.gameObject.SetActive(false); if (toastBacking != null) toastBacking.enabled = false;
        open.onClick.AddListener(Open); close.onClick.AddListener(Close);
    }
    private void Start()
    {
        progress = PlayerProgress.Current;
        if (progress != null) progress.SpeciesDiscovered += Discovered;
    }
    private void Update()
    {
        open.gameObject.SetActive(!GameInputBlock.IsBlocked);
        toastRemaining -= Time.unscaledDeltaTime;
        toast.gameObject.SetActive(toastRemaining > 0 && !GameInputBlock.IsBlocked);
        if (toastBacking != null) toastBacking.enabled = toast.gameObject.activeSelf;
        if (IsOpen && UiEscape.Pressed) { UiEscape.Consume(); Close(); return; }
        if (!Application.isFocused) return;
#if ENABLE_INPUT_SYSTEM
        bool toggle = Keyboard.current != null && Keyboard.current.jKey.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
        bool toggle = Input.GetKeyDown(KeyCode.J);
#else
        bool toggle = false;
#endif
        if (toggle) { if (IsOpen) Close(); else Open(); }
    }
    private void Open()
    {
        if (IsOpen || GameInputBlock.IsBlocked || Time.timeScale <= 0 || progress == null || !progress.IsReady) return;
        var inventory = PlayerInventory.Instance;
        if (inventory == null || inventory.Catalog == null) return;
        window.SetActive(true); GamePause.Set(this, true); GameInputBlock.Set(this, true);
        GameAudio.Instance?.Play(AudioCue.UiOpen);
        int discovered = 0, used = 0;
        foreach (var definition in inventory.Catalog.Definitions)
        {
            if (definition == null) continue;
            var selected = definition; int number = used + 1;
            bool known = progress.KnowsSpecies(definition.SpeciesId);
            if (known) discovered++;
            if (used == rows.Count) rows.Add(Instantiate(rowPrefab, content));
            var row = rows[used++]; row.gameObject.SetActive(true);
            row.Bind(known ? definition.Sprite : null, () => Select(selected, number));
        }
        for (int i = used; i < rows.Count; i++) rows[i].gameObject.SetActive(false);
        count.text = "SỔ CÁ · " + discovered + "/" + used + " LOÀI";
        detailName.text = "Bộ sưu tập của bạn"; portrait.enabled = false;
        detail.text = "Chọn một loài bên trái.\n\nTự câu lần đầu để mở hình ảnh và thông tin. Bán hoặc giao cá không làm mất ghi nhận trong sổ.";
        LayoutRebuilder.ForceRebuildLayoutImmediate(content); scroll.verticalNormalizedPosition = 1;
    }
    private void Select(FishDefinition definition, int number)
    {
        for (int i = 0; i < rows.Count; i++) rows[i].SetSelected(i == number - 1);
        bool known = progress.KnowsSpecies(definition.SpeciesId);
        detailName.text = "#" + number.ToString("000") + " · " + (known ? definition.DisplayName : "???");
        portrait.sprite = known ? definition.Sprite : null; portrait.enabled = portrait.sprite != null;
        if (!known) { detail.text = "LOÀI CHƯA KHÁM PHÁ\n\nTự câu loài này để mở thông tin trong sổ."; return; }
        string baits = "";
        foreach (var bait in definition.PreferredBaits)
            if (bait != null) baits += (baits.Length > 0 ? ", " : "") + bait.DisplayName;
        string style = definition.SwimStyle == FishSwimStyle.Patrol ? "Bơi tuần tra" : definition.SwimStyle == FishSwimStyle.Wave ? "Bơi lượn sóng" : "Bơi xen kẽ nghỉ";
        detail.text = "Tổng đã tự câu: " + progress.CatchCount(definition.SpeciesId) +
            "\nĐộ sâu: " + definition.MinDepth.ToString("0.##") + " – " + definition.MaxDepth.ToString("0.##") +
            " đơn vị tính từ mặt nước\nMồi ưa thích: " + (baits.Length > 0 ? baits : "Chưa có dữ liệu") + "\nTập tính: " + style;
    }
    private void Discovered(string id)
    {
        var catalog = PlayerInventory.Instance.Catalog;
        if (!catalog.TryGetFish(id, out var definition)) return;
        toast.text = "KHÁM PHÁ LOÀI MỚI: " + definition.DisplayName + " · [J] xem sổ cá"; toastRemaining = 4;
    }
    private void Close()
    {
        if (IsOpen) GameAudio.Instance?.Play(AudioCue.UiClose);
        window.SetActive(false); GamePause.Set(this, false); GameInputBlock.Set(this, false);
    }
    private void OnDisable() { GamePause.Set(this, false); GameInputBlock.Set(this, false); }
    private void OnDestroy() { if (progress != null) progress.SpeciesDiscovered -= Discovered; }
}
