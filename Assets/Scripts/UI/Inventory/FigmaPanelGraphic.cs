using UnityEngine;
using UnityEngine.UI;

namespace AnglerVillage.UI
{
    // Native uGUI translation of Figma Panel/Content (103:75) and slot borders.
    // Fixed 3 px bevel and 6 px stepped corners, center stretches without distortion.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class FigmaPanelGraphic : MaskableGraphic
    {
        [SerializeField] private Color border = new Color32(48, 41, 35, 255);
        [SerializeField] private Color light = new Color32(234, 211, 161, 255);
        [SerializeField] private Color shade = new Color32(41, 58, 48, 255);
        [SerializeField] private bool selected;
        public void SetSelected(bool value)
        {
            if (selected == value) return;
            selected = value;
            SetVerticesDirty();
        }
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            Rect r = GetPixelAdjustedRect();
            float w = r.width, h = r.height;
            if (w < 12 || h < 12) return;
            Color edge = selected ? new Color32(212, 179, 123, 255) : border;
            Quad(vh, r, 6, 6, w - 12, h - 12, color);
            Quad(vh, r, 6, 0, w - 12, 3, edge);
            Quad(vh, r, 6, h - 3, w - 12, 3, edge);
            Quad(vh, r, 0, 6, 3, h - 12, edge);
            Quad(vh, r, w - 3, 6, 3, h - 12, edge);
            foreach (float x in new[] { 3f, w - 6 })
                foreach (float y in new[] { 3f, h - 6 }) Quad(vh, r, x, y, 3, 3, edge);
            Quad(vh, r, 6, 3, w - 12, 3, light);
            Quad(vh, r, 3, 6, 3, h - 12, light);
            Quad(vh, r, 6, h - 6, w - 12, 3, shade);
            Quad(vh, r, w - 6, 6, 3, h - 12, shade);
            if (!selected) return;
            Color cream = new Color32(240, 227, 189, 255);
            foreach (float x in new[] { 6f, w - 30 })
                foreach (float y in new[] { 6f, h - 12 }) Quad(vh, r, x, y, 24, 6, cream);
            foreach (float x in new[] { 6f, w - 12 })
                foreach (float y in new[] { 6f, h - 30 }) Quad(vh, r, x, y, 6, 24, cream);
        }
        private static void Quad(VertexHelper vh, Rect r, float x, float y, float w, float h, Color c)
        {
            int n = vh.currentVertCount;
            float left = r.xMin + x, top = r.yMax - y;
            vh.AddVert(new Vector3(left, top - h, 0), c, Vector2.zero);
            vh.AddVert(new Vector3(left, top, 0), c, Vector2.zero);
            vh.AddVert(new Vector3(left + w, top, 0), c, Vector2.zero);
            vh.AddVert(new Vector3(left + w, top - h, 0), c, Vector2.zero);
            vh.AddTriangle(n, n + 1, n + 2);
            vh.AddTriangle(n + 2, n + 3, n);
        }
    }
}
