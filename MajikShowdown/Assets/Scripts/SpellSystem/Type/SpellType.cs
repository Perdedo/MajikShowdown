using System;
using System.Collections.Generic;
using UnityEngine;


public abstract class SpellType : SpellNode
{
    public bool DealDamage = true;
    public CollisionOptions Collisions;
    //public SpellType Type;

    public StatTypes FinalStats;
    public List<StatTypes> StatBuffs = new List<StatTypes>();
    public StatTypes StatMultipliers = new StatTypes(1);

    /*public SpellTrigger[] Triggers = new SpellTrigger[3];
    public SpellTrajectory Trajectory;
    public SpellEffect Effect;
    public SpellStat Stat;*/

    [Serializable]
    public struct CollisionOptions
    {
        public bool Players;
        public bool Enemies;
        public bool Objects;
    }
    public enum SpellTypes { Projectile, Area, Explosion, Hazard, Laser, Ray, Breath }
    public void CalculateFinalStats()
    {
        FinalStats = BaseStats;
        foreach (StatTypes s in StatBuffs)
        {
            FinalStats += s;
        }
        FinalStats *= StatMultipliers;
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

}

public class TypeArea : SpellType
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
