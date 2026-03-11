using UnityEngine;

public class SpellStat : SpellNode
{

}
public abstract class Stat
{
    
    public Stat(float value)
    {
        Value = value;
    }
    public float Value;
    public virtual float GetStat()
    {
        return Value;
    }
}
public class StatVelocity : Stat
{
    public StatVelocity(float value) : base(value)
    {
    }
}
public class StatDuration : Stat
{
    public StatDuration(float value) : base(value)
    {
    }
}
public class StatSize : Stat
{
    public StatSize(float value) : base(value)
    {
    }
}
public class StatDamage : Stat
{
    public StatDamage(float value) : base(value)
    {
    }
}
public class StatPiercing : Stat
{
    public StatPiercing(float value) : base(value)
    {
    }
}
public class StatBounce : Stat
{
    public StatBounce(float value) : base(value)
    {
    }
}
public class StatKnockback : Stat
{
    public StatKnockback(float value) : base(value)
    {
    }
}
