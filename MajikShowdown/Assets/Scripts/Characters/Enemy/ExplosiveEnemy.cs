using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Mirror;
public class ExplosiveEnemy : Enemy
{
    public float explosionRadius = 10;
    public GameObject explosionVFX;
    public LayerMask affectedByExplosion;
    Collider[] hits;
    protected override void AttackPlayer()
    {
        GameObject inst = Instantiate(explosionVFX, transform.position, Quaternion.identity);
        NetworkServer.Spawn(inst);
        hits = Physics.OverlapSphere(transform.position, explosionRadius, affectedByExplosion);
        foreach(Collider collider in hits)
        {
            if(collider.gameObject != this.gameObject)
            {
                collider.gameObject.GetComponent<IGameCharacter>().DamageHandler.TakeDamage(dmgCtrl);
            }
        }
        this.DamageHandler.Die();
    }
}
