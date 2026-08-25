using System;
using DG.Tweening;
using UnityEngine;

public class Menu3DInteractable : MonoBehaviour
{
    [Header("Material")]
    [SerializeField] private Renderer shardRenderer;

    [Header("Hover")]
    [SerializeField, ColorUsage(false, true)] private Color hoverEmissionColor = new Color(0f, 3f, 4f, 1f);
    [SerializeField] private float hoverDuration = 0.2f;

    [Header("Activation")]
    [SerializeField] private float startYPosition;
    [SerializeField] private float riseDuration = 0.7f;
    [SerializeField] private float flickerDuration = 0.08f;
    [SerializeField] private int flickerCount = 5;

    [Header("Idle Animation")]
    [SerializeField] private float floatHeight = 0.2f;
    [SerializeField] private float floatDuration = 1.5f;
    [SerializeField] private float rotationAmount = 5f;
    [SerializeField] private float rotationDuration = 2f;
    [SerializeField] private float randomDelay = 0.5f;

    public event Action<Menu3DInteractable> OnActivationFinished;

    private Material materialInstance;
    private Color defaultEmissionColor;
    private Tween emissionTween;
    private Tween floatTween;
    private Tween rotationTween;
    private Sequence activationSequence;
    private Vector3 initialLocalPosition;
    private Vector3 activeLocalPosition;
    private Vector3 initialLocalRotation;
    private bool activated;

    private void Awake()
    {
        initialLocalPosition = transform.localPosition;
        activeLocalPosition = initialLocalPosition;
        initialLocalRotation = transform.localEulerAngles;

        if (shardRenderer != null)
        {
            materialInstance = shardRenderer.material;
            defaultEmissionColor = materialInstance.GetColor("_EmissionColor");
            materialInstance.EnableKeyword("_EMISSION");
            materialInstance.SetColor("_EmissionColor", Color.black);
        }

        transform.localPosition = new Vector3(initialLocalPosition.x, startYPosition, initialLocalPosition.z);
        transform.localEulerAngles = Vector3.zero;
    }

    public void Activate()
    {
        if (materialInstance == null) return;
        if (activated) return;

        activated = true;

        activationSequence?.Kill();

        activationSequence = DOTween.Sequence();

        for (int i = 0; i < flickerCount; i++)
        {
            activationSequence.Append(materialInstance.DOColor(defaultEmissionColor, "_EmissionColor", flickerDuration).SetEase(Ease.Linear));
            activationSequence.Append(materialInstance.DOColor(Color.black, "_EmissionColor", flickerDuration).SetEase(Ease.Linear));
        }

        activationSequence.Append(materialInstance.DOColor(defaultEmissionColor, "_EmissionColor", flickerDuration).SetEase(Ease.Linear));
        activationSequence.Join(transform.DOLocalMove(activeLocalPosition, riseDuration).SetEase(Ease.OutCubic));
        activationSequence.Join(transform.DOLocalRotate(initialLocalRotation, riseDuration, RotateMode.Fast).SetEase(Ease.OutCubic));

        activationSequence.OnComplete(() =>
        {
            StartIdleAnimation();
            OnActivationFinished?.Invoke(this);
        });
    }

    private void StartIdleAnimation()
    {
        float delay = UnityEngine.Random.Range(0f, randomDelay);

        floatTween = transform.DOLocalMoveY(activeLocalPosition.y + floatHeight, floatDuration).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).SetDelay(delay);
        rotationTween = transform.DOLocalRotate(initialLocalRotation + new Vector3(0f, rotationAmount, rotationAmount * 0.5f), rotationDuration, RotateMode.Fast).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).SetDelay(delay);
    }

    public void SetHovered(bool hovered)
    {
        if (materialInstance == null) return;

        emissionTween?.Kill();

        Color targetColor = hovered ? hoverEmissionColor : defaultEmissionColor;
        emissionTween = materialInstance.DOColor(targetColor, "_EmissionColor", hoverDuration).SetEase(Ease.OutSine);
    }

    private void OnDestroy()
    {
        emissionTween?.Kill();
        floatTween?.Kill();
        rotationTween?.Kill();
        activationSequence?.Kill();
    }
}