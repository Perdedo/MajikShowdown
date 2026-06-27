using Mirror;
using UnityEngine;

public class DeathPlane : NetworkBehaviour
{
    public Transform spawn;
    private void OnTriggerEnter(Collider other)
    {
        if(!isServer)
        {
            return;
        }
        if(other.gameObject.TryGetComponent<Enemy>(out Enemy e))
        {
            e.DamageHandler.Die();
        }
        else if(other.gameObject.TryGetComponent<Player>(out Player p))
        {
            p.gameObject.transform.position = spawn.position;
        }
    }
}
