using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
public static class UiEscape
{
    private static int consumedFrame = -1;
    public static bool Pressed
    {
        get
        {
            if (consumedFrame == Time.frameCount || !Application.isFocused) return false;
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyDown(KeyCode.Escape);
#else
            return false;
#endif
        }
    }
    public static void Consume() => consumedFrame = Time.frameCount;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reset() => consumedFrame = -1;
}
