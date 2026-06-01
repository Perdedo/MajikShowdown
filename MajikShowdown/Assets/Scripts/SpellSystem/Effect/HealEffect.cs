using UnityEngine;

[CreateAssetMenu(fileName = "HealEffect", menuName = "Spell Nodes/Spell Effects/HealEffect")]
public class HealEffect : SpellEffect
{
    [AddExtraVar("Rune Heal")]
    public ExtraVar HealAmount;
    public override bool Repeatable {protected set; get; } = true;
    public override void ApplyEffect(CharacterDamageHandler target)
    {
        target.Heal(HealAmount.Value);
    }
}
