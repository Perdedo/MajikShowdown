using System;
using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class NodeTween : MonoBehaviour
{
    [Header("Slide")]
    [SerializeField] private float slideDuration = 0.25f;
    [SerializeField] private Ease slideEase = Ease.OutCubic;

    private RectTransform rectTransform;
    private Tween movementTween;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void SlideFrom(Vector3 startWorldPosition)
    {
        movementTween?.Kill();
        Vector3 targetWorldPosition = rectTransform.position;
        rectTransform.position = startWorldPosition;
        movementTween = rectTransform.DOMove(targetWorldPosition, slideDuration).SetEase(slideEase).SetUpdate(true);
    }

    public void SlideTo(Vector2 targetAnchoredPosition, Action onComplete = null)
    {
        movementTween?.Kill();
        movementTween = rectTransform.DOAnchorPos(targetAnchoredPosition, slideDuration).SetEase(slideEase).SetUpdate(true).OnComplete(() => onComplete?.Invoke());
    }

    public void Stop()
    {
        movementTween?.Kill();
    }

    private void OnDestroy()
    {
        movementTween?.Kill();
    }
}