using System;
using AnglerVillage.UI;
using UnityEngine;
using UnityEngine.UI;

public sealed class JourneyListRowView : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private FigmaPanelGraphic frame;
    [SerializeField] private Image icon;
    [SerializeField] private Text title;
    [SerializeField] private Text subtitle;
    private Action selected;
    private void Awake()
    {
        foreach (var text in GetComponentsInChildren<Text>(true)) InventoryUIFont.Ensure(text);
        UIButtonAudio.Ensure(button.gameObject);
        button.onClick.AddListener(() => selected?.Invoke());
    }
    public void SetSelected(bool value) { if (frame != null) frame.SetSelected(value); }
    public void Bind(string name, string detail, Sprite sprite, Action callback)
    { SetSelected(false); title.text = name; subtitle.text = detail; icon.sprite = sprite; icon.enabled = sprite != null; selected = callback; }
}
