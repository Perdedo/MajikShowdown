using Mirror;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class Explosion : NetworkBehaviour
{
    public float duration = 1;
    Timer disappearTimer = new Timer(false);
    public bool network = true;
    public int VFXIndex = -1;
    Elements enemyElement = Elements.Fire;
    private void Start()
    {
        if(!isServer && network)
        {
            return;
        }

        disappearTimer.SetTimer(0);
        disappearTimer.Paused = false;
    }

    public void Initialize(Elements element)
    {
        if(element == Elements.None)
        {
            enemyElement = Elements.Fire;
        }
        else
        {
            enemyElement = element;
        }
        VFXIndex = BuffersControl.Instance.SpawnEffect(enemyElement, SpellTypes.Explosion, transform, 10, duration);
    }

    private void Update()
    {
        if(!isServer && network)
        {
            return;
        }

        if(disappearTimer.timer(duration, Time.deltaTime, false, false))
        {
            BuffersControl.Instance.UnspawnEffect(enemyElement, SpellTypes.Explosion, VFXIndex);
            Disappear();
        }
    }

    void Disappear()
    {
        if(network)
        {
            NetworkServer.Destroy(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
