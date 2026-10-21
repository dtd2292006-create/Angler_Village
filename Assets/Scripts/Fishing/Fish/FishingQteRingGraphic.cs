using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class FishingQteRingGraphic : MaskableGraphic
{
    [SerializeField, Min(2f)] private float ringThickness = 12f;
    [SerializeField, Min(1f)] private float cursorThickness = 4f;
    [SerializeField, Range(32, 192)] private int segments = 128;
    [SerializeField] private Color trackColor = new Color(0.18f, 0.22f, 0.26f, 1f);
    [SerializeField] private Color goodColor = Color.white;
    [SerializeField] private Color greatColor = new Color(0.15f, 0.65f, 1f, 1f);
    [SerializeField] private Color cursorColor = new Color(1f, 0.85f, 0.2f, 1f);

    private float progress;
    private float goodStart;
    private float goodWidth;
    private float greatStart;
    private float greatWidth;
    private FishingQteResult result;

    public void ShowRound(float cursor, float goodBegin, float goodSize,
        float greatBegin, float greatSize, FishingQteResult feedback)
    {
        progress = cursor;
        goodStart = goodBegin;
        goodWidth = goodSize;
        greatStart = greatBegin;
        greatWidth = greatSize;
        result = feedback;
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        Rect rect = rectTransform.rect;
        float outer = Mathf.Min(rect.width, rect.height) * 0.43f;
        if (outer <= 0f) return;
        float inner = Mathf.Max(0f, outer - ringThickness);
        Vector2 center = rect.center;
        DrawArc(vh, center, inner, outer, 0f, 1f, trackColor);
        DrawArc(vh, center, inner, outer, goodStart, goodWidth, goodColor);
        DrawArc(vh, center, inner, outer, greatStart, greatWidth, greatColor);

        Color cursor = result == FishingQteResult.Great ? greatColor
            : result == FishingQteResult.Good ? new Color(0.2f, 1f, 0.4f)
            : result == FishingQteResult.Fail ? new Color(1f, 0.2f, 0.2f) : cursorColor;
        float angle = (0.25f - progress) * Mathf.PI * 2f;
        Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        Vector2 side = new Vector2(-direction.y, direction.x) * cursorThickness * 0.5f;
        Vector2 a = center + direction * Mathf.Max(0f, inner - 8f);
        Vector2 b = center + direction * (outer + 8f);
        AddQuad(vh, a - side, a + side, b + side, b - side, cursor);
    }

    private void DrawArc(VertexHelper vh, Vector2 center, float inner, float outer,
        float start, float width, Color tint)
    {
        if (width <= 0f) return;
        int count = Mathf.Max(1, Mathf.CeilToInt(segments * width));
        for (int i = 0; i < count; i++)
        {
            float t0 = start + width * i / count;
            float t1 = start + width * (i + 1) / count;
            float a0 = (0.25f - t0) * Mathf.PI * 2f;
            float a1 = (0.25f - t1) * Mathf.PI * 2f;
            Vector2 d0 = new Vector2(Mathf.Cos(a0), Mathf.Sin(a0));
            Vector2 d1 = new Vector2(Mathf.Cos(a1), Mathf.Sin(a1));
            AddQuad(vh, center + d0 * inner, center + d0 * outer,
                center + d1 * outer, center + d1 * inner, tint);
        }
    }

    private void AddQuad(VertexHelper vh, Vector2 a, Vector2 b, Vector2 c, Vector2 d, Color tint)
    {
        int start = vh.currentVertCount;
        UIVertex vertex = UIVertex.simpleVert;
        vertex.color = tint * color;
        vertex.position = a; vh.AddVert(vertex);
        vertex.position = b; vh.AddVert(vertex);
        vertex.position = c; vh.AddVert(vertex);
        vertex.position = d; vh.AddVert(vertex);
        vh.AddTriangle(start, start + 1, start + 2);
        vh.AddTriangle(start, start + 2, start + 3);
    }
}
