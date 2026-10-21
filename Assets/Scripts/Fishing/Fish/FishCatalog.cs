using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FishCatalog", menuName = "Fishing/Fish Catalog")]
public class FishCatalog : ScriptableObject
{
    [SerializeField] private List<FishDefinition> fishDefinitions = new List<FishDefinition>();
    public IReadOnlyList<FishDefinition> Definitions => fishDefinitions;
    private Dictionary<string, FishDefinition> index;

    public bool TryGetFish(string speciesId, out FishDefinition definition)
    {
        definition = null;
        if (string.IsNullOrEmpty(speciesId)) return false;
        if (index == null) RebuildIndex();
        return index.TryGetValue(speciesId, out definition);
    }

    public void RebuildIndex()
    {
        index = new Dictionary<string, FishDefinition>(StringComparer.Ordinal);
        foreach (FishDefinition definition in fishDefinitions)
        {
            if (definition == null || string.IsNullOrEmpty(definition.SpeciesId)) continue;
            if (index.ContainsKey(definition.SpeciesId))
            {
                Debug.LogError("FishCatalog: duplicate species ID. Refresh the catalog in the Editor.", this);
                continue;
            }
            index.Add(definition.SpeciesId, definition);
        }
    }

    private void OnEnable() { RebuildIndex(); }
    private void OnValidate() { index = null; }
}
