
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpellCardUI : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI spellNameLabel;
    public TextMeshProUGUI cooldownLabel;

    public Button cardButton;
    public Button unequipButton;
    public Button deleteButton;

    public Spell boundSpell;
    public bool isSelected;
    public Image cardColor;
    public int instanceIndex;
    public SpellInventoryUI spellInventory;
    public Image spellIcon;
    public SpellVisualDatabase visualDatabase;

    [Header("Double Click")]
    [SerializeField] private float doubleClickTime = 0.3f;

    private Coroutine clickCoroutine;
    private int clickCount;

    public void Setup(Spell spell)
    {
        if (boundSpell != null) boundSpell.OnSpellUpdated -= RefreshUI;

        cardColor = cardButton.GetComponent<Image>();
        boundSpell = spell;
        isSelected = false;
        clickCount = 0;

        if (clickCoroutine != null)
        {
            StopCoroutine(clickCoroutine);
            clickCoroutine = null;
        }

        // Card Button Events
        cardButton.onClick.RemoveAllListeners();
        cardButton.onClick.AddListener(HandleCardClick);

        // Unequip Spell Button Events
        unequipButton.onClick.RemoveAllListeners();
        unequipButton.onClick.AddListener(Unequip);

        // Delete Spell Button Events
        deleteButton.onClick.RemoveAllListeners();
        deleteButton.onClick.AddListener(Delete);

        unequipButton.gameObject.SetActive(false);
        deleteButton.gameObject.SetActive(false);

        RefreshUI();
        boundSpell.OnSpellUpdated += RefreshUI;
    }

    public Spell GetSpell()
    {
        return boundSpell;
    }


    void HandleCardClick()
    {
        if (boundSpell == null) return;

        if (clickCount == 1)
        {
            if (clickCoroutine != null) StopCoroutine(clickCoroutine);

            clickCoroutine = null;
            clickCount = 0;
            OpenEdit();
            return;
        }

        clickCount = 1;
        OnCardClicked();
        clickCoroutine = StartCoroutine(ResetClickCount());
    }

    IEnumerator ResetClickCount()
    {
        yield return new WaitForSecondsRealtime(doubleClickTime);
        clickCount = 0;
        clickCoroutine = null;
    }

    void OnCardClicked()
    {
        if (boundSpell == null) return;

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
        if (boundSpell == null) return;

        if (spellInventory != null) spellInventory.DeselectAllCards();

        isSelected = true;
        unequipButton.gameObject.SetActive(IsEquipped());
        deleteButton.gameObject.SetActive(true);
        cardColor.color = Color.cyan;

        GameManager.Instance.uiController.playerUI.StartEquipSpell(boundSpell);
        boundSpell.Caster.commander.SelectSCUI(this);
    }

    public void Deselect()
    {
        isSelected = false;
        unequipButton.gameObject.SetActive(false);
        deleteButton.gameObject.SetActive(false);
        cardColor.color = Color.white;

        if (GameManager.Instance.uiController.playerUI.spellToEquip == boundSpell) GameManager.Instance.uiController.playerUI.spellToEquip = null;

        if (boundSpell != null) boundSpell.Caster.commander.DeselectSCUI(this);
    }

    void OpenEdit()
    {
        if (boundSpell == null) return;

        Deselect();
        GameManager.Instance.uiController.playerUI.OpenEditSpellHUD(boundSpell);
    }

    void Unequip()
    {
        if (boundSpell == null) return;

        GameManager.Instance.uiController.playerUI.UnequipSpell(boundSpell);
        Deselect();
    }

    public bool IsEquipped()
    {
        if (boundSpell == null) return false;

        foreach (Spell spell in boundSpell.Caster.equippedSpells)
        {
            if (spell == boundSpell) return true;
        }

        return false;
    }

    public void RefreshUI()
    {
        if (boundSpell == null) return;

        spellNameLabel.text = boundSpell.spellName;
        cooldownLabel.text = boundSpell.SpellCooldown.ToString("0.0") + "s";
        spellIcon.sprite = visualDatabase.icons[boundSpell.symbolIndex];
        spellIcon.color = visualDatabase.colors[boundSpell.colorIndex];
    }

    void Delete()
    {
        if (boundSpell == null) return;

        Spell deletedSpell = boundSpell;
        SpellCaster caster = deletedSpell.Caster;

        foreach (SpellNode node in caster.runtimeNodes)
        {
            if (node is not SpellTrigger trigger) continue;

            if (trigger.TriggeredSpell == deletedSpell)
            {
                trigger.TriggeredSpell = null;
                trigger.UpdateTrigger();
            }
        }

        caster.commander.DeleteSCUI(this);

        for (int i = 0; i < caster.equippedSpells.Length; i++)
        {
            if (caster.equippedSpells[i] == deletedSpell)
            {
                caster.equippedSpells[i] = null;
                GameManager.Instance.uiController.playerUI.equipSlotTexts[i].text = "Slot " + (i + 1);
            }
        }

        GameManager.Instance.uiController.playerUI.UpdateEquipSlotIcons();

        if (deletedSpell.grid != null)
        {
            deletedSpell.grid.ReturnAllNodesToInventory();
            Destroy(deletedSpell.grid.gameObject);
        }

        caster.spells.Remove(deletedSpell);
        deletedSpell.OnSpellUpdated -= RefreshUI;

        if (clickCoroutine != null) StopCoroutine(clickCoroutine);

        clickCoroutine = null;
        clickCount = 0;
        boundSpell = null;

        Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (clickCoroutine != null) StopCoroutine(clickCoroutine);

        if (boundSpell != null) boundSpell.OnSpellUpdated -= RefreshUI;
    }
}
