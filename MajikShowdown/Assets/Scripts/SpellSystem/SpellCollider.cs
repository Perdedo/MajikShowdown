using System.Collections;
using System.Collections.Generic;
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
    float LifeTime = 0;

    public UnityEvent OnCast = new UnityEvent(), OnHit = new UnityEvent(), OnDeath = new UnityEvent();
    public void Initialize(Spell owner, bool isPrimary)
    {
        //projectileConfig.CalculateFinalStats();
        OwnerSpell = owner;;
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

        if(OwnerSpell.primaryNode.Type == SpellType.SpellTypes.Explosion)
        {
            transform.localScale = Vector3.zero;
        }
    }
    void Update()
    {
        if (HitOnCooldown)
        {
            HitOnCooldown = HitTimer.timer(OwnerSpell.primaryNode.HitCooldown, Time.deltaTime, true, false);
        }
        rb.Velocity = ToLookDirection(OwnerSpell.primaryNode.GetVelocity());
        foreach (SpellTrigger t in OwnerSpell.triggers)
        {
            t.UpdateTrigger();
        }

        switch (OwnerSpell.primaryNode.Type)
        {
            case SpellType.SpellTypes.Explosion:
            transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one* stats.Size, LifeTime/stats.Duration);
            break;
        }

        LifeTime += Time.deltaTime;
        if(LifeTime >= stats.Duration)
        {
            Die();
        }
        
    }
    public void InitiateTriggeredSpells()
    {
        foreach (SpellTrigger t in OwnerSpell.triggers)
        {
            if (t.TriggeredSpell == null) continue;
            switch (t.trigger)
            {
                case SpellTrigger.Triggers.OnCast:
                    AddSpellToEvent(OnCast, t.TriggeredSpell, t);
                    break;
                case SpellTrigger.Triggers.OnHit:
                    AddSpellToEvent(OnHit, t.TriggeredSpell, t);
                    break;
                case SpellTrigger.Triggers.OnDeath:
                    AddSpellToEvent(OnDeath, t.TriggeredSpell, t);
                    break;
            }
        }
    }
    public void AddSpellToEvent(UnityEvent e, Spell spell, SpellTrigger trigger)
    {
        UnityAction action = () =>
        {
            if(!trigger.SpellOnCooldown)
            {
                OwnerSpell.Owner.InstantiateSpellCollider(spell, transform.position, transform.forward);
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
        if(!OwnerSpell.primaryNode.HitOnStay)
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
        if (OwnerSpell.primaryNode.Collisions.Enemies && LayerMaskUtility.BelongsInMask(other.gameObject.layer, OwnerSpell.Owner.EnemyLayer))
        {
            OnHit.Invoke();
            CollideCreature(other);
        }
        else if (OwnerSpell.primaryNode.Collisions.Players && LayerMaskUtility.BelongsInMask(other.gameObject.layer, OwnerSpell.Owner.PlayerLayer))
        {
            OnHit.Invoke();
            CollideCreature(other);
        }
        else if (OwnerSpell.primaryNode.Collisions.Objects && LayerMaskUtility.BelongsInMask(other.gameObject.layer, OwnerSpell.Owner.ObjectLayer))
        {
            OnHit.Invoke();
            CollideObject();
        }
    }
    
    public void CollideObject()
    {
        CheckBounce();
    }
    public void CollideCreature(Collider c)
    {
        CharacterDamageHandler character = c.GetComponent<CharacterDamageHandler>();
        if (character != null)        {
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
            CheckBounce();
        }
    }
    public void CheckBounce()
    {
        if (bounceCount >= 1)
        {
            bounceCount--;
            Bounce();
        }
        else
        {
            Die();
        }
    }
    public void Bounce()
    {

    }
    public void Die()
    {
        OnDeath.Invoke();
        Destroy(gameObject);
    }
}
