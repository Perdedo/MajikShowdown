using System.Collections.Generic;
using UnityEngine;

public class Spell
{
    Player Owner;
    public List<SpellNode> spellNodes = new List<SpellNode>();
    public float SpellCooldown = 0;
    public SpellType primaryNode;

    public void UpdateSpell()
    {
        SpellCooldown = 0;
        foreach(SpellNode s in spellNodes)
        {
            SpellCooldown += s.Cooldown;
            if(s is SpellType t)
            {
                t.CalculateFinalStats();
            }
        }
    }
}
