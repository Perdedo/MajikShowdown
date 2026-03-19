using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Type Node", menuName = "Spell Nodes/TypeNode")]
public class SpellType : SpellNode
{
    public enum SpellTypes { Projectile, Area, Explosion, Hazard, Laser, Ray, Breath }
    public SpellTypes Type;

    public bool DealDamage = true;
    public CollisionOptions Collisions;
    //public SpellType Type;

    public StatTypes StatMultipliers = new StatTypes(1);
    public StatTypes FinalStats;
    public List<StatTypes> StatBuffs = new List<StatTypes>();

   // public SubSpell subSpell;
    public override void Initialize()
    {
        base.Initialize();
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
    public virtual Vector3 GetVelocity()
    {
        Vector3 traj = Vector3.zero;
        foreach (SpellNode c in ConectedNodes)
        {
            if (c is SpellTrajectory t)
            {
                traj += t.GetTrajectory();
            }
        }
        return traj.normalized * FinalStats.Speed;

    }
    public void UpdateNode()
    {
        CalculateFinalStats();
    }
    public override List<SpellNode> GetSubspellList(List<SpellNode> list)
    {
        if(list.Count != 0) return list;
        return base.GetSubspellList(list);
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
