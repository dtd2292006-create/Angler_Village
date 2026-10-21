using UnityEngine;
using UnityEngine.EventSystems;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class PlayerInputReader : MonoBehaviour
{
    private bool waitForMouseRelease;

    private void Update()
    {
        if (AnglerVillage.UI.GameInputBlock.IsBlocked) waitForMouseRelease = true;
        else if (!RawMouseHeld) waitForMouseRelease = false;
    }

    private bool CanReadMouse => CanRead && !waitForMouseRelease;
    private static bool RawMouseHeld
    {
        get
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current != null && Mouse.current.leftButton.isPressed;
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetMouseButton(0);
#else
            return false;
#endif
        }
    }

    private bool CanRead =>
        isActiveAndEnabled &&
        !AnglerVillage.UI.GameInputBlock.IsBlocked &&
        Time.timeScale > 0f &&
        Application.isFocused;

    public float MoveAxis
    {
        get
        {
            if (!CanRead)
                return 0f;

#if ENABLE_INPUT_SYSTEM
            Keyboard keyboard = Keyboard.current;

            if (keyboard == null)
                return 0f;

            bool left = keyboard.aKey.isPressed ||
                        keyboard.leftArrowKey.isPressed;

            bool right = keyboard.dKey.isPressed ||
                         keyboard.rightArrowKey.isPressed;

            return (right ? 1f : 0f) - (left ? 1f : 0f);

#elif ENABLE_LEGACY_INPUT_MANAGER
            bool left = Input.GetKey(KeyCode.A) ||
                        Input.GetKey(KeyCode.LeftArrow);

            bool right = Input.GetKey(KeyCode.D) ||
                         Input.GetKey(KeyCode.RightArrow);

            return (right ? 1f : 0f) - (left ? 1f : 0f);
#else
            return 0f;
#endif
        }
    }

    public bool CastPressed
    {
        get
        {
            if (!CanReadMouse)
                return false;

            // Bấm vào UI không bắt đầu quăng cần.
            if (EventSystem.current != null &&
                EventSystem.current.IsPointerOverGameObject())
            {
                return false;
            }

#if ENABLE_INPUT_SYSTEM
            return Mouse.current != null &&
                   Mouse.current.leftButton.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetMouseButtonDown(0);
#else
            return false;
#endif
        }
    }

    public bool CastHeld
    {
        get
        {
            if (!CanReadMouse)
                return false;

#if ENABLE_INPUT_SYSTEM
            return Mouse.current != null &&
                   Mouse.current.leftButton.isPressed;
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetMouseButton(0);
#else
            return false;
#endif
        }
    }
}
