using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public enum AudioCue { MenuMusic, LakeMusic, Ambience, Step, Cast, Splash, Reel, Bite, QteAppear, Good, Great, Fail, LineBreak, FishLanded, UiClick, UiOpen, UiClose }
[Serializable]
public sealed class AudioCueSource { public AudioCue cue; public AudioSource source; }

[DefaultExecutionOrder(-2000)]
[DisallowMultipleComponent]
public sealed class GameAudio : MonoBehaviour
{
    [SerializeField] private AudioMixer mixer;
    [SerializeField] private AudioCueSource[] sources = new AudioCueSource[0];
    public static GameAudio Instance { get; private set; }
    private bool started;
    private string activeScene;
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        foreach (var entry in sources)
            if (entry != null && entry.source != null) entry.source.ignoreListenerPause = entry.cue == AudioCue.MenuMusic ||
                entry.cue == AudioCue.LakeMusic || entry.cue == AudioCue.UiClick || entry.cue == AudioCue.UiOpen || entry.cue == AudioCue.UiClose;
        GameSettings.Changed += ApplyVolumes;
        SceneManager.sceneLoaded += SceneLoaded;
    }
    private void Start() { if (Instance != this) return; started = true; ApplyVolumes(); GameSettings.ApplySavedVideo(); SetScene(SceneManager.GetActiveScene().name); }
    private void SceneLoaded(Scene scene, LoadSceneMode mode) { if (started) SetScene(scene.name); }
    private void SetScene(string name)
    {
        if (activeScene == name) return;
        activeScene = name;
        foreach (var entry in sources) if (entry != null && entry.source != null && entry.source.loop) entry.source.Stop();
        SetLoop(name == "MainMenu" ? AudioCue.MenuMusic : AudioCue.LakeMusic, true);
        SetLoop(AudioCue.Ambience, name != "MainMenu");
    }
    private AudioSource Find(AudioCue cue)
    {
        foreach (var entry in sources) if (entry != null && entry.cue == cue) return entry.source;
        return null;
    }
    public void Play(AudioCue cue)
    {
        var source = Find(cue);
        if (source == null || source.clip == null) return;
        source.PlayOneShot(source.clip);
    }
    public void SetLoop(AudioCue cue, bool playing)
    {
        var source = Find(cue);
        if (source == null) return;
        if (playing && source.clip != null && !source.isPlaying) source.Play();
        else if (!playing && source.isPlaying) source.Stop();
    }
    private void ApplyVolumes()
    {
        if (mixer == null) return;
        mixer.SetFloat("MasterVolume", Decibels(GameSettings.Master));
        mixer.SetFloat("MusicVolume", Decibels(GameSettings.MusicMuted ? 0 : GameSettings.Music));
        mixer.SetFloat("SfxVolume", Decibels(GameSettings.SfxMuted ? 0 : GameSettings.Sfx));
    }
    private static float Decibels(float value) => value <= 0.0001f ? -80 : Mathf.Log10(value) * 20;
    private void OnDestroy()
    {
        if (Instance != this) return;
        GameSettings.Changed -= ApplyVolumes;
        SceneManager.sceneLoaded -= SceneLoaded;
        Instance = null;
    }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reset() => Instance = null;
}
