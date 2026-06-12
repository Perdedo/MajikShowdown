using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

//using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static SpellTrigger;

[Serializable]
public class Spell
{
    public string spellName;
    public int colorIndex;
    public int symbolIndex;
    public readonly SpellCaster Caster;
    //public List<SubSpell> SubSpells = new List<SubSpell>();
    public List<SpellNode> spellNodes = new List<SpellNode>();
    public float SpellCooldown = 0;
    public float auxCooldown = 0;
    public SpellType coreNode;
    public List<SpellTrigger> triggers = new List<SpellTrigger>();
    public List<SpellEffect> spellEffects = new List<SpellEffect>();
    public bool validSpell;
    public HexGrid grid;
    public int instanceIndex;
    [HideInInspector] public System.Action OnSpellUpdated;
    [NonSerialized] public LayerMask spellCollisionLayers;
    public Timer cooldownTimer = new Timer(false);
    [NonSerialized] public bool onCooldown = false;
    public Spell(SpellCaster owner)
    {
        Caster = owner;
    }
    public HashSet<SpellTrigger> triggerCalls = new HashSet<SpellTrigger>();
    int callAux = 0;
    public void UpdateSpell(SpellTrigger updatedCall = null)
    {
        //CreateSubSpells();
        if (coreNode == null)
        {
            SpellCooldown = 0;
            spellNodes.Clear();
            triggers.Clear();
            spellEffects.Clear();

            OnSpellUpdated?.Invoke();
            return;
        }
        coreNode.hierarchy = 0;
        SpellCooldown = 0;
        auxCooldown = 0;

        spellNodes = coreNode.GetSpellList(new List<SpellNode>());
        coreNode.StatBuffs.Clear();
        triggers.Clear();
        spellEffects.Clear();
        foreach (SpellNode s in spellNodes)
        {
            if (s != coreNode)
            {
                coreNode.AddBuff(s.BaseStats);
            }
            s.OwnerSpell = this;
            if (s is SpellTrigger t)
            {
                triggers.Add(t);
                SpellCooldown += s.Cooldown;
                continue;
            }
            if (s is SpellEffect e)
            {
                if (e.Repeatable || !spellEffects.Any(x => x.GetType() == e.GetType()))
                {
                    spellEffects.Add(e);
                }
            }

            SpellCooldown += s.Cooldown;
            auxCooldown += s.Cooldown;
        }
        SpellCooldown = Mathf.Max(SpellCooldown, 0.1f);
        auxCooldown = Mathf.Max(auxCooldown, 0.1f);
        if (updatedCall != null)
        {
            if(callAux == 0)
            {
                Caster.StartCoroutine(ResetCalls());
            }
            callAux++;
            triggerCalls.Add(updatedCall);
        }
        foreach (SpellNode n in Caster.runtimeNodes)
        {
            if (n is SpellTrigger trigger && !triggerCalls.Contains(trigger) && trigger.TriggeredSpell == this)
            {
                trigger.UpdateTrigger();
            }
        }

        spellCollisionLayers = 0;
        if (coreNode.Collisions.Objects)
        {
            spellCollisionLayers |= Caster.ObjectLayer;
        }
        if (coreNode.Collisions.Players)
        {
            spellCollisionLayers |= Caster.PlayerLayer;
        }
        if (coreNode.Collisions.Enemies)
        {
            spellCollisionLayers |= Caster.EnemyLayer;
        }
        coreNode.UpdateNode();
        OnSpellUpdated?.Invoke();
        /*foreach(SubSpell s in SubSpells)
        {
            SpellCooldown += s.CooldownCost;
            s.UpdateSubSpell();
        }*/
    }
    /*public void CreateSubSpells()
    {
        SubSpells.Clear();
        foreach (SpellNode spellNode in spellNodes)
        {
            if (spellNode is SpellType t)
            {
                SubSpells.Add(new SubSpell(t, this));
            }
        }
    }*/
    public IEnumerator ResetCalls()
    {
        yield return new WaitForEndOfFrame();
        callAux = 0;
        triggerCalls.Clear();
    }
}


/*[Serializable]
public class SubSpell
{
    public SpellType Type;
    public Spell spell;
    public float CooldownCost = 0;
    public List<SpellTrigger> triggers;
    public List<SpellNode> spellNodes;
    public SubSpell(SpellType type, Spell spellOwner)
    {
        Type = type;
        spell = spellOwner;
        triggers = new List<SpellTrigger>();
        //spellNodes.Add(type);
    }
    public void UpdateSubSpell()
    {
        spellNodes = Type.GetSubspellList(new List<SpellNode>());
        Type.StatBuffs.Clear();
        triggers.Clear();
        foreach (SpellNode s in spellNodes)
        {
            if(s is SpellTrigger t)
            {
                triggers.Add(t);
            }
            CooldownCost += s.Cooldown;
            if(s != Type)
            {
                Type.AddBuff(s.BaseStats);
            }
            s.OwnerSubspell = this;
        }
        Type.CalculateFinalStats();
    }
}*/
