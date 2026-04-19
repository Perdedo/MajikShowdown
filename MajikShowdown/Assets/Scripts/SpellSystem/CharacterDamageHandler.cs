using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterDamageHandler : NetworkBehaviour
{
    public float MaxHealth;
    public float Health;
    public List<Resistance> Resistances;
    void Awake()
    {
        Health = MaxHealth;
    }

    public void TakeDamage(Damage damage)
    {
        float finalDamage = damage.Value;
        for (int i = 0; i < Resistances.Count; i++)
        {
            if (Resistances[i].Element == damage.Element)
            {
                finalDamage *= 1 - Resistances[i].PercentValue / 100;
                i = Resistances.Count;
            }
        }
        CMDTakeDamage(finalDamage);
    }

    [Command]
    public void CMDTakeDamage(float finalDamage)
    {
        Health = MathF.Max(Health - finalDamage, 0);
        if (Health <= 0)
        {
            Die();
        }
    }
    public void Heal(float amount)
    {
        Health = Mathf.Min(Health + amount, MaxHealth);
    }

    [Server]
    public void Die()
    {
        //Destroy(gameObject);
        NetworkServer.Destroy(gameObject);
    }
}
public enum Elements { None, Fire, Ice, Earth, Lightning, Radiance, Darkness, Poison }

public class Damage
{
    public Damage(float value, Elements element, IGameCharacter damageSource)
    {
        Value = value;
        Element = element;
        DamageSource = damageSource;
    }
    public Damage(float value, Elements element)
    {
        Value = value;
        Element = element;
        DamageSource = null;
    }
    public float Value;
    public Elements Element;
    public IGameCharacter DamageSource;
}
public class MagicDamage : Damage
{
    public MagicDamage(float value, Elements element, IGameCharacter damageSource, SpellCollider spell) : base(value, element, damageSource)
    {
        spellCollider = spell;
    }
    public SpellCollider spellCollider;

}
[Serializable]
public class Resistance
{
    public Elements Element;
    [Range(-100, 100)] public float PercentValue;
}
public interface IGameCharacter
{
    public CharacterDamageHandler DamageHandler { get; }
}

