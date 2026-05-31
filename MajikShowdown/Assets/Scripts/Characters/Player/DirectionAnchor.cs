using UnityEngine;
using Mirror;
public class DirectionAnchor : NetworkBehaviour
{
    public Player player;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(!isLocalPlayer && player.network)
        {
            return;
        }
        MoveAnchor();
        if(!isServer && player.network && NetworkClient.ready)
        {
            CMDMoveAnchor();
        }
        //transform.position = player.playerCamera.transform.position;
        //transform.LookAt(new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z));
        
        //transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
    }

    void MoveAnchor()
    {
        transform.position = player.playerCamera.transform.position;
        transform.LookAt(new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z));
    }

    [Command]
    void CMDMoveAnchor()
    {
        transform.position = player.playerCamera.transform.position;
        transform.LookAt(new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z));
    }
}
