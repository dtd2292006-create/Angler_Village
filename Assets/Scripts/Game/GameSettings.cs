using System;
using UnityEngine;

public static class GameSettings
{
    private const string Prefix = "AnglerVillage.Settings.";
    public static event Action Changed;
    public static float Master => PlayerPrefs.GetFloat(Prefix + "Master", 1);
    public static float Music => PlayerPrefs.GetFloat(Prefix + "Music", 0.6f);
    public static float Sfx => PlayerPrefs.GetFloat(Prefix + "Sfx", 0.8f);
    public static bool MusicMuted => PlayerPrefs.GetInt(Prefix + "MuteMusic", 0) != 0;
    public static bool SfxMuted => PlayerPrefs.GetInt(Prefix + "MuteSfx", 0) != 0;
    public static bool Fullscreen => PlayerPrefs.GetInt(Prefix + "Fullscreen", Screen.fullScreen ? 1 : 0) != 0;
    public static bool Vsync => PlayerPrefs.GetInt(Prefix + "Vsync", 1) != 0;
    public static int ResolutionIndex => Mathf.Clamp(PlayerPrefs.GetInt(Prefix + "Resolution", 2), 0, 2);
    public static readonly Vector2Int[] Resolutions = { new Vector2Int(1280, 720), new Vector2Int(1600, 900), new Vector2Int(1920, 1080) };
    public static void SetVolume(string bus, float value)
    {
        if (bus != "Master" && bus != "Music" && bus != "Sfx") return;
        PlayerPrefs.SetFloat(Prefix + bus, Mathf.Clamp01(value));
        Changed?.Invoke();
    }
    public static void SetMute(bool music, bool value)
    {
        PlayerPrefs.SetInt(Prefix + (music ? "MuteMusic" : "MuteSfx"), value ? 1 : 0);
        Changed?.Invoke();
    }
    public static void ApplyVideo(int resolution, bool fullscreen, bool vsync)
    {
        resolution = Mathf.Clamp(resolution, 0, Resolutions.Length - 1);
        PlayerPrefs.SetInt(Prefix + "Resolution", resolution);
        PlayerPrefs.SetInt(Prefix + "Fullscreen", fullscreen ? 1 : 0);
        PlayerPrefs.SetInt(Prefix + "Vsync", vsync ? 1 : 0);
        QualitySettings.vSyncCount = vsync ? 1 : 0;
        Application.targetFrameRate = vsync ? -1 : 120;
        var size = Resolutions[resolution];
        Screen.SetResolution(size.x, size.y, fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);
        Save();
    }
    public static void ApplySavedVideo()
    {
        if (PlayerPrefs.HasKey(Prefix + "Resolution")) ApplyVideo(ResolutionIndex, Fullscreen, Vsync);
        else { QualitySettings.vSyncCount = Vsync ? 1 : 0; Application.targetFrameRate = Vsync ? -1 : 120; }
    }
    public static void Save() => PlayerPrefs.Save();
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetEvents() => Changed = null;
}
