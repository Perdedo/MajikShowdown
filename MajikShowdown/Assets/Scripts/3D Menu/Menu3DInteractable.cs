using DG.Tweening;
using UnityEngine;

public class Menu3DInteractable : MonoBehaviour
{
    [Header("Material")]
    [SerializeField] private Renderer shardRenderer;

    [Header("Hover")]
    [SerializeField, ColorUsage(false, true)] private Color hoverEmissionColor = new Color(0f, 3f, 4f, 1f);
    [SerializeField] private float hoverDuration = 0.2f;

    private Material materialInstance;
    private Color defaultEmissionColor;
    private Tween emissionTween;

    private void Awake()
    {
        if (shardRenderer != null)
        {
            materialInstance = shardRenderer.material;
            defaultEmissionColor = materialInstance.GetColor("_EmissionColor");
            materialInstance.EnableKeyword("_EMISSION");
        }
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
    }
}