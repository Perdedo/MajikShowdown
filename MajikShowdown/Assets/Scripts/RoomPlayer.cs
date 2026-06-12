using UnityEngine;
using Mirror;
using Steamworks;
public class RoomPlayer : NetworkRoomPlayer
{
    [SyncVar] public int connectionID;
    [SyncVar] public ulong playerSteamID;
    [SyncVar(hook = nameof(PlayerNameUpdate))] public string playerName;

    public override void OnStartAuthority()
    {
        CMDSetPlayerName(SteamFriends.GetPersonaName().ToString());
    }
    public override void OnStartClient()
    {
        NetworkManager.singleton.GetComponent<RoomManager>().playerList.Add(this);
        GameManager.Instance.uiController.UpdatePlayerList();
    }

    public override void OnStopClient()
    {
        NetworkManager.singleton.GetComponent<RoomManager>().playerList.Remove(this);
        GameManager.Instance.uiController.UpdatePlayerList();
    }

    public void PlayerNameUpdate(string oldVal, string newVal)
    {
        if(isServer)
        {
            this.playerName = newVal;
        }
        if(isClient)
        {
            GameManager.Instance.uiController.UpdatePlayerList();
        }
    }

    public override void ReadyStateChanged(bool oldReadyState, bool newReadyState)
    {
        GameManager.Instance.uiController.UpdatePlayerList();
    }

    [Command]
    void CMDSetPlayerName(string pName)
    {
        this.PlayerNameUpdate(this.playerName, pName);
    }
}
