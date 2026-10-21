using AnglerVillage.UI;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(-400)]
public sealed class LakePauseView : MonoBehaviour
{
    [SerializeField] private SceneFlow flow;
    [SerializeField] private HookController hook;
    [SerializeField] private FishInventoryView inventory;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject helpPanel;
    [SerializeField] private GameObject confirmation;
    [SerializeField] private Text confirmationText;
    [SerializeField] private Text status;
    [SerializeField] private Button open;
    [SerializeField] private Button resume;
    [SerializeField] private Button settings;
    [SerializeField] private Button help;
    [SerializeField] private Button closeHelp;
    [SerializeField] private Button menu;
    [SerializeField] private Button quit;
    [SerializeField] private Button village;
    [SerializeField] private Button quickVillage;
    [SerializeField] private Button confirm;
    [SerializeField] private Button cancel;
    [SerializeField] private SettingsView settingsView;
    private bool quitting;
    private bool returningVillage;
    private void Awake()
    {
        foreach (var text in GetComponentsInChildren<Text>(true)) InventoryUIFont.Ensure(text);
        pausePanel.SetActive(false); helpPanel.SetActive(false); confirmation.SetActive(false);
        open.onClick.AddListener(Open);
        resume.onClick.AddListener(Resume);
        settings.onClick.AddListener(settingsView.Open);
        help.onClick.AddListener(Help);
        closeHelp.onClick.AddListener(CloseHelp);
        menu.onClick.AddListener(Menu);
        quit.onClick.AddListener(Quit);
        if (village != null) village.onClick.AddListener(ReturnVillage);
        if (quickVillage != null) quickVillage.onClick.AddListener(RequestVillageReturn);
        confirm.onClick.AddListener(Confirm);
        cancel.onClick.AddListener(Cancel);
        flow.Failed += Failed;
    }
    private void Update()
    {
        if (flow.IsLoading || !UiEscape.Pressed) return;
        UiEscape.Consume();
        if (settingsView.gameObject.activeSelf) settingsView.Close();
        else if (confirmation.activeSelf) Cancel();
        else if (helpPanel.activeSelf) CloseHelp();
        else if (inventory != null && inventory.IsOpen) inventory.Close();
        else if (pausePanel.activeSelf) Resume();
        else Open();
    }
    public void Open()
    {
        if (flow.IsLoading) return;
        if (inventory != null) inventory.Close();
        if (hook != null) hook.SetReelHeld(false);
        pausePanel.SetActive(true);
        status.text = "Đã tạm dừng";
        GameInputBlock.Set(this, true);
        GamePause.Set(this, true);
        GameAudio.Instance?.Play(AudioCue.UiOpen);
    }
    private void Resume()
    {
        settingsView.Close(); helpPanel.SetActive(false); confirmation.SetActive(false); pausePanel.SetActive(false);
        GameInputBlock.Set(this, false); GamePause.Set(this, false);
        GameAudio.Instance?.Play(AudioCue.UiClose);
    }
    private void Help() => helpPanel.SetActive(true);
    private void CloseHelp() => helpPanel.SetActive(false);
    private void Menu() { quitting = false; returningVillage = false; confirmationText.text = "Lưu tiến trình và về màn hình chính? Lượt câu đang diễn ra sẽ kết thúc."; confirmation.SetActive(true); }
    private void Quit() { quitting = true; returningVillage = false; confirmationText.text = "Lưu tiến trình và thoát game?"; confirmation.SetActive(true); }
    private void RequestVillageReturn() { if (flow.IsLoading) return; Open(); ReturnVillage(); }
    private void ReturnVillage() { quitting = false; returningVillage = true; confirmationText.text = "Thu dọn lượt câu và trở về làng? Cá đã cất trong túi được giữ nguyên."; confirmation.SetActive(true); }
    private void Confirm() { confirmation.SetActive(false); if (quitting) flow.Quit(); else if (returningVillage) flow.TravelToVillage(); else flow.ReturnToMenu(); }
    private void Cancel() => confirmation.SetActive(false);
    private void Failed(string message) => status.text = message;
    private void OnApplicationFocus(bool focused) { if (!focused && Time.timeScale > 0 && !pausePanel.activeSelf && !flow.IsLoading) Open(); }
    private void OnDisable() { GameInputBlock.Set(this, false); GamePause.Set(this, false); }
    private void OnDestroy() { if (flow != null) flow.Failed -= Failed; }
}
