using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RunePickupUI : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] private RectTransform runeRoot;
    [SerializeField] private Image mainImage;
    [SerializeField] private Image nodeSymbol;
    [SerializeField] private Image borderImage;
    [SerializeField] private SpellNodeInfos info;

    [Header("Animation")]
    [SerializeField] private float startScale = 0.20f;
    [SerializeField] private float normalScale = 1f;
    [SerializeField] private float pulseScale = 1.18f;

    [SerializeField] private float introDuration = 0.5f;

    [SerializeField] private float flipHalfDuration = 0.12f;
    [SerializeField] private float flipPhaseDuration = 2f;

    [SerializeField] private float settleDuration = 0.3f;

    [SerializeField] private float pulseUpDuration = 0.18f;
    [SerializeField] private float pulseDownDuration = 0.22f;

    [SerializeField] private float holdDuration = 0.4f;

    [SerializeField] private float disappearDuration = 0.3f;
    [SerializeField] private float nextRuneDelay = 0.5f;

    [Header("Back Visual")]
    [SerializeField] private Color backColor = Color.gray;

    private readonly Queue<SpellNode> runeQueue = new Queue<SpellNode>();

    private bool isPlaying;

    private Color originalMainColor;
    private bool currentNodeHasSymbol;

    private void Awake()
    {
        if (runeRoot != null)
        {
            runeRoot.gameObject.SetActive(false);
        }
    }

    public void ShowRune(SpellNode node)
    {
        if (node == null)
        {
            return;
        }

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

            yield return PlayAnimation();

            // Só espera se realmente tiver outra rune na fila
            if (runeQueue.Count > 0)
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
            mainImage.color = hasVisualInfo
                ? visualInfo.color
                : node.color;

            originalMainColor = mainImage.color;
        }

        currentNodeHasSymbol = node.nodeSymbolSprite != null;

        if (nodeSymbol != null)
        {
            nodeSymbol.gameObject.SetActive(currentNodeHasSymbol);

            if (currentNodeHasSymbol)
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

        if (borderImage != null && info != null)
        {
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

        ShowFront();
    }

    private IEnumerator PlayAnimation()
    {
        if (runeRoot == null)
        {
            yield break;
        }

        runeRoot.gameObject.SetActive(true);
        runeRoot.localRotation = Quaternion.identity;

        ShowFront();

        // 1. Começa pequena
        runeRoot.localScale = Vector3.one * startScale;

        // 2. Cresce antes dos flips
        yield return ScaleAnimation(
            startScale,
            0.65f,
            introDuration
        );

        // 3. Fase de flip
        float flipTimer = 0f;
        bool showingFront = true;

        while (flipTimer < flipPhaseDuration)
        {
            yield return FlipScaleX(1f, 0f);

            if (showingFront)
            {
                ShowBack();
            }
            else
            {
                ShowFront();
            }

            showingFront = !showingFront;

            yield return FlipScaleX(0f, 1f);

            flipTimer += flipHalfDuration * 2f;
        }

        // 4. Garante que termina de frente
        if (!showingFront)
        {
            yield return FlipScaleX(1f, 0f);

            ShowFront();

            yield return FlipScaleX(0f, 1f);
        }

        ShowFront();

        // 5. Vai para tamanho normal
        yield return ScaleAnimation(
            runeRoot.localScale.y,
            normalScale,
            settleDuration
        );

        // 6. Pulso
        yield return ScaleAnimation(
            normalScale,
            pulseScale,
            pulseUpDuration
        );

        yield return ScaleAnimation(
            pulseScale,
            normalScale,
            pulseDownDuration
        );

        // 7. Segura um pouco
        if (holdDuration > 0f)
        {
            yield return new WaitForSeconds(holdDuration);
        }

        // 8. Encolhe até desaparecer
        yield return ScaleAnimation(
            normalScale,
            0f,
            disappearDuration
        );

        runeRoot.gameObject.SetActive(false);

        runeRoot.localScale = Vector3.one;
        runeRoot.localRotation = Quaternion.identity;
    }

    private IEnumerator FlipScaleX(float from, float to)
    {
        float timer = 0f;

        float currentScale = runeRoot.localScale.y;

        while (timer < flipHalfDuration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / flipHalfDuration);
            t = Mathf.SmoothStep(0f, 1f, t);

            float scaleX = Mathf.Lerp(from, to, t);

            runeRoot.localScale = new Vector3(
                scaleX * currentScale,
                currentScale,
                1f
            );

            yield return null;
        }

        runeRoot.localScale = new Vector3(
            to * currentScale,
            currentScale,
            1f
        );
    }

    private IEnumerator ScaleAnimation(
        float from,
        float to,
        float duration)
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

    private void ShowFront()
    {
        if (mainImage != null)
        {
            mainImage.color = originalMainColor;
        }

        if (nodeSymbol != null)
        {
            nodeSymbol.gameObject.SetActive(currentNodeHasSymbol);
        }

        if (borderImage != null)
        {
            borderImage.gameObject.SetActive(true);
        }
    }

    private void ShowBack()
    {
        if (mainImage != null)
        {
            mainImage.color = backColor;
        }

        if (nodeSymbol != null)
        {
            nodeSymbol.gameObject.SetActive(false);
        }

        if (borderImage != null)
        {
            borderImage.gameObject.SetActive(false);
        }
    }
}