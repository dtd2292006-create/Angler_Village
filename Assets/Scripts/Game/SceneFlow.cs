using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class SceneFlow : MonoBehaviour
{
    [SerializeField] private string lakeScene = "Lake";
    [SerializeField] private string villageScene = "Village";
    [SerializeField] private string menuScene = "MainMenu";
    public bool IsLoading { get; private set; }
    public event Action<string> Failed;
    public static string SavePath => Path.Combine(Application.persistentDataPath, "Saves", "slot_01", "player.json");
    public static bool HasSave
    {
        get
        {
            if (!File.Exists(SavePath) && !File.Exists(SavePath + ".bak") && !File.Exists(SavePath + ".tmp")) return false;
            return new PlayerSaveFileStore(SavePath).Load(out _, out _) != PlayerSaveLoadStatus.Failed;
        }
    }
    public void ContinueGame()
    {
        if (IsLoading) return;
        var store = new PlayerSaveFileStore(SavePath);
        if (store.Load(out _, out var error) == PlayerSaveLoadStatus.Failed) { Failed?.Invoke(error); return; }
        StartCoroutine(Load(store.LoadedProgress != null ? store.LoadedProgress.currentScene : villageScene, false));
    }
    public void TravelToLake() { if (!IsLoading) StartCoroutine(Load(lakeScene, false, "Lake")); }
    public void TravelToVillage() { if (!IsLoading) StartCoroutine(Load(villageScene, false, "Lake")); }
    public void NewGame() { if (!IsLoading) StartCoroutine(Load(villageScene, true)); }
    public void ReturnToMenu() { if (!IsLoading) StartCoroutine(Load(menuScene, false)); }
    private IEnumerator Load(string target, bool fresh, string arrival = null)
    {
        if (!Application.CanStreamedLevelBeLoaded(target))
        {
            Failed?.Invoke("Không tìm thấy màn chơi: " + target);
            yield break;
        }
        IsLoading = true;
        // Freeze gameplay while changing profiles and releasing the old persistent PlayerData.
        GamePause.Set(this, true);
        var progress = PlayerProgress.Current;
        var previousProgress = progress != null ? progress.Export() : null;
        if (arrival != null && progress != null) progress.SetDestination(target, arrival);
        bool saved = PlayerInventory.Instance == null || PlayerInventory.Instance.GetComponent<PlayerSaveController>().SaveNow();
        if (!saved)
        {
            if (progress != null && previousProgress != null) { progress.Initialize(previousProgress); progress.Notify(); }
            Abort("Không lưu được tiến trình. Hãy kiểm tra dung lượng và quyền ghi."); yield break;
        }
        if (PlayerInventory.Instance != null)
        {
            Destroy(PlayerInventory.Instance.gameObject);
            yield return null;
        }
        if (fresh)
        {
            string error = null;
            try
            {
                string directory = Path.GetDirectoryName(SavePath);
                if (Directory.Exists(directory)) Directory.Move(directory,
                    directory + "_backup_" + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss") + "_" + Guid.NewGuid().ToString("N").Substring(0, 6));
            }
            catch (Exception exception) { error = exception.Message; }
            if (error != null) { Debug.LogError(error, this); Abort("Không tạo được hồ sơ mới. Tiến trình cũ vẫn được giữ."); yield break; }
        }
        GameSettings.Save();
        var operation = SceneManager.LoadSceneAsync(target);
        if (operation == null) { Abort("Không tải được màn chơi."); yield break; }
        while (!operation.isDone) yield return null;
        IsLoading = false;
    }
    private void Abort(string message)
    {
        IsLoading = false;
        GamePause.Set(this, false);
        Failed?.Invoke(message);
    }
    public void Quit()
    {
        if (IsLoading) return;
        if (PlayerInventory.Instance != null && !PlayerInventory.Instance.GetComponent<PlayerSaveController>().SaveNow())
        { Failed?.Invoke("Không lưu được tiến trình. Chưa thoát game."); return; }
        GameSettings.Save();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    private void OnDisable() { if (IsLoading) GamePause.Set(this, false); }
}
