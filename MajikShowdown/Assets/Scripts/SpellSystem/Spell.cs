using System;
using System.Collections.Generic;
using System.Linq;

//using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static SpellTrigger;

[Serializable]
public class Spell
{
    public string spellName;
    public readonly SpellCaster Owner;
    //public List<SubSpell> SubSpells = new List<SubSpell>();
    public List<SpellNode> spellNodes = new List<SpellNode>();
    public float SpellCooldown = 0;
    public SpellType primaryNode;
    public List<SpellTrigger> triggers = new List<SpellTrigger>();
    public List<SpellEffect> spellEffects = new List<SpellEffect>();
    public bool validSpell;
    public HexGrid grid;
    public Spell(SpellCaster owner)
    {
        Owner = owner;
    }

    public void UpdateSpell()
    {
        //CreateSubSpells();
        primaryNode.hierarchy = 0;
        SpellCooldown = 0;

        spellNodes = primaryNode.GetSpellList(new List<SpellNode>());
        primaryNode.StatBuffs.Clear();
        triggers.Clear();
        spellEffects.Clear();
        foreach (SpellNode s in spellNodes)
        {
            if (s is SpellTrigger t)
            {
                triggers.Add(t);
            }
            if (s is SpellEffect e)
            {
                if(e.Repeatable || !spellEffects.Any(x => x.GetType() == e.GetType()))
                {
                    spellEffects.Add(e);
                }
            }
            
            SpellCooldown += s.Cooldown;
            if (s != primaryNode)
            {
                primaryNode.AddBuff(s.BaseStats);
            }
            s.OwnerSpell = this;
        }
        primaryNode.CalculateFinalStats();
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
