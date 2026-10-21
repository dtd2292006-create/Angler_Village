using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
[DisallowMultipleComponent]
public sealed class UIButtonAudio : MonoBehaviour
{
    private Button button;
    public static void Ensure(GameObject target)
    {
        if (target != null && target.GetComponent<UIButtonAudio>() == null) target.AddComponent<UIButtonAudio>();
    }
    private void Awake() { button = GetComponent<Button>(); button.onClick.AddListener(Play); }
    private void Play() => GameAudio.Instance?.Play(AudioCue.UiClick);
    private void OnDestroy() { if (button != null) button.onClick.RemoveListener(Play); }
}
