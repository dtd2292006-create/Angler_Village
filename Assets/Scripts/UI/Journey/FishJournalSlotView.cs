using System;
using AnglerVillage.UI;
using UnityEngine;
using UnityEngine.UI;

public sealed class FishJournalSlotView : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Image icon;
    [SerializeField] private Text unknown;
    [SerializeField] private FigmaPanelGraphic frame;
    private Action selected;
    private void Awake()
    {
        InventoryUIFont.Ensure(unknown);
        UIButtonAudio.Ensure(button.gameObject);
        button.onClick.AddListener(() => selected?.Invoke());
    }
    public void Bind(Sprite sprite, Action callback)
    {
        icon.sprite = sprite; icon.enabled = sprite != null;
        unknown.gameObject.SetActive(sprite == null);
        selected = callback; SetSelected(false);
    }
    public void SetSelected(bool value) => frame.SetSelected(value);
}
