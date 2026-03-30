using System.Collections.Generic;
using UnityEngine;

public class SpellInventoryUI : MonoBehaviour
{
    public SpellCaster caster;
    public List<Spell> spells = new List<Spell>();
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
        spells.Add(newSpell);
        CreateSpellButton(newSpell);
    }

    void CreateSpellButton(Spell spell)
    {
        GameObject buttonObj = Instantiate(spellPrefab, spellListContent);
        SpellButtonUI buttonUI = buttonObj.GetComponent<SpellButtonUI>();
        buttonUI.Setup(spell);
    }
}
