using System;
using System.Collections.Generic;
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
        [HideInInspector] public bool popupShown;
        public TutorialPopupData(TutorialPage[] pages)
        {
            this.pages = pages;
            this.popupShown = false;
        }

    }
    [Serializable]
    public struct TutorialPage
    {
        public string title;
        [TextArea(5, 10)] public string description;
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
    public int activeMission = 0;

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
                if(activeMission < tutorialMissions.Length - 1)
                {
                    activeMission++;
                    tutorialMissions[activeMission].StartMission();
                }
            }
        }
        if(GameManager.Instance.uiController.playerUI.editSpellPanel.activeSelf)
        {
            if (tutorialMissions[activeMission].runePopupIndex != -1 && !tutorialPopup[tutorialMissions[activeMission].runePopupIndex].popupShown)
            {
                ResetPageIndex();
                ShowPopup(tutorialMissions[activeMission].runePopupIndex);
            }
        }
        if(GameManager.Instance.uiController.playerUI.spellPanel.activeSelf)
        {
            if (!tutorialPopup[0].popupShown)
            {
                ResetPageIndex();
                ShowPopup(0);
            }
        }
    }

    public void ShowPopup(int tutorialIndex)
    {
        if (tutorialPopup[tutorialIndex].pages.Length > 0)
        {
            currentTutorialIndex = tutorialIndex;
            ShowPage(currentPageIndex, tutorialIndex);
            //GameManager.Instance.uiController.playerUI.EnableUICursor();
            tutorialPopup[tutorialIndex].popupShown = true;
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

    public void ResetPageIndex()
    {
        currentPageIndex = 0;
    }

    public void IncreasePageIndex()
    {
        currentPageIndex++;
        ShowPopup(currentTutorialIndex);
    }

    public void DecreasePageIndex()
    {
        currentPageIndex--;
        ShowPopup(currentTutorialIndex);
    }

    public void SpawnRuneLootBox(int poolIndex)
    {
        LootSpawner.Instance.SpawnLootBox(GameManager.Instance.Players[0].gameObject.transform.position + (Vector3.up * 0.5f), poolIndex);
    }

    public void SpawnEnemy()
    {
        this.GetComponent<TrainingEnemySpawner>().Interact(GameManager.Instance.Players[0]);
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
        CastTriggerSpell,
        Walk,
        Dash,
        Jump
    }

    public string missionName;
    [TextArea(5, 10)] public string missionDescription;
    public MissionType missionType;
    public UnityEvent missionStarted;
    public UnityEvent missionCompleted;
    [HideInInspector] public bool isStarted;
    [HideInInspector] public bool isCompleted;
    public List<int> utilityInts;
    public int runePopupIndex = -1;
    public void StartMission()
    {
        isStarted = true;
        Debug.Log("Mission Started: " + missionName);
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
                if (SpellColliderManager.Instance.activeSpellColliders.Count > 0)
                {
                    check = SpellColliderManager.Instance.activeSpellColliders[0].OwnerSpell.spellNodes.Count == utilityInts[0];
                }
                break;
            case MissionType.CastTriggerSpell:
                if (SpellColliderManager.Instance.activeSpellColliders.Count > 0)
                {
                    foreach(SpellCollider col in SpellColliderManager.Instance.activeSpellColliders)
                    {
                        if(col.OwnerSpell.spellNodes.Count == utilityInts[0] && col.triggerInfos.Count > 0 && col.triggerInfos[0].Trigger.TriggeredSpell != null && col.triggerInfos[0].Trigger.trigger == SpellTrigger.Triggers.OnHit)
                        {
                            check = col.triggerInfos[0].Trigger.TriggeredSpell.spellNodes.Count == utilityInts[1];
                        }
                    }
                }
                break;
            case MissionType.Walk:
                check = GameManager.Instance.Players[0].directionInput != Vector2.zero;
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
        Debug.Log("Mission Completed: " + missionName);
    }
}
