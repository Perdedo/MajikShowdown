using Mirror;
using NUnit.Framework;
using Steamworks;
//using Unity.Multiplayer.PlayMode;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
public class RoomManager : NetworkRoomManager
{
    public List<RoomPlayer> playerList = new List<RoomPlayer>();
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        clientIndex++;

        if (Utils.IsSceneActive(RoomScene))
        {
            allPlayersReady = false;

            GameObject newRoomGameObject = OnRoomServerCreateRoomPlayer(conn);
            if (newRoomGameObject == null)
            {
                newRoomGameObject = Instantiate(roomPlayerPrefab.gameObject, Vector3.zero, Quaternion.identity);
            }
            newRoomGameObject.GetComponent<RoomPlayer>().connectionID = conn.connectionId;
            newRoomGameObject.GetComponent<RoomPlayer>().playerSteamID = (ulong)SteamMatchmaking.GetLobbyMemberByIndex((CSteamID)SteamLobby.instance.lobbyID, playerList.Count);
            NetworkServer.AddPlayerForConnection(conn, newRoomGameObject);
        }
        else
        {
            Debug.Log($"Not in Room scene...disconnecting {conn}");
            conn.Disconnect();
        }
    }
    public override void OnRoomServerPlayersReady()
    {
        if(GameManager.Instance.uiController.startGameButton != null)
        {
            GameManager.Instance.uiController.startGameButton.GetComponent<Button>().interactable = true;
        }
    }

    public override void OnRoomServerPlayersNotReady()
    {
        if (GameManager.Instance.uiController.startGameButton != null)
        {
            GameManager.Instance.uiController.startGameButton.GetComponent<Button>().interactable = false;
        }
    }

    public override void ReadyStatusChanged()
    {
        int CurrentPlayers = 0;
        int ReadyPlayers = 0;

        foreach (NetworkRoomPlayer item in roomSlots)
        {
            if (item != null)
            {
                CurrentPlayers++;
                if (item.readyToBegin)
                    ReadyPlayers++;
            }
        }

        GameManager.Instance.uiController.playersReady.GetComponent<SyncedUIElement>().SyncText(ReadyPlayers + "/" + CurrentPlayers + " Players Ready");

        if (CurrentPlayers == ReadyPlayers)
        {
            CheckReadyToBegin();
        }
        else
        {
            allPlayersReady = false;
        }

        RoomUIChangesAlt();
    }

    public void GoToGameScene()
    {
        ServerChangeScene(GameplayScene);
    }

    public override void OnRoomClientEnter() 
    {
        GameManager.Instance.uiController.roomName.GetComponent<SyncedUIElement>().SyncText(SteamMatchmaking.GetLobbyData(new CSteamID(SteamLobby.instance.lobbyID), "name"));
        RoomUIChanges();
    }
    public override void OnRoomClientExit() 
    { 
        RoomUIChanges();
    }

    public void RoomUIChanges()
    {
        int CurrentPlayers = 0;
        int ReadyPlayers = 0;

        foreach (NetworkRoomPlayer item in roomSlots)
        {
            if (item != null)
            {
                CurrentPlayers++;
                if (item.readyToBegin)
                    ReadyPlayers++;
            }
        }
        GameManager.Instance.uiController.playersReady.GetComponent<SyncedUIElement>().SyncText(ReadyPlayers + "/" + CurrentPlayers + " Players Ready");
        if(GameManager.Instance.uiController.roomObjectsVisible)
        {
            GameManager.Instance.uiController.inviteButton.GetComponent<SyncedUIElement>().ShowOnlyForHost(CurrentPlayers < maxConnections);
        }

        if (allPlayersReady)
        {
            GameManager.Instance.uiController.waiting.GetComponent<SyncedUIElement>().SyncText("Waiting for the host to start the game...");
            if (GameManager.Instance.uiController.roomObjectsVisible)
            {
                GameManager.Instance.uiController.waiting.GetComponent<SyncedUIElement>().ShowOnlyForClients(false);
                GameManager.Instance.uiController.startGameButton.GetComponent<SyncedUIElement>().ShowOnlyForHost(true);
            }
        }
        else
        {
            GameManager.Instance.uiController.waiting.GetComponent<SyncedUIElement>().SyncText("Waiting for players to get ready...");
            if (GameManager.Instance.uiController.roomObjectsVisible)
            {
                GameManager.Instance.uiController.startGameButton.GetComponent<SyncedUIElement>().HideForAll();
                GameManager.Instance.uiController.waiting.GetComponent<SyncedUIElement>().ShowForAll(false);
            }
        }
    }

    public void RoomUIChangesAlt()
    {
        if (allPlayersReady)
        {
            GameManager.Instance.uiController.waiting.GetComponent<SyncedUIElement>().ShowOnlyForClients(false);
            GameManager.Instance.uiController.startGameButton.GetComponent<SyncedUIElement>().ShowOnlyForHost(true);
            GameManager.Instance.uiController.waiting.GetComponent<SyncedUIElement>().SyncText("Waiting for the host to start the game...");
        }
        else
        {
            GameManager.Instance.uiController.startGameButton.GetComponent<SyncedUIElement>().HideForAll();
            GameManager.Instance.uiController.waiting.GetComponent<SyncedUIElement>().ShowForAll(false);
            GameManager.Instance.uiController.waiting.GetComponent<SyncedUIElement>().SyncText("Waiting for players to get ready...");
        }
    }
}
