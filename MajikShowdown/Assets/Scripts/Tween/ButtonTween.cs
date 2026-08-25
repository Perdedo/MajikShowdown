using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonTween : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Hover")]
    [SerializeField] private float hoverScale = 1.1f;
    [SerializeField] private float animationDuration = 0.15f;
    [SerializeField] private Ease animationEase = Ease.OutBack;
    private Vector3 originalScale;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.DOKill();
        transform.DOScale(originalScale * hoverScale, animationDuration).SetEase(animationEase).SetUpdate(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.DOKill();
        transform.DOScale(originalScale, animationDuration).SetEase(animationEase).SetUpdate(true);
    }

    private void OnDisable()
    {
        transform.DOKill();
        transform.localScale = originalScale;
    }
}