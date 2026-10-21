using AnglerVillage.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class JourneyHudView : MonoBehaviour
{
    [SerializeField] private GuildService guild;
    [SerializeField] private GameObject hud;
    [SerializeField] private GameObject rankPanel;
    [SerializeField] private Text rankLabel;
    [SerializeField] private Slider experience;
    [SerializeField] private Text objective;
    [SerializeField] private GameObject objectivePanel;
    private PlayerFishingController fishing;
    [SerializeField] private Text coins;
    private float refresh;
    private void Awake() { foreach (var text in GetComponentsInChildren<Text>(true)) InventoryUIFont.Ensure(text); }
    private void Start() => fishing = FindFirstObjectByType<PlayerFishingController>();
    private void Update()
    {
        hud.SetActive(!GameInputBlock.IsBlocked);
        if (objectivePanel != null) objectivePanel.SetActive(fishing == null || !fishing.IsCasting);
        refresh -= Time.unscaledDeltaTime; if (refresh > 0) return; refresh = 0.25f;
        var progress = PlayerProgress.Current; if (progress == null || !progress.IsReady) return;
        rankPanel.SetActive(progress.Certified);
        rankLabel.text = "CẦN THỦ · CẤP " + progress.Level + "\n" + progress.XpInLevel + "/" + PlayerProgress.LevelCost(progress.Level) + " XP";
        experience.SetValueWithoutNotify(progress.Level >= 100 ? 1 : (float)progress.XpInLevel / PlayerProgress.LevelCost(progress.Level));
        coins.text = progress.Coins + " XU";
        bool lake = SceneManager.GetActiveScene().name == "Lake";
        objective.text = !progress.Certified ? "VIỆC CẦN LÀM\nĐến guild gặp cô tiếp tân [E]." :
            !progress.StarterKitClaimed ? "VIỆC CẦN LÀM\nQua cửa hàng bên phải, nhận dây và mồi miễn phí." :
            !progress.LakeVisited ? "CHUYẾN CÂU ĐẦU\nĐi tiếp sang phải đến biển dẫn ra hồ." :
            guild.Summary() + (lake ? "\nĐi về mép trái hồ để trở về làng." : "\n[E] gặp tiếp tân để nhận / giao việc.");
    }
}
