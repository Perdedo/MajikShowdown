using System.Collections.Generic;
using UnityEngine;

public class SpellInventoryUI : MonoBehaviour
{
    public SpellCaster caster;
    public List<Spell> spells = new List<Spell>();

    [Header("UI")]
    public Transform spellListContent;
    public GameObject spellButtonPrefab;

    public void CreateNewSpell()
    {
        Spell newSpell = new Spell(caster);
        spells.Add(newSpell);
        CreateSpellButton(newSpell);
    }

    void CreateSpellButton(Spell spell)
    {
        GameObject buttonObj = Instantiate(spellButtonPrefab, spellListContent);

        SpellButtonUI buttonUI = buttonObj.GetComponent<SpellButtonUI>();
        buttonUI.Setup(spell);
    }
}
