using AnglerVillage.UI;
using UnityEngine;
using UnityEngine.UI;

public sealed class MainMenuView : MonoBehaviour
{
    [SerializeField] private SceneFlow flow;
    [SerializeField] private Button newGame;
    [SerializeField] private Button continueGame;
    [SerializeField] private Button settings;
    [SerializeField] private Button quit;
    [SerializeField] private SettingsView settingsView;
    [SerializeField] private GameObject confirmation;
    [SerializeField] private Text confirmationText;
    [SerializeField] private Button confirm;
    [SerializeField] private Button cancel;
    [SerializeField] private Text status;
    private bool quitPending;
    private bool canContinue;
    private void Awake()
    {
        GamePause.Clear();
        foreach (var text in GetComponentsInChildren<Text>(true)) InventoryUIFont.Ensure(text);
        confirmation.SetActive(false);
        newGame.onClick.AddListener(NewGame);
        continueGame.onClick.AddListener(Continue);
        settings.onClick.AddListener(settingsView.Open);
        quit.onClick.AddListener(Quit);
        confirm.onClick.AddListener(Confirm);
        cancel.onClick.AddListener(Cancel);
        flow.Failed += Failed;
    }
    private void Start()
    {
        canContinue = SceneFlow.HasSave;
        continueGame.interactable = canContinue;
        status.text = continueGame.interactable ? "Tiếp tục chuyến câu của bạn." : "Bắt đầu một chuyến câu mới.";
    }
    private void Update()
    {
        newGame.interactable = !flow.IsLoading;
        continueGame.interactable = canContinue && !flow.IsLoading;
        settings.interactable = !flow.IsLoading;
        quit.interactable = !flow.IsLoading;
        if (flow.IsLoading) status.text = "Đang tải…";
        if (confirmation.activeSelf && UiEscape.Pressed) { UiEscape.Consume(); Cancel(); }
    }
    private void NewGame()
    {
        quitPending = false;
        if (!SceneFlow.HasSave) { flow.NewGame(); return; }
        confirmationText.text = "Bắt đầu hành trình mới? Tiến trình hiện tại sẽ được cất vào bản sao lưu.";
        confirmation.SetActive(true);
    }
    private void Continue() { if (SceneFlow.HasSave) flow.ContinueGame(); }
    private void Quit() { quitPending = true; confirmationText.text = "Bạn muốn thoát game?"; confirmation.SetActive(true); }
    private void Confirm()
    {
        confirmation.SetActive(false);
        if (quitPending) flow.Quit(); else flow.NewGame();
    }
    private void Cancel() => confirmation.SetActive(false);
    private void Failed(string message) => status.text = message;
    private void OnDestroy() { if (flow != null) flow.Failed -= Failed; }
}
