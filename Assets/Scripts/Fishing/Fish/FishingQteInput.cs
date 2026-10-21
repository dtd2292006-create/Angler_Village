using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[DefaultExecutionOrder(-50)]
[DisallowMultipleComponent]
[RequireComponent(typeof(FishingQteController))]
public class FishingQteInput : MonoBehaviour
{
    private FishingQteController qte;
    private int observedRound = -1;
    private bool armed;

    private void Awake()
    {
        qte = GetComponent<FishingQteController>();
    }

    private void Update()
    {
        if (!Application.isFocused || Time.timeScale == 0f) return;
        if (!qte.IsActive)
        {
            observedRound = -1;
            armed = false;
            return;
        }

        bool held;
        bool pressed;
        ReadMouse(out held, out pressed);
        if (observedRound != qte.RoundId)
        {
            observedRound = qte.RoundId;
            armed = !held;
        }
        if (!held) armed = true;
        if (!pressed || !armed) return;

        armed = false;
        qte.Submit();
    }

    private static void ReadMouse(out bool held, out bool pressed)
    {
#if ENABLE_INPUT_SYSTEM
        held = Mouse.current != null && Mouse.current.leftButton.isPressed;
        pressed = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
        held = Input.GetMouseButton(0);
        pressed = Input.GetMouseButtonDown(0);
#else
        held = false;
        pressed = false;
#endif
    }
}
