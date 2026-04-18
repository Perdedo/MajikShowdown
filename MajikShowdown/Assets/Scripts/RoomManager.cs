using Mirror;
using NUnit.Framework;
using Steamworks;
//using Unity.Multiplayer.PlayMode;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
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

        if(NetworkClient.active)
        {
            GameManager.Instance.uiController.playersReady.GetComponent<SyncedUIElement>().CMDSyncText(ReadyPlayers + "/" + CurrentPlayers + " Players Ready");
        }

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
        if (NetworkClient.active)
        {
            //GameManager.Instance.uiController.roomName.GetComponent<SyncedUIElement>().CMDSyncText(SteamMatchmaking.GetLobbyData(new CSteamID(SteamLobby.instance.lobbyID), "name"));
            StartCoroutine(SyncTextWhenReady(GameManager.Instance.uiController.roomName.GetComponent<SyncedUIElement>(), SteamMatchmaking.GetLobbyData(new CSteamID(SteamLobby.instance.lobbyID), "name")));
        }
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
        if (Utils.IsSceneActive(RoomScene))
        {
            if (NetworkClient.active)
            {
                //GameManager.Instance.uiController.playersReady.GetComponent<SyncedUIElement>().CMDSyncText(ReadyPlayers + "/" + CurrentPlayers + " Players Ready");
                StartCoroutine(SyncTextWhenReady(GameManager.Instance.uiController.playersReady.GetComponent<SyncedUIElement>(), ReadyPlayers + "/" + CurrentPlayers + " Players Ready"));
            }
            if (GameManager.Instance.uiController.roomObjectsVisible)
            {
                GameManager.Instance.uiController.inviteButton.GetComponent<SyncedUIElement>().ShowOnlyForHost(CurrentPlayers < maxConnections);
            }

            if (allPlayersReady)
            {
                if (NetworkClient.active)
                {
                    //GameManager.Instance.uiController.waiting.GetComponent<SyncedUIElement>().CMDSyncText("Waiting for the host to start the game...");
                    StartCoroutine(SyncTextWhenReady(GameManager.Instance.uiController.waiting.GetComponent<SyncedUIElement>(), "Waiting for the host to start the game..."));
                }
                if (GameManager.Instance.uiController.roomObjectsVisible)
                {
                    GameManager.Instance.uiController.waiting.GetComponent<SyncedUIElement>().ShowOnlyForClients(false);
                    GameManager.Instance.uiController.startGameButton.GetComponent<SyncedUIElement>().ShowOnlyForHost(true);
                }
            }
            else
            {
                if (NetworkClient.active)
                {
                    //GameManager.Instance.uiController.waiting.GetComponent<SyncedUIElement>().CMDSyncText("Waiting for players to get ready...");
                    StartCoroutine(SyncTextWhenReady(GameManager.Instance.uiController.waiting.GetComponent<SyncedUIElement>(), "Waiting for players to get ready..."));
                }
                if (GameManager.Instance.uiController.roomObjectsVisible)
                {
                    GameManager.Instance.uiController.startGameButton.GetComponent<SyncedUIElement>().HideForAll();
                    GameManager.Instance.uiController.waiting.GetComponent<SyncedUIElement>().ShowForAll(false);
                }
            }
        }
    }

    public void RoomUIChangesAlt()
    {
        if (allPlayersReady)
        {
            GameManager.Instance.uiController.waiting.GetComponent<SyncedUIElement>().ShowOnlyForClients(false);
            GameManager.Instance.uiController.startGameButton.GetComponent<SyncedUIElement>().ShowOnlyForHost(true);
            if (NetworkClient.active)
            {
                GameManager.Instance.uiController.waiting.GetComponent<SyncedUIElement>().CMDSyncText("Waiting for the host to start the game...");
            }
        }
        else
        {
            GameManager.Instance.uiController.startGameButton.GetComponent<SyncedUIElement>().HideForAll();
            GameManager.Instance.uiController.waiting.GetComponent<SyncedUIElement>().ShowForAll(false);
            if (NetworkClient.active)
            {
                GameManager.Instance.uiController.waiting.GetComponent<SyncedUIElement>().CMDSyncText("Waiting for players to get ready...");
            }
        }
    }

    IEnumerator SyncTextWhenReady(SyncedUIElement ui, string txt)
    {
        yield return new WaitUntil(() => NetworkClient.ready && ui.netId != 0);
        ui.CMDSyncText(txt);
    }
}
