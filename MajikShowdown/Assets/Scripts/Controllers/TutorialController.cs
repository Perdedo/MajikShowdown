using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.Events;
using System.ComponentModel;

public class TutorialController : MonoBehaviour
{
    [Serializable]
    public struct TutorialPopupData
    {
        public TutorialPage[] pages;
        public TutorialPopupData(TutorialPage[] pages)
        {
            this.pages = pages;
        }
    }
    [Serializable]
    public struct TutorialPage
    {
        public string title;
        [TextArea(3, 10)] public string description;
        public Sprite sprite;
        public VideoClip videoClip;
        public TutorialPage(string title, string description, Sprite sprite, VideoClip videoClip)
        {
            this.title = title;
            this.description = description;
            this.sprite = sprite;
            this.videoClip = videoClip;
        }
    }
    public static TutorialController Instance;
    public TutorialPopupData[] tutorialPopup;
    public TutorialMission[] tutorialMissions;
    public TutorialPopup tutorialPopupPrefab;
    public Transform tutorialTransform;
    public SpellCaster caster;

    private int currentPageIndex = 0;
    private int currentTutorialIndex = 0;
    private TutorialPopup popupInstance;
    private bool firstTutorial = true;
    private int activeMission = 0;

    private void Awake()
    {
        Instance = this;
    }

    public void Update()
    {
        if(activeMission < tutorialMissions.Length)
        {
            tutorialMissions[activeMission].CheckCompletion();
            if (tutorialMissions[activeMission].isCompleted)
            {
                activeMission++;
            }
        }
        if(GameManager.Instance.uiController.playerUI.editSpellPanel.activeSelf)
        {
            if (tutorialMissions[activeMission].runePopupIndex != -1)
            {
                ShowPopup(tutorialMissions[activeMission].runePopupIndex);
            }
        }
    }

    public void ShowPopup(int tutorialIndex)
    {
        if (tutorialPopup[tutorialIndex].pages.Length > 0)
        {
            ShowPage(currentPageIndex, tutorialIndex);
            Debug.Log(GameManager.Instance.uiController);
            Debug.Log(GameManager.Instance.uiController.playerUI);
            GameManager.Instance.uiController.playerUI.EnableUICursor();
        }
    }

    private void ShowPage(int pageIndex, int tutorialIndex)
    {
        if (pageIndex < 0 || pageIndex >= tutorialPopup[tutorialIndex].pages.Length) return;

        TutorialPage page = tutorialPopup[tutorialIndex].pages[pageIndex];
        if(firstTutorial)
        {
            TutorialPopup popup = Instantiate(tutorialPopupPrefab, tutorialTransform);
            popupInstance = popup;
            firstTutorial = false;
        }
        popupInstance.ChangePopupDetails(page.title, page.description, page.sprite, page.videoClip, pageIndex + 1, tutorialPopup[tutorialIndex].pages.Length);
    }

    /*public void ShowNextTutorial()
    {
        ShowPopup(currentTutorialIndex);
        currentTutorialIndex++;
    }*/
}

[Serializable]
public class TutorialMission
{
    public enum MissionType
    {
        Collect,
        Defeat,
        CastSpell,
        Walk,
        Dash,
        Jump
    }

    public string missionName;
    public string missionDescription;
    public MissionType missionType;
    public UnityEvent missionStarted;
    public UnityEvent missionCompleted;
    [HideInInspector] public bool isStarted;
    [HideInInspector] public bool isCompleted;
    public int utilityInt;
    public int runePopupIndex = -1;
    public void StartMission()
    {
        isStarted = true;
        missionStarted.Invoke();
    }
    public void CheckCompletion()
    {
        if (!isStarted || isCompleted) return;

        bool check = false;
        switch (missionType)
        {
            case MissionType.Collect:
                check = LootSpawner.Instance.ActiveLootBoxes.Count == 0;
                break;
            case MissionType.Defeat:
                check = GameManager.Instance.trainingController.UsedEnemies.Count == 0;
                break;
            case MissionType.CastSpell:
                check = GameManager.Instance.Players[0].caster.spells[0].spellNodes.Count == utilityInt;
                break;
            case MissionType.Walk:
                check = GameManager.Instance.Players[0].lastHorizontalDirection != Vector3.zero; // Example distance
                break;
            case MissionType.Dash:
                check = GameManager.Instance.Players[0].dashOnCooldown;
                break;
            case MissionType.Jump:
                check = GameManager.Instance.Players[0].CvState == Character.CharVerticalState.jumping;
                break;
        }
        if (check)
        {
            CompleteMission();
        }
    }
    private void CompleteMission()
    {
        isCompleted = true;
        missionCompleted.Invoke();
        Debug.Log("Mission Completed" + missionName);
    }
}
