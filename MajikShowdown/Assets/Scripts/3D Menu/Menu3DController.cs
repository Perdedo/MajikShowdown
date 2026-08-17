using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

public class Menu3DController : MonoBehaviour
{
    [Header("Cameras")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private GameObject defaultCamera;
    [SerializeField] private GameObject selectedCamera;

    [Header("Detection")]
    [SerializeField] private LayerMask shardLayer;
    [SerializeField] private float rayDistance = 100f;
    [SerializeField] private float mouseMovementThreshold = 1f;

    [Header("Movement")]
    [SerializeField] private float cameraMovementDuration = 0.35f;

    private Menu3DInteractable hoveredShard;
    private Menu3DInteractable selectedShard;
    private Vector2 lastMousePosition;
    private float selectedCameraZ;
    private Tween movementTween;

    private void Awake()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        if (selectedCamera != null)
        {
            selectedCameraZ = selectedCamera.transform.position.z;
            selectedCamera.SetActive(false);
        }

        if (defaultCamera != null) defaultCamera.SetActive(true);
    }

    private void Start()
    {
        if (Mouse.current == null) return;
        lastMousePosition = Mouse.current.position.ReadValue();
        UpdateHover(lastMousePosition);
    }

    private void Update()
    {
        if (Mouse.current == null || mainCamera == null) return;

        Vector2 mousePosition = Mouse.current.position.ReadValue();

        if (Vector2.Distance(mousePosition, lastMousePosition) >= mouseMovementThreshold)
        {
            lastMousePosition = mousePosition;
            UpdateHover(mousePosition);
        }

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            HandleClick(mousePosition);
        }
    }

    private void UpdateHover(Vector2 mousePosition)
    {
        Menu3DInteractable detectedShard = RaycastShard(mousePosition);

        if (detectedShard == hoveredShard) return;

        if (hoveredShard != null) hoveredShard.SetHovered(false);

        hoveredShard = detectedShard;

        if (hoveredShard != null) hoveredShard.SetHovered(true);
    }

    private void HandleClick(Vector2 mousePosition)
    {
        Menu3DInteractable clickedShard = RaycastShard(mousePosition);

        if (clickedShard == null)
        {
            ReturnToDefaultCamera();
            return;
        }

        SelectShard(clickedShard);
    }

    private Menu3DInteractable RaycastShard(Vector2 mousePosition)
    {
        Ray ray = mainCamera.ScreenPointToRay(mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, shardLayer))
        {
            return hit.collider.GetComponentInParent<Menu3DInteractable>();
        }

        return null;
    }

    private void SelectShard(Menu3DInteractable shard)
    {
        if (shard == null || selectedCamera == null) return;

        Vector3 shardPosition = shard.transform.position;
        Vector3 destination = new Vector3(shardPosition.x, shardPosition.y, selectedCameraZ);

        if (!selectedCamera.activeSelf)
        {
            movementTween?.Kill();
            selectedCamera.transform.position = destination;
            selectedCamera.SetActive(true);
        }
        else
        {
            movementTween?.Kill();
            movementTween = selectedCamera.transform.DOMove(destination, cameraMovementDuration).SetEase(Ease.InOutSine);
        }

        selectedShard = shard;
    }

    private void ReturnToDefaultCamera()
    {
        selectedShard = null;
        movementTween?.Kill();

        if (selectedCamera != null) selectedCamera.SetActive(false);
    }

    private void OnDestroy()
    {
        movementTween?.Kill();

        if (hoveredShard != null) hoveredShard.SetHovered(false);
    }
}