using UnityEngine;

public abstract class SpellEffect : SpellNode
{
    public bool Repeatable {protected set; get; } = false;
    public abstract void ApplyEffect(CharacterDamageHandler target);
}

