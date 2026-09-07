using Mirror;
using UnityEngine;

public class TankEnemy : Enemy
{
    public Transform hitPoint;
    public float hitRadius = 5, tremorRadius = 15, hitKnockbackStrength = 100, tremorKnockbackStrength = 50;
    public int tremorDamage = 5;
    public LayerMask affectedByHit;
    Collider[] inRadius;
    Damage tremorDmgCtrl;
    Vector3 hitDir;
    IGameCharacter aux;
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
                hitDir = collider.transform.position - hitPoint.position;
                aux = collider.gameObject.GetComponent<IGameCharacter>();
                if (Vector3.SqrMagnitude(hitDir) < hitRadius * hitRadius)
                {
                    if(aux is Character)
                    {
                        aux.DamageHandler.TakeDamage(dmgCtrl);
                    }
                    aux.Knockback((hitDir.normalized + Vector3.up).normalized, hitKnockbackStrength);
                }
                else
                {
                    if(aux is Character)
                    {
                        aux.DamageHandler.TakeDamage(tremorDmgCtrl);
                    }
                    aux.Knockback((hitDir.normalized + Vector3.up).normalized, tremorKnockbackStrength);
                }
            }
        }
    }
}
