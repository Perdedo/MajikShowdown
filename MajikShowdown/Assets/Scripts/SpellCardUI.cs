using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpellCardUI : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI spellNameLabel;
    public TextMeshProUGUI cooldownLabel;

    public Button cardButton;
    public Button editButton;
    public Button deleteButton;

    Spell boundSpell;
    bool isSelected;
    Image cardColor;

    public void Setup(Spell spell)
    {
        cardColor = cardButton.GetComponent<Image>();
        boundSpell = spell;

        //Card Button Events
        cardButton.onClick.RemoveAllListeners();
        cardButton.onClick.AddListener(OnCardClicked);
        //Edit Spell Button Events
        editButton.onClick.RemoveAllListeners();
        editButton.onClick.AddListener(OpenEdit);
        //Delete Spell Button Events
        deleteButton.onClick.RemoveAllListeners();
        deleteButton.onClick.AddListener(Delete);

        editButton.gameObject.SetActive(false);
        deleteButton.gameObject.SetActive(false);
        RefreshUI();
        boundSpell.OnSpellUpdated += RefreshUI;
    }

    public Spell GetSpell()
    {
        return boundSpell;
    }

    void OnCardClicked()
    {
        if (!isSelected)
        {
            Select();
        }
        else
        {
            Deselect();
        }
    }

    void Select()
    {
        var spellInventory = FindAnyObjectByType<SpellInventoryUI>();
        if (spellInventory != null)
        {
            spellInventory.DeselectAllCards();
        }
        isSelected = true;
        editButton.gameObject.SetActive(true);
        deleteButton.gameObject.SetActive(true);
        cardColor.color = Color.cyan; 
        GameManager.Instance.uiController.StartEquipSpell(boundSpell);
    }

    public void Deselect()
    {
        isSelected = false;
        editButton.gameObject.SetActive(false);
        deleteButton.gameObject.SetActive(false);
        cardColor.color = Color.white;
    }

    void OpenEdit()
    {
        Deselect();
        GameManager.Instance.uiController.OpenEditSpellHUD(boundSpell);
    }

    void RefreshUI()
    {
        if (boundSpell == null) return; 
        spellNameLabel.text = boundSpell.spellName;
        cooldownLabel.text = "Cooldown: " + boundSpell.SpellCooldown.ToString("0.0") + "s";
    }

    void Delete()
    {
        if (boundSpell == null) return;
        for (int i = 0; i < boundSpell.Caster.equippedSpells.Length; i++)
        {
            if (boundSpell.Caster.equippedSpells[i] == boundSpell)
            {
                boundSpell.Caster.equippedSpells[i] = null;
                GameManager.Instance.uiController.equipSlotTexts[i].text = "Spell Slot " + (i + 1);
            }
        }
        if (boundSpell.grid != null)
        {
            boundSpell.grid.ReturnAllNodesToInventory();
            Destroy(boundSpell.grid.gameObject);
        }
        boundSpell.Caster.spells.Remove(boundSpell);
        boundSpell.OnSpellUpdated -= RefreshUI;
        Destroy(gameObject);
        boundSpell = null;
    }

    void OnDestroy()
    {
        if (boundSpell != null)
        {
            boundSpell.OnSpellUpdated -= RefreshUI;
        }
    }
}