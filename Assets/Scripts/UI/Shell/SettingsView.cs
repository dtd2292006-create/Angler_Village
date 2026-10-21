using AnglerVillage.UI;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(-500)]
public sealed class SettingsView : MonoBehaviour
{
    [SerializeField] private Slider master;
    [SerializeField] private Slider music;
    [SerializeField] private Slider sfx;
    [SerializeField] private Text masterLabel;
    [SerializeField] private Text musicLabel;
    [SerializeField] private Text sfxLabel;
    [SerializeField] private Button muteMusic;
    [SerializeField] private Button muteSfx;
    [SerializeField] private Text muteMusicLabel;
    [SerializeField] private Text muteSfxLabel;
    [SerializeField] private Button resolution;
    [SerializeField] private Text resolutionLabel;
    [SerializeField] private Button fullscreen;
    [SerializeField] private Text fullscreenLabel;
    [SerializeField] private Button vsync;
    [SerializeField] private Text vsyncLabel;
    [SerializeField] private Button applyVideo;
    [SerializeField] private Button close;
    private int chosenResolution;
    private bool chosenFullscreen;
    private bool chosenVsync;
    private void Awake()
    {
        foreach (Text text in GetComponentsInChildren<Text>(true)) InventoryUIFont.Ensure(text);
        master.onValueChanged.AddListener(Master);
        music.onValueChanged.AddListener(Music);
        sfx.onValueChanged.AddListener(Sfx);
        muteMusic.onClick.AddListener(MuteMusic);
        muteSfx.onClick.AddListener(MuteSfx);
        resolution.onClick.AddListener(Resolution);
        fullscreen.onClick.AddListener(Fullscreen);
        vsync.onClick.AddListener(Vsync);
        applyVideo.onClick.AddListener(ApplyVideo);
        close.onClick.AddListener(Close);
    }
    private void OnEnable()
    {
        master.SetValueWithoutNotify(GameSettings.Master);
        music.SetValueWithoutNotify(GameSettings.Music);
        sfx.SetValueWithoutNotify(GameSettings.Sfx);
        chosenResolution = GameSettings.ResolutionIndex;
        chosenFullscreen = GameSettings.Fullscreen;
        chosenVsync = GameSettings.Vsync;
        Labels();
    }
    private void Update() { if (UiEscape.Pressed) { UiEscape.Consume(); Close(); } }
    public void Open() { gameObject.SetActive(true); GameAudio.Instance?.Play(AudioCue.UiOpen); }
    public void Close() { if (!gameObject.activeSelf) return; GameSettings.Save(); gameObject.SetActive(false); GameAudio.Instance?.Play(AudioCue.UiClose); }
    private void Master(float value) { GameSettings.SetVolume("Master", value); Labels(); }
    private void Music(float value) { GameSettings.SetVolume("Music", value); Labels(); }
    private void Sfx(float value) { GameSettings.SetVolume("Sfx", value); Labels(); }
    private void MuteMusic() { GameSettings.SetMute(true, !GameSettings.MusicMuted); Labels(); }
    private void MuteSfx() { GameSettings.SetMute(false, !GameSettings.SfxMuted); Labels(); }
    private void Resolution() { chosenResolution = (chosenResolution + 1) % 3; Labels(); }
    private void Fullscreen() { chosenFullscreen = !chosenFullscreen; Labels(); }
    private void Vsync() { chosenVsync = !chosenVsync; Labels(); }
    private void ApplyVideo() { GameSettings.ApplyVideo(chosenResolution, chosenFullscreen, chosenVsync); }
    private void Labels()
    {
        masterLabel.text = "Tổng: " + Mathf.RoundToInt(GameSettings.Master * 100) + "%";
        musicLabel.text = "Nhạc: " + Mathf.RoundToInt(GameSettings.Music * 100) + "%";
        sfxLabel.text = "Hiệu ứng: " + Mathf.RoundToInt(GameSettings.Sfx * 100) + "%";
        muteMusicLabel.text = GameSettings.MusicMuted ? "Nhạc: TẮT" : "Nhạc: BẬT";
        muteSfxLabel.text = GameSettings.SfxMuted ? "Hiệu ứng: TẮT" : "Hiệu ứng: BẬT";
        var size = GameSettings.Resolutions[chosenResolution];
        resolutionLabel.text = size.x + " × " + size.y;
        fullscreenLabel.text = chosenFullscreen ? "Toàn màn hình" : "Cửa sổ";
        vsyncLabel.text = chosenVsync ? "VSync: BẬT" : "VSync: TẮT";
    }
    private void OnDisable() => GameSettings.Save();
    // Listeners belong to the same UI hierarchy and are destroyed with it.
}
