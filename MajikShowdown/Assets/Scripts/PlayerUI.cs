using Mirror;
using System;
using System.Collections.Generic;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
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

    [HideInInspector]
    public ConfigData data;
    public HexGrid activeGrid;
    public SpellNodeDescription spellNodeDescription;
    Spell activeSpell;
    public SpellNodeInterface selectedNode;
    public SpellInventoryUI inventory;
    public Player myPlayer;

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
        InitializeStatsUI();
    }

    public void Update()
    {
        if(!isLocalPlayer && network)
        {
            return;
        }
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (spellPanel.activeSelf)
            {
                if (editSpellPanel.activeSelf)
                {
                    CloseEditSpellHUD();
                }
                else
                {
                    spellPanel.SetActive(false);
                    myPlayer.input.ActivateInput();
                    myPlayer.playerCamera.GetComponent<CinemachineInputAxisController>().enabled = true;
                    caster.canCast = true;
                }
            }
            else
            {
                spellPanel.SetActive(true);
                ActivateSpellsInventoryPage();
                Debug.Log(myPlayer);
                Debug.Log(myPlayer.input);
                myPlayer.input.DeactivateInput();
                myPlayer.playerCamera.GetComponent<CinemachineInputAxisController>().enabled = false;
                caster.canCast = false;
            }
        }
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
        if (activeSpell.primaryNode == null)
        {
            InitializeStatsUI();
            return;
        }

        var stats = activeSpell.primaryNode.FinalStats;
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
        editSpellPanel.gameObject.SetActive(false);
        spellNameInput.onValueChanged.RemoveAllListeners();
        spellNodeDescription.HideDescription();

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

        spellToEquip = null;

        if (inventory != null)
        {
            inventory.DeselectAllCards();
        }
    }

    [Command]
    public void CMDEquipSpell(int index)
    {
        for (int i = 0; i < caster.equippedSpells.Length; i++)
        {
            if (caster.equippedSpells[i] == spellToEquip)
            {
                caster.equippedSpells[i] = null;
            }
        }

        caster.equippedSpells[index] = spellToEquip;

        spellToEquip = null;
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
}
