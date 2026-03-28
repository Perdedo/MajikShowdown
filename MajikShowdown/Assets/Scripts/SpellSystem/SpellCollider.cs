using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SpellCollider : MonoBehaviour
{
    public StaticRB rb;
    public StatTypes stats;
    public Spell OwnerSpell;
    int pierceCount, bounceCount;
    public bool primarySpell;

    public UnityEvent OnCast = new UnityEvent(), OnHit = new UnityEvent(), OnDeath = new UnityEvent();
    private void Start()
    {
        //projectileConfig.CalculateFinalStats();
        stats = OwnerSpell.primaryNode.FinalStats;
        transform.localScale = Vector3.one * stats.Size;
        Invoke("Die", stats.Duration);
        pierceCount = (int)stats.Piercing;
        bounceCount = (int)stats.Bounce;
        if (primarySpell)
        {
            InitiateTriggeredSpells();
        }
        OnCast.Invoke();
    }
    void Update()
    {
        rb.Velocity = ToLookDirection(OwnerSpell.primaryNode.GetVelocity());
    }
    public void InitiateTriggeredSpells()
    {
        foreach (SpellTrigger t in OwnerSpell.triggers)
        {
            if (t.TriggeredSpell == null) continue;
            switch (t.trigger)
            {
                case SpellTrigger.Triggers.OnCast:
                    AddSpellToEvent(OnCast, t.TriggeredSpell);
                    break;
                case SpellTrigger.Triggers.OnHit:
                    AddSpellToEvent(OnHit, t.TriggeredSpell);
                    break;
                case SpellTrigger.Triggers.OnDeath:
                    AddSpellToEvent(OnDeath, t.TriggeredSpell);
                    break;
            }
        }
    }
    public void AddSpellToEvent(UnityEvent e, Spell spell)
    {
        e.AddListener(() => { OwnerSpell.Owner.InstantiateSpellCollider(spell, transform.position,transform.forward); });
    }
    public Vector3 ToLookDirection(Vector3 rawDir)
    {
        return transform.forward * rawDir.z + transform.up * rawDir.y + transform.right * rawDir.x;
    }
    public void Die()
    {
        OnDeath.Invoke();
        Destroy(gameObject);
    }
    void OnTriggerEnter(Collider other)
    {
        if (OwnerSpell.primaryNode.Collisions.Enemies && LayerMaskUtility.BelongsInMask(other.gameObject.layer, OwnerSpell.Owner.EnemyLayer))
        {
            OnHit.Invoke();
            CollideCreature();
        }
        else if (OwnerSpell.primaryNode.Collisions.Players && LayerMaskUtility.BelongsInMask(other.gameObject.layer, OwnerSpell.Owner.PlayerLayer))
        {
            OnHit.Invoke();
            CollideCreature();
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
    public void CollideCreature()
    {
        if (pierceCount > 0)
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
        if (bounceCount > 0)
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
}
