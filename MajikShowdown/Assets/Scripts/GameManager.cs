using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public UIController uiController;
    public List<Player> Players = new List<Player>();
    public NetworkAuxiliarControl netCtrl;

    public HexGrid TestGrid;
    public SpellCollider ProjectilePrefab;
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
        netCtrl.ffManager.gameObject.SetActive(true);
    }

    public void RemovePlayer(Player player)
    {
        Players.Remove(player);
        if(Players.Count <= 0)
        {
            netCtrl.ffManager.gameObject.SetActive(false);
        }
    }
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.E))
        {
            InstantiateSpellCollider(TestGrid.spell.SubSpells[0],Vector3.up);
        }
    }
    public void InstantiateSpellCollider(SubSpell subSpell, Vector3 pos)
    {
        GameObject g = Instantiate(ProjectilePrefab.gameObject, pos, Quaternion.identity);
        g.GetComponent<SpellCollider>().subSpell = subSpell;
    }
}
