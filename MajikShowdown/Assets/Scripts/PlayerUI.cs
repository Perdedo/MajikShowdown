using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerUI : NetworkBehaviour
{
    [Header("Test Panels")]
    public GameObject spellPanel;
    public GameObject createSpellPanel;
    public GameObject editSpellPanel;
    public Spell spellToEquip;
    public Button[] equipSlotButtons;
    public TMP_Text[] equipSlotTexts;
    public SpellCaster caster;
    public TMP_InputField spellNameInput;
    public TextMeshProUGUI spellNameText;
    public TextMeshProUGUI spellCooldownText;
    public GameObject spellPage;
    public GameObject runePage;
    public GameObject spellsInventoryPageButton;
    public GameObject runesInventoryPageButton;
    public TextMeshProUGUI[] spellStats;
    public Slider healthSlider;
    public Image[] cooldownFills;

    [Header("Game State Panels")]
    public GameObject victoryPanel;
    public GameObject defeatPanel;
    public GameObject deathPanel;

    [Header("Pause Panels and Objects")]
    public GameObject pausePanel;
    public GameObject optionsPanel;
    public GameObject confirmLeavePanel;
    public Slider _masterVolumeSlider;
    public Slider _musicSlider;
    public Slider _sfxSlider;
    public Toggle vsyncToggle;
    public Image vsyncToggleImage;
    public Sprite toggleConfirm;
    public Sprite toggleDeny;
    Resolution[] allRes;
    List<Resolution> selectedResList = new List<Resolution>();
    public TMP_Dropdown resDropdown;
    public TMP_Dropdown screenModeDropdown;

    [Header("Spell Customization")]
    public SpellVisualDatabase visualDatabase;
    [SerializeField] private GameObject customizationPanel;
    [SerializeField] private Image previewImage;
    [SerializeField] private Image previewButtonImage;
    [SerializeField] private GameObject[] slotVisuals;
    [SerializeField] private Image[] slotIcons;
    [SerializeField] private Button[] colorButtons;
    [SerializeField] private Image[] iconSlots;
    [SerializeField] private Button[] iconButtons;
    [SerializeField] private Image[] cooldownIcons;

    [Header("Trigger Spell Selection")]
    public GameObject triggerSpellSelectionPanel;
    public Transform triggerSpellContent;
    public SpellSelectionEntry spellEntryPrefab;

    [HideInInspector]
    public ConfigData data;
    public HexGrid activeGrid;
    public SpellNodeDescription spellNodeDescription;
    Spell activeSpell;
    public SpellNodeInterface selectedNode;
    public SpellInventoryUI inventory;
    public Player myPlayer;
    public PlayerDamageHandler damageHandler;
    public GameObject crosshair;
    [HideInInspector] public bool inGame = false;
    public PopupManager popupManager;
    [Header("Network")]
    public bool network = true;

    bool loaded;

    private void Start()
    {
        if(isLocalPlayer || !network)
        {
            GameManager.Instance.uiController.playerUI = this;
        }
        else
        {
            gameObject.SetActive(false);
        }
        if(spellPanel != null)
        {
            spellPanel.SetActive(false);
        }
        if(pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
        if(optionsPanel != null)
        {
            optionsPanel.SetActive(false);
        }
        if(confirmLeavePanel != null)
        {
            confirmLeavePanel.SetActive(false);
        }
        if(victoryPanel != null)
        {
            victoryPanel.SetActive(false);
        }
        if(defeatPanel != null)
        {
            defeatPanel.SetActive(false);
        }
        if(deathPanel != null)
        {
            deathPanel.SetActive(false);
        }
        ResolutionDropdown();
        ScreenModeDropdown();
        loaded = true;
        data = SaveManager.LoadConfig(ref loaded);
        if (!loaded)
        {
            SaveManager.SaveConfig(data);
        }
        ConfigUpdate();
        if (vsyncToggle != null)
        {
            vsyncToggle.onValueChanged.RemoveAllListeners();
            vsyncToggle.onValueChanged.AddListener(ChangeVsyncToggle);
        }
        if (!AudioController.instance.musicSource.isPlaying)
        {
            AudioController.instance.StartMusic();
        }
        InitializeStatsUI();
        healthSlider.maxValue = damageHandler.MaxHealth;
        healthSlider.value = damageHandler.Health;
        SetupColors();
        SetupIcons();
        UpdateEquipSlotIcons();
        EnableGameplayCursor();
    }
    private void Update()
    {
        if (!isLocalPlayer && network) return;
        //UpdateHealthUI();
        UpdateCooldownFills();
        UpdateCooldownIcon();
    }

    public void OpenPanel(GameObject panel)
    {
        panel.SetActive(true);
    }

    public void ClosePanel(GameObject panel)
    {
        panel.SetActive(false);
        if (panel == optionsPanel)
        {
            SaveManager.SaveConfig(data);
        }
    }
    public void UpdateVsyncToggleImages(bool isOn)
    {
        if (isOn)
        {
            vsyncToggleImage.sprite = toggleConfirm;
        }
        else
        {
            vsyncToggleImage.sprite = toggleDeny;
        }
    }
    public void ChangeVsyncToggle(bool isOn)
    {
        QualitySettings.vSyncCount = isOn ? 1 : 0;
        data.vsyncEnabled = isOn;
        UpdateVsyncToggleImages(isOn);
    }
    public void ConfigUpdate()
    {
        if (resDropdown != null)
        {
            resDropdown.value = data.selectedRes;
        }
        if (screenModeDropdown != null)
        {
            screenModeDropdown.value = data.screenMode;
        }
        AudioController.instance.ChangeMasterVol(data.master);
        if (_masterVolumeSlider != null)
        {
            _masterVolumeSlider.value = Mathf.InverseLerp(-30f, 0f, data.master) * 30f;
        }
        AudioController.instance.ChangeMusicVol(data.music);
        if (_musicSlider != null)
        {
            _musicSlider.value = Mathf.InverseLerp(-30f, 0f, data.music) * 30f;
        }
        AudioController.instance.ChangeSFXVol(data.sfx);
        if (_sfxSlider != null)
        {
            _sfxSlider.value = Mathf.InverseLerp(-30f, 0f, data.sfx) * 30f;
        }
        if (vsyncToggle != null)
        {
            vsyncToggle.isOn = data.vsyncEnabled;
            QualitySettings.vSyncCount = data.vsyncEnabled ? 1 : 0;
            UpdateVsyncToggleImages(data.vsyncEnabled);
        }
    }


    public void ChangeMasterVolume()
    {
        float value = _masterVolumeSlider.value;
        float dB = (value == 0f) ? -80f : Mathf.Lerp(-30f, 0f, value / 30f);
        data.master = dB;
        AudioController.instance.ChangeMasterVol(dB);
    }

    public void ChangeMusicVolume()
    {
        float value = _musicSlider.value;
        float dB = (value == 0f) ? -80f : Mathf.Lerp(-30f, 0f, value / 30f);
        data.music = dB;
        AudioController.instance.ChangeMusicVol(dB);
    }

    public void ChangeSFXVolume()
    {
        float value = _sfxSlider.value;
        float dB = (value == 0f) ? -80f : Mathf.Lerp(-30f, 0f, value / 30f);
        data.sfx = dB;
        AudioController.instance.ChangeSFXVol(dB);
    }

    public void ResolutionDropdown()
    {
        allRes = Screen.resolutions;
        Array.Sort(allRes, (a, b) =>
        {
            int widthComparison = b.width.CompareTo(a.width);
            return widthComparison == 0 ? b.height.CompareTo(a.height) : widthComparison;
        });
        string newRes;
        List<string> resStringList = new List<string>();
        foreach (Resolution res in allRes)
        {
            float aspectRatio = (float)res.width / res.height;
            if (Math.Abs(aspectRatio - 16f / 9f) < 0.01f)
            {
                if (res.width >= 800)
                {
                    newRes = res.width.ToString() + "x" + res.height.ToString();
                    if (!resStringList.Contains(newRes))
                    {
                        resStringList.Add(newRes);
                        selectedResList.Add(res);
                    }
                }
            }
        }
        if (resDropdown != null)
        {
            resDropdown.ClearOptions();
            resDropdown.AddOptions(resStringList);
        }
    }

    public void ChangeRes()
    {
        data.selectedRes = resDropdown.value;
        Screen.SetResolution(selectedResList[data.selectedRes].width, selectedResList[data.selectedRes].height, Screen.fullScreenMode);
    }

    public void ScreenModeDropdown()
    {
        List<string> screenModes = new List<string> { "Fullscreen Mode", "Borderless Mode", "Window Mode" };
        if (screenModeDropdown != null)
        {
            screenModeDropdown.ClearOptions();
            screenModeDropdown.AddOptions(screenModes);
            screenModeDropdown.onValueChanged.AddListener((int index) =>
            {
                if (index == 0)
                {
                    data.screenMode = 0;
                    Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
                }
                else if (index == 1)
                {
                    data.screenMode = 1;
                    Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                }
                else if (index == 2)
                {
                    data.screenMode = 2;
                    Screen.fullScreenMode = FullScreenMode.Windowed;
                }
            });
        }
    }

    void UpdateCooldownIcon()
    {
        for (int i = 0; i < caster.equippedSpells.Length; i++)
        {
            Spell spell = caster.equippedSpells[i];

            if (spell == null)
            {
                cooldownIcons[i].enabled = false;
                continue;
            }
            cooldownIcons[i].enabled = true;
            cooldownIcons[i].sprite = visualDatabase.icons[spell.symbolIndex];
            cooldownIcons[i].color = visualDatabase.colors[spell.colorIndex];
        }
    }

    void UpdateCooldownFills()
    {
        for (int i = 0; i < caster.equippedSpells.Length; i++)
        {
            Spell spell = caster.equippedSpells[i];
            if (spell == null)
            {
                cooldownFills[i].fillAmount = 0;
                continue;
            }
            if (spell.onCooldown)
            {
                float remaining = spell.SpellCooldown - spell.cooldownTimer.Timestamp;
                remaining = Mathf.Max(remaining, 0);
                cooldownFills[i].fillAmount = remaining / spell.SpellCooldown;
            }
            else
            {
                cooldownFills[i].fillAmount = 0;
            }
        }
    }

    public void UpdateHealthUI()
    {
        if (damageHandler == null) return;
        healthSlider.maxValue = damageHandler.MaxHealth;
        healthSlider.value = damageHandler.Health;
    }

    public void LeavePauseButton()
    {
        if (!isLocalPlayer && network)
        {
            return;
        }
        pausePanel.SetActive(false);
        myPlayer.input.ActivateInput();
        myPlayer.playerCamera.GetComponent<CinemachineInputAxisController>().enabled = true;
        caster.canCast = true;
    }

    void InitializeStatsUI()
    {
        float value = 0f;
        for (int i = 0; i < spellStats.Length; i++)
        {
            if (i != 1)
            {
                spellStats[i].text = FormatStat(value);
            }
            else
            {
                spellStats[i].text = FormatStat(value) + "s";
            }
        }
    }

    public void OpenEditSpellHUD(Spell spell)
    {
        if (!isLocalPlayer && network)
        {
            return;
        }
        if (network)
        {
            CMDdisableSpellColliders(spell.instanceIndex);
        }
        else
        {
            DisableSpellColliders(spell.instanceIndex);
        }
        createSpellPanel.gameObject.SetActive(false);
        editSpellPanel.gameObject.SetActive(true);

        if (activeGrid != null)
        {
            activeGrid.gameObject.SetActive(false);
        }
        activeGrid = spell.grid;
        activeGrid.gameObject.SetActive(true);
        SetActiveSpell(spell);
    }
    public void DisableSpellColliders(int index)
    {
        foreach(SpellCollider col in SpellColliderManager.Instance.activeSpellColliders)
        {
            if(col.OwnerSpell == caster.spells[index])
            {
                col.OnDeath.RemoveAllListeners();
                col.MarkedToDie = true;
            }
        }
    }
    [Command]
    public void CMDdisableSpellColliders(int index)
    {
        foreach(SpellCollider col in SpellColliderManager.Instance.activeSpellColliders)
        {
            if(col.OwnerSpell == caster.spells[index])
            {
                col.OnDeath.RemoveAllListeners();
                col.MarkedToDie = true;
            }
        }
    }

    void SetActiveSpell(Spell spell)
    {
        if (!isLocalPlayer && network)
        {
            return;
        }
        if (activeSpell != null)
        {
            activeSpell.OnSpellUpdated -= RefreshSpellInfo;
        }
        activeSpell = spell;
        if (activeSpell == null) return;
        spellNameInput.onValueChanged.RemoveAllListeners();
        spellNameInput.text = activeSpell.spellName;
        spellNameInput.onValueChanged.AddListener(OnSpellNameChanged);
        RefreshSpellInfo();
        activeSpell.OnSpellUpdated += RefreshSpellInfo;
        RefreshCustomizationPreview();
    }

    void OnSpellNameChanged(string newName)
    {
        if (!isLocalPlayer && network)
        {
            return;
        }
        if (activeSpell == null) return;
        if (string.IsNullOrWhiteSpace(newName)) return;

        activeSpell.spellName = newName;
        activeSpell.OnSpellUpdated?.Invoke();
        for (int i = 0; i < caster.equippedSpells.Length; i++)
        {
            if (caster.equippedSpells[i] == activeSpell)
            {
                equipSlotTexts[i].text = newName;
                break;
            }
        }
    }

    void UpdateAllEquippedSlots()
    {
        if (!isLocalPlayer && network)
        {
            return;
        }
        if (caster == null) return;

        for (int i = 0; i < equipSlotTexts.Length; i++)
        {
            equipSlotTexts[i].text = caster.IsSlotValid(i)
                ? caster.equippedSpells[i].spellName
                : "Spell Slot " + (i + 1);
        }
    }

    public void UpdateEquipSlotIcons()
    {
        for (int i = 0; i < caster.equippedSpells.Length; i++)
        {
            Spell spell = caster.equippedSpells[i];
            if (spell == null)
            {
                slotVisuals[i].transform.GetChild(2).gameObject.SetActive(false);
                continue;
            }
            slotVisuals[i].transform.GetChild(2).gameObject.SetActive(true);
            slotIcons[i].sprite = visualDatabase.icons[spell.symbolIndex];
            slotIcons[i].color = visualDatabase.colors[spell.colorIndex];
        }
    }

    string FormatStat(float value)
    {
        if (float.IsInfinity(value)) return value > 0 ? "+\u221E" : "-\u221E";
        if (float.IsNaN(value)) return "NaN";
        return value.ToString("F1");
    }

    void RefreshSpellInfo()
    {
        if (!isLocalPlayer && network) return;
        if (activeSpell == null) return;
        spellCooldownText.text = FormatStat(activeSpell.SpellCooldown) + "s";
        if (activeSpell.coreNode == null)
        {
            InitializeStatsUI();
            return;
        }

        var stats = activeSpell.coreNode.FinalStats;
        spellStats[0].text = FormatStat(stats.Speed);
        spellStats[1].text = FormatStat(stats.Duration) + "s";
        spellStats[2].text = FormatStat(stats.Size);
        spellStats[3].text = FormatStat(stats.Damage);
        spellStats[4].text = FormatStat(stats.Piercing);
        spellStats[5].text = FormatStat(stats.Bounce);
        spellStats[6].text = FormatStat(stats.Knockback);
    }

    public void CloseEditSpellHUD()
    {
        if (!isLocalPlayer && network)
        {
            return;
        }
        CloseCustomizationPanel();
        editSpellPanel.gameObject.SetActive(false);
        spellNameInput.onValueChanged.RemoveAllListeners();
        spellNodeDescription.HideAll();

        if (activeSpell != null)
            activeSpell.OnSpellUpdated -= RefreshSpellInfo;

        activeSpell = null;
        selectedNode = null;

        createSpellPanel.gameObject.SetActive(true);
        UpdateAllEquippedSlots();
    }

    public void StartEquipSpell(Spell spell)
    {
        if (!isLocalPlayer && network)
        {
            return;
        }
        spellToEquip = spell;
        if(!isServer && network)
        {
            StartCoroutine(WaitStartEquipSpell(spell));
        }
    }

    IEnumerator WaitStartEquipSpell(Spell spell)
    {
        yield return new WaitUntil(() => caster.spells.Exists(s => s.instanceIndex == spell.instanceIndex));
        yield return new WaitUntil(() => NetworkClient.ready);
        CMDStartEquipSpell(spell.instanceIndex);
    }

    public void UnequipSpell(Spell spell)
    {
        for (int i = 0; i < caster.equippedSpells.Length; i++)
        {
            if (caster.equippedSpells[i] == spell)
            {
                caster.equippedSpells[i] = null;
                equipSlotTexts[i].text = "Spell Slot " + (i + 1);
                UpdateEquipSlotIcons();
            }
        }
    }

    [Command]
    public void CMDStartEquipSpell(int index)
    {
        spellToEquip = caster.spells.Find(s => s.instanceIndex == index);
    }

    public void EquipSpellToSlot(int index)
    {
        if (!isLocalPlayer && network)
        {
            return;
        }
        if (spellToEquip == null) return;
        if (caster.equippedSpells[index] == spellToEquip)
        {
            spellToEquip = null;
            return;
        }

        for (int i = 0; i < caster.equippedSpells.Length; i++)
        {
            if (caster.equippedSpells[i] == spellToEquip)
            {
                caster.equippedSpells[i] = null;
                equipSlotTexts[i].text = "Spell Slot " + (i + 1);
            }
        }

        caster.equippedSpells[index] = spellToEquip;
        equipSlotTexts[index].text = spellToEquip.spellName;
        UpdateEquipSlotIcons();
        if (!isServer && network)
        {
            CMDEquipSpell(index);
        }
        spellToEquip = null;

        if (inventory != null)
        {
            inventory.DeselectAllCards();
        }
    }

    [Command]
    public void CMDEquipSpell(int index)
    {
        if (spellToEquip == null) return;
        if (caster.equippedSpells[index] == spellToEquip)
        {
            spellToEquip = null;
            return;
        }

        for (int i = 0; i < caster.equippedSpells.Length; i++)
        {
            if (caster.equippedSpells[i] == spellToEquip)
            {
                caster.equippedSpells[i] = null;
                equipSlotTexts[i].text = "Spell Slot " + (i + 1);
            }
        }

        caster.equippedSpells[index] = spellToEquip;
        equipSlotTexts[index].text = spellToEquip.spellName;
        spellToEquip = null;

        if (inventory != null)
        {
            inventory.DeselectAllCards();
        }
    }

    public void OnSpellNameInputSelected(string currentText)
    {
        if (!isLocalPlayer && network)
        {
            return;
        }
        if (string.IsNullOrEmpty(currentText))
        {
            if (spellNameInput.placeholder != null)
            {
                spellNameInput.placeholder.gameObject.SetActive(false);
            }
        }
    }

    public void OnSpellNameInputDeselected(string currentText)
    {
        if (!isLocalPlayer && network)
        {
            return;
        }
        if (string.IsNullOrEmpty(currentText))
        {
            if (spellNameInput.placeholder != null)
            {
                spellNameInput.placeholder.gameObject.SetActive(true);
            }
        }
    }

    public void ActivateRunesInventoryPage()
    {
        if (!isLocalPlayer && network)
        {
            return;
        }
        spellsInventoryPageButton.GetComponent<Image>().color = Color.grey;
        spellPage.SetActive(false);

        runesInventoryPageButton.GetComponent<Image>().color = Color.white;
        runePage.SetActive(true);
    }

    public void ActivateSpellsInventoryPage()
    {
        if (!isLocalPlayer && network)
        {
            return;
        }
        runesInventoryPageButton.GetComponent<Image>().color = Color.grey;
        runePage.SetActive(false);

        spellsInventoryPageButton.GetComponent<Image>().color = Color.white;
        spellPage.SetActive(true);
    }

    void SetGameplayInput(bool state)
    {
        if (state)
        {
            myPlayer.input.actions["Move"].Enable();
            myPlayer.input.actions["Jump"].Enable();
            myPlayer.input.actions["Dash"].Enable();
            myPlayer.input.actions["CastFirstSpell"].Enable();
            myPlayer.input.actions["CastSecondSpell"].Enable();
            myPlayer.input.actions["CastThirdSpell"].Enable();
            myPlayer.input.actions["CastFourthSpell"].Enable();
        }
        else
        {
            myPlayer.input.actions["Move"].Disable();
            myPlayer.input.actions["Jump"].Disable();
            myPlayer.input.actions["Dash"].Disable();
            myPlayer.input.actions["CastFirstSpell"].Disable();
            myPlayer.input.actions["CastSecondSpell"].Disable();
            myPlayer.input.actions["CastThirdSpell"].Disable();
            myPlayer.input.actions["CastFourthSpell"].Disable();
        }
    }

    public void PauseInput(InputAction.CallbackContext context)
    {
        if (!isLocalPlayer && network) return;
        if (!context.started) return;
        if (GameManager.Instance.hordeController != null && !GameManager.Instance.hordeController.running) return;
        if (pausePanel.activeSelf && !spellPanel.activeSelf)
        {
            if(optionsPanel.activeSelf)
            {
                ClosePanel(optionsPanel);
            }
            else if(confirmLeavePanel.activeSelf)
            {
                ClosePanel(confirmLeavePanel);
            }
            else
            {
                pausePanel.SetActive(false);
                SetGameplayInput(true);
                caster.canCast = true;
                EnableGameplayCursor();
            }
        }
        else if (!pausePanel.activeSelf && !spellPanel.activeSelf)
        {
            pausePanel.SetActive(true);
            SetGameplayInput(false);
            caster.canCast = false;
            EnableUICursor();
        }
    }

    public void OpenSpellPanelInput(InputAction.CallbackContext context)
    {
        if (!isLocalPlayer && network) return;
        if (!context.started) return;
        if (myPlayer.dead) return;
        if (GameManager.Instance.hordeController != null && !GameManager.Instance.hordeController.running) return;
        if (spellPanel.activeSelf && !pausePanel.activeSelf)
        {
            if (editSpellPanel.activeSelf)
            {
                CloseEditSpellHUD();
            }
            else
            {
                spellPanel.SetActive(false);
                SetGameplayInput(true);
                if(GameManager.Instance.uiController.sharedUI != null)
                {
                    GameManager.Instance.uiController.sharedUI.SetActive(true);
                }
                caster.canCast = true;
                EnableGameplayCursor();
            }
        }
        else if (!spellPanel.activeSelf && !pausePanel.activeSelf)
        {
            if(GameManager.Instance.uiController.sharedUI != null)
            {
                GameManager.Instance.uiController.sharedUI.SetActive(false);
            }
            spellPanel.SetActive(true);
            ActivateSpellsInventoryPage();
            SetGameplayInput(false);
            caster.canCast = false;
            EnableUICursor();
        }
    }

    void SetupColors()
    {
        for (int i = 0; i < colorButtons.Length; i++)
        {
            int index = i;
            colorButtons[i].GetComponent<Image>().color = visualDatabase.colors[i];
            colorButtons[i].onClick.RemoveAllListeners();
            colorButtons[i].onClick.AddListener(() =>
            {
                SelectColor(index);
            });
        }
    }

    void SetupIcons()
    {
        int count = Mathf.Min(iconSlots.Length, visualDatabase.icons.Count);
        for (int i = 0; i < count; i++)
        {
            int index = i;
            iconSlots[i].sprite = visualDatabase.icons[i];
            iconButtons[i].onClick.RemoveAllListeners();
            iconButtons[i].onClick.AddListener(() =>
            {
                SelectIcon(index);
            });
        }
    }

    public void SelectColor(int index)
    {
        if (activeSpell == null) return;

        activeSpell.colorIndex = index;
        activeSpell.OnSpellUpdated?.Invoke();
        RefreshCustomizationPreview();
    }

    public void SelectIcon(int index)
    {
        if (activeSpell == null) return;

        activeSpell.symbolIndex = index;
        activeSpell.OnSpellUpdated?.Invoke();
        RefreshCustomizationPreview();
    }

    void RefreshCustomizationPreview()
    {
        if (activeSpell == null) return;

        previewImage.sprite = visualDatabase.icons[activeSpell.symbolIndex];
        previewImage.color = visualDatabase.colors[activeSpell.colorIndex];
        previewButtonImage.sprite = visualDatabase.icons[activeSpell.symbolIndex];
        previewButtonImage.color = visualDatabase.colors[activeSpell.colorIndex];
        UpdateEquipSlotIcons();
    }

    public void OpenCloseCustomizationPanel()
    {
        if (customizationPanel.activeSelf)
        {
            CloseCustomizationPanel();
        }
        else
        {
            OpenCustomizationPanel();
        }
    }

    public void OpenCustomizationPanel()
    {
        customizationPanel.SetActive(true);
        RefreshCustomizationPreview();
    }

    public void CloseCustomizationPanel()
    {
        customizationPanel.SetActive(false);
    }

    public void LeaveGameButton()
    {
        if(isServer)
        {
            SteamLobby.instance.LeaveLobby();
        }
        else
        {
            HandleClientLeave();
        }
        EnableUICursor();
    }

    [Command]
    public void HandleClientLeave()
    {
        foreach(SpellCollider col in SpellColliderManager.Instance.activeSpellColliders)
        {
            if(col.OwnerSpell.Caster == this.caster)
            {
                col.OnDeath.RemoveAllListeners();
                col.MarkedToDie = true;
            }
        }
        GameManager.Instance.RemovePlayer(myPlayer);
        FlowFieldManager.instance.UpdateFlowField();
        ClientLeaveGame(this.connectionToClient);
    }

    [TargetRpc]
    public void ClientLeaveGame(NetworkConnection target)
    {
        SteamLobby.instance.LeaveLobby();
    }

    public void EnableGameplayCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        crosshair.SetActive(true);
        myPlayer.playerCamera.GetComponent<CinemachineInputAxisController>().enabled = true;
        inGame = true;
    }

    public void EnableUICursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        crosshair.SetActive(false);
        myPlayer.playerCamera.GetComponent<CinemachineInputAxisController>().enabled = false;
        inGame = false;
    }

    public void TriggerSpellSelection(List<Spell> spells, Action<Spell> callback)
    {
        foreach (Transform child in triggerSpellContent)
        {
            if (child.name == "None") continue;
            Destroy(child.gameObject);
        }
        foreach (Spell spell in spells)
        {
            if (spell == null) continue;

            SpellSelectionEntry entry = Instantiate(spellEntryPrefab, triggerSpellContent);
            entry.Setup(spell, visualDatabase.icons[spell.symbolIndex], visualDatabase.colors[spell.colorIndex], callback);
        }
        triggerSpellSelectionPanel.SetActive(true);
    }

    public void CloseTriggerSpellSelection()
    {
        triggerSpellSelectionPanel.SetActive(false);
    }
}
