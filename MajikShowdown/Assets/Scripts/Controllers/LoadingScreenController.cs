using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class LoadingScreenController : MonoBehaviour
{
    public static LoadingScreenController Instance { get; private set; }

    [Header("Loading")]
    [SerializeField] private float minimumDisplayTime = 3f;

    [Header("Fade")]
    [SerializeField] private float fadeDuration = 0.25f;

    [Header("Spinner")]
    [SerializeField] private RectTransform spinner;
    [SerializeField] private float rotationDuration = 1f;

    [Header("Text")]
    [SerializeField] private TMP_Text loadingText;
    [SerializeField] private float dotsInterval = 0.35f;

    private CanvasGroup canvasGroup;
    private Tween fadeTween;
    private Tween spinnerTween;

    private Coroutine hideCoroutine;
    private Coroutine dotsCoroutine;

    private float showStartTime;
    private bool isShowing;

    public bool IsShowing => isShowing;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        StartSpinner();
    }

    public void Show()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }
        showStartTime = Time.realtimeSinceStartup;
        isShowing = true;
        fadeTween?.Kill();
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        fadeTween = canvasGroup.DOFade(1f, fadeDuration).SetUpdate(true);
        StartLoadingText();
    }

    public void WaitUntilMinimumTime(Action onComplete)
    {
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
        }
        hideCoroutine = StartCoroutine(WaitUntilMinimumTimeRoutine(onComplete));
    }

    private IEnumerator WaitUntilMinimumTimeRoutine(Action onComplete)
    {
        float elapsedTime = Time.realtimeSinceStartup - showStartTime;
        float remainingTime = Mathf.Max(0f, minimumDisplayTime - elapsedTime);
        if (remainingTime > 0f)
        {
            yield return new WaitForSecondsRealtime(remainingTime);
        }
        hideCoroutine = null;
        onComplete?.Invoke();
    }

    public void Hide(Action onComplete = null)
    {
        if (!isShowing)
        {
            onComplete?.Invoke();
            return;
        }
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }
        fadeTween?.Kill();
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        fadeTween = canvasGroup.DOFade(0f, fadeDuration).SetUpdate(true).OnComplete(() =>
        {
            isShowing = false;
            StopLoadingText();
            onComplete?.Invoke();
        });
    }

    private void StartSpinner()
    {
        if (spinner == null) return;

        spinnerTween?.Kill();
        spinnerTween = spinner.DORotate(new Vector3(0f, 0f, -360f), rotationDuration, RotateMode.FastBeyond360).SetEase(Ease.Linear).SetLoops(-1).SetUpdate(true);
    }

    private void StartLoadingText()
    {
        StopLoadingText();
        if (loadingText != null)
        {
            dotsCoroutine = StartCoroutine(AnimateDots());
        }
    }

    private IEnumerator AnimateDots()
    {
        int dotCount = 1;
        while (true)
        {
            loadingText.text ="Loading" + new string('.', dotCount);
            dotCount++;
            if (dotCount > 3)
            {
                dotCount = 1;
            }
            yield return new WaitForSecondsRealtime(dotsInterval);
        }
    }

    private void StopLoadingText()
    {
        if (dotsCoroutine == null) return;

        StopCoroutine(dotsCoroutine);
        dotsCoroutine = null;
    }

    private void OnDestroy()
    {
        fadeTween?.Kill();
        spinnerTween?.Kill();
        if (Instance == this)
        {
            Instance = null;
        }
    }
}