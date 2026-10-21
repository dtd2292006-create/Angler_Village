using UnityEngine;

[DisallowMultipleComponent]
public class FishActor : MonoBehaviour
{
    [Header("Species")]
    [SerializeField]
    private FishDefinition definition;

    [Header("Visual")]
    [SerializeField]
    private SpriteRenderer visualRenderer;

    [Tooltip("Bat de lay Sprite tu du lieu loai. " +
             "Tat neu muon tu gan Sprite tren Renderer.")]
    [SerializeField]
    private bool useDefinitionSprite = true;

    public FishDefinition Definition => definition;

    private void Awake()
    {
        ApplyDefinition();
    }

    private void OnValidate()
    {
        ApplyDefinition();
    }

    [ContextMenu("Apply Definition")]
    public void ApplyDefinition()
    {
        if (definition == null || visualRenderer == null)
            return;

        if (useDefinitionSprite)
            visualRenderer.sprite = definition.Sprite;
    }
}