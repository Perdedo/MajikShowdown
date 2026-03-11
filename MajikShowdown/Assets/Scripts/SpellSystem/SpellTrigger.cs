using UnityEngine;

public class SpellTrigger : SpellNode
{
    public Triggers trigger;
    public enum Triggers { OnCast, OnHit, OnDeath};
}
