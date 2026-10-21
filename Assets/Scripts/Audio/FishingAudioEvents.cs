using UnityEngine;

public sealed class FishingAudioEvents : MonoBehaviour
{
    [SerializeField] private HookController hook;
    [SerializeField] private HookCatchController catchController;
    [SerializeField] private FishingQteController qte;
    [SerializeField] private PlayerMovement movement;
    private bool deployed;
    private bool underwater;
    private int qteRound;
    private float stepTimer;
    private void OnEnable()
    {
        catchController.AttachedFishChanged += Bite;
        catchController.FishLanded += Landed;
        hook.LineBroken += Broken;
        qte.Resolved += Resolved;
    }
    private void Update()
    {
        if (GameAudio.Instance == null) return;
        if (Time.timeScale <= 0) return;
        bool now = hook.IsDeployed;
        if (now && !deployed) GameAudio.Instance.Play(AudioCue.Cast);
        if (now && hook.IsUnderwater && !underwater) GameAudio.Instance.Play(AudioCue.Splash);
        deployed = now;
        underwater = now && hook.IsUnderwater;
        GameAudio.Instance.SetLoop(AudioCue.Reel, now && hook.IsReeling);
        if (qte.IsActive && qte.RoundId != qteRound)
        { qteRound = qte.RoundId; GameAudio.Instance.Play(AudioCue.QteAppear); }
        if (movement != null && movement.IsWalking)
        {
            stepTimer -= Time.deltaTime;
            if (stepTimer <= 0) { stepTimer = 0.35f; GameAudio.Instance.Play(AudioCue.Step); }
        }
        else stepTimer = 0;
    }
    private void Bite(FishCatchable fish) { if (fish != null) GameAudio.Instance?.Play(AudioCue.Bite); }
    private void Landed(FishCatchable fish) => GameAudio.Instance?.Play(AudioCue.FishLanded);
    private void Broken() => GameAudio.Instance?.Play(AudioCue.LineBreak);
    private void Resolved(FishingQteResult result) => GameAudio.Instance?.Play(
        result == FishingQteResult.Great ? AudioCue.Great : result == FishingQteResult.Good ? AudioCue.Good : AudioCue.Fail);
    private void OnDisable()
    {
        catchController.AttachedFishChanged -= Bite;
        catchController.FishLanded -= Landed;
        hook.LineBroken -= Broken;
        qte.Resolved -= Resolved;
        GameAudio.Instance?.SetLoop(AudioCue.Reel, false);
    }
}
