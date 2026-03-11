using System;
using UnityEngine;

public class SpellType : SpellNode
{
    public bool DealDamage = true;
    public CollisionOptions Collisions;
    public SpellType Type;
    public float DamageMultiplier;

    SpellTrigger[] Triggers = new SpellTrigger[3];
    SpellTrajectory Trajectory;
    SpellEffect Effect;
    SpellStat Stat;




    [Serializable]
    public struct CollisionOptions
    {
        public bool Players;
        public bool Enemies;
        public bool Objects;
    }
    public enum SpellTypes { Projectile, Lobbed, Area, Explosion, Hazard, Laser, Ray, Breath}

}
