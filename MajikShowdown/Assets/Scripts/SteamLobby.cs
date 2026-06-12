using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Steamworks;

public class SteamLobby : MonoBehaviour
{
    public static SteamLobby instance;
    public ulong lobbyID;

    protected Callback<LobbyCreated_t> lobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> joinRequest;
    protected Callback<LobbyEnter_t> lobbyEntered;

    private const string HOSTADDRESSKEY = "HostAddress";

    void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    void Start()
    {
        if(!SteamManager.Initialized)
        {
            return;
        }
        lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        joinRequest = Callback<GameLobbyJoinRequested_t>.Create(OnJoinRequest);
        lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
    }

    public void HostLobby()
    {
        Debug.Log("Botão clicado");

        if (!SteamManager.Initialized)
        {
            Debug.LogError("Steam não iniciou");
            return;
        }

        Debug.Log("Criando lobby...");
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, NetworkManager.singleton.maxConnections);
    }

    void OnLobbyCreated(LobbyCreated_t callback)
    {
        if (callback.m_eResult != EResult.k_EResultOK)
        {
            return;
        }
        NetworkManager.singleton.StartHost();
        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HOSTADDRESSKEY, SteamUser.GetSteamID().ToString());
        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), "name", SteamFriends.GetPersonaName().ToString() + "'s Lobby");
        lobbyID = callback.m_ulSteamIDLobby;
    }

    void OnJoinRequest(GameLobbyJoinRequested_t callback)
    {
        if(NetworkClient.isConnected || NetworkClient.active)
        {
            NetworkManager.singleton.StopClient();
            NetworkClient.Shutdown();
        }
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    void OnLobbyEntered(LobbyEnter_t callback)
    {
        if(NetworkServer.active)
        {
            return;
        }
        lobbyID = callback.m_ulSteamIDLobby;
        NetworkManager.singleton.networkAddress = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HOSTADDRESSKEY);
        NetworkManager.singleton.StartClient();
    }

    public void LeaveLobby()
    {
        CSteamID currentOwner = SteamMatchmaking.GetLobbyOwner(new CSteamID(lobbyID));
        CSteamID me = SteamUser.GetSteamID();
        var lobby = new CSteamID(lobbyID);
        List<CSteamID> members = new List<CSteamID>();
        int count = SteamMatchmaking.GetNumLobbyMembers(lobby);
        for(int i = 0; i < count; i++)
        {
            members.Add(SteamMatchmaking.GetLobbyMemberByIndex(lobby, i));
        }
        if(lobbyID != 0)
        {
            SteamMatchmaking.LeaveLobby(new CSteamID(lobbyID));
            lobbyID = 0;
        }
        if(NetworkServer.active && currentOwner == me)
        {
            NetworkManager.singleton.StopHost();
        }
        else if(NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopClient();
        }
    }
}
