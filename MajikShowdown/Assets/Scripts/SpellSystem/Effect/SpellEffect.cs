using UnityEngine;

public abstract class SpellEffect : SpellNode
{
    public virtual bool Repeatable { get; protected set;} = false;
    public abstract void ApplyEffect(CharacterDamageHandler target);
}

