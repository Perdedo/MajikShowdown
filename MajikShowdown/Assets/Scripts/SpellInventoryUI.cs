using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellInventoryUI : NetworkBehaviour
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

    [Header("Network")]
    public bool network = true;
    public void CreateNewSpell()
    {
        if(isLocalPlayer || !network)
        {
            DeselectAllCards();
            Spell newSpell = new Spell(caster);
            newSpell.spellName = GenerateSpellName();
            HexGrid newGrid = Instantiate(gridPrefab, gridParent);
            newGrid.caster = caster;
            newGrid.instanceIndex = caster.spells.Count;
            caster.commander.grids.Add(newGrid);
            newGrid.SetSpell(newSpell);
            newGrid.Initialize();
            newGrid.gameObject.SetActive(false);
            newSpell.grid = newGrid;
            Debug.Log(caster);
            Debug.Log(caster.spells);
            Debug.Log(newSpell);
            caster.spells.Add(newSpell);
            CreateSpellCard(newSpell);
            GameManager.Instance.uiController.playerUI.spellNodeDescription.RefreshTriggerUI();
            if(!isServer && network)
            {
                if(NetworkClient.ready)
                {
                    CMDCreateNewSpell();
                }
                else
                {
                    StartCoroutine(WaitCreateNewSpell());
                }
            }
        }
    }
    IEnumerator WaitCreateNewSpell()
    {
        yield return new WaitUntil(() => NetworkClient.ready);
        CMDCreateNewSpell();
    }
    [Command]
    public void CMDCreateNewSpell()
    {
        DeselectAllCards();
        Spell newSpell = new Spell(caster);
        newSpell.spellName = GenerateSpellName();
        HexGrid newGrid = Instantiate(gridPrefab, gridParent);
        newGrid.caster = caster;
        newGrid.instanceIndex = caster.spells.Count;
        caster.commander.grids.Add(newGrid);
        newGrid.SetSpell(newSpell);
        newGrid.Initialize();
        newGrid.gameObject.SetActive(false);
        newSpell.grid = newGrid;
        Debug.Log(caster);
        Debug.Log(caster.spells);
        Debug.Log(newSpell);
        caster.spells.Add(newSpell);
        CreateSpellCard(newSpell);
        GameManager.Instance.uiController.playerUI.spellNodeDescription.RefreshTriggerUI();
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
        cardUI.instanceIndex = caster.commander.cards.Count;
        caster.commander.cards.Add(cardUI);
        createSpellCard.SetAsLastSibling();
    }

    public void DeselectAllCards()
    {
        if(isLocalPlayer || !network)
        {
            SpellCardUI[] cards = GetComponentsInChildren<SpellCardUI>();
            foreach (var card in cards)
            {
                card.Deselect();
            }
        }
    }
}