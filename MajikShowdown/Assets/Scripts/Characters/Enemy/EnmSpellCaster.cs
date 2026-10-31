using Mirror;
using Mirror.BouncyCastle.Math.EC;
using Mirror.Examples.Basic;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static SpellInventoryUI;


public class EnmSpellCaster : SpellCaster
{
    public CasterEnemy enemy;

    int startingEquippedSpells = 0;
    List<StartingTrigger> startingTriggers = new List<StartingTrigger>();

    public Elements defaultElement;
    protected override void Awake()
    {
        if (!network)
        {
            enemy.caster = this;
            //DamageHandler = GetComponent<CharacterDamageHandler>();
            //equippedSpells = new Spell[4];
            foreach (var nodeData in ownedNodes)
            {
                InstantiateNode(nodeData);
            }
            CreateStartingSpells();
        }


        /*foreach (var grid in SpellGrids)
        {
            grid.caster = this;
        }*/
    }

    public override void OnStartServer()
    {
        enemy.caster = this;
        //DamageHandler = GetComponent<CharacterDamageHandler>();
        equippedSpells = new Spell[4];
        foreach (var nodeData in ownedNodes)
        {
            InstantiateNode(nodeData);
        }
        CreateStartingSpells();
    }
    public void CreateStartingSpells()
    {
        startingEquippedSpells = 0;
        startingTriggers.Clear();
        for (int i = 0; i < startingSpells.Count; i++)
        {
            CreateStartingSpell(startingSpells[i]);
        }

        foreach (StartingTrigger trigger in startingTriggers)
        {
            foreach (Spell spell in spells)
            {
                if (spell.spellName == trigger.triggerSpellName)
                {
                    trigger.node.TriggeredSpell = spell;
                    trigger.node.UpdateTrigger();
                }
            }
        }
    }
    public void CreateStartingSpell(SavedSpellAsset ssa)
    {
        Spell newSpell = new Spell(this);
        newSpell.spellName = ssa.spellName;
        newSpell.instanceIndex = spells.Count;
        spells.Add(newSpell);

        if (ssa.startEquipped && startingEquippedSpells < equippedSpells.Length)
        {
            equippedSpells[startingEquippedSpells] = newSpell;
            startingEquippedSpells++;
        }

        foreach (SavedSpellNode ssn in ssa.nodes)
        {
            SpellNode runtimeNode = AddRune(ssn.node, false);

            runtimeNode.OwnerSpell = newSpell;
            runtimeNode.startingNode = true;

            if (runtimeNode is SpellCore)
            {
                if(enemy.element != Elements.None)
                {
                    ((SpellCore)runtimeNode).Element = enemy.element;
                }
                else
                {
                    ((SpellCore)runtimeNode).Element = defaultElement;
                }
                newSpell.validSpell = true;
                newSpell.coreNode = runtimeNode as SpellCore;
            }
            if (ssn.triggeredSpellName != "" && runtimeNode is SpellTrigger)
            {
                startingTriggers.Add(new StartingTrigger((SpellTrigger)runtimeNode, ssn.triggeredSpellName));
            }
            newSpell.spellNodes.Add(runtimeNode);
        }
        newSpell.UpdateEnemySpell();
    }
    protected override SpellNode InstantiateNode(SpellNode nodePrefab)
    {
        SpellNode runtimeNode = Instantiate(nodePrefab);
        //runtimeNode.instanceIndex = commander.runtimeNodes.Count;
        runtimeNode.owner = this;
        //commander.runtimeNodes.Add(runtimeNode);
        /*if(isOwned || !network)
        {
            runtimeNode.ClientInitialize();
        }
        else
        {
            runtimeNode.ServerInitialize();
        }*/
        runtimeNode.ClientInitialize();
        runtimeNodes.Add(runtimeNode);
        return runtimeNode;
    }


    protected override void Update()
    {
        if (!isServer && network)
        {
            return;
        }
        if (!canCast)
        {
            return;
        }
        for (int i = 0; i < equippedSpells.Length; i++)
        {
            if (equippedSpells[i] != null && equippedSpells[i].onCooldown)
            {
                if (equippedSpells[i].cooldownTimer.timer(equippedSpells[i].SpellCooldown, Time.deltaTime, false, true))
                {
                    equippedSpells[i].onCooldown = false;
                    if(!isServer && network)
                    {
                        RemoveCooldown(i);
                    }
                }
            }
        }
        if(enemy != null && enemy.Casting)
        {
            if(castPoseTimer.timer(enemy.CastPoseTime,Time.deltaTime, false, false))
            {
                enemy.Casting = false;
            }
        }
    }



    /*[Command]
    public override void CMDCastSpell(int spellInd, Vector3 aimPoint)
    {
        Spell spell = equippedSpells[spellInd];
        if (spell.onCooldown) return;
        spell.onCooldown = true;
        AddCooldown(this.connectionToClient, spellInd);

        if (spell.validSpell)
        {
            enemy.CastAnim();
            //ServerInstantiateSpellCollider(spell, CastingPoint.position,transform.forward, true);
            if (spell.coreNode.castPoint == null)
            {
                SpellColliderManager.Instance.ServerInitializeSpellCollider(spell, CastingPoint, (aimPoint - CastingPoint.position).normalized, true);
                //ServerInstantiateSpellCollider(spell, CastingPoint, (AimController.AimPoint - CastingPoint.position).normalized, true);
            }
            else
            {
                Vector3 castPos = spell.coreNode.castPoint.GetCastPoint(transform, aimPoint);
                Vector3 dir = (aimPoint - castPos).normalized;
                //Vector3 castPos = spell.coreNode.castPoint.GetCastPoint(transform, AimController.AimPoint);
                //Vector3 dir = (AimController.AimPoint - castPos).normalized;
                if (dir == Vector3.zero)
                {
                    dir = aimPoint - transform.position;
                    //dir = AimController.AimPoint - transform.position;
                }
                SpellColliderManager.Instance.ServerInitializeSpellCollider(spell, castPos, dir, true);
            }
            if(enemy != null)
            {
                enemy.Casting = true;
                castPoseTimer.SetTimer(0);
            }
        }
    }*/



    public override void CastSpell(int spellInd)
    {
        /*if (network)
        {
            CMDCastSpell(spellInd, enemy.target.transform.position);
            return;
        }*/
        Spell spell = equippedSpells[spellInd];
        if (spell.onCooldown) return;
        spell.onCooldown = true;
        if (spell.validSpell)
        {
            enemy.CastAnim();
            spell.onCooldown = true;
            if (spell.coreNode.castPoint == null)
            {
                if(network)
                {
                    SpellColliderManager.Instance.ServerInitializeSpellCollider(spell, CastingPoint, enemy.target.transform.position - CastingPoint.position, true);
                }
                else
                {
                    SpellColliderManager.Instance.InitializeSpellCollider(spell, CastingPoint, enemy.target.transform.position - CastingPoint.position, true);
                }
            }
            else
            {
                Vector3 castPos = spell.coreNode.castPoint.GetCastPoint(transform, enemy.target.transform.position);
                Vector3 dir = (enemy.target.transform.position - castPos).normalized;
                if (dir == Vector3.zero)
                {
                    dir = enemy.targetVector;
                }
                if (network)
                {
                    SpellColliderManager.Instance.ServerInitializeSpellCollider(spell, castPos, dir, true);
                }
                else
                {
                    SpellColliderManager.Instance.InitializeSpellCollider(spell, castPos, dir, true);
                }
            }
            if(enemy != null)
            {
                enemy.Casting = true;
                castPoseTimer.SetTimer(0);
            }
        }
    }
}
