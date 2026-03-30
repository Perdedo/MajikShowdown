using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpellButtonUI : MonoBehaviour
{
    public TMP_InputField spellNameInput;
    public Button editButton;
    public Button equipButton;
    public TextMeshProUGUI equipText;

    Spell spell;

    public void Setup(Spell newSpell)
    {
        spell = newSpell;
        spellNameInput.onValueChanged.RemoveAllListeners();
        spellNameInput.onValueChanged.AddListener(UpdateSpellName);
        editButton.onClick.RemoveAllListeners();
        editButton.onClick.AddListener(EditSpell);
        equipButton.onClick.RemoveAllListeners();
        equipButton.onClick.AddListener(EquipSpell);
    }

    public Spell GetSpell()
    {
        return spell;
    }

    void UpdateSpellName(string newName)
    {
        spell.spellName = newName;
    }

    void EditSpell()
    {
        GameManager.Instance.uiController.OpenEditSpellHUD(spell);
    }

    void EquipSpell()
    {
        if (IsSpellEquipped(spell))
        {
            UnequipSpell();
        }
        else
        {
            GameManager.Instance.uiController.StartEquipSpell(spell);
        }
    }

    void UnequipSpell()
    {
        for (int i = 0; i < spell.Owner.equippedSpells.Length; i++)
        {
            if (spell.Owner.equippedSpells[i] == spell)
            {
                spell.Owner.equippedSpells[i] = null;
                GameManager.Instance.uiController.equipSlotTexts[i].text = "Slot " + (i + 1);
                break;
            }
        }
        equipText.text = "Equip";
    }

    public bool IsSpellEquipped(Spell spell)
    {
        foreach (var s in spell.Owner.equippedSpells)
        {
            if (s == spell)
                return true;
        }
        return false;
    }

    public void SetEquipText(string text)
    {
        equipText.text = text;
    }
}