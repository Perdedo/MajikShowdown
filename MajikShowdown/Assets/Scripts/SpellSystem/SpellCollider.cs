using System;
using System.Collections;
using System.Collections.Generic;
using Mirror.Examples.Billiards;
using UnityEngine;
using UnityEngine.Events;

public class SpellCollider : MonoBehaviour
{
    public StaticRB rb;
    public StatTypes stats;
    public Spell OwnerSpell;
    float pierceCount, bounceCount;
    public bool primarySpell;
    bool HitOnCooldown;
    Timer HitTimer = new Timer();
    [NonSerialized] public float LifeTime = 0;
    List<TriggerInfo> triggerInfos = new List<TriggerInfo>();

    public UnityEvent OnCast = new UnityEvent(), OnHit = new UnityEvent(), OnDeath = new UnityEvent();
    public Collider spellCol;
    public void Initialize(Spell owner, bool isPrimary)
    {
        //projectileConfig.CalculateFinalStats();
        OwnerSpell = owner; ;
        primarySpell = isPrimary;
        stats = OwnerSpell.primaryNode.FinalStats;
        transform.localScale = Vector3.one * stats.Size;
        //Invoke("Die", stats.Duration);
        pierceCount = stats.Piercing;
        bounceCount = stats.Bounce;
        OnHit.AddListener(StartHitCooldown);
        if (primarySpell)
        {
            InitiateTriggeredSpells();
        }
        OnCast.Invoke();

        if (OwnerSpell.primaryNode.Type == SpellType.SpellTypes.Explosion)
        {
            transform.localScale = Vector3.zero;
        }
        spellCol = GetComponent<Collider>();

    }
    void Update()
    {
        if (HitOnCooldown)
        {
            HitOnCooldown = HitTimer.timer(OwnerSpell.primaryNode.HitCooldown, Time.deltaTime, true, false);
        }
        /*Vector3 centerVel = ToLookDirection(OwnerSpell.primaryNode.GetVelocity(LifeTime));
        float x = Mathf.Cos(LifeTime*5) * 1;
        float z = Mathf.Sin(LifeTime*5) * 1;
        Vector3 relativeVel = new Vector3(x, 0, z);
        rb.Velocity = centerVel + relativeVel*OwnerSpell.primaryNode.FinalStats.Speed;*/
        rb.Velocity = OwnerSpell.primaryNode.GetVelocity(this);
        foreach (TriggerInfo t in triggerInfos)
        {
            t.UpdateTrigger();
        }

        switch (OwnerSpell.primaryNode.Type)
        {
            case SpellType.SpellTypes.Explosion:
                transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one * stats.Size, LifeTime / stats.Duration);
                break;
        }

        LifeTime += Time.deltaTime;
        if (OwnerSpell.primaryNode.trajectory == null || OwnerSpell.primaryNode.trajectory.trajectoryType != SpellTrajectory.TrajectoryType.Boomerang)
        {
            if (LifeTime >= stats.Duration)
            {
                Die();
            }
        }


    }
    public void InitiateTriggeredSpells()
    {
        foreach (SpellTrigger t in OwnerSpell.triggers)
        {
            triggerInfos.Add(new TriggerInfo(t));
        }
        foreach (TriggerInfo t in triggerInfos)
        {
            if (t.Trigger.TriggeredSpell == null) continue;
            switch (t.Trigger.trigger)
            {
                case SpellTrigger.Triggers.OnCast:
                    AddSpellToEvent(OnCast, t.Trigger.TriggeredSpell, t);
                    break;
                case SpellTrigger.Triggers.OnHit:
                    AddSpellToEvent(OnHit, t.Trigger.TriggeredSpell, t);
                    break;
                case SpellTrigger.Triggers.OnDeath:
                    AddSpellToEvent(OnDeath, t.Trigger.TriggeredSpell, t);
                    break;
            }
        }
    }
    public void AddSpellToEvent(UnityEvent e, Spell spell, TriggerInfo trigger)
    {
        UnityAction action = () =>
        {
            if (!trigger.SpellOnCooldown)
            {
                OwnerSpell.Caster.InstantiateSpellCollider(spell, transform.position, transform.forward);
                trigger.SpellOnCooldown = true;
            }
        };
        e.AddListener(action);
    }
    public Vector3 ToLookDirection(Vector3 rawDir)
    {
        return transform.forward * rawDir.z + transform.up * rawDir.y + transform.right * rawDir.x;
    }
    void StartHitCooldown()
    {
        HitOnCooldown = true;
        HitTimer.SetTimer(0);
    }
    void OnTriggerEnter(Collider other)
    {
        if (!OwnerSpell.primaryNode.HitOnStay)
        {
            HandleTrigger(other);
        }
    }
    void OnTriggerStay(Collider other)
    {
        if (OwnerSpell.primaryNode.HitOnStay)
        {
            HandleTrigger(other);
        }
    }
    public void HandleTrigger(Collider other)
    {
        if (HitOnCooldown) return;
        CollisionData ColData = new CollisionData(other,this);
        if (OwnerSpell.primaryNode.Collisions.Enemies && LayerMaskUtility.BelongsInMask(other.gameObject.layer, OwnerSpell.Caster.EnemyLayer))
        {
            OnHit.Invoke();
            CollideCreature(ColData);
        }
        else if (OwnerSpell.primaryNode.Collisions.Players && LayerMaskUtility.BelongsInMask(other.gameObject.layer, OwnerSpell.Caster.PlayerLayer))
        {
            OnHit.Invoke();
            CollideCreature(ColData);
        }
        else if (OwnerSpell.primaryNode.Collisions.Objects && LayerMaskUtility.BelongsInMask(other.gameObject.layer, OwnerSpell.Caster.ObjectLayer))
        {
            OnHit.Invoke();
            CollideObject(ColData);
        }
    }

    public void CollideObject(CollisionData data)
    {
        CheckBounce(data);
    }
    public void CollideCreature(CollisionData data)
    {
        CharacterDamageHandler character = data.collision.GetComponent<CharacterDamageHandler>();
        if (character != null)
        {
            foreach (SpellEffect e in OwnerSpell.spellEffects)
            {
                e.ApplyEffect(character);
            }
        }
        if (pierceCount >= 1)
        {
            pierceCount--;
        }
        else
        {
            CheckBounce(data);
        }
    }
    public void CheckBounce(CollisionData data)
    {
        if (bounceCount >= 1)
        {
            bounceCount--;
            Bounce(data);
        }
        else
        {
            if (OwnerSpell.primaryNode.DieOnObjectCollide)
            {
                Die();
            }
        }
    }
    public void Bounce(CollisionData data)
    {
        transform.LookAt(transform.position + Vector3.Reflect(rb.Velocity, data.hitNormal));
    }
    public void Die()
    {
        OnDeath.Invoke();
        Destroy(gameObject);
    }
}
public struct CollisionData
{
    public Collider collision;
    public SpellCollider Object;
    public Vector3 hitPoint;
    public Vector3 hitNormal;
    public float Distance;
    public CollisionData(Collider col, SpellCollider obj)
    {
        collision = col;
        Object = obj;
        Physics.ComputePenetration(obj.spellCol,obj.transform.position, obj.transform.rotation,col, col.transform.position, col.transform.rotation, out hitNormal, out Distance);
        Physics.SphereCast(obj.transform.position,Distance+0.1f,Vector3.zero,out RaycastHit hitInfo, 0,col.gameObject.layer);
        hitPoint = hitInfo.point;
        //Physics.Raycast(obj.transform.position, obj.rb.Velocity.normalized, out RaycastHit hit, Distance+0.1f);
        //hitPoint = hit.point;
    }
}
