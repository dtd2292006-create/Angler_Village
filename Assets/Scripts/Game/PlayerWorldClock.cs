using System;
using AnglerVillage.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public sealed class PlayerWorldClock : MonoBehaviour
{
    [SerializeField, Min(0.01f)] private float gameMinutesPerSecond = 2;
    public double ElapsedMinutes { get; private set; } = 480;
    public int Day => (int)(ElapsedMinutes / 1440) + 1;
    public int Hour => (int)(ElapsedMinutes % 1440) / 60;
    public int Minute => (int)(ElapsedMinutes % 60);
    public event Action SaveRequested;
    private float saveTimer;
    public void Initialize(double minutes) { ElapsedMinutes = minutes; saveTimer = 0; }
    private void Update()
    {
        if (SceneManager.GetActiveScene().name == "MainMenu" || Time.timeScale <= 0 || GameInputBlock.IsBlocked) return;
        ElapsedMinutes += Time.deltaTime * gameMinutesPerSecond;
        saveTimer += Time.deltaTime;
        if (saveTimer < 15) return;
        saveTimer = 0;
        SaveRequested?.Invoke();
    }
}
