using UnityEngine;

[CreateAssetMenu(fileName = "Damage Effect", menuName = "Spell Nodes/Spell Effects/DamageEffect")]
public class DamageEffect : SpellEffect
{
    public override void ApplyEffect(CharacterDamageHandler target)
    {
        Damage damage = new Damage(OwnerSpell.primaryNode.FinalStats.Damage, OwnerSpell.primaryNode.Element, OwnerSpell.Owner);
        target.TakeDamage(damage);
    }
}
