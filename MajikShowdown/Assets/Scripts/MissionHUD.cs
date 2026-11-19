
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MissionHUD : MonoBehaviour
{
    [Header("Mission UI")]
    [SerializeField] private RectTransform missionPanel;
    [SerializeField] private TextMeshProUGUI missionTitle;
    [SerializeField] private TextMeshProUGUI missionDescription;
    [SerializeField] private Button helpButton;

    [Header("Panels To Monitor")]
    [SerializeField] private GameObject[] panelsThatShowHelper;

    [Header("Animation")]
    [SerializeField] private float animationDuration = 0.3f;
    [SerializeField] private float hiddenPadding = 20f;

    private Vector2 shownPosition;
    private Vector2 hiddenPosition;
    private Coroutine panelAnimation;
    private float defaultPanelHeight;
    private float defaultDescriptionHeight;
    private Vector2 defaultPanelOffsetMin;
    private string lastMissionDescription;
    private bool screenOpen;
    private bool helperExpanded;

    private void Awake()
    {
        if (helpButton != null)
            helpButton.onClick.AddListener(ToggleMissionPanel);
    }

    private void Start()
    {
        if (missionPanel == null || missionTitle == null || missionDescription == null || helpButton == null)
        {
            Debug.LogError("[MissionHUD] Missing UI references!");
            enabled = false;
            return;
        }

        defaultPanelHeight = missionPanel.rect.height;
        defaultDescriptionHeight = missionDescription.rectTransform.rect.height;
        defaultPanelOffsetMin = missionPanel.offsetMin;

        shownPosition = missionPanel.anchoredPosition;
        hiddenPosition = CalculateHiddenPosition();

        missionPanel.gameObject.SetActive(true);

        screenOpen = IsAnyWatchedPanelOpen();

        if (screenOpen)
        {
            missionPanel.anchoredPosition = hiddenPosition;
            helpButton.gameObject.SetActive(true);
        }
        else
        {
            missionPanel.anchoredPosition = shownPosition;
            helpButton.gameObject.SetActive(false);
        }

        UpdateMissionInfo();
    }

    private void Update()
    {
        UpdateMissionInfo();

        bool currentScreenOpen = IsAnyWatchedPanelOpen();

        if (currentScreenOpen == screenOpen) return;

        screenOpen = currentScreenOpen;
        helperExpanded = false;

        if (screenOpen)
        {
            helpButton.gameObject.SetActive(true);
            AnimatePanel(hiddenPosition);
        }
        else
        {
            helpButton.gameObject.SetActive(false);
            AnimatePanel(shownPosition);
        }
    }

    private void UpdateMissionInfo()
    {
        TutorialController controller = TutorialController.Instance;

        if (controller == null || controller.tutorialMissions == null || controller.activeMission < 0 || controller.activeMission >= controller.tutorialMissions.Length)
        {
            missionTitle.text = "";
            missionDescription.text = "";
        }
        else
        {
            TutorialMission mission = controller.tutorialMissions[controller.activeMission];

            if (missionTitle.text != mission.missionName)
                missionTitle.text = mission.missionName;

            if (missionDescription.text != mission.missionDescription)
                missionDescription.text = mission.missionDescription;
        }

        if (lastMissionDescription != missionDescription.text)
        {
            lastMissionDescription = missionDescription.text;
            UpdateMissionPanelHeight();
        }
    }

    private void UpdateMissionPanelHeight()
    {
        missionDescription.ForceMeshUpdate();

        float requiredHeight = Mathf.Max(defaultDescriptionHeight, missionDescription.preferredHeight);
        float extraHeight = requiredHeight - defaultDescriptionHeight;

        RectTransform descriptionRect = missionDescription.rectTransform;
        descriptionRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, requiredHeight);

        Vector2 panelOffsetMin = defaultPanelOffsetMin;
        panelOffsetMin.y -= extraHeight;
        missionPanel.offsetMin = panelOffsetMin;
    }

    private bool IsAnyWatchedPanelOpen()
    {
        if (panelsThatShowHelper == null) return false;

        foreach (GameObject panel in panelsThatShowHelper)
        {
            if (panel != null && panel.activeInHierarchy)
                return true;
        }

        return false;
    }

    private void ToggleMissionPanel()
    {
        if (!screenOpen) return;

        helperExpanded = !helperExpanded;

        AnimatePanel(helperExpanded ? shownPosition : hiddenPosition);
    }

    private void AnimatePanel(Vector2 targetPosition)
    {
        if (panelAnimation != null)
            StopCoroutine(panelAnimation);

        if (!missionPanel.gameObject.activeSelf)
            missionPanel.gameObject.SetActive(true);

        if (animationDuration <= 0f)
        {
            missionPanel.anchoredPosition = targetPosition;
            return;
        }

        panelAnimation = StartCoroutine(AnimatePanelCoroutine(targetPosition));
    }

    private IEnumerator AnimatePanelCoroutine(Vector2 targetPosition)
    {
        Vector2 startPosition = missionPanel.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(elapsed / animationDuration);
            t = t * t * (3f - 2f * t);

            missionPanel.anchoredPosition = Vector2.LerpUnclamped(startPosition, targetPosition, t);

            yield return null;
        }

        missionPanel.anchoredPosition = targetPosition;
        panelAnimation = null;
    }

    private Vector2 CalculateHiddenPosition()
    {
        RectTransform parentRect = missionPanel.parent as RectTransform;

        if (parentRect == null)
            return shownPosition + Vector2.right * (missionPanel.rect.width + hiddenPadding);

        Vector3[] corners = new Vector3[4];
        missionPanel.GetWorldCorners(corners);

        float rightMostX = float.MinValue;

        foreach (Vector3 corner in corners)
        {
            float localX = parentRect.InverseTransformPoint(corner).x;
            rightMostX = Mathf.Max(rightMostX, localX);
        }

        float offset = parentRect.rect.xMax - rightMostX + hiddenPadding;

        return shownPosition + Vector2.right * Mathf.Max(0f, offset);
    }

    private void OnDestroy()
    {
        if (helpButton != null)
            helpButton.onClick.RemoveListener(ToggleMissionPanel);

        if (panelAnimation != null)
            StopCoroutine(panelAnimation);
    }
}
