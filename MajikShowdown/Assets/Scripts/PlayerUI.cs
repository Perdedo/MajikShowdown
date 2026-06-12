using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerUI : NetworkBehaviour
{
    [Header("Test Panels")]
    public GameObject spellPanel;
    public GameObject createSpellPanel;
    public GameObject editSpellPanel;
    public GameObject pausePanel;
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

    [Header("Spell Customization")]
    [SerializeField] private SpellVisualDatabase visualDatabase;
    [SerializeField] private GameObject customizationPanel;
    [SerializeField] private Image previewImage;
    [SerializeField] private Button[] colorButtons;
    [SerializeField] private Image[] iconSlots;
    [SerializeField] private Button[] iconButtons;
    [SerializeField] private Image[] cooldownIcons;

    [HideInInspector]
    public ConfigData data;
    public HexGrid activeGrid;
    public SpellNodeDescription spellNodeDescription;
    Spell activeSpell;
    public SpellNodeInterface selectedNode;
    public SpellInventoryUI inventory;
    public Player myPlayer;
    PlayerDamageHandler damageHandler;

    [Header("Network")]
    public bool network = true;
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
        InitializeStatsUI();
        damageHandler = myPlayer.GetComponent<PlayerDamageHandler>();
        healthSlider.maxValue = damageHandler.MaxHealth;
        healthSlider.value = damageHandler.Health;
        SetupColors();
        SetupIcons();
    }

    private void Update()
    {
        if (!isLocalPlayer && network) return;
        UpdateHealthUI();
        UpdateCooldownFills();
        UpdateCooldownIcon();
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

    void UpdateHealthUI()
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
        if(!isServer && network)
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

        if (pausePanel.activeSelf && !spellPanel.activeSelf)
        {
            pausePanel.SetActive(false);
            SetGameplayInput(true);
            myPlayer.playerCamera.GetComponent<CinemachineInputAxisController>().enabled = true;
            caster.canCast = true;
        }
        else if (!pausePanel.activeSelf && !spellPanel.activeSelf)
        {
            pausePanel.SetActive(true);
            SetGameplayInput(false);
            myPlayer.playerCamera.GetComponent<CinemachineInputAxisController>().enabled = false;
            caster.canCast = false;
        }
    }

    public void OpenSpellPanelInput(InputAction.CallbackContext context)
    {
        if (!isLocalPlayer && network) return;
        if (!context.started) return;

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
                if(GameManager.Instance.hordeController != null)
                {
                    GameManager.Instance.hordeController.timerTxt.gameObject.SetActive(true);
                }
                myPlayer.playerCamera.GetComponent<CinemachineInputAxisController>().enabled = true;
                caster.canCast = true;
            }
        }
        else if (!spellPanel.activeSelf && !pausePanel.activeSelf)
        {
            if (GameManager.Instance.hordeController != null && !GameManager.Instance.hordeController.inPause)
            {
                return;
            }
            if(GameManager.Instance.hordeController != null)
            {
                GameManager.Instance.hordeController.timerTxt.gameObject.SetActive(false);
            }
            spellPanel.SetActive(true);
            ActivateSpellsInventoryPage();
            SetGameplayInput(false);
            myPlayer.playerCamera.GetComponent<CinemachineInputAxisController>().enabled = false;
            caster.canCast = false;
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
        int count = Mathf.Min(
            iconSlots.Length,
            visualDatabase.icons.Count
        );
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

        }
    }

    [Command]
    public void HandleClientLeave()
    {

    }
}
