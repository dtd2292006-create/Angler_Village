using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
namespace AnglerVillage.UI
{
    [DefaultExecutionOrder(-300)]
    public sealed class InventoryWindowInput : MonoBehaviour
    {
        [SerializeField] private FishInventoryView view;
        private void Update()
        {
            if (!Application.isFocused || Time.timeScale <= 0) return;
#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            if (keyboard == null) return;
            bool toggle = keyboard.iKey.wasPressedThisFrame;
            bool close = keyboard.escapeKey.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
            bool toggle = Input.GetKeyDown(KeyCode.I);
            bool close = Input.GetKeyDown(KeyCode.Escape);
#else
            bool toggle = false, close = false;
#endif
            if (close && view.IsOpen) view.Close();
            else if (toggle) view.Toggle();
        }
    }
}
