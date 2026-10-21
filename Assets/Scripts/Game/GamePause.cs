using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class GamePause
{
    private static readonly HashSet<Object> owners = new HashSet<Object>();
    private static float previousScale = 1;
    public static void Set(Object owner, bool paused)
    {
        if (owner == null) return;
        owners.RemoveWhere(item => item == null);
        if (paused)
        {
            if (owners.Count == 0) previousScale = Time.timeScale > 0 ? Time.timeScale : 1;
            owners.Add(owner);
        }
        else owners.Remove(owner);
        Time.timeScale = owners.Count > 0 ? 0 : previousScale;
        AudioListener.pause = owners.Count > 0;
    }
    public static void Clear()
    {
        owners.Clear();
        Time.timeScale = 1;
        AudioListener.pause = false;
    }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reset()
    {
        previousScale = 1; Clear();
        SceneManager.sceneLoaded -= Loaded;
        SceneManager.sceneLoaded += Loaded;
    }
    private static void Loaded(Scene scene, LoadSceneMode mode) => Clear();
}
