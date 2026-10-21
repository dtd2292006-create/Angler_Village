using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class FishingQteSetupWindow : EditorWindow
{
    private HookController hook;
    private Canvas canvas;
    private FishingLineDefinition line;

    [MenuItem("Tools/Fishing/Setup Resistance and QTE")]
    private static void Open()
    {
        GetWindow<FishingQteSetupWindow>("Fishing QTE Setup");
    }

    private void OnGUI()
    {
        EditorGUILayout.HelpBox("Assign the scene Hook, HUD Canvas and line asset. This disables the old LineTensionController and creates/reuses the two HUD panels. Existing movement and Animator are untouched.", MessageType.Info);
        hook = (HookController)EditorGUILayout.ObjectField("Scene Hook", hook, typeof(HookController), true);
        canvas = (Canvas)EditorGUILayout.ObjectField("HUD Canvas", canvas, typeof(Canvas), true);
        line = (FishingLineDefinition)EditorGUILayout.ObjectField("Line Asset", line, typeof(FishingLineDefinition), false);
        bool valid = hook != null && canvas != null && line != null &&
            !EditorApplication.isPlaying && !EditorUtility.IsPersistent(hook) &&
            !EditorUtility.IsPersistent(canvas);
        using (new EditorGUI.DisabledScope(!valid))
        {
            if (GUILayout.Button("Set up Hook and HUD")) Setup();
        }
    }

    private void Setup()
    {
        Undo.IncrementCurrentGroup();
        int undoGroup = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Set up fishing QTE");

        // Editor-only migration: old source may or may not still be in the project.
        foreach (MonoBehaviour component in hook.GetComponents<MonoBehaviour>())
        {
            if (component == null || component.GetType().Name != "LineTensionController") continue;
            Undo.RecordObject(component, "Disable old line tension");
            component.enabled = false;
            PrefabUtility.RecordPrefabInstancePropertyModifications(component);
        }

        FishResistanceController resistance = EnsureComponent<FishResistanceController>(hook.gameObject);
        FishingQteController qte = EnsureComponent<FishingQteController>(hook.gameObject);
        EnsureComponent<FishingQteInput>(hook.gameObject);
        SetReference(resistance, "lineDefinition", line);

        FishingTensionView resistanceView = null;
        foreach (FishingTensionView candidate in Object.FindObjectsByType<FishingTensionView>(
            FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (candidate.transform.IsChildOf(canvas.transform))
            {
                resistanceView = candidate;
                break;
            }
        }
        if (resistanceView == null) resistanceView = CreateResistanceView();
        Undo.RecordObject(resistanceView, "Enable resistance HUD");
        resistanceView.enabled = true;
        SetReference(resistanceView, "source", resistance);

        Transform oldPanel = canvas.transform.Find("FishingQtePanel");
        GameObject panel = oldPanel != null ? oldPanel.gameObject :
            MakeUI("FishingQtePanel", canvas.transform, new Vector2(240f, 240f),
                new Vector2(0.5f, 0.5f), new Vector2(0f, -80f));
        EnsureComponent<CanvasGroup>(panel);
        FishingQteView view = EnsureComponent<FishingQteView>(panel);
        FishingQteRingGraphic ring = panel.GetComponentInChildren<FishingQteRingGraphic>(true);
        if (ring == null)
        {
            GameObject graphic = MakeUI("Ring", panel.transform, new Vector2(220f, 220f),
                new Vector2(0.5f, 0.5f), Vector2.zero);
            ring = Undo.AddComponent<FishingQteRingGraphic>(graphic);
        }
        SetReference(view, "source", qte);
        SetReference(view, "ring", ring);
        Undo.RecordObject(ring, "Configure QTE graphic");
        ring.raycastTarget = false;
        EditorUtility.SetDirty(ring);

        EditorSceneManager.MarkSceneDirty(hook.gameObject.scene);
        EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
        Undo.CollapseUndoOperations(undoGroup);
        Selection.activeGameObject = panel;
        Debug.Log("Fishing QTE setup complete. Save the scene, then run Tools > Fishing > Run Fight Rule Checks before Play.", hook);
    }

    private FishingTensionView CreateResistanceView()
    {
        GameObject panel = MakeUI("FishingResistancePanel", canvas.transform,
            new Vector2(260f, 24f), new Vector2(0.5f, 1f), new Vector2(0f, -60f));
        EnsureComponent<CanvasGroup>(panel);
        FishingTensionView view = Undo.AddComponent<FishingTensionView>(panel);

        GameObject sliderObject = MakeUI("ResistanceSlider", panel.transform,
            new Vector2(260f, 24f), new Vector2(0.5f, 0.5f), Vector2.zero);
        Image background = Undo.AddComponent<Image>(sliderObject);
        background.color = new Color(0.12f, 0.15f, 0.18f, 1f);
        background.raycastTarget = false;

        GameObject fillArea = MakeUI("Fill Area", sliderObject.transform,
            Vector2.zero, new Vector2(0.5f, 0.5f), Vector2.zero);
        RectTransform areaRect = fillArea.GetComponent<RectTransform>();
        areaRect.anchorMin = Vector2.zero;
        areaRect.anchorMax = Vector2.one;
        areaRect.offsetMin = new Vector2(2f, 2f);
        areaRect.offsetMax = new Vector2(-2f, -2f);
        GameObject fillObject = MakeUI("Fill", fillArea.transform,
            Vector2.zero, new Vector2(0.5f, 0.5f), Vector2.zero);
        RectTransform fillRect = fillObject.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        Image fill = Undo.AddComponent<Image>(fillObject);
        fill.color = Color.green;
        fill.raycastTarget = false;

        Slider slider = Undo.AddComponent<Slider>(sliderObject);
        slider.fillRect = fillRect;
        slider.targetGraphic = background;
        slider.direction = Slider.Direction.LeftToRight;
        slider.interactable = false;
        slider.transition = Selectable.Transition.None;
        SetReference(view, "slider", slider);
        SetReference(view, "fillImage", fill);
        return view;
    }

    private static T EnsureComponent<T>(GameObject target) where T : Component
    {
        T existing = target.GetComponent<T>();
        T component = existing != null ? existing : Undo.AddComponent<T>(target);
        Behaviour behaviour = component as Behaviour;
        if (behaviour != null && !behaviour.enabled)
        {
            Undo.RecordObject(behaviour, "Enable fishing component");
            behaviour.enabled = true;
        }
        return component;
    }

    private static void SetReference(Object target, string propertyName, Object value)
    {
        SerializedObject serialized = new SerializedObject(target);
        SerializedProperty property = serialized.FindProperty(propertyName);
        if (property == null) throw new System.InvalidOperationException("Missing property: " + propertyName);
        property.objectReferenceValue = value;
        serialized.ApplyModifiedProperties();
    }

    private static GameObject MakeUI(string name, Transform parent, Vector2 size,
        Vector2 anchor, Vector2 position)
    {
        GameObject target = new GameObject(name, typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(target, "Create fishing HUD");
        target.transform.SetParent(parent, false);
        RectTransform rect = target.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, anchor.y == 1f ? 1f : 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
        return target;
    }
}
