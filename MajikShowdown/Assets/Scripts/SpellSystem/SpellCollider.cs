using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using Mirror.Examples.Billiards;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

public class SpellCollider : NetworkBehaviour
{
    public GameObject mesh;
    public StaticRB rb;
    public StatTypes stats;
    public Spell OwnerSpell;
    [NonSerialized] float pierceCount, bounceCount;
    [NonSerialized] public bool primarySpell;
    bool HitOnCooldown;
    bool CollisionOnCooldown;
    Timer HitTimer = new Timer();
    [NonSerialized] public float LifeTime = 0;
    List<TriggerInfo> triggerInfos = new List<TriggerInfo>();
    [NonSerialized] public Vector3 previousVelocity;

    public UnityEvent OnCast = new UnityEvent(), OnHit = new UnityEvent(), OnDeath = new UnityEvent();
    //public Collider spellCol;
    [NonSerialized] public RaycastHit[] collisionBuffer = new RaycastHit[64];
    [NonSerialized] public Collider[] previousColisions;
    [NonSerialized] public Collider[] staticCollisionsBuffer = new Collider[64];
    [NonSerialized] public Collider[] previousStaticColisions;
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
    bool expanding = true;
    float currentSize = 0;
    public TrajectoryInfo TrajectoryTransform;

    //[Server]
    public void Initialize(Spell owner, bool isPrimary)
    {
        SetTrajectoryForward(transform.forward);
        //projectileConfig.CalculateFinalStats();
        OwnerSpell = owner;
        primarySpell = isPrimary;
        stats = OwnerSpell.coreNode.FinalStats;
        //transform.localScale = Vector3.one * stats.Size;
        //Invoke("Die", stats.Duration);
        pierceCount = stats.Piercing;
        bounceCount = stats.Bounce;
        //OnHit.AddListener(() => { if (OwnerSpell.coreNode.HitCooldown > 0 && !routineStarted) StartCoroutine(StartHitCooldown()); });
        if (primarySpell)
        {
            InitiateTriggeredSpells();
        }
        OnCast.Invoke();

        /*if (OwnerSpell.coreNode.Type == SpellType.SpellTypes.Explosion)
        {
            transform.localScale = Vector3.zero;
        }*/
        //transform.localScale = Vector3.zero;
        //spellCol = GetComponent<Collider>();

    }
    //[Server]
    void Update()
    {
        if (!isServer && GameManager.Instance.uiController.playerUI.network)
        {
            return;
        }
        if (HitOnCooldown)
        {
            HitOnCooldown = HitTimer.timer(OwnerSpell.coreNode.HitCooldown, Time.deltaTime, true, false);
        }
        if (!CollisionOnCooldown)
        {
            if (rb.Velocity.sqrMagnitude > 0.01f)
            {
                CheckMovingColisions();
            }
            else if (!HitOnCooldown)
            {
                CheckStationaryCollisions();
            }
        }

        if (UseAcceleration)
        {
            rb.LerpToVelocity(OwnerSpell.coreNode.GetVelocity(this), stats.Speed * 2);
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

        Expand();

        LifeTime += Time.deltaTime;
        if (OwnerSpell.coreNode.trajectory == null || OwnerSpell.coreNode.trajectory.trajectoryType != SpellTrajectory.TrajectoryType.Boomerang)
        {
            if (LifeTime >= stats.Duration)
            {
                Die();
            }
        }
        transform.LookAt(transform.position + rb.Velocity.normalized);
        mesh.transform.localScale = Vector3.one * currentSize;
        //Debug.DrawRay(transform.position, TrajectoryTransform.Forward * 5, Color.red);


    }
    //[Server]
    void CheckMovingColisions()
    {
        int amount = Physics.SphereCastNonAlloc(transform.position, currentSize / 2, rb.Velocity.normalized, collisionBuffer, rb.Velocity.magnitude * Time.deltaTime, OwnerSpell.spellCollisionLayers);
        RaycastHit closest = default;
        float closestDist = float.MaxValue;
        if (amount == collisionBuffer.Length)
        {
            RaycastHit[] temporaryBuffer = Physics.SphereCastAll(transform.position, currentSize / 2, rb.Velocity.normalized, rb.Velocity.magnitude * Time.deltaTime, OwnerSpell.spellCollisionLayers); ;
            foreach (RaycastHit hit in temporaryBuffer)
            {
                if (isValidHit(hit))
                {
                    HandleCollision(hit.collider);
                    getClosest(hit);
                }

            }
        }
        else
        {
            for (int i = 0; i < amount; i++)
            {
                if (isValidHit(collisionBuffer[i]))
                {
                    HandleCollision(collisionBuffer[i].collider);
                    getClosest(collisionBuffer[i]);
                }
            }
        }
        if (amount > 0 && closest.collider != null)
        {
            if (pierceCount < 1 || LayerMaskUtility.BelongsInMask(closest.collider.gameObject.layer, OwnerSpell.Caster.ObjectLayer))
            {
                CheckBounce(closest);
            }
        }
        if (!OwnerSpell.coreNode.HitOnStay)
        {
            previousColisions = collisionBuffer.Take(amount).Select(h => h.collider).ToArray();
        }
        void getClosest(RaycastHit hit)
        {
            if (closest.collider == null)
            {
                closest = hit;
                closestDist = hit.distance;
                return;
            }
            bool hitIsObject = LayerMaskUtility.BelongsInMask(hit.collider.gameObject.layer, OwnerSpell.Caster.ObjectLayer);
            bool closestIsObject = LayerMaskUtility.BelongsInMask(closest.collider.gameObject.layer, OwnerSpell.Caster.ObjectLayer);
            if (hitIsObject && !closestIsObject)
            {
                closest = hit;
                closestDist = hit.distance;
                return;
            }
            if (!hitIsObject && closestIsObject)
            {
                return;
            }
            if (hit.distance < closestDist)
            {
                closest = hit;
                closestDist = hit.distance;
            }
        }
        bool isValidHit(RaycastHit hit)
        {
            if (hit.collider.gameObject == this) return false;
            if (!OwnerSpell.coreNode.HitOnStay && previousColisions.Contains(hit.collider)) return false;
            return true;
        }

    }
    public void CheckStationaryCollisions()
    {
        int amount = Physics.OverlapSphereNonAlloc(transform.position, currentSize / 2, staticCollisionsBuffer, OwnerSpell.spellCollisionLayers);
        for (int i = 0; i < amount; i++)
        {
            Collider col = staticCollisionsBuffer[i];
            if (col.gameObject == this) continue;
            if (!OwnerSpell.coreNode.HitOnStay && previousStaticColisions != null && previousStaticColisions.Contains(col)) continue;
            HandleCollision(staticCollisionsBuffer[i]);
        }
        if (!OwnerSpell.coreNode.HitOnStay)
        {
            previousStaticColisions = staticCollisionsBuffer.Take(amount).ToArray();
        }
    }
    //[Server]
    void HandleCollision(Collider col)
    {
        if (LayerMaskUtility.BelongsInMask(col.gameObject.layer, OwnerSpell.Caster.EnemyLayer | OwnerSpell.Caster.PlayerLayer))
        {
            if (HitOnCooldown) return;
            OnHit.Invoke();
            CollideCreature(col);
        }
        else
        {
            OnHit.Invoke();
            CollideObject(col);
            //Debug.Log("Collided " + Time.time);
        }
    }
    /*void OnDrawGizmos()
    {
        Gizmos.DrawSphere(transform.position + rb.Velocity.normalized * stats.Speed * Time.deltaTime, currentSize / 2);
    }*/
    //[Server]
    void Expand()
    {
        if (expanding)
        {
            float scaleTime;
            switch (OwnerSpell.coreNode.Type)
            {
                case SpellType.SpellTypes.Explosion:
                    scaleTime = stats.Duration;
                    break;
                default:
                    scaleTime = 0.3f;
                    break;
            }
            if (LifeTime <= scaleTime)
            {
                currentSize = Mathf.Lerp(0, stats.Size, LifeTime / scaleTime);
            }
            else
            {
                currentSize = stats.Size;
                expanding = false;
            }
        }
    }
    //[Server]
    public void SetTrajectoryForward(Vector3 forward)
    {
        if (forward == Vector3.zero) return;
        TrajectoryTransform.Forward = forward.normalized;
        TrajectoryTransform.Right = new Vector3(TrajectoryTransform.Forward.z, 0, -TrajectoryTransform.Forward.x).normalized;
        TrajectoryTransform.Up = Vector3.Cross(TrajectoryTransform.Right, TrajectoryTransform.Forward);
    }
    //[Server]
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
    //[Server]
    public void AddSpellToEvent(UnityEvent e, Spell spell, TriggerInfo trigger)
    {
        UnityAction action = () =>
        {
            if (!trigger.SpellOnCooldown)
            {
                OwnerSpell.Caster.ServerInstantiateSpellCollider(spell, transform.position, transform.forward);
                trigger.SpellOnCooldown = true;
            }
        };
        e.AddListener(action);
    }
    //[Server]
    public Vector3 ToTrajDirection(Vector3 rawDir)
    {
        return TrajectoryTransform.Forward * rawDir.z + TrajectoryTransform.Up * rawDir.y + TrajectoryTransform.Right * rawDir.x;
    }
    bool routineStarted;
    //[Server]
    IEnumerator StartHitCooldown()
    {
        routineStarted = true;
        yield return new WaitForEndOfFrame();
        HitOnCooldown = true;
        HitTimer.SetTimer(0);
        routineStarted = false;
    }
    IEnumerator ColideCooldown(float cooldown)
    {
        CollisionOnCooldown = true;
        yield return new WaitForSeconds(cooldown);
        CollisionOnCooldown = false;
    }
    /*[Server]
    void OnTriggerEnter(Collider other)
    {
        if (!isServer)
        {
            return;
        }
        if (!OwnerSpell.coreNode.HitOnStay)
        {
            HandleTrigger(other);
        }
    }
    [Server]
    void OnTriggerStay(Collider other)
    {
        if (!isServer)
        {
            return;
        }
        if (OwnerSpell.coreNode.HitOnStay)
        {
            HandleTrigger(other);
        }
    }
    [Server]
    public void HandleTrigger(Collider other)
    {

        CollisionData ColData = new CollisionData(other, this);
        if (OwnerSpell.coreNode.Collisions.Enemies && LayerMaskUtility.BelongsInMask(other.gameObject.layer, OwnerSpell.Caster.EnemyLayer))
        {
            if (HitOnCooldown) return;
            OnHit.Invoke();
            CollideCreature(ColData);
        }
        else if (OwnerSpell.coreNode.Collisions.Players && LayerMaskUtility.BelongsInMask(other.gameObject.layer, OwnerSpell.Caster.PlayerLayer))
        {
            if (HitOnCooldown) return;
            OnHit.Invoke();
            CollideCreature(ColData);
        }
        else if (OwnerSpell.coreNode.Collisions.Objects && LayerMaskUtility.BelongsInMask(other.gameObject.layer, OwnerSpell.Caster.ObjectLayer))
        {
            OnHit.Invoke();
            CollideObject(ColData);
        }
    }*/

    //[Server]
    public void CollideObject(Collider col)
    {

    }

    //[Server]
    public void CollideCreature(Collider col)
    {
        if (OwnerSpell.coreNode.HitCooldown > 0 && !routineStarted) StartCoroutine(StartHitCooldown());
        Character character = col.GetComponent<Character>();
        if (character != null)
        {
            foreach (SpellEffect e in OwnerSpell.spellEffects)
            {
                e.ApplyEffect(character.damageHandler);
            }
            character.KnockBack(((col.transform.position - transform.position) + rb.Velocity).normalized, stats.Knockback);

        }
        if (pierceCount >= 1)
        {
            pierceCount--;
        }
    }

    //[Server]
    public void CheckBounce(RaycastHit data)
    {
        if (bounceCount >= 1)
        {
            bounceCount--;
            Bounce(data);
            StartCoroutine(ColideCooldown(0.1f));
        }
        else
        {
            if (OwnerSpell.coreNode.DieOnObjectCollide)
            {
                Die();
            }
        }
    }

    //[Server]
    public void Bounce(RaycastHit data)
    {
        previousVelocity = Vector3.zero;
        inverseBounceMultiplier *= -1;
        Vector3 normal = data.normal;
        if (Vector3.Dot(rb.Velocity, normal) > 0)
        {
            normal = -normal;
        }
        if (OwnerSpell.coreNode.trajectory.trajectoryType == SpellTrajectory.TrajectoryType.Lobbed)
        {
            float upDot = Vector3.Dot(normal, Vector3.up);
            if (upDot < 0.5f)
            {
                Vector3 reflection = Vector3.Reflect(new Vector3(rb.Velocity.x, 0, rb.Velocity.z), normal);
                reflection.y = rb.Velocity.y;
                SetTrajectoryForward(reflection);
            }

        }
        else
        {
            //SetTrajectoryForward(Vector3.Reflect(TrajectoryTransform.Forward, data.hitNormal));
            SetTrajectoryForward(Vector3.Reflect(rb.Velocity, normal));
        }

        /*if(OwnerSpell.primaryNode.trajectory.trajectoryType == SpellTrajectory.TrajectoryType.Lobbed)
        {
            previousDir = Vector3.zero;
        }*/
    }

    //[Server]
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
    public CollisionData(RaycastHit col, SpellCollider obj)
    {
        collision = col.collider;
        Object = obj;
        hitPoint = col.point;
        hitNormal = col.normal;
        Distance = col.distance;
        //Physics.Raycast(obj.transform.position, obj.rb.Velocity.normalized, out RaycastHit hit, Distance+0.1f);
        //hitPoint = hit.point;
    }
}
