using Mirror;
using UnityEngine;

public class Explosion : NetworkBehaviour
{
    public float duration = 1;
    Timer disappearTimer = new Timer(false);
    public bool network = true;
    private void Start()
    {
        if(!isServer)
        {
            return;
        }

        disappearTimer.SetTimer(0);
        disappearTimer.Paused = false;
    }

    private void Update()
    {
        if(!isServer)
        {
            return;
        }

        if(disappearTimer.timer(duration, Time.deltaTime, false, false))
        {
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
