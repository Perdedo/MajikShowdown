using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

public class Menu3DController : MonoBehaviour
{
    private enum MenuState
    {
        WakingUp,
        WaitingForSwitch,
        Activating,
        Ready,
        ShardSelected
    }

    [Header("Cameras")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private GameObject introCamera;
    [SerializeField] private GameObject defaultCamera;
    [SerializeField] private GameObject selectedCamera;

    [Header("Wake Up")]
    [SerializeField] private Transform introCameraTransform;
    [SerializeField] private float startXRotation;
    [SerializeField] private float raiseHeadDuration;
    [SerializeField] private float lookLeftAngle;
    [SerializeField] private float lookRightAngle;
    [SerializeField] private float lookDuration;
    [SerializeField] private float lookHoldDuration;
    [SerializeField] private float centerDuration;
    [SerializeField] private float shakeMaxAngle;
    [SerializeField] private float shakeStartDuration;

    [Header("Wake Up Visual")]
    [SerializeField] private Material wakeUpMaterial;
    [SerializeField, Range(0f, 0.005f)] private float startBlurStrength = 0.005f;
    [SerializeField, Range(0f, 1f)] private float startDarkness = 0.95f;
    [SerializeField, Range(0f, 0.005f)] private float raiseBlurStrength = 0.0035f;
    [SerializeField, Range(0f, 1f)] private float raiseDarkness = 0.7f;
    [SerializeField, Range(0f, 0.005f)] private float lookBlurStrength = 0.002f;
    [SerializeField, Range(0f, 1f)] private float lookDarkness = 0.4f;
    [SerializeField, Range(0f, 0.005f)] private float centerBlurStrength = 0.001f;
    [SerializeField, Range(0f, 1f)] private float centerDarkness = 0.15f;

    [Header("Switch")]
    [SerializeField] private Transform menuSwitch;
    [SerializeField] private Renderer switchRenderer;
    [SerializeField] private float switchActiveZRotation = -15f;
    [SerializeField] private float switchDuration = 0.12f;
    [SerializeField, ColorUsage(false, true)] private Color switchOffEmissionColor = new Color(3f, 0f, 0f, 1f);
    [SerializeField, ColorUsage(false, true)] private Color switchOnEmissionColor = new Color(0f, 3f, 0f, 1f);

    [Header("Shards")]
    [SerializeField] private Menu3DInteractable[] shards;
    [SerializeField] private float shardCameraDelay = 0.6f;

    [Header("Detection")]
    [SerializeField] private LayerMask menuLayer;
    [SerializeField] private float rayDistance = 100f;
    [SerializeField] private float mouseMovementThreshold = 1f;

    [Header("Movement")]
    [SerializeField] private float cameraMovementDuration = 0.35f;

    private static readonly int BlurStrengthID = Shader.PropertyToID("_BlurStrength");
    private static readonly int DarknessID = Shader.PropertyToID("_Darkness");

    private MenuState currentState;
    private Menu3DInteractable hoveredShard;
    private Menu3DInteractable selectedShard;
    private Material switchMaterialInstance;
    private Vector2 lastMousePosition;
    private Vector3 introInitialRotation;
    private float selectedCameraZ;
    private int activatedShardCount;
    private int totalActiveShards;
    private Tween movementTween;
    private Tween switchEmissionTween;
    private Sequence wakeUpSequence;
    private Sequence switchSequence;

    private void Awake()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (introCameraTransform != null)
        {
            introInitialRotation = introCameraTransform.localEulerAngles;
            introCameraTransform.localEulerAngles = new Vector3(startXRotation, introInitialRotation.y, introInitialRotation.z);
        }

        if (wakeUpMaterial != null)
        {
            wakeUpMaterial.SetFloat(BlurStrengthID, startBlurStrength);
            wakeUpMaterial.SetFloat(DarknessID, startDarkness);
        }

        if (switchRenderer != null)
        {
            switchMaterialInstance = switchRenderer.material;
            switchMaterialInstance.EnableKeyword("_EMISSION");
            switchMaterialInstance.SetColor("_EmissionColor", switchOffEmissionColor);
        }

        if (selectedCamera != null)
        {
            selectedCameraZ = selectedCamera.transform.position.z;
            selectedCamera.SetActive(false);
        }

        if (defaultCamera != null)
        {
            defaultCamera.SetActive(false);
        }

        if (introCamera != null)
        {
            introCamera.SetActive(true);
        }

        currentState = MenuState.WakingUp;
    }

    private void Start()
    {
        StartWakeUpSequence();
    }

    private void Update()
    {
        if (Mouse.current == null || mainCamera == null) return;

        Vector2 mousePosition = Mouse.current.position.ReadValue();

        if (currentState == MenuState.WaitingForSwitch)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                HandleSwitchClick(mousePosition);
            }

            return;
        }

        if (currentState != MenuState.Ready && currentState != MenuState.ShardSelected) return;

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

    private void StartWakeUpSequence()
    {
        if (introCameraTransform == null)
        {
            FinishWakeUp();
            return;
        }

        wakeUpSequence?.Kill();

        Vector3 leftRotation = introInitialRotation + new Vector3(0f, lookLeftAngle, 0f);
        Vector3 rightRotation = introInitialRotation + new Vector3(0f, lookRightAngle, 0f);

        wakeUpSequence = DOTween.Sequence();

        wakeUpSequence.Append(introCameraTransform.DOLocalRotate(introInitialRotation, raiseHeadDuration, RotateMode.Fast).SetEase(Ease.OutSine));
        AppendVisualTransition(raiseBlurStrength, raiseDarkness, raiseHeadDuration);

        wakeUpSequence.AppendInterval(0.2f);

        wakeUpSequence.Append(introCameraTransform.DOLocalRotate(leftRotation, lookDuration, RotateMode.Fast).SetEase(Ease.InOutSine));
        AppendVisualTransition(lookBlurStrength, lookDarkness, lookDuration);

        wakeUpSequence.AppendInterval(lookHoldDuration);

        wakeUpSequence.Append(introCameraTransform.DOLocalRotate(rightRotation, lookDuration * 1.2f, RotateMode.Fast).SetEase(Ease.InOutSine));
        AppendVisualTransition(centerBlurStrength, centerDarkness, lookDuration * 1.2f);

        wakeUpSequence.AppendInterval(lookHoldDuration);

        wakeUpSequence.Append(introCameraTransform.DOLocalRotate(introInitialRotation, centerDuration, RotateMode.Fast).SetEase(Ease.OutSine));
        wakeUpSequence.AppendInterval(0.1f);

        AppendWakeUpShake();

        wakeUpSequence.OnComplete(FinishWakeUp);
    }

    private void AppendVisualTransition(float blurTarget, float darknessTarget, float duration)
    {
        if (wakeUpMaterial == null) return;

        wakeUpSequence.Join(DOTween.To(() => wakeUpMaterial.GetFloat(BlurStrengthID), value => wakeUpMaterial.SetFloat(BlurStrengthID, value), blurTarget, duration).SetEase(Ease.InOutSine));
        wakeUpSequence.Join(DOTween.To(() => wakeUpMaterial.GetFloat(DarknessID), value => wakeUpMaterial.SetFloat(DarknessID, value), darknessTarget, duration).SetEase(Ease.InOutSine));
    }

    private void AppendWakeUpShake()
    {
        float[] strengths = { 0.2f, 0.4f, 0.6f, 0.8f, 1f };
        float[] speedMultipliers = { 1f, 0.9f, 0.8f, 0.7f, 0.6f };

        Sequence shakeSequence = DOTween.Sequence();

        for (int i = 0; i < strengths.Length; i++)
        {
            float angle = shakeMaxAngle * strengths[i];
            float duration = shakeStartDuration * speedMultipliers[i];
            Vector3 leftRotation = introInitialRotation + new Vector3(0f, -angle, -angle * 0.15f);
            Vector3 rightRotation = introInitialRotation + new Vector3(0f, angle, angle * 0.15f);

            shakeSequence.Append(introCameraTransform.DOLocalRotate(leftRotation, duration, RotateMode.Fast).SetEase(Ease.InOutSine));
            shakeSequence.Append(introCameraTransform.DOLocalRotate(rightRotation, duration, RotateMode.Fast).SetEase(Ease.InOutSine));
        }

        shakeSequence.Append(introCameraTransform.DOLocalRotate(introInitialRotation, 0.08f, RotateMode.Fast).SetEase(Ease.OutSine));

        float shakeDuration = shakeSequence.Duration();

        wakeUpSequence.Append(shakeSequence);

        if (wakeUpMaterial != null)
        {
            wakeUpSequence.Join(DOTween.To(() => wakeUpMaterial.GetFloat(BlurStrengthID), value => wakeUpMaterial.SetFloat(BlurStrengthID, value), 0f, shakeDuration).SetEase(Ease.InQuad));
            wakeUpSequence.Join(DOTween.To(() => wakeUpMaterial.GetFloat(DarknessID), value => wakeUpMaterial.SetFloat(DarknessID, value), 0f, shakeDuration).SetEase(Ease.InQuad));
        }
    }

    private void FinishWakeUp()
    {
        if (introCameraTransform != null)
        {
            introCameraTransform.localEulerAngles = introInitialRotation;
        }

        if (wakeUpMaterial != null)
        {
            wakeUpMaterial.SetFloat(BlurStrengthID, 0f);
            wakeUpMaterial.SetFloat(DarknessID, 0f);
        }

        currentState = MenuState.WaitingForSwitch;
    }

    private void HandleSwitchClick(Vector2 mousePosition)
    {
        if (menuSwitch == null) return;

        Ray ray = mainCamera.ScreenPointToRay(mousePosition);

        if (!Physics.Raycast(ray, out RaycastHit hit, rayDistance, menuLayer)) return;
        if (hit.transform != menuSwitch && !hit.transform.IsChildOf(menuSwitch)) return;

        ActivateSwitch();
    }

    private void ActivateSwitch()
    {
        if (currentState != MenuState.WaitingForSwitch) return;
        if (menuSwitch == null) return;

        currentState = MenuState.Activating;

        switchSequence?.Kill();
        switchEmissionTween?.Kill();

        Vector3 targetRotation = menuSwitch.localEulerAngles;
        targetRotation.z = switchActiveZRotation;

        switchSequence = DOTween.Sequence();
        switchSequence.Append(menuSwitch.DOLocalRotate(targetRotation, switchDuration, RotateMode.Fast).SetEase(Ease.OutQuad));

        if (switchMaterialInstance != null)
        {
            switchEmissionTween = switchMaterialInstance.DOColor(switchOnEmissionColor, "_EmissionColor", switchDuration).SetEase(Ease.OutQuad);
            switchSequence.Join(switchEmissionTween);
        }

        switchSequence.AppendCallback(ActivateShards);
        switchSequence.AppendInterval(shardCameraDelay);
        switchSequence.OnComplete(ActivateDefaultCamera);
    }

    private void ActivateShards()
    {
        activatedShardCount = 0;
        totalActiveShards = 0;

        foreach (Menu3DInteractable shard in shards)
        {
            if (shard == null) continue;

            totalActiveShards++;

            shard.OnActivationFinished -= HandleShardActivationFinished;
            shard.OnActivationFinished += HandleShardActivationFinished;
            shard.Activate();
        }

        if (totalActiveShards == 0)
        {
            EnableShardInteraction();
        }
    }

    private void HandleShardActivationFinished(Menu3DInteractable shard)
    {
        shard.OnActivationFinished -= HandleShardActivationFinished;

        activatedShardCount++;

        if (activatedShardCount < totalActiveShards) return;

        EnableShardInteraction();
    }

    private void EnableShardInteraction()
    {
        currentState = MenuState.Ready;

        if (Mouse.current != null)
        {
            lastMousePosition = Mouse.current.position.ReadValue();
            UpdateHover(lastMousePosition);
        }
    }

    private void ActivateDefaultCamera()
    {
        if (introCamera != null)
        {
            introCamera.SetActive(false);
        }

        if (defaultCamera != null)
        {
            defaultCamera.SetActive(true);
        }
    }

    private void UpdateHover(Vector2 mousePosition)
    {
        Menu3DInteractable detectedShard = RaycastShard(mousePosition);

        if (detectedShard == hoveredShard) return;

        if (hoveredShard != null)
        {
            hoveredShard.SetHovered(false);
        }

        hoveredShard = detectedShard;

        if (hoveredShard != null)
        {
            hoveredShard.SetHovered(true);
        }
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

        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, menuLayer))
        {
            return hit.collider.GetComponentInParent<Menu3DInteractable>();
        }

        return null;
    }

    private void SelectShard(Menu3DInteractable shard)
    {
        if (shard == null || selectedCamera == null) return;

        Vector3 shardPosition = shard.transform.position;
        Vector3 destination = new Vector3(shardPosition.x, shardPosition.y + 0.15f, selectedCameraZ);

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
        currentState = MenuState.ShardSelected;
    }

    private void ReturnToDefaultCamera()
    {
        selectedShard = null;
        movementTween?.Kill();

        if (selectedCamera != null)
        {
            selectedCamera.SetActive(false);
        }

        currentState = MenuState.Ready;
    }

    private void OnDestroy()
    {
        movementTween?.Kill();
        switchEmissionTween?.Kill();
        wakeUpSequence?.Kill();
        switchSequence?.Kill();

        foreach (Menu3DInteractable shard in shards)
        {
            if (shard == null) continue;

            shard.OnActivationFinished -= HandleShardActivationFinished;
        }

        if (hoveredShard != null)
        {
            hoveredShard.SetHovered(false);
        }
    }
}