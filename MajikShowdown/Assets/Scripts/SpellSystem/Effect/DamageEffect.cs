using UnityEngine;

[CreateAssetMenu(fileName = "Damage Effect", menuName = "Spell Nodes/Spell Effects/DamageEffect")]
public class DamageEffect : SpellEffect
{
    public override void ApplyEffect(CharacterDamageHandler target)
    {
        Damage damage = new Damage(OwnerNode.coreNode.FinalStats.Damage, OwnerNode.coreNode.Element, OwnerNode.Caster.player);
        target.TakeDamage(damage);
    }
}