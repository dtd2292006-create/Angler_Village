using UnityEngine;
using UnityEngine.UI;

namespace AnglerVillage.UI
{
    // Resolve the engine resource by name rather than assuming a serialized built-in file ID.
    public static class InventoryUIFont
    {
        public static bool Ensure(Text text)
        {
            if (text == null || text.font != null) return false;
            Font fallback = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (fallback == null)
            {
                Debug.LogError("Inventory UI: Unity could not load LegacyRuntime.ttf.", text);
                return false;
            }
            text.font = fallback;
            return true;
        }
    }
}
