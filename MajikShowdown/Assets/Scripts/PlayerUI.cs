using Mirror;
using System;
using System.Collections.Generic;
using TMPro;
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

    [HideInInspector]
    public ConfigData data;
    public HexGrid activeGrid;
    public SpellNodeDescription spellNodeDescription;
    Spell activeSpell;
    public SpellNodeInterface selectedNode;
    public SpellInventoryUI inventory;
    private void Start()
    {
        if(isLocalPlayer)
        {
            GameManager.Instance.uiController.playerUI = this;
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public void Update()
    {
        if(!isLocalPlayer)
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
                }
            }
            else
            {
                spellPanel.SetActive(true);
                ActivateSpellsInventoryPage();
            }
        }
    }

    public void OpenEditSpellHUD(Spell spell)
    {
        if (!isLocalPlayer)
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
        if (!isLocalPlayer)
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
        if (!isLocalPlayer)
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
        if (!isLocalPlayer)
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

    void RefreshSpellInfo()
    {
        if (!isLocalPlayer)
        {
            return;
        }
        if (activeSpell == null) return;
        spellCooldownText.text = "Cooldown: " + activeSpell.SpellCooldown.ToString("0.0") + "s";
    }

    public void CloseEditSpellHUD()
    {
        if (!isLocalPlayer)
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
        if (!isLocalPlayer)
        {
            return;
        }
        spellToEquip = spell;
    }

    public void EquipSpellToSlot(int index)
    {
        if (!isLocalPlayer)
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
        if (!isLocalPlayer)
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
        if (!isLocalPlayer)
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
        if (!isLocalPlayer)
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
        if (!isLocalPlayer)
        {
            return;
        }
        runesInventoryPageButton.GetComponent<Image>().color = Color.grey;
        runePage.SetActive(false);

        spellsInventoryPageButton.GetComponent<Image>().color = Color.white;
        spellPage.SetActive(true);
    }
}
