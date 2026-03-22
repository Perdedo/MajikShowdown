using System.Collections.Generic;
//using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Spell
{
    public readonly SpellCaster Owner;
    public List<SubSpell> SubSpells = new List<SubSpell>();
    public List<SpellNode> spellNodes = new List<SpellNode>();
    public float SpellCooldown = 0;
    public SpellType primaryNode;
    public bool validSpell;
    public Spell(SpellCaster owner)
    {
        Owner = owner;
    }

    public void UpdateSpell()
    {
        CreateSubSpells();
        primaryNode.hierarchy = 0;
        SpellCooldown = 0;
        foreach(SubSpell s in SubSpells)
        {
            SpellCooldown += s.CooldownCost;
            s.UpdateSubSpell();
        }
    }
    public void CreateSubSpells()
    {
        SubSpells.Clear();
        foreach (SpellNode spellNode in spellNodes)
        {
            if (spellNode is SpellType t)
            {
                SubSpells.Add(new SubSpell(t, this));
            }
        }
    }
}
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
}
