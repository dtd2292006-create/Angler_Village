using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(CanvasGroup))]
public class FishingQteView : MonoBehaviour
{
    [SerializeField] private FishingQteController source;
    [SerializeField] private FishingQteRingGraphic ring;
    [Tooltip("Optional legacy UI Text. Leave empty if using a different text system.")]
    [SerializeField] private Text resultLabel;
    [SerializeField, Min(0f)] private float feedbackSeconds = 0.5f;

    private CanvasGroup group;
    private float feedbackUntil;
    private FishingQteResult feedback;

    private void Awake()
    {
        group = GetComponent<CanvasGroup>();
        group.interactable = false;
        group.blocksRaycasts = false;
        group.alpha = 0f;
        if (source == null || ring == null)
        {
            Debug.LogError("FishingQteView: Assign Source and Ring.", this);
            enabled = false;
            return;
        }
        ring.raycastTarget = false;
    }

    private void OnEnable()
    {
        if (source != null) source.Resolved += HandleResolved;
    }

    private void HandleResolved(FishingQteResult result)
    {
        feedback = result;
        feedbackUntil = Time.time + feedbackSeconds;
    }

    private void LateUpdate()
    {
        bool active = source != null && source.IsActive;
        bool showingFeedback = source != null && source.HasFight && Time.time < feedbackUntil;
        group.alpha = active || showingFeedback ? 1f : 0f;
        if (!active && !showingFeedback) return;

        ring.ShowRound(source.Progress, source.GoodStart, source.GoodWidth,
            source.GreatStart, source.GreatWidth, active ? FishingQteResult.None : feedback);
        if (resultLabel != null)
            resultLabel.text = active ? "Nha chuot roi nhap" : feedback.ToString();
    }

    private void OnDisable()
    {
        if (source != null) source.Resolved -= HandleResolved;
        if (group != null) group.alpha = 0f;
        feedbackUntil = 0f;
    }
}
