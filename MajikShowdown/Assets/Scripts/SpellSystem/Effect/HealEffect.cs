using UnityEngine;

[CreateAssetMenu(fileName = "HealEffect", menuName = "Spell Nodes/Spell Effects/HealEffect")]
public class HealEffect : SpellEffect
{
    [AddExtraVar("Rune Heal")]
    public ExtraVar HealAmount;
    public override bool Repeatable { get => base.Repeatable; protected set => base.Repeatable = true;}
    public override void ApplyEffect(CharacterDamageHandler target)
    {
        target.Heal(HealAmount.Value);
    }
}
