using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public UIController uiController;
    public List<Player> Players = new List<Player>();
    public NetworkAuxiliarControl netCtrl;

    
    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        //Physics.IgnoreLayerCollision(LayerMask.GetMask("Enemy"), LayerMask.GetMask("Enemy"));
    }

    public void AddPlayer(Player player)
    {
        Players.Add(player);
        netCtrl?.ffManager.gameObject.SetActive(true);
    }

    public void RemovePlayer(Player player)
    {
        Players.Remove(player);
        if(Players.Count <= 0)
        {
            netCtrl?.ffManager?.gameObject.SetActive(false);
        }
    }
    
}
