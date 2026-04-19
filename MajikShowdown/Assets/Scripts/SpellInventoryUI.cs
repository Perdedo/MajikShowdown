using System.Collections.Generic;
using UnityEngine;

public class SpellInventoryUI : MonoBehaviour
{
    [Header("Data")]
    public SpellCaster caster;

    [Header("Grid")]
    public HexGrid gridPrefab;
    public Transform gridParent;

    [Header("UI")]
    public Transform content;
    public GameObject spellCardPrefab;
    public Transform createSpellCard;

    int nextSpellId = 1;

    public void CreateNewSpell()
    {
        DeselectAllCards();
        Spell newSpell = new Spell(caster);
        newSpell.spellName = GenerateSpellName();
        HexGrid newGrid = Instantiate(gridPrefab, gridParent);
        newGrid.caster = caster;
        newGrid.SetSpell(newSpell);
        newGrid.Initialize();
        newGrid.gameObject.SetActive(false);
        newSpell.grid = newGrid;
        caster.spells.Add(newSpell);
        CreateSpellCard(newSpell);
        GameManager.Instance.uiController.spellNodeDescription.RefreshTriggerUI();
    }

    string GenerateSpellName()
    {
        return "Nameless";
    }

    void CreateSpellCard(Spell spell)
    {
        GameObject cardObj = Instantiate(spellCardPrefab, content);
        cardObj.transform.SetSiblingIndex(createSpellCard.GetSiblingIndex());
        SpellCardUI cardUI = cardObj.GetComponent<SpellCardUI>();
        cardUI.Setup(spell);
        createSpellCard.SetAsLastSibling();
    }

    public void DeselectAllCards()
    {
        SpellCardUI[] cards = GetComponentsInChildren<SpellCardUI>();
        foreach (var card in cards)
        {
            card.Deselect();
        }
    }
}