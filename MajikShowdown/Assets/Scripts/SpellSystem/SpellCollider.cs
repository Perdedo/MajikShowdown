using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SpellCollider : MonoBehaviour
{
    public StaticRB rb;
    public StatTypes stats;
    public SubSpell subSpell;
    int pierceCount, bounceCount;

    public UnityEvent OnCast, OnHit, OnDeath;
    private void Start()
    {
        //projectileConfig.CalculateFinalStats();
        stats = subSpell.Type.FinalStats;
        transform.localScale = Vector3.one * stats.Size;
        Invoke("Die", stats.Duration);
        pierceCount = (int)stats.Piercing;
        bounceCount = (int)stats.Bounce;
        InitiateTriggeredSpells();
        OnCast.Invoke();
    }
    void Update()
    {
        rb.Velocity = ToLookDirection(subSpell.Type.GetVelocity());
    }
    public void InitiateTriggeredSpells()
    {
        foreach (SpellTrigger t in subSpell.triggers)
        {
            switch (t.trigger)
            {
                case SpellTrigger.Triggers.OnCast:
                    AddSpellsToEvent(OnCast, t.TriggeredSubspells());
                    break;
                case SpellTrigger.Triggers.OnHit:
                    AddSpellsToEvent(OnHit, t.TriggeredSubspells());
                    break;
                case SpellTrigger.Triggers.OnDeath:
                    AddSpellsToEvent(OnDeath, t.TriggeredSubspells());
                    break;
            }
        }
    }
    public void AddSpellsToEvent(UnityEvent e, List<SubSpell> subSpells)
    {
        foreach (SubSpell s in subSpells)
        {
            e.AddListener(() => { subSpell.spell.Owner.InstantiateSpellCollider(s, transform.position); });
        }
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
        if (subSpell.Type.Collisions.Enemies && LayerMaskUtility.BelongsInMask(other.gameObject.layer, subSpell.spell.Owner.EnemyLayer))
        {
            OnHit.Invoke();
            CollideCreature();
        }
        else if (subSpell.Type.Collisions.Players && LayerMaskUtility.BelongsInMask(other.gameObject.layer, subSpell.spell.Owner.PlayerLayer))
        {
            OnHit.Invoke();
            CollideCreature();
        }
        else if (subSpell.Type.Collisions.Objects && LayerMaskUtility.BelongsInMask(other.gameObject.layer, subSpell.spell.Owner.ObjectLayer))
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
