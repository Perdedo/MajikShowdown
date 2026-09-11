using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Mirror;
public class ExplosiveEnemy : Enemy
{
    public float explosionRadius = 10, knockbackStrength = 100, knockbackLift = 0.5f;
    public GameObject explosionVFX;
    public LayerMask affectedByExplosion;
    Collider[] hits;
    bool exploded = false;
    Vector3 knockbackDir;
    public override void Initialize()
    {
        base.Initialize();
        exploded = false;
    }
    protected override void AttackPlayer()
    {
        this.DamageHandler.Die();
    }

    void Explode()
    {
        exploded = true;
        GameObject inst = Instantiate(explosionVFX, transform.position, Quaternion.identity);
        NetworkServer.Spawn(inst);
        hits = Physics.OverlapSphere(transform.position, explosionRadius, affectedByExplosion);
        foreach (Collider collider in hits)
        {
            if (collider.gameObject != this.gameObject)
            {
                collider.gameObject.GetComponent<IGameCharacter>().DamageHandler.TakeDamage(dmgCtrl);
                knockbackDir = collider.transform.position - transform.position;
                knockbackDir.y = 0;
                collider.gameObject.GetComponent<IGameCharacter>().Knockback(knockbackDir.normalized + Vector3.up * knockbackLift, knockbackStrength);
            }
        }
    }

    public override void Die()
    {
        if(!exploded)
        {
            Explode();
        }
        base.Die();
    }
}
