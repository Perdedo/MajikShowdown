using System;
using System.Collections.Generic;
using UnityEngine;
using static SpellTrigger;

public enum SpellTypes { Projectile, Area, Explosion, Hazard, Laser, Ray, Breath }
[CreateAssetMenu(fileName = "Core Node", menuName = "Spell Nodes/CoreNode")]
public class SpellCore : SpellNode
{
    
    [Header("Core Config")]
    public SpellTypes Type;
    public CollisionOptions Collisions;
    public StatTypes StatMultipliers = new StatTypes(1);

    [Header("Hidden Colision Config")]
    public bool HitOnStay;
    public float HitCooldown;
    public float SpawnTriggeredSpellCooldown = 0.5f;
    public bool DieOnObjectCollide = true;

    [Header("Debug")]
    public Elements Element;
    public StatTypes FinalStats;
    public List<StatTypes> StatBuffs = new List<StatTypes>();
    [NonSerialized] public SpellTrajectory trajectory;
    [NonSerialized] public SpellCastPoint castPoint;

    // public SubSpell subSpell;

    public override void ClientInitialize()
    {
        SetupNodeVisual();
        if (RandomizeOnStart)
        {
            RandomizeStats();
            if(owner is not EnmSpellCaster)
            {
                owner.commander.SetCoreSpellNodeValues(this, Cooldown, BaseStats, extraValues, Element);
            }
        }
        runeType = GetCategory() == NodeCategory.Core ? "Core" : GetCategory().ToString();
        //conections = new NodeConection[]{new(this), new(this), new(this),new(this), new(this), new(this)};
    }
    public override void RandomizeStats()
    {
        base.RandomizeStats();
        //Type = (SpellTypes)RandomizeEnum<SpellTypes>();
        Element = RandomizeEnum<Elements>(new string[] { "None", "Lightning" });
    }
    public void SetCoreStats(float cooldown, StatTypes baseStats, List<float> extras, int elementInd)
    {
        Cooldown = cooldown;
        BaseStats = baseStats;
        SetExtras(extras);
        Element = (Elements)elementInd;
    }

    [Serializable]
    public struct CollisionOptions
    {
        public bool Allies;
        public bool Self;
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
        FinalStats = StatTypes.ClampToZero(FinalStats);
    }
    public void AddBuff(StatTypes s)
    {
        StatBuffs.Add(s);
    }
    public virtual Vector3 GetVelocity(SpellCollider collider)
    {
        Vector3 traj = Vector3.zero;
        if(trajectory != null)
        {
            traj = trajectory.GetTrajectory(collider);
        }
        /*if(trajectory.trajectoryType == SpellTrajectory.TrajectoryType.Orbital)
        {
            return traj;
        }*/
        return traj * FinalStats.Speed;

    }
    public void GetConectedNodes()
    {
        trajectory = null;
        castPoint = null;
        foreach (SpellNode c in ConectedNodes)
        {
            if (c is SpellTrajectory t)
            {
                trajectory = t;
            }
            else if (c is SpellCastPoint cp)
            {
                castPoint = cp;
            }
        }
        
    }
    public void GetEnemyConectedNodes()
    {
        trajectory = null;
        castPoint = null;
        foreach (SpellNode c in OwnerSpell.spellNodes)
        {
            if (c is SpellTrajectory t)
            {
                trajectory = t;
            }
            else if (c is SpellCastPoint cp)
            {
                castPoint = cp;
            }
        }
        
    }
    public void UpdateNode()
    {
        CalculateFinalStats();
        if(OwnerSpell.Caster is not EnmSpellCaster)
        {
            GetConectedNodes();
        }
        else
        {
            GetEnemyConectedNodes();
        }
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
