using Mirror;
using UnityEngine;

public class TankEnemy : Enemy
{
    public Transform hitPoint;
    public float hitRadius = 5, tremorRadius = 15;
    public int tremorDamage = 5;
    public LayerMask affectedByHit;
    Collider[] inRadius;
    Damage tremorDmgCtrl;

    public override void Initialize()
    {
        base.Initialize();
        tremorDmgCtrl = new Damage(tremorDamage, element);
    }
    protected override void AttackPlayer()
    {
        inRadius = Physics.OverlapSphere(hitPoint.position, tremorRadius, affectedByHit);
        foreach (Collider collider in inRadius)
        {
            if (collider.gameObject != this.gameObject)
            {
                if(Vector3.SqrMagnitude(hitPoint.position - collider.transform.position) < hitRadius * hitRadius)
                {
                    collider.gameObject.GetComponent<IGameCharacter>().DamageHandler.TakeDamage(dmgCtrl);
                }
                else
                {
                    collider.gameObject.GetComponent<IGameCharacter>().DamageHandler.TakeDamage(tremorDmgCtrl);
                }
            }
        }
    }
}
