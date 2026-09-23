using TMPro;
using UnityEngine;

public class WorldInteractionIndicator : MonoBehaviour
{
    public enum PositionMode
    {
        Fixed,
        Orbit
    }

    [Header("References")]
    [SerializeField] private GameObject indicator;
    [SerializeField] private TMP_Text interactionText;

    [Header("Position")]
    [SerializeField] private PositionMode positionMode = PositionMode.Fixed;

    [Header("Orbit")]
    [SerializeField] private Transform orbitCenter;
    [SerializeField] private float orbitRadius = 1.5f;
    [SerializeField] private float orbitHeight = 2f;

    [Header("Billboard")]
    [SerializeField] private bool faceCamera = true;

    private Player targetPlayer;
    private Camera targetCamera;

    private void Awake()
    {
        if (indicator != null)
        {
            indicator.SetActive(false);
        }
    }

    private void LateUpdate()
    {
        if (indicator == null || !indicator.activeSelf) return;

        if (positionMode == PositionMode.Orbit)
        {
            UpdateOrbitPosition();
        }

        if (faceCamera)
        {
            UpdateBillboard();
        }
    }

    public void Show(string message, Player player)
    {
        targetPlayer = player;

        if (interactionText != null)
        {
            interactionText.text = message;
        }

        if (positionMode == PositionMode.Orbit)
        {
            UpdateOrbitPosition();
        }

        if (indicator != null)
        {
            indicator.SetActive(true);
        }
    }

    public void Hide()
    {
        targetPlayer = null;

        if (indicator != null)
        {
            indicator.SetActive(false);
        }
    }

    private void UpdateOrbitPosition()
    {
        if (targetPlayer == null || orbitCenter == null) return;

        Vector3 direction = targetPlayer.transform.position - orbitCenter.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f) return;

        direction.Normalize();

        transform.position =
            orbitCenter.position +
            direction * orbitRadius +
            Vector3.up * orbitHeight;
    }

    private void UpdateBillboard()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (targetCamera == null) return;

        Vector3 direction = transform.position - targetCamera.transform.position;

        if (direction.sqrMagnitude <= 0.001f) return;

        transform.rotation = Quaternion.LookRotation(direction);
    }
}