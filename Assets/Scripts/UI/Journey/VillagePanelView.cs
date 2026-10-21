using System;
using System.Collections.Generic;
using AnglerVillage.UI;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(-600)]
public sealed class VillagePanelView : MonoBehaviour
{
    [SerializeField] private GuildService guild;
    [SerializeField] private Graphic modalBackdrop;
    [SerializeField] private Graphic browserFrame;
    [SerializeField] private Button dialogueContinue;
    [SerializeField] private Text dialogueContinueLabel;
    [SerializeField] private GameObject browserPanel;
    [SerializeField] private GameObject certificateSheet;
    [SerializeField] private Button certificateContinue;
    [SerializeField] private Image speakerPortrait;
    [SerializeField] private Text speakerName;
    [SerializeField] private Sprite receptionistPortrait;
    [SerializeField] private Sprite shopkeeperPortrait;
    [SerializeField] private ShopService shop;
    [SerializeField] private GameObject window;
    [SerializeField] private GameObject browser;
    [SerializeField] private GameObject dialogue;
    [SerializeField] private Text dialogueText;
    [SerializeField] private Text title;
    [SerializeField] private Text body;
    [SerializeField] private Text status;
    [SerializeField] private Text wallet;
    [SerializeField] private Button primary;
    [SerializeField] private Text primaryLabel;
    [SerializeField] private Button secondary;
    [SerializeField] private Text secondaryLabel;
    [SerializeField] private Button close;
    [SerializeField] private Button equipmentTab;
    [SerializeField] private Button sellFishTab;
    [SerializeField] private Button sellGearTab;
    [SerializeField] private GameObject tabs;
    [SerializeField] private RectTransform content;
    [SerializeField] private ScrollRect scroll;
    [SerializeField] private JourneyListRowView rowPrefab;
    private readonly List<JourneyListRowView> rows = new List<JourneyListRowView>();
    private Action primaryAction;
    private Action secondaryAction;
    private string[] pages;
    private int page;
    private Action dialogueFinished;
    private int rowCount;
    private int lastCoins = -1;
    public bool IsOpen => window.activeSelf;
    private PlayerProgress Progress => PlayerProgress.Current;
    private void Awake()
    {
        foreach (var text in GetComponentsInChildren<Text>(true)) InventoryUIFont.Ensure(text);
        window.SetActive(false);
        if (certificateContinue != null) certificateContinue.onClick.AddListener(() =>
        {
            certificateSheet.SetActive(false); browserPanel.SetActive(true); GuildBrowser();
        });
        primary.onClick.AddListener(() => primaryAction?.Invoke());
        if (dialogueContinue != null) dialogueContinue.onClick.AddListener(() => primaryAction?.Invoke());
        secondary.onClick.AddListener(() => secondaryAction?.Invoke());
        close.onClick.AddListener(Close);
        equipmentTab.onClick.AddListener(() => ShopTab(0));
        sellFishTab.onClick.AddListener(() => ShopTab(1));
        sellGearTab.onClick.AddListener(() => ShopTab(2));
    }
    private void Start()
    {
        if (Progress != null && Progress.IsReady && !Progress.WelcomeRead)
            ShowDialogue("MỘT NGÀY MỚI", new[] { "Bạn đã có chiếc cần đầu tiên.\n\nHãy đi sang phải đến guild, gặp cô tiếp tân để nhận chứng nhận cần thủ và ủy thác đầu tiên.\n\nA/D: di chuyển · E: nói chuyện · I: túi đồ" }, () => { Progress.ReadWelcome(); Close(); });
    }
    private void Update()
    {
        if (!IsOpen) return;
        if (Progress != null && lastCoins != Progress.Coins) { lastCoins = Progress.Coins; wallet.text = lastCoins + " XU"; }
        if (UiEscape.Pressed) { UiEscape.Consume(); Close(); }
    }
    private bool Open()
    {
        if (Progress == null || !Progress.IsReady) return false;
        if (!IsOpen && (GameInputBlock.IsBlocked || Time.timeScale <= 0)) return false;
        if (!IsOpen) GameAudio.Instance?.Play(AudioCue.UiOpen);
        window.SetActive(true); GameInputBlock.Set(this, true); GamePause.Set(this, true);
        status.text = ""; return true;
    }
    public void Close()
    {
        if (IsOpen) GameAudio.Instance?.Play(AudioCue.UiClose);
        window.SetActive(false);
        if (certificateSheet != null) certificateSheet.SetActive(false);
        if (browserPanel != null) browserPanel.SetActive(true);
        GameInputBlock.Set(this, false); GamePause.Set(this, false);
        primaryAction = null; secondaryAction = null; dialogueFinished = null;
    }
    private void ShowDialogue(string heading, string[] text, Action finished)
    {
        if (!Open()) return;
        SetDialoguePresentation(true);
        title.text = heading;
        if (speakerName != null) speakerName.text = heading;
        if (speakerPortrait != null)
        {
            speakerPortrait.sprite = heading.StartsWith("CỬA HÀNG") ? shopkeeperPortrait : receptionistPortrait;
            speakerPortrait.enabled = heading.StartsWith("CỬA HÀNG") || heading.StartsWith("GUILD");
        }
        pages = text; page = 0; dialogueFinished = finished;
        browser.SetActive(false); dialogue.SetActive(true); tabs.SetActive(false);
        secondary.gameObject.SetActive(false); ShowPage();
    }
    private void SetDialoguePresentation(bool value)
    {
        if (browserFrame != null) browserFrame.enabled = !value;
        if (modalBackdrop != null)
        {
            var tint = modalBackdrop.color; tint.a = value ? 0.25f : 0.96f; modalBackdrop.color = tint;
        }
        title.gameObject.SetActive(!value); wallet.gameObject.SetActive(!value);
        status.gameObject.SetActive(!value); close.gameObject.SetActive(!value);
        primary.gameObject.SetActive(!value || dialogueContinue == null);
    }
    private void ShowPage()
    {
        dialogueText.text = pages[page]; primaryLabel.text = page < pages.Length - 1 ? "Tiếp tục" : "Đã hiểu";
        if (dialogueContinueLabel != null) dialogueContinueLabel.text = primaryLabel.text;
        primary.interactable = true;
        primaryAction = () => { if (++page < pages.Length) ShowPage(); else { var done = dialogueFinished; dialogueFinished = null; done?.Invoke(); } };
    }
    public void OpenGuild()
    {
        if (!Open()) return;
        if (!Progress.Certified)
        {
            ShowDialogue("GUILD · CÔ TIẾP TÂN", new[] {
                "Chào mừng đến guild! Tôi phụ trách giao và nhận ủy thác.\n\nMỗi ủy thác ghi rõ loại cá, số lượng, thời hạn và phần thưởng. Bạn nhận một ủy thác tại một thời điểm.",
                "Mang đủ cá về nói chuyện với tôi và bấm Giao cá. Chỉ những con cá được giao mới rời túi.\n\nMột số việc nhận cá cũ; việc yêu cầu cá mới chỉ tính cá bạn tự câu sau lúc nhận việc.",
                "Hết hạn hoặc hủy thì ủy thác thất bại, nhưng cá trong túi vẫn giữ. Bạn có thể nhận lại và tiếp tục câu.\n\nHoàn thành sẽ nhận xu và kinh nghiệm để tăng cấp cần thủ.",
                "Đây là chứng nhận cần thủ và ủy thác đầu tiên của bạn: câu một con cá mới ở hồ.\n\nQua cửa hàng bên phải gặp chú chủ tiệm để nhận bộ dây và mồi đầu tiên miễn phí. Sau đó đi tiếp tới biển dẫn ra hồ." }, () => {
                    bool enrolled = guild.Enroll(out var message); GuildBrowser();
                    if (enrolled && certificateSheet != null && browserPanel != null)
                    { browserPanel.SetActive(false); certificateSheet.SetActive(true); }
                    status.text = message ?? "Đã nhận chứng nhận và ủy thác đầu tiên. Qua cửa hàng bên phải nhận dây và mồi.";
                });
        }
        else GuildBrowser();
    }
    private void PrepareBrowser(string heading, bool shopTabs)
    {
        SetDialoguePresentation(false);
        title.text = heading; dialogue.SetActive(false); browser.SetActive(true); tabs.SetActive(shopTabs);
        foreach (var row in rows) row.gameObject.SetActive(false);
        rowCount = 0; secondary.gameObject.SetActive(false); primary.gameObject.SetActive(true); primary.interactable = true;
        body.text = "Chọn một mục bên trái để xem chi tiết.";
        primaryLabel.text = "Chọn mục"; primary.interactable = false; primaryAction = null;
    }
    private void AddRow(string name, string detail, Sprite sprite, Action select)
    {
        if (rowCount == rows.Count) rows.Add(Instantiate(rowPrefab, content));
        var row = rows[rowCount++]; row.gameObject.SetActive(true); row.Bind(name, detail, sprite, () =>
        {
            foreach (var entry in rows) entry.SetSelected(entry == row);
            select();
        });
    }
    private void FinishRows() { LayoutRebuilder.ForceRebuildLayoutImmediate(content); scroll.verticalNormalizedPosition = 1; }
    private void GuildBrowser()
    {
        PrepareBrowser("GUILD · ỦY THÁC", false); guild.RefreshDeadline();
        var active = Progress.Commission;
        if (active != null && !active.failed && guild.ActiveDefinition != null)
        {
            var definition = guild.ActiveDefinition;
            AddRow(definition.title, "Đang nhận · " + guild.EligibleFish().Count + "/" + definition.quantity, definition.species != null ? definition.species.Sprite : null, () => ActiveCommission(definition));
            ActiveCommission(definition);
        }
        else
        {
            if (active != null) status.text = "Ủy thác cũ đã thất bại. Cá trong túi vẫn giữ nguyên; chọn việc để nhận lại.";
            var definitions = !Progress.FirstDelivery ? new[] { guild.Content.firstCommission } : guild.Content.commissions;
            foreach (var definition in definitions)
            {
                if (definition == null) continue;
                var selected = definition;
                AddRow(selected.title, "Cấp " + selected.minimumLevel + " · " + selected.rewardCoins + " xu", selected.species != null ? selected.species.Sprite : null, () => {
                    body.text = Description(selected); primaryLabel.text = "Nhận ủy thác"; primary.interactable = Progress.Level >= selected.minimumLevel;
                    primaryAction = () => { guild.Accept(selected, out var message); GuildBrowser(); status.text = message ?? "Đã nhận ủy thác."; };
                });
            }
        }
        FinishRows();
    }
    private static string Description(CommissionDefinition definition) => definition.description + "\n\nYêu cầu: " + definition.quantity + " × " +
        (definition.species != null ? definition.species.DisplayName : "cá bất kỳ") +
        (definition.requiresFreshCatch ? "\nChỉ tính cá mới tự câu sau khi nhận việc." : "\nCó thể dùng cá đã có trong túi.") +
        "\nThời hạn: " + definition.durationGameMinutes.ToString("0") + " phút game\nThưởng: " + definition.rewardCoins + " xu · " + definition.rewardXp + " XP";
    private void ActiveCommission(CommissionDefinition definition)
    {
        body.text = Description(definition) + "\n\n" + guild.Summary();
        primaryLabel.text = "Giao cá và nhận thưởng"; primary.interactable = guild.EligibleFish().Count >= definition.quantity;
        primaryAction = () => { guild.TurnIn(out var message); GuildBrowser(); status.text = message; };
        secondary.gameObject.SetActive(true); secondaryLabel.text = "Hủy ủy thác";
        secondaryAction = () => ShowDialogue("HỦY ỦY THÁC?", new[] { "Hủy sẽ tính là thất bại. Cá trong túi vẫn được giữ.\n\nBấm Đã hiểu để xác nhận hủy, hoặc Esc để quay lại sau." }, () => { guild.Abandon(); GuildBrowser(); });
    }
    public void OpenShop()
    {
        if (!Open()) return;
        if (!Progress.Certified)
        { ShowDialogue("CỬA HÀNG · CHỦ TIỆM", new[] { "Chào cháu! Hãy qua guild bên trái gặp cô tiếp tân trước.\n\nCó chứng nhận cần thủ rồi quay lại đây, chú tặng bộ dây và mồi đầu tiên." }, Close); return; }
        if (!Progress.StarterKitClaimed)
        {
            ShowDialogue("CỬA HÀNG · BỘ KHỞI ĐẦU", new[] {
                "Cô tiếp tân giới thiệu cháu qua phải không?\n\nChú thu mua cá và mua bán cần, mồi, dây. Bán cá để kiếm xu, hoặc dùng phần thưởng ủy thác để mua trang bị tốt hơn.",
                "Bộ dây cơ bản và mồi giun đầu tiên miễn phí, chú sẽ trang bị giúp luôn.\n\nSau này chọn đồ trong túi [I] để thay. Khi cần dây cơ bản thay thế, cửa hàng có thể cấp miễn phí nếu cháu đã hết loại dây đó.\n\nNhận xong, đi tiếp sang phải đến biển để ra hồ." }, () => { shop.ClaimStarter(out var message); ShopTab(0); status.text = message; });
        }
        else ShopTab(0);
    }
    private void ShopTab(int tab)
    {
        PrepareBrowser("CỬA HÀNG · MUA / BÁN", true);
        if (tab == 0)
        {
            for (int i = 0; i < shop.Content.equipment.Length; i++)
            {
                var offer = shop.Content.equipment[i]; if (offer == null || offer.item == null) continue;
                int index = i;
                AddRow(offer.item.DisplayName, offer.price == 0 ? "Miễn phí khi đã hết" : offer.price + " xu", offer.item.Icon, () => {
                    body.text = offer.item.Description + "\n\nGiá: " + offer.price + " xu\nMua xong mở túi [I] để trang bị.";
                    primaryLabel.text = "Mua 1"; primary.interactable = true;
                    primaryAction = () => { shop.BuyEquipment(index, out var message); status.text = message; };
                });
            }
        }
        else if (tab == 1)
        {
            foreach (var item in PlayerInventory.Instance.Fish)
            {
                var selected = item; var price = shop.Content.FishPrice(selected.SpeciesId);
                if (price == null) continue;
                AddRow(price.fish.DisplayName, "#" + selected.CatchSequence + " · bán " + price.sellPrice + " xu", price.fish.Sprite, () => {
                    body.text = "Bán con cá #" + selected.CatchSequence + ".\n\nNếu đang dành cá cho ủy thác, hãy giao ở guild trước. Cá đã bán sẽ rời túi.";
                    primaryLabel.text = "Bán · " + price.sellPrice + " xu"; primary.interactable = true;
                    primaryAction = () => { shop.SellFish(selected.InstanceId, out var message); ShopTab(1); status.text = message; };
                });
            }
        }
        else
        {
            var equipment = PlayerInventory.Instance.GetComponent<PlayerEquipment>();
            foreach (var item in equipment.Items)
            {
                if (!equipment.TryGet(item.ItemId, out var definition)) continue;
                var selected = item; int price = shop.EquipmentSellPrice(item.ItemId);
                AddRow(definition.DisplayName, "×" + item.Quantity + " · bán " + price + " xu", definition.Icon, () => {
                    body.text = "Bán 1 " + definition.DisplayName + ".\n\nGiữ lại ít nhất một cần và một mồi. Món đang trang bị sẽ được bỏ trang bị nếu số lượng về 0.";
                    primaryLabel.text = "Bán 1 · " + price + " xu"; primary.interactable = true;
                    primaryAction = () => { shop.SellEquipment(selected.ItemId, out var message); ShopTab(2); status.text = message; };
                });
            }
        }
        if (rowCount == 0) body.text = "Chưa có vật phẩm ở mục này.";
        FinishRows();
    }
    public void ShowTravelError(string message) => ShowDialogue("CHƯA THỂ CHUYỂN KHU VỰC", new[] { message }, Close);
    public void ExplainLockedEntrance() => ShowDialogue("CHUYẾN CÂU ĐẦU TIÊN", new[] { "Hãy gặp cô tiếp tân ở guild để nhận chứng nhận và ủy thác.\n\nSau đó qua cửa hàng nhận bộ dây + mồi miễn phí, rồi quay lại biển này để ra hồ." }, Close);
    private void OnDisable() { GameInputBlock.Set(this, false); GamePause.Set(this, false); }
}
