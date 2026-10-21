using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
[RequireComponent(typeof(Animator))]
public sealed class PlayerAnimationEvent : MonoBehaviour
{
    [Header("Cast Events")]
    [SerializeField]
    private UnityEvent castReleased = new UnityEvent();

    [SerializeField]
    private UnityEvent castFinished = new UnityEvent();

    public UnityEvent CastReleased => castReleased;
    public UnityEvent CastFinished => castFinished;

    // Animation Event tại frame thứ 4 gọi hàm này.
    public void OnCastRelease()
    {
        castReleased.Invoke();
    }

    // Animation Event ở cuối clip gọi hàm này.
    public void OnCastFinished()
    {
        castFinished.Invoke();
    }
}