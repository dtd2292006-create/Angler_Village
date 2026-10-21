using System;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// Put this file in Assets/.../Editor/. Reads scene state; does not edit the HUD.
public static class FishingQteDiagnostics
{
    private static double deadline;
    private static int firstActiveFrame = -1;

    [MenuItem("Tools/Fishing/Capture Next QTE UI Report")]
    public static void Arm()
    {
        Stop();
        if (!EditorApplication.isPlaying)
        {
            Debug.LogWarning("Enter Play Mode, then run Capture Next QTE UI Report.");
            return;
        }
        deadline = EditorApplication.timeSinceStartup + 30d;
        firstActiveFrame = -1;
        EditorApplication.update += WaitForQte;
        Debug.Log("QTE UI report armed for 30 seconds. Keep playing until a QTE starts; a report will appear automatically.");
    }

    [MenuItem("Tools/Fishing/Cancel QTE UI Capture")]
    public static void Stop()
    {
        EditorApplication.update -= WaitForQte;
    }

    private static void WaitForQte()
    {
        if (!EditorApplication.isPlaying || EditorApplication.timeSinceStartup > deadline)
        {
            Stop();
            Debug.LogWarning("QTE UI capture stopped: no active round captured. Run it again while a fish is hooked.");
            return;
        }

        bool active = false;
        foreach (FishingQteController controller in UnityEngine.Object.FindObjectsByType<FishingQteController>(
            FindObjectsInactive.Include, FindObjectsSortMode.None))
            active |= controller != null && controller.isActiveAndEnabled && controller.IsActive;

        if (!active)
        {
            firstActiveFrame = -1;
            return;
        }
        if (firstActiveFrame < 0)
        {
            firstActiveFrame = Time.frameCount;
            return;
        }
        // Allow the view's LateUpdate to run before reading alpha and geometry.
        if (Time.frameCount <= firstActiveFrame) return;
        Stop();
        try { Report(); }
        catch (Exception error) { Debug.LogException(error); }
    }

    private static void Report()
    {
        Canvas.ForceUpdateCanvases();
        StringBuilder text = new StringBuilder("=== QTE UI REPORT ===\n");
        text.AppendLine("Frame=" + Time.frameCount + ", Game screen=" + Screen.width + "x" + Screen.height);
        foreach (FishingQteController controller in UnityEngine.Object.FindObjectsByType<FishingQteController>(
            FindObjectsInactive.Include, FindObjectsSortMode.None))
            text.AppendLine("Controller: " + Path(controller.transform) +
                " | enabled=" + controller.isActiveAndEnabled + " activeRound=" + controller.IsActive +
                " hasFight=" + controller.HasFight + " progress=" + controller.Progress.ToString("F3"));

        FishingQteView[] views = UnityEngine.Object.FindObjectsByType<FishingQteView>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (views.Length == 0) text.AppendLine("ERROR: No FishingQteView exists. The panel name alone does not create a working view.");
        foreach (FishingQteView view in views) DescribeView(text, view);
        text.AppendLine("=== END QTE UI REPORT ===");
        Debug.Log(text.ToString());
    }

    private static void DescribeView(StringBuilder text, FishingQteView view)
    {
        text.AppendLine("\nView: " + Path(view.transform));
        text.AppendLine("View enabled=" + view.enabled + " activeInHierarchy=" + view.gameObject.activeInHierarchy);
        SerializedObject fields = new SerializedObject(view);
        FishingQteController source = fields.FindProperty("source").objectReferenceValue as FishingQteController;
        FishingQteRingGraphic ring = fields.FindProperty("ring").objectReferenceValue as FishingQteRingGraphic;
        text.AppendLine("Bound source=" + (source != null ? Path(source.transform) : "MISSING") +
            " | source activeRound=" + (source != null && source.IsActive));
        if (ring == null)
        {
            text.AppendLine("ERROR: Ring reference is missing.");
            return;
        }

        text.AppendLine("Ring: " + Path(ring.transform) + " enabled=" + ring.enabled +
            " activeInHierarchy=" + ring.gameObject.activeInHierarchy);
        RectTransform rect = ring.rectTransform;
        text.AppendLine("Ring local rect=" + rect.rect + ", world scale=" + rect.lossyScale);
        if (rect.rect.width <= 1f || rect.rect.height <= 1f)
            text.AppendLine("ERROR: Ring rect is zero or too small to draw.");
        if (Mathf.Abs(rect.lossyScale.x) < 0.0001f || Mathf.Abs(rect.lossyScale.y) < 0.0001f)
            text.AppendLine("ERROR: A transform scale collapses the ring.");

        Canvas canvas = ring.canvas;
        if (canvas == null)
        {
            text.AppendLine("ERROR: Ring has no active Canvas.");
            return;
        }
        Canvas root = canvas.rootCanvas;
        Camera camera = root.renderMode == RenderMode.ScreenSpaceOverlay ? null : root.worldCamera;
        text.AppendLine("Canvas=" + Path(canvas.transform) + " root=" + Path(root.transform) +
            " mode=" + root.renderMode + " enabled=" + root.isActiveAndEnabled +
            " sortingOrder=" + root.sortingOrder + " display=" + root.targetDisplay);
        text.AppendLine("Camera=" + (camera != null ? camera.name : "NONE (normal for Overlay)"));
        if (root.renderMode == RenderMode.ScreenSpaceCamera && camera == null)
            text.AppendLine("WARNING: Screen Space Camera Canvas has no assigned camera.");
        if (root.targetDisplay != 0) text.AppendLine("WARNING: Canvas targets another display; check the Game view display selector.");

        Rect screenRect = ScreenRect(rect, camera);
        text.AppendLine("Ring screen rect=" + screenRect);
        if (root.renderMode != RenderMode.WorldSpace && root.targetDisplay == 0 &&
            !screenRect.Overlaps(new Rect(0f, 0f, Screen.width, Screen.height)))
            text.AppendLine("ERROR: Ring is outside the Game screen.");
        if (camera != null)
        {
            Vector3 point = camera.WorldToScreenPoint(rect.TransformPoint(rect.rect.center));
            text.AppendLine("Ring camera depth=" + point.z.ToString("F3") +
                ", clip=" + camera.nearClipPlane + ".." + camera.farClipPlane);
        }

        CanvasRenderer renderer = ring.canvasRenderer;
        Mesh mesh = renderer.GetMesh();
        text.AppendLine("Graphic alpha=" + ring.color.a.ToString("F3") +
            " renderer alpha=" + renderer.GetAlpha().ToString("F3") +
            " inherited alpha=" + renderer.GetInheritedAlpha().ToString("F3"));
        text.AppendLine("Renderer cull=" + renderer.cull + " rectClipping=" + renderer.hasRectClipping +
            " mesh vertices=" + (mesh != null ? mesh.vertexCount : 0));
        if (mesh == null || mesh.vertexCount == 0)
            text.AppendLine("ERROR: Renderer currently has no ring geometry.");
        if (ring.color.a <= 0.001f || renderer.GetAlpha() <= 0.001f || renderer.GetInheritedAlpha() <= 0.001f)
            text.AppendLine("ERROR: An alpha value makes the ring transparent.");
        if (renderer.cull) text.AppendLine("WARNING: CanvasRenderer is currently culling the ring.");
        Material material = ring.materialForRendering;
        text.AppendLine("Material=" + (material != null ? material.name : "NONE") +
            " shader=" + (material != null && material.shader != null ? material.shader.name : "NONE"));

        for (Transform parent = ring.transform; parent != null; parent = parent.parent)
        {
            foreach (CanvasGroup group in parent.GetComponents<CanvasGroup>())
                text.AppendLine("CanvasGroup " + Path(parent) + ": enabled=" + group.enabled +
                    " alpha=" + group.alpha + " ignoreParents=" + group.ignoreParentGroups);
            RectMask2D rectMask = parent.GetComponent<RectMask2D>();
            if (rectMask != null && rectMask.isActiveAndEnabled)
            {
                Rect clip = ScreenRect(rectMask.rectTransform, camera);
                text.AppendLine("ACTIVE RectMask2D: " + Path(parent) + " screenRect=" + clip);
                if (!clip.Overlaps(screenRect)) text.AppendLine("ERROR: Ring is completely outside this rectangular mask.");
            }
            Mask mask = parent.GetComponent<Mask>();
            if (mask != null && mask.isActiveAndEnabled)
                text.AppendLine("ACTIVE Mask: " + Path(parent) + " (may clip the ring)");
        }
    }

    private static Rect ScreenRect(RectTransform transform, Camera camera)
    {
        Vector3[] corners = new Vector3[4];
        transform.GetWorldCorners(corners);
        Vector2 min = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
        Vector2 max = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
        foreach (Vector3 corner in corners)
        {
            Vector2 point = RectTransformUtility.WorldToScreenPoint(camera, corner);
            min = Vector2.Min(min, point);
            max = Vector2.Max(max, point);
        }
        return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
    }

    private static string Path(Transform transform)
    {
        string result = transform.name;
        while (transform.parent != null)
        {
            transform = transform.parent;
            result = transform.name + "/" + result;
        }
        return result;
    }
}
