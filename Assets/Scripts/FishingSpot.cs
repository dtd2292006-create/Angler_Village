using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class FishingSpot : MonoBehaviour
{
    [Header("Standing Position")]
    [SerializeField] private Transform standPoint;
    [SerializeField] private bool faceRight = true;

    [Tooltip("Khoang cach ngang toi StandPoint de bat dau cau.")]
    [SerializeField, Min(0.001f)]
    private float arrivalTolerance = 0.03f;

    public Transform StandPoint => standPoint;
    public bool FaceRight => faceRight;

    private PlayerModeController currentPlayer;
    private float previousPlayerX;

    // Sau khi da kich hoat, phai roi hoan toan khoi vung
    // moi duoc kich hoat lai.
    private bool activationAttempted;

    private readonly HashSet<Collider2D> contacts =
        new HashSet<Collider2D>();

    private void Reset()
    {
        GetComponent<BoxCollider2D>().isTrigger = true;
    }

    private void Awake()
    {
        BoxCollider2D trigger = GetComponent<BoxCollider2D>();

        if (!trigger.isTrigger || standPoint == null)
        {
            Debug.LogError(
                "FishingSpot: Bat Is Trigger va gan StandPoint.",
                this);

            enabled = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActiveAndEnabled)
            return;

        PlayerModeController player =
            other.GetComponentInParent<PlayerModeController>();

        if (player == null || !player.isActiveAndEnabled)
            return;

        if (currentPlayer != null && currentPlayer != player)
            return;

        bool firstContact = contacts.Count == 0;

        currentPlayer = player;
        contacts.Add(other);

        if (firstContact)
        {
            previousPlayerX = player.transform.position.x;
            activationAttempted = false;
        }

        // Chi ghi nhan vao vung, chua chuyen Prepare.
    }

    private void Update()
    {
        if (Time.timeScale == 0f ||
            currentPlayer == null ||
            !currentPlayer.isActiveAndEnabled ||
            activationAttempted ||
            contacts.Count == 0)
        {
            return;
        }

        float currentX = currentPlayer.transform.position.x;
        float targetX = standPoint.position.x;

        bool closeEnough =
            Mathf.Abs(currentX - targetX) <= arrivalTolerance;

        // Bat ca truong hop Player di qua diem giua hai frame,
        // tranh bo lo StandPoint khi toc do di chuyen cao.
        bool crossedPoint =
            (previousPlayerX < targetX && currentX >= targetX) ||
            (previousPlayerX > targetX && currentX <= targetX);

        previousPlayerX = currentX;

        if (!closeEnough && !crossedPoint)
            return;

        if (currentPlayer.IsFishing)
            return;

        activationAttempted = true;
        currentPlayer.TryEnterFishing(this);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!contacts.Remove(other) || contacts.Count > 0)
            return;

        ReleasePlayer();
    }

    private void ReleasePlayer()
    {
        if (currentPlayer != null &&
            currentPlayer.CurrentSpot == this)
        {
            currentPlayer.ExitFishing();
        }

        contacts.Clear();
        currentPlayer = null;
        activationAttempted = false;
    }

    private void OnDisable()
    {
        ReleasePlayer();
    }
}