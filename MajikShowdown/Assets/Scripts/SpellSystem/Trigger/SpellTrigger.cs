using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Trigger Node", menuName = "Spell Nodes/TriggerNode")]
public class SpellTrigger : SpellNode
{
    public Triggers trigger;
    [NonSerialized]public Spell TriggeredSpell;
    //public SpellType[] triggeredSpells = new SpellType[2];
    public enum Triggers { OnCast, OnHit, OnDeath};
    public override List<SpellNode> GetSpellList(List<SpellNode> list)
    {
        /*if(hierarchy > list[0].hierarchy)
        {
            list.Add(this);
        }*/
        list.Add(this);
        return list;
    }
    /*public List<SubSpell> TriggeredSubspells()
    {
        List<SubSpell> aux = new List<SubSpell>();
        foreach(SpellNode s in ConectedNodes)
        {
            if(s is SpellType t && t != OwnerSpell.Type)
            {
                aux.Add(t.OwnerSpell);
            }
        }
        return aux;
    }*/
}
