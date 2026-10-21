using System;
using AnglerVillage.UI;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.UI;

[CustomEditor(typeof(FigmaPanelGraphic))]
public sealed class FigmaPanelGraphicInspector : GraphicEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("border"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("light"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("shade"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("selected"));
        serializedObject.ApplyModifiedProperties();
    }
}

public static class InventoryUiTools
{
    [MenuItem("Tools/Fishing/Validate Fish Inventory UI")]
    private static void Validate()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogWarning("Stop Play before validating inventory prefab assets.");
            return;
        }
        try
        {
            var root = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/FishInventoryUI.prefab");
            var cell = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/FishInventorySlot.prefab");
            Require(root != null && cell != null, "Both inventory prefabs must import successfully.");
            var view = root.GetComponent<FishInventoryView>();
            Require(view != null, "Canvas prefab is missing FishInventoryView.");
            var fields = new SerializedObject(view);
            foreach (string field in new[] { "window", "openButton", "closeButton", "scroll", "content",
                "slotPrefab", "countLabel", "emptyLabel", "detailImage", "detailName", "detailText" })
                Require(fields.FindProperty(field).objectReferenceValue != null, "Missing UI reference: " + field);
            Require(!((GameObject)fields.FindProperty("window").objectReferenceValue).activeSelf,
                "Window must start closed.");
            foreach (GameObject prefab in new[] { root, cell })
            {
                foreach (var graphic in prefab.GetComponentsInChildren<Graphic>(true))
                    Require(graphic.GetComponent<CanvasRenderer>() != null, graphic.name + " lacks CanvasRenderer.");
                foreach (var text in prefab.GetComponentsInChildren<Text>(true))
                    Require(text.font != null, text.name + " is missing its font.");
                foreach (var button in prefab.GetComponentsInChildren<Button>(true))
                    Require(button.navigation.mode == Navigation.Mode.None, "UI navigation must not consume A/D.");
                foreach (var transform in prefab.GetComponentsInChildren<Transform>(true))
                    Require(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject) == 0,
                        "Missing script on " + transform.name);
            }
            CheckInputOwners();
            Debug.Log("Inventory UI asset and input ownership checks passed. Play-test the scene next.", root);
        }
        catch (Exception error) { Debug.LogException(error); }
    }

    private static void CheckInputOwners()
    {
        var first = new GameObject("InventoryInputCheckA") { hideFlags = HideFlags.HideAndDontSave };
        var second = new GameObject("InventoryInputCheckB") { hideFlags = HideFlags.HideAndDontSave };
        try
        {
            GameInputBlock.Set(first, true);
            GameInputBlock.Set(second, true);
            GameInputBlock.Set(first, false);
            Require(GameInputBlock.IsBlocked, "Closing one modal must not release another modal's input block.");
            UnityEngine.Object.DestroyImmediate(second);
            Require(!GameInputBlock.IsBlocked, "Destroyed modal must not leave an input block behind.");
        }
        finally
        {
            GameInputBlock.Set(first, false);
            GameInputBlock.Set(second, false);
            if (first != null) UnityEngine.Object.DestroyImmediate(first);
            if (second != null) UnityEngine.Object.DestroyImmediate(second);
        }
    }
    private static void Require(bool valid, string message)
    {
        if (!valid) throw new InvalidOperationException(message);
    }
}
