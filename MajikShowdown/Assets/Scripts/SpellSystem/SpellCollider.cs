using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using Mirror.Examples.Billiards;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

public class SpellCollider : NetworkBehaviour
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
    [NonSerialized] public Vector3 previousVelocity;

    public UnityEvent OnCast = new UnityEvent(), OnHit = new UnityEvent(), OnDeath = new UnityEvent();
    public Collider spellCol;
    [NonSerialized] public int inverseBounceMultiplier = 1;
    [HideInInspector] public bool UseAcceleration = false;
    [NonSerialized] public Transform SpawnTransform;
    [NonSerialized] public Vector3 SpawnPoint;
    public struct TrajectoryInfo
    {
        public Vector3 Forward;
        public Vector3 Right;
        public Vector3 Up;
    }
    public TrajectoryInfo TrajectoryTransform;
    public void Initialize(Spell owner, bool isPrimary)
    {
        SetTrajectoryForward(transform.forward);
        //projectileConfig.CalculateFinalStats();
        OwnerSpell = owner; ;
        primarySpell = isPrimary;
        stats = OwnerSpell.coreNode.FinalStats;
        transform.localScale = Vector3.one * stats.Size;
        //Invoke("Die", stats.Duration);
        pierceCount = stats.Piercing;
        bounceCount = stats.Bounce;
        OnHit.AddListener(() => { if (OwnerSpell.coreNode.HitCooldown > 0 && !routineStarted) StartCoroutine(StartHitCooldown()); });
        if (primarySpell)
        {
            InitiateTriggeredSpells();
        }
        OnCast.Invoke();

        if (OwnerSpell.coreNode.Type == SpellType.SpellTypes.Explosion)
        {
            transform.localScale = Vector3.zero;
        }
        spellCol = GetComponent<Collider>();

    }
    void Update()
    {
        if (HitOnCooldown)
        {
            HitOnCooldown = HitTimer.timer(OwnerSpell.coreNode.HitCooldown, Time.deltaTime, true, false);
        }
        if (UseAcceleration)
        {
            rb.LerpToVelocity(OwnerSpell.coreNode.GetVelocity(this), stats.Speed*2);
            SetTrajectoryForward(rb.Velocity);
        }
        else
        {
            rb.CancelLerp();
            rb.Velocity = OwnerSpell.coreNode.GetVelocity(this);
        }
        previousVelocity = rb.Velocity;
        foreach (TriggerInfo t in triggerInfos)
        {
            t.UpdateTrigger();
        }

        switch (OwnerSpell.coreNode.Type)
        {
            case SpellType.SpellTypes.Explosion:
                transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one * stats.Size, LifeTime / stats.Duration);
                break;
        }

        LifeTime += Time.deltaTime;
        if (OwnerSpell.coreNode.trajectory == null || OwnerSpell.coreNode.trajectory.trajectoryType != SpellTrajectory.TrajectoryType.Boomerang)
        {
            if (LifeTime >= stats.Duration)
            {
                Die();
            }
        }
        transform.LookAt(transform.position + rb.Velocity.normalized);
        //Debug.DrawRay(transform.position, TrajectoryTransform.Forward * 5, Color.red);


    }
    public void SetTrajectoryForward(Vector3 forward)
    {
        if (forward == Vector3.zero) return;
        TrajectoryTransform.Forward = forward.normalized;
        TrajectoryTransform.Right = new Vector3(TrajectoryTransform.Forward.z, 0, -TrajectoryTransform.Forward.x).normalized;
        TrajectoryTransform.Up = Vector3.Cross(TrajectoryTransform.Right, TrajectoryTransform.Forward);
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
    public Vector3 ToTrajDirection(Vector3 rawDir)
    {
        return TrajectoryTransform.Forward * rawDir.z + TrajectoryTransform.Up * rawDir.y + TrajectoryTransform.Right * rawDir.x;
    }
    bool routineStarted;
    IEnumerator StartHitCooldown()
    {
        routineStarted = true;
        yield return new WaitForEndOfFrame();
        HitOnCooldown = true;
        HitTimer.SetTimer(0);
        routineStarted = false;
    }
    void OnTriggerEnter(Collider other)
    {
        if (!OwnerSpell.coreNode.HitOnStay)
        {
            HandleTrigger(other);
        }
    }
    void OnTriggerStay(Collider other)
    {
        if (OwnerSpell.coreNode.HitOnStay)
        {
            HandleTrigger(other);
        }
    }
    public void HandleTrigger(Collider other)
    {
        if (HitOnCooldown) return;
        CollisionData ColData = new CollisionData(other, this);
        if (OwnerSpell.coreNode.Collisions.Enemies && LayerMaskUtility.BelongsInMask(other.gameObject.layer, OwnerSpell.Caster.EnemyLayer))
        {
            OnHit.Invoke();
            CollideCreature(ColData);
        }
        else if (OwnerSpell.coreNode.Collisions.Players && LayerMaskUtility.BelongsInMask(other.gameObject.layer, OwnerSpell.Caster.PlayerLayer))
        {
            OnHit.Invoke();
            CollideCreature(ColData);
        }
        else if (OwnerSpell.coreNode.Collisions.Objects && LayerMaskUtility.BelongsInMask(other.gameObject.layer, OwnerSpell.Caster.ObjectLayer))
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
        Character character = data.collision.GetComponent<Character>();
        if (character != null)
        {
            foreach (SpellEffect e in OwnerSpell.spellEffects)
            {
                e.ApplyEffect(character.damageHandler);
            }
            character.KnockBack(((data.collision.transform.position - data.Object.transform.position) + data.Object.rb.Velocity).normalized, stats.Knockback);

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
            if (OwnerSpell.coreNode.DieOnObjectCollide)
            {
                Die();
            }
        }
    }
    public void Bounce(CollisionData data)
    {
        previousVelocity = Vector3.zero;
        inverseBounceMultiplier *= -1;
        if (OwnerSpell.coreNode.trajectory.trajectoryType == SpellTrajectory.TrajectoryType.Lobbed)
        {
            float upDot = Vector3.Dot(data.hitNormal, Vector3.up);
            if (upDot < 0.5f)
            {
                Vector3 reflection = Vector3.Reflect(new Vector3(rb.Velocity.x, 0, rb.Velocity.z), data.hitNormal);
                reflection.y = rb.Velocity.y;
                SetTrajectoryForward(reflection);
            }

        }
        else
        {
            //SetTrajectoryForward(Vector3.Reflect(TrajectoryTransform.Forward, data.hitNormal));
            SetTrajectoryForward(Vector3.Reflect(rb.Velocity, data.hitNormal));
        }

        /*if(OwnerSpell.primaryNode.trajectory.trajectoryType == SpellTrajectory.TrajectoryType.Lobbed)
        {
            previousDir = Vector3.zero;
        }*/
    }
    public void Die()
    {
        OnDeath.Invoke();
        if (isServer && OwnerSpell.Caster.network)
        {
            NetworkServer.Destroy(this.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
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
        Physics.ComputePenetration(obj.spellCol, obj.transform.position, obj.transform.rotation, col, col.transform.position, col.transform.rotation, out hitNormal, out Distance);
        Physics.SphereCast(obj.transform.position, Distance + 0.1f, Vector3.zero, out RaycastHit hitInfo, 0, col.gameObject.layer);
        hitPoint = hitInfo.point;
        //Physics.Raycast(obj.transform.position, obj.rb.Velocity.normalized, out RaycastHit hit, Distance+0.1f);
        //hitPoint = hit.point;
    }
}
