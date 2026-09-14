using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RunePickupUI : MonoBehaviour
{
    [Header("Rune Visual")]
    [SerializeField] private RectTransform runeRoot;
    [SerializeField] private Image mainImage;
    [SerializeField] private Image nodeSymbol;
    [SerializeField] private Image borderImage;
    [SerializeField] private SpellNodeInfos info;

    [Header("Travel Glow")]
    [SerializeField] private RectTransform animationContainer;
    [SerializeField] private RectTransform travelGlow;
    [SerializeField] private float travelDuration = 0.4f;
    [SerializeField] private float glowStartScale = 0.6f;
    [SerializeField] private float glowEndScale = 1.2f;

    [Header("Rune Animation")]
    [SerializeField] private float normalScale = 1f;
    [SerializeField] private float pulseScale = 1.12f;
    [SerializeField] private float pulseUpDuration = 0.1f;
    [SerializeField] private float pulseDownDuration = 0.1f;
    [SerializeField] private int pulseCount = 2;
    [SerializeField] private float holdDuration = 0.4f;
    [SerializeField] private float disappearDuration = 0.2f;
    [SerializeField] private float nextRuneDelay = 0.25f;

    private readonly Queue<SpellNode> runeQueue = new Queue<SpellNode>();
    private bool isPlaying;

    private void Awake()
    {
        if (runeRoot != null)
        {
            runeRoot.gameObject.SetActive(false);
        }

        if (travelGlow != null)
        {
            travelGlow.gameObject.SetActive(false);
        }
    }

    public void ShowRune(SpellNode node, Vector3 worldPosition)
    {
        if (node == null)
        {
            return;
        }

        StartCoroutine(TravelAndQueueRune(node, worldPosition));
    }

    private IEnumerator TravelAndQueueRune(SpellNode node, Vector3 worldPosition)
    {
        yield return PlayTravelAnimation(worldPosition);

        runeQueue.Enqueue(node);

        if (!isPlaying)
        {
            StartCoroutine(ProcessQueue());
        }
    }

    private IEnumerator ProcessQueue()
    {
        isPlaying = true;

        while (runeQueue.Count > 0)
        {
            SpellNode node = runeQueue.Dequeue();

            SetupVisual(node);
            yield return PlayRuneAnimation();

            if (runeQueue.Count > 0 && nextRuneDelay > 0f)
            {
                yield return new WaitForSeconds(nextRuneDelay);
            }
        }

        isPlaying = false;
    }

    private void SetupVisual(SpellNode node)
    {
        bool hasVisualInfo = node.spellInfos != null;
        NodeVisualInfo visualInfo = default;

        if (hasVisualInfo)
        {
            visualInfo = node.spellInfos.GetInfo(node.GetCategory());
        }

        if (mainImage != null)
        {
            mainImage.color = hasVisualInfo ? visualInfo.color : node.color;
        }

        bool hasSymbol = node.nodeSymbolSprite != null;

        if (nodeSymbol != null)
        {
            nodeSymbol.gameObject.SetActive(hasSymbol);

            if (hasSymbol)
            {
                nodeSymbol.enabled = true;
                nodeSymbol.sprite = node.nodeSymbolSprite;

                if (hasVisualInfo)
                {
                    nodeSymbol.color = visualInfo.internSymbolColor;
                }
                else
                {
                    Color symbolColor = node.symbolColor;

                    if (symbolColor.a <= 0f)
                    {
                        symbolColor.a = 1f;
                    }

                    nodeSymbol.color = symbolColor;
                }
            }
        }

        SetupBorder(node);
    }

    private void SetupBorder(SpellNode node)
    {
        if (borderImage == null || info == null)
        {
            return;
        }

        borderImage.gameObject.SetActive(true);

        switch (node.GetCategory())
        {
            case NodeCategory.Type:
                borderImage.sprite = info.core.borderSprite;
                break;

            case NodeCategory.Effect:
                borderImage.sprite = info.effect.borderSprite;
                break;

            case NodeCategory.Trajectory:
                borderImage.sprite = info.trajectory.borderSprite;
                break;

            case NodeCategory.Stat:
                borderImage.sprite = info.stat.borderSprite;
                break;

            case NodeCategory.Trigger:
                borderImage.sprite = info.trigger.borderSprite;
                break;

            case NodeCategory.CastingPoint:
                borderImage.sprite = info.castingPoint.borderSprite;
                break;
        }
    }

    private IEnumerator PlayTravelAnimation(Vector3 worldPosition)
    {
        if (animationContainer == null || travelGlow == null || runeRoot == null)
        {
            yield break;
        }

        Camera mainCamera = Camera.main;

        if (mainCamera == null)
        {
            yield break;
        }

        Vector3 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(animationContainer, screenPosition, null, out Vector2 startPosition);

        Vector3 targetScreenPosition = RectTransformUtility.WorldToScreenPoint(null, runeRoot.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(animationContainer, targetScreenPosition, null, out Vector2 targetPosition);

        RectTransform glowInstance = Instantiate(travelGlow, animationContainer);
        glowInstance.gameObject.SetActive(true);
        glowInstance.anchoredPosition = startPosition;
        glowInstance.localScale = Vector3.one * glowStartScale;

        Image glowImage = glowInstance.GetComponent<Image>();

        if (glowImage == null)
        {
            glowImage = glowInstance.GetComponentInChildren<Image>(true);
        }

        Color originalGlowColor = glowImage != null ? glowImage.color : Color.white;
        float timer = 0f;

        while (timer < travelDuration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / travelDuration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            glowInstance.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, smoothT);

            float scale = Mathf.Lerp(glowStartScale, glowEndScale, smoothT);
            glowInstance.localScale = Vector3.one * scale;

            if (glowImage != null)
            {
                Color glowColor = originalGlowColor;
                glowColor.a = Mathf.Lerp(originalGlowColor.a, 0.7f, smoothT);
                glowImage.color = glowColor;
            }

            yield return null;
        }

        glowInstance.anchoredPosition = targetPosition;
        Destroy(glowInstance.gameObject);
    }

    private IEnumerator PlayRuneAnimation()
    {
        if (runeRoot == null)
        {
            yield break;
        }

        runeRoot.gameObject.SetActive(true);
        runeRoot.localRotation = Quaternion.identity;
        runeRoot.localScale = Vector3.one * normalScale;

        for (int i = 0; i < pulseCount; i++)
        {
            yield return ScaleAnimation(normalScale, pulseScale, pulseUpDuration);
            yield return ScaleAnimation(pulseScale, normalScale, pulseDownDuration);
        }

        if (holdDuration > 0f)
        {
            yield return new WaitForSeconds(holdDuration);
        }

        yield return ScaleAnimation(normalScale, 0f, disappearDuration);

        runeRoot.gameObject.SetActive(false);
        runeRoot.localScale = Vector3.one * normalScale;
    }

    private IEnumerator ScaleAnimation(float from, float to, float duration)
    {
        if (duration <= 0f)
        {
            runeRoot.localScale = Vector3.one * to;
            yield break;
        }

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / duration);
            t = Mathf.SmoothStep(0f, 1f, t);

            float scale = Mathf.Lerp(from, to, t);
            runeRoot.localScale = Vector3.one * scale;

            yield return null;
        }

        runeRoot.localScale = Vector3.one * to;
    }
}