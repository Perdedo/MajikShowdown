using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public UIController uiController;
    public List<Player> Players = new List<Player>();
    public NetworkAuxiliarControl netCtrl;
    public HordeController hordeController;
    [Header("Interactable Objects")]
    public float interactionRadius = 2;
    protected List<InteractableObject> interactables = new List<InteractableObject>();

    void Update()
    {
        foreach (InteractableObject i in interactables)
        {
            i.CheckForPlayer();
        }
    }
    public void AddInteractable(InteractableObject interactable)
    {
        interactables.Add(interactable);
    }
    public void RemoveInteractable(InteractableObject interactable)
    {
        interactables.Remove(interactable);
        foreach(Player p in Players)
        {
            if(p.currentInteraction == interactable)
            {
                p.currentInteraction = null;
            }
        }
    }
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
        if(NetworkManager.singleton != null && NetworkManager.singleton.GetComponent<RoomManager>() != null && Players.Count != NetworkManager.singleton.GetComponent<RoomManager>().playerList.Count)
        {
            return;
        }
        netCtrl?.ffManager.gameObject.SetActive(true);
        if(hordeController != null)
        {
            hordeController.Initialize();
        }
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
