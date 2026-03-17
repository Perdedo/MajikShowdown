using System.Collections.Generic;
using UnityEngine;

public class SpellTrigger : SpellNode
{
    public Triggers trigger;
    public SpellType[] triggeredSpells = new SpellType[2];
    public enum Triggers { OnCast, OnHit, OnDeath};
    public override List<SpellNode> GetSubspellList(List<SpellNode> list)
    {
        if(hierarchy > list[0].hierarchy)
        {
            list.Add(this);
        }
        return list;
    }
}
