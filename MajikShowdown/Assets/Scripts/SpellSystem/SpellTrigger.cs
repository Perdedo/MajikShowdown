using UnityEngine;

public class SpellTrigger : SpellNode
{
    public Triggers trigger;
    public SpellType[] triggeredSpells = new SpellType[2];
    public enum Triggers { OnCast, OnHit, OnDeath};
}
