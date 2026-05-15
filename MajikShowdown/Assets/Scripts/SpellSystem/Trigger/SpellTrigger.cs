using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Trigger Node", menuName = "Spell Nodes/TriggerNode")]
public class SpellTrigger : SpellNode
{
    public Triggers trigger;
    [NonSerialized] public Spell TriggeredSpell;
    //public SpellType[] triggeredSpells = new SpellType[2];
    public enum Triggers { OnCast, OnHit, OnDeath };
    
    
    
    public override List<SpellNode> GetSpellList(List<SpellNode> list)
    {
        /*if(hierarchy > list[0].hierarchy)
        {
            list.Add(this);
        }*/
        list.Add(this);
        return list;
    }
    public override void RandomizeStats()
    {
        base.RandomizeStats();
        trigger = RandomizeEnum<Triggers>();
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

    public override void SetupNodeVisual()
    {
        color = HexToColor("A84B0D");

        ConectionPorts = new NodeConection.Conections[]
        {
        NodeConection.Conections.Square,
        NodeConection.Conections.None,
        NodeConection.Conections.Square,
        NodeConection.Conections.None,
        NodeConection.Conections.Square,
        NodeConection.Conections.None
        };
    }
}
public class TriggerInfo
{
    public SpellTrigger Trigger;
    public bool SpellOnCooldown;
    Timer TriggerTimer = new Timer();
    public TriggerInfo(SpellTrigger trigger)
    {
        Trigger = trigger;
    }
    public void UpdateTrigger()
    {
        if (SpellOnCooldown)
        {
            SpellOnCooldown = TriggerTimer.timer(Trigger.OwnerSpell.primaryNode.SpawnTriggeredSpellCooldown, Time.deltaTime, true, true);
        }
    }
}
