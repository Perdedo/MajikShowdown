using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellInventoryUI : NetworkBehaviour
{
    [Header("Data")]
    public SpellCaster caster;
    //public List<SavedSpellAsset> startingSpells = new List<SavedSpellAsset>();

    [Header("Grid")]
    public HexGrid gridPrefab;
    public Transform gridParent;

    [Header("UI")]
    public Transform content;
    public GameObject spellCardPrefab;
    public Transform createSpellCard;
    public PlayerUI playerUI;

    [Header("Network")]
    public bool network = true;

    int startingEquippedSpells = 0;
    List<StartingTrigger> startingTriggers = new List<StartingTrigger>();

    public struct StartingTrigger
    {
        public SpellTrigger node;
        public string triggerSpellName;

        public StartingTrigger(SpellTrigger node, string triggerSpellName)
        {
            this.node = node;
            this.triggerSpellName = triggerSpellName;
        }
    }
    /*public override void OnStartLocalPlayer()
    {
        for (int i = 0; i < caster.startingSpells.Count; i++)
        {
            CreateStartingSpell(caster.startingSpells[i], i);
        }
        caster.startingSpellsReady = true;
        if (!isServer && network)
        {
            if (NetworkClient.ready)
            {
                CMDCreateStartingSpells();
            }
            else
            {
                StartCoroutine(WaitCreateStartingSpells());
            }
        }
    }*/

    public void CreateStartingSpells()
    {
        startingEquippedSpells = 0;
        startingTriggers.Clear();
        for (int i = 0; i < caster.startingSpells.Count; i++)
        {
            CreateStartingSpell(caster.startingSpells[i]);
        }

        foreach (StartingTrigger trigger in startingTriggers)
        {
            foreach (Spell spell in caster.spells)
            {
                if (spell.spellName == trigger.triggerSpellName)
                {
                    trigger.node.TriggeredSpell = spell;
                    trigger.node.UpdateTrigger();
                }
            }
        }

        caster.startingSpellsReady = true;

        if (!isServer && network)
        {
            if (NetworkClient.ready)
            {
                CMDCreateStartingSpells();
            }
            else
            {
                StartCoroutine(WaitCreateStartingSpells());
            }
        }
    }

    public void CreateNewSpell()
    {
        if(isLocalPlayer || !network)
        {
            DeselectAllCards();
            GameManager.Instance.uiController.playerUI.spellToEquip = null;
            Spell newSpell = new Spell(caster);
            newSpell.spellName = GenerateSpellName();
            newSpell.instanceIndex = caster.spells.Count;
            HexGrid newGrid = Instantiate(gridPrefab, gridParent);
            newGrid.caster = caster;
            newGrid.instanceIndex = caster.commander.gridIndexRef;
            caster.commander.gridIndexRef++;
            //newGrid.instanceIndex = caster.spells.Count;
            caster.commander.grids.Add(newGrid);
            newGrid.SetSpell(newSpell);
            newSpell.grid = newGrid;
            newGrid.Initialize();
            newGrid.gameObject.SetActive(false);
            caster.spells.Add(newSpell);
            CreateSpellCard(newSpell);
            GameManager.Instance.uiController.playerUI.spellNodeDescription.RefreshTriggerUI();
            if (!isServer && network)
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
        GameManager.Instance.uiController.playerUI.spellToEquip = null;
        Spell newSpell = new Spell(caster);
        newSpell.spellName = GenerateSpellName();
        newSpell.instanceIndex = caster.spells.Count;
        HexGrid newGrid = Instantiate(gridPrefab, gridParent);
        newGrid.caster = caster;
        //newGrid.instanceIndex = caster.spells.Count;
        newGrid.instanceIndex = caster.commander.gridIndexRef;
        caster.commander.gridIndexRef++;
        caster.commander.grids.Add(newGrid);
        newGrid.SetSpell(newSpell);
        newSpell.grid = newGrid;
        newGrid.Initialize();
        newGrid.gameObject.SetActive(false);
        caster.spells.Add(newSpell);
        CreateSpellCard(newSpell);
        GameManager.Instance.uiController.playerUI.spellNodeDescription.RefreshTriggerUI();
    }

    public void CreateStartingSpell(SavedSpellAsset ssa)
    {
        Spell newSpell = new Spell(caster);
        newSpell.spellName = ssa.spellName;
        newSpell.instanceIndex = caster.spells.Count;

        HexGrid newGrid = Instantiate(gridPrefab, gridParent);
        newGrid.caster = caster;
        newGrid.instanceIndex = caster.commander.gridIndexRef;
        caster.commander.gridIndexRef++;
        caster.commander.grids.Add(newGrid);

        newGrid.SetSpell(newSpell);
        newSpell.grid = newGrid;

        newGrid.Initialize();
        newGrid.gameObject.SetActive(false);

        caster.spells.Add(newSpell);

        newSpell.colorIndex = ssa.colorIndex;
        newSpell.symbolIndex = ssa.symbolIndex;

        CreateSpellCard(newSpell);

        if(ssa.startEquipped && startingEquippedSpells < caster.equippedSpells.Length)
        {
            playerUI.StartEquipSpell(newSpell);
            playerUI.EquipSpellToSlot(startingEquippedSpells);
            startingEquippedSpells++;
        }

        List<SpellNode> startingRuntimeNodes = new List<SpellNode>();

        foreach (SavedSpellNode ssn in ssa.nodes)
        {
            SpellNode runtimeNode = caster.AddRune(ssn.node, false);

            runtimeNode.OwnerSpell = newSpell;
            runtimeNode.startingGridInd = ssn.gridIndex;
            runtimeNode.startingNode = true;

            startingRuntimeNodes.Add(runtimeNode);

            if(runtimeNode is SpellTrigger)
            {
                startingTriggers.Add(new StartingTrigger((SpellTrigger)runtimeNode, ssn.triggeredSpellName));
            }
        }

        foreach (SpellNode runtimeNode in startingRuntimeNodes)
        {
            foreach (NodeInventory inventory in caster.inventories)
            {
                inventory.EnsureNode(runtimeNode);
            }
        }
    }

    IEnumerator WaitCreateStartingSpells()
    {
        yield return new WaitUntil(() => NetworkClient.ready);
        CMDCreateStartingSpells();
    }

    [Command]
    public void CMDCreateStartingSpells()
    {
        startingEquippedSpells = 0;
        startingTriggers.Clear();
        for (int i = 0; i < caster.startingSpells.Count; i++)
        {
            CreateStartingSpell(caster.startingSpells[i]);
        }

        foreach(StartingTrigger trigger in startingTriggers)
        {
            foreach(Spell spell in caster.spells)
            {
                if(spell.spellName == trigger.triggerSpellName)
                {
                    trigger.node.TriggeredSpell = spell;
                    trigger.node.UpdateTrigger();
                }
            }
        }
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
        cardUI.spellInventory = this;
        cardUI.Setup(spell);
        cardUI.instanceIndex = caster.commander.cardIndexRef;
        caster.commander.cardIndexRef++;
        //cardUI.instanceIndex = caster.commander.cards.Count;
        caster.commander.cards.Add(cardUI);
        createSpellCard.SetAsLastSibling();
    }

    public void DeselectAllCards()
    {
        if (isLocalPlayer || !network)
        {
            SpellCardUI[] cards = GetComponentsInChildren<SpellCardUI>();
            foreach (var card in cards)
            {
                if (card.isSelected)
                {
                    card.Deselect();
                }
            }
            GameManager.Instance.uiController.playerUI.spellToEquip = null;
        }
    }
}