using System.Collections.Generic;
using UnityEngine;

public class SpellInventoryUI : MonoBehaviour
{
    public SpellCaster caster;
    public HexGrid gridPrefab;
    public Transform gridParent;

    [Header("UI")]
    public Transform spellListContent;
    public GameObject spellPrefab;

    public void CreateNewSpell()
    {
        Spell newSpell = new Spell(caster);
        HexGrid newGrid = Instantiate(gridPrefab, gridParent);
        newGrid.caster = caster;
        newGrid.gameObject.SetActive(false);
        newGrid.SetSpell(newSpell);
        newSpell.grid = newGrid;
        caster.spells.Add(newSpell);
        CreateSpellButton(newSpell);
        GameManager.Instance.uiController.spellNodeDescription.RefreshTriggerDropdown();
    }

    void CreateSpellButton(Spell spell)
    {
        GameObject buttonObj = Instantiate(spellPrefab, spellListContent);
        SpellButtonUI buttonUI = buttonObj.GetComponent<SpellButtonUI>();
        buttonUI.Setup(spell);
    }
}
