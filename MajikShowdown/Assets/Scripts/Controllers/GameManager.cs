using Mirror;
using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public UIController uiController;
    public List<Player> Players = new List<Player>();
    public NetworkAuxiliarControl netCtrl;
    public HordeController hordeController;
    public TrainingEnemiesController trainingController;
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
        if(Players.Count > 1)
        {
            foreach(Player p1 in Players)
            {
                foreach(Player p2 in Players)
                {
                    if(p1 != p2)
                    {
                        foreach(AllyCardUI card in p1.myUI.allyCards)
                        {
                            if(!p1.myUI.usedAllyCards.Contains(card))
                            {
                                p1.myUI.usedAllyCards.Add(card);
                                if(p2.steamSynced && p2.nameSynced)
                                {
                                    card.Initialize(p2);
                                }
                                else
                                {
                                    StartCoroutine(WaitCardInitialize(p2, card));
                                }
                                break;
                            }
                        }
                    }
                }
            }
        }
        /*if(hordeController != null)
        {
            hordeController.Initialize();
        }*/
    }


    IEnumerator WaitCardInitialize(Player p, AllyCardUI card)
    {
        yield return new WaitUntil(() => p.steamSynced);
        yield return new WaitUntil(() => p.nameSynced);
        card.Initialize(p);
    }

    public void RemovePlayer(Player player, bool isServer)
    {
        Players.Remove(player);
        if(Players.Count <= 0)
        {
            netCtrl?.ffManager?.gameObject.SetActive(false);
        }
        else if(isServer)
        {
            foreach(Player p in Players)
            {
                foreach(AllyCardUI card in p.myUI.usedAllyCards)
                {
                    if(card.myPlayer == player)
                    {
                        card.Disable(card.id.connectionToClient);
                        break;
                    }
                }
            }
        }
    }
    
}
