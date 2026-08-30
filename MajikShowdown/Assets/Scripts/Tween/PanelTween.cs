using System;
using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(RectTransform))]
public class PanelTween : MonoBehaviour
{
    public enum PanelAnimationType
    {
        Scale,
        SlideFromTop
    }

    [Header("Animation")]
    [SerializeField] private PanelAnimationType animationType;
    [SerializeField] private float animationDuration = 0.6f;
    [SerializeField] private Ease showEase = Ease.OutBack;
    [SerializeField] private Ease hideEase = Ease.InBack;

    [Header("Scale")]
    [SerializeField] private float initialScale = 0.1f;

    [Header("Slide")]
    [SerializeField] private float slideDistance = 1200f;

    private RectTransform rectTransform;
    private Vector3 originalScale;
    private Vector2 originalPosition;
    private Tween currentTween;

    private bool initialized;
    private bool isHiding;

    private void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (initialized) return;
        rectTransform = GetComponent<RectTransform>();
        originalScale = rectTransform.localScale;
        originalPosition = rectTransform.anchoredPosition;
        initialized = true;
    }

    public void Show()
    {
        Initialize();
        isHiding = false;
        currentTween?.Kill();
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }
        switch (animationType)
        {
            case PanelAnimationType.Scale:
                ShowWithScale();
                break;

            case PanelAnimationType.SlideFromTop:
                ShowFromTop();
                break;
        }
    }

    public void Hide(Action onComplete = null)
    {
        Initialize();
        if (isHiding || !gameObject.activeSelf) return;
        isHiding = true;
        currentTween?.Kill();
        switch (animationType)
        {
            case PanelAnimationType.Scale:
                HideWithScale(onComplete);
                break;

            case PanelAnimationType.SlideFromTop:
                HideToTop(onComplete);
                break;
        }
    }

    private void ShowWithScale()
    {
        rectTransform.anchoredPosition = originalPosition;
        rectTransform.localScale = originalScale * initialScale;
        currentTween = rectTransform.DOScale(originalScale, animationDuration).SetEase(showEase).SetUpdate(true);
    }

    private void HideWithScale(Action onComplete)
    {
        currentTween = rectTransform.DOScale(originalScale * initialScale, animationDuration).SetEase(hideEase).SetUpdate(true).OnComplete(() => DisablePanel(onComplete));
    }


    private void ShowFromTop()
    {
        rectTransform.localScale = originalScale;
        Vector2 hiddenPosition = originalPosition + Vector2.up * slideDistance;
        rectTransform.anchoredPosition = hiddenPosition;
        currentTween = rectTransform.DOAnchorPos(originalPosition, animationDuration).SetEase(showEase).SetUpdate(true);
    }

    private void HideToTop(Action onComplete)
    {
        Vector2 hiddenPosition = originalPosition + Vector2.up * slideDistance;
        currentTween = rectTransform.DOAnchorPos(hiddenPosition, animationDuration).SetEase(hideEase).SetUpdate(true).OnComplete(() => DisablePanel(onComplete));
    }

    private void DisablePanel(Action onComplete)
    {
        isHiding = false;
        gameObject.SetActive(false);
        onComplete?.Invoke();
    }

    private void OnDestroy()
    {
        currentTween?.Kill();
    }
}