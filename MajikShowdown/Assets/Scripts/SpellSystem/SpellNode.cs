using System;
using UnityEngine;

public abstract class SpellNode
{
    public float Cooldown = 0;
    public StatTypes BaseStats = new StatTypes();
    public enum NodeEntry { None, Type, Stat, Trigger, Trajectory, Effect, All };
}
[Serializable]
public struct StatTypes
{
    public StatTypes(float DefaultValue = 0)
    {
        Speed = DefaultValue;
        Duration = DefaultValue;
        Size = DefaultValue;
        Damage = DefaultValue;
        Piercing = DefaultValue;
        Bounce = DefaultValue;
        Knockback = DefaultValue;
    }
    public float Speed;
    public float Duration;
    public float Size;
    public float Damage;
    public float Piercing;
    public float Bounce;
    public float Knockback;

    public static StatTypes operator +(StatTypes s1, StatTypes s2)
    {
        return new StatTypes()
        {
            Speed = s1.Speed + s2.Speed,
            Duration = s1.Duration + s2.Duration,
            Size = s1.Size + s2.Size,
            Damage = s1.Damage + s2.Damage,
            Piercing = s1.Piercing + s2.Piercing,
            Bounce = s1.Bounce + s2.Bounce,
            Knockback = s1.Knockback + s2.Knockback
        };
    }
    public static StatTypes operator *(StatTypes s1, StatTypes s2)
    {
        return new StatTypes()
        {
            Speed = s1.Speed * s2.Speed,
            Duration = s1.Duration * s2.Duration,
            Size = s1.Size * s2.Size,
            Damage = s1.Damage * s2.Damage,
            Piercing = s1.Piercing * s2.Piercing,
            Bounce = s1.Bounce * s2.Bounce,
            Knockback = s1.Knockback * s2.Knockback
        };
    }
    public static StatTypes operator -(StatTypes s1, StatTypes s2)
    {
        return new StatTypes()
        {
            Speed = s1.Speed - s2.Speed,
            Duration = s1.Duration - s2.Duration,
            Size = s1.Size - s2.Size,
            Damage = s1.Damage - s2.Damage,
            Piercing = s1.Piercing - s2.Piercing,
            Bounce = s1.Bounce - s2.Bounce,
            Knockback = s1.Knockback - s2.Knockback
        };
    }
}
