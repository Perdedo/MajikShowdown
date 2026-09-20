using TMPro;
using UnityEngine;

public class DamageIndicator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_Text damageText;

    [Header("Movement")]
    [SerializeField] private float duration = 0.8f;
    [SerializeField] private float horizontalDistance = 1f;
    [SerializeField] private float verticalDistance = 1.5f;
    [SerializeField] private AnimationCurve verticalCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Fade")]
    [SerializeField] private float fadeStart = 0.6f;

    private Vector3 startPosition;
    private Vector3 horizontalDirection;
    private Camera targetCamera;
    private float timer;

    public void Initialize(float damage, Elements element, Transform target)
    {
        targetCamera = Camera.main;

        int side = Random.value < 0.5f ? -1 : 1;

        Vector3 right = targetCamera != null ? targetCamera.transform.right : Vector3.right;
        right.y = 0f;
        right.Normalize();

        startPosition = target.position + Vector3.up * 1.5f + right * side * 0.7f;
        horizontalDirection = right * side;

        transform.position = startPosition;

        damageText.text = Mathf.RoundToInt(damage).ToString();
        damageText.color = GetElementColor(element);

        timer = 0f;
    }

    private void LateUpdate()
    {
        timer += Time.deltaTime;

        float t = Mathf.Clamp01(timer / duration);

        UpdatePosition(t);
        UpdateBillboard();
        UpdateFade(t);

        if (timer >= duration)
        {
            Destroy(gameObject);
        }
    }

    private void UpdatePosition(float t)
    {
        float horizontal = horizontalDistance * t;
        float vertical = verticalCurve.Evaluate(t) * verticalDistance;

        transform.position = startPosition + horizontalDirection * horizontal + Vector3.up * vertical;
    }

    private void UpdateBillboard()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (targetCamera == null) return;

        Vector3 direction = transform.position - targetCamera.transform.position;

        if (direction.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    private void UpdateFade(float t)
    {
        if (t < fadeStart) return;

        float fadeT = Mathf.InverseLerp(fadeStart, 1f, t);

        Color color = damageText.color;
        color.a = 1f - fadeT;
        damageText.color = color;
    }

    private Color GetElementColor(Elements element)
    {
        switch (element)
        {
            case Elements.Fire:
                return new Color(1f, 0.3f, 0.05f);

            case Elements.Ice:
                return new Color(0.3f, 0.8f, 1f);

            case Elements.Earth:
                return new Color(0.5f, 0.3f, 0.12f);

            case Elements.Lightning:
                return new Color(1f, 0.9f, 0.1f);

            case Elements.Radiance:
                return new Color(1f, 0.95f, 0.65f);

            case Elements.Darkness:
                return new Color(0.45f, 0.15f, 0.65f);

            case Elements.Poison:
                return new Color(0.25f, 0.85f, 0.2f);

            case Elements.None:
            default:
                return Color.white;
        }
    }
}