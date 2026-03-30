using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpellButtonUI : MonoBehaviour
{
    public TMP_InputField spellNameInput;
    public Button editButton;
    public Button equipButton;

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

    void UpdateSpellName(string newName)
    {
        spell.spellName = newName;
    }

    void EditSpell()
    {
        GameManager.Instance.uiController.OpenEditSpellHUD();
    }

    void EquipSpell()
    {
        Debug.Log("Equipar spell: " + spell.spellName);
    }
}
