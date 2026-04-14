using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Type Node", menuName = "Spell Nodes/TypeNode")]
public class SpellType : SpellNode
{
    public enum SpellTypes { Projectile, Area, Explosion, Hazard, Laser, Ray, Breath }
    [Header("Core Config")]
    public SpellTypes Type;
    public CollisionOptions Collisions;
    public StatTypes StatMultipliers = new StatTypes(1);

    [Header("Hidden Colision Config")]
    public bool HitOnStay;
    public float HitCooldown;
    public float SpawnTriggeredSpellCooldown = 0.5f;
    public bool DieOnObjectCollide = true;

    //public bool DealDamage = true;
    //public SpellType Type;
    [Header("Debug")]
    public Elements Element;
    public StatTypes FinalStats;
    public List<StatTypes> StatBuffs = new List<StatTypes>();

    // public SubSpell subSpell;
    public override void RandomizeStats()
    {
        base.RandomizeStats();
        //Type = (SpellTypes)RandomizeEnum<SpellTypes>();
        Element = RandomizeEnum<Elements>(new string[] { "None" });
    }

    [Serializable]
    public struct CollisionOptions
    {
        public bool Players;
        public bool Enemies;
        public bool Objects;
    }
    public void CalculateFinalStats()
    {
        FinalStats = BaseStats;
        foreach (StatTypes s in StatBuffs)
        {
            FinalStats += s;
        }
        FinalStats *= StatMultipliers;
    }
    public void AddBuff(StatTypes s)
    {
        StatBuffs.Add(s);
    }
    public virtual Vector3 GetVelocity(float lifetime)
    {
        Vector3 traj = Vector3.zero;
        foreach (SpellNode c in ConectedNodes)
        {
            if (c is SpellTrajectory t)
            {
                traj += t.GetTrajectory(lifetime);
            }
        }
        return traj.normalized * FinalStats.Speed;

    }
    public void UpdateNode()
    {
        CalculateFinalStats();
    }
    public override List<SpellNode> GetSpellList(List<SpellNode> list)
    {
        if(list.Count != 0) return list;
        return base.GetSpellList(list);
    }
}

/*public class TypeArea : SpellType
{

}
public class TypeExplosion : SpellType
{

}
public class TypeHazard : SpellType
{

}
public class TypeLaser : SpellType
{

}
public class TypeRay : SpellType
{

}
public class TypeBreath : SpellType
{
    
}
[CreateAssetMenu(fileName = "Projectile Node", menuName = "Spell Nodes/TypeNodes/Projectile")]
public class TypeProjectile : SpellType
{

}*/
