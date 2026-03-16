using Mirror;
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
            CheckReadyToBegin();
        else
            allPlayersReady = false;

    }

    public void GoToGameScene()
    {
        ServerChangeScene(GameplayScene);
    }
}
