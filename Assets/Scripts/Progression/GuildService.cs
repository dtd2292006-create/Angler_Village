using System.Collections.Generic;
using UnityEngine;

public sealed class GuildService : MonoBehaviour
{
    [SerializeField] private VillageContent content;
    public VillageContent Content => content;
    private PlayerProgress Progress => PlayerProgress.Current;
    private PlayerInventory Inventory => PlayerInventory.Instance;
    private double Now => Inventory.GetComponent<PlayerWorldClock>().ElapsedMinutes;
    public CommissionDefinition ActiveDefinition => Progress != null && Progress.Commission != null ? content.Find(Progress.Commission.definitionId) : null;
    private void Update() => RefreshDeadline();
    public void RefreshDeadline()
    {
        if (Progress == null || !Progress.IsReady) return;
        var receipt = Progress.Commission;
        if (receipt != null && !receipt.failed && (content.Find(receipt.definitionId) == null || Now >= receipt.deadlineMinutes)) Progress.FailCommission();
    }
    public bool Enroll(out string message)
    {
        message = null;
        if (Progress == null || !Progress.IsReady) { message = "Hồ sơ chưa sẵn sàng."; return false; }
        if (Progress.Certified) { message = "Bạn đã có chứng nhận cần thủ."; return false; }
        if (content.firstCommission == null) { message = "Chưa cấu hình ủy thác đầu tiên."; return false; }
        using (Inventory.GetComponent<PlayerSaveController>().DeferWrites())
        {
            Progress.Certify();
            AcceptInternal(content.firstCommission);
            Progress.Notify();
        }
        return true;
    }
    public bool Accept(CommissionDefinition definition, out string message)
    {
        message = null; RefreshDeadline();
        if (Progress == null || !Progress.Certified || definition == null || content.Find(definition.id) != definition)
        { message = "Hãy nhận chứng nhận cần thủ trước."; return false; }
        var current = Progress.Commission;
        if (current != null && !current.failed) { message = "Hãy hoàn thành hoặc hủy ủy thác hiện tại trước."; return false; }
        if (!Progress.FirstDelivery && definition != content.firstCommission)
        { message = "Hãy hoàn thành chuyến câu đầu tiên trước."; return false; }
        if (Progress.Level < definition.minimumLevel) { message = "Chưa đủ cấp cần thủ."; return false; }
        AcceptInternal(definition); Progress.Notify(); return true;
    }
    private void AcceptInternal(CommissionDefinition definition)
    {
        Progress.SetCommission(new CommissionSaveData { definitionId = definition.id,
            acceptedSequence = Inventory.LastCatchSequence, deadlineMinutes = Now + Mathf.Max(1, definition.durationGameMinutes) });
    }
    public List<CaughtFishInstance> EligibleFish()
    {
        var result = new List<CaughtFishInstance>(); var definition = ActiveDefinition; var receipt = Progress != null ? Progress.Commission : null;
        if (definition == null || receipt == null || receipt.failed) return result;
        foreach (var fish in Inventory.Fish)
        {
            if (definition.species != null && fish.SpeciesId != definition.species.SpeciesId) continue;
            if (definition.requiresFreshCatch && (fish.CatchSequence <= receipt.acceptedSequence)) continue;
            result.Add(fish);
            if (result.Count >= definition.quantity) break;
        }
        return result;
    }
    public bool TurnIn(out string message)
    {
        message = null; RefreshDeadline();
        var definition = ActiveDefinition;
        if (definition == null || Progress.Commission.failed) { message = "Ủy thác không còn hiệu lực."; return false; }
        var fish = EligibleFish();
        if (fish.Count < definition.quantity) { message = "Chưa đủ cá phù hợp. Chỉ tính cá câu sau lúc nhận ủy thác."; return false; }
        if ((long)Progress.Coins + definition.rewardCoins > int.MaxValue || (long)Progress.TotalXp + definition.rewardXp > int.MaxValue)
        { message = "Đã đạt giới hạn tiền hoặc kinh nghiệm."; return false; }
        using (Inventory.GetComponent<PlayerSaveController>().DeferWrites())
        {
            foreach (var item in fish) Inventory.TryRemoveFish(item.InstanceId);
            Progress.Reward(definition.rewardCoins, definition.rewardXp);
            Progress.Deliver(); Progress.Notify();
        }
        message = "Hoàn thành! +" + definition.rewardCoins + " xu · +" + definition.rewardXp + " XP";
        return true;
    }
    public void Abandon() { if (Progress != null && Progress.Commission != null) Progress.FailCommission(); }
    public string Summary()
    {
        RefreshDeadline();
        var receipt = Progress != null ? Progress.Commission : null; var definition = ActiveDefinition;
        if (receipt == null) return "Chưa nhận ủy thác. Đến guild để chọn việc.";
        if (definition == null) return "Không tìm thấy ủy thác cũ. Nhờ guild cấp lại.";
        if (receipt.failed) return "Ủy thác đã thất bại. Cá vẫn trong túi. Về guild để nhận lại.";
        int minutes = Mathf.Max(0, Mathf.CeilToInt((float)(receipt.deadlineMinutes - Now)));
        return definition.title + "\n" + EligibleFish().Count + "/" + definition.quantity + " cá · còn " + minutes / 60 + "h " + minutes % 60 + "p";
    }
}
