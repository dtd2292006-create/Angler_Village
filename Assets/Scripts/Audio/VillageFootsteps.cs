using UnityEngine;
public sealed class VillageFootsteps : MonoBehaviour
{
    [SerializeField] private PlayerMovement movement;
    private float timer;
    private void Update()
    {
        if (Time.timeScale <= 0) return;
        if (!movement.IsWalking) { timer = 0; return; }
        timer -= Time.deltaTime;
        if (timer <= 0) { timer = 0.35f; GameAudio.Instance?.Play(AudioCue.Step); }
    }
}
