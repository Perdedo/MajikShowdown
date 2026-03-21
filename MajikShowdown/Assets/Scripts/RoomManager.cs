using Mirror;
using Steamworks;
using Unity.Multiplayer.PlayMode;
using UnityEngine;
using UnityEngine.UI;
public class RoomManager : NetworkRoomManager
{
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
