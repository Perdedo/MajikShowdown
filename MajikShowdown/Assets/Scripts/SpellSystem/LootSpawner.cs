using Mirror;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LootSpawner : NetworkBehaviour
{
    public static LootSpawner Instance;
    public RuneLootBox prefab;
    public List<RuneLootBox> runeLootBoxes;
    public List<RuneLootBox> ActiveLootBoxes;
    public List<RuneLootBox> InactiveLootBoxes;
    public List<RuneLootPool> lootPools;
    public bool network = true;

    void Update()
    {
        /*if (Input.GetKeyDown(KeyCode.L))
        {
            SpawnLootBox(Vector3.up, test);
        }*/
    }
    void Awake()
    {
        Instance = this;
    }
    public void SpawnLootBox(Vector3 position, /*RuneLootPool pool*/ int poolInd)
    {
        if (!isServer && network)
        {
            return;
        }
        RuneLootBox inst;
        GameObject g;
        if (InactiveLootBoxes.Count > 0)
        {
            inst = InactiveLootBoxes[0];
            InactiveLootBoxes.Remove(inst);
            g = inst.gameObject;
            inst.transform.position = position;
            g.SetActive(true);
            if(network) RPCActivateLootBox(g);
        }
        else
        {
            g = Instantiate(prefab.gameObject, position, Quaternion.identity);
            if(network) NetworkServer.Spawn(g);
            inst = g.GetComponent<RuneLootBox>();
            runeLootBoxes.Add(inst);
        }
        //inst.lootPool = lootPools[poolInd];
        inst.lootPoolInd = poolInd;
        ActiveLootBoxes.Add(inst);
        inst.Initialize();
    }
    public void DespawnLootBox(RuneLootBox box)
    {
        box.lootPool = null;
        box.gameObject.SetActive(false);
        if(network) RPCDeactivateLootBox(box.gameObject);
        ActiveLootBoxes.Remove(box);
        InactiveLootBoxes.Add(box);
    }

    [ClientRpc]
    public void RPCActivateLootBox(GameObject obj)
    {
        obj.SetActive(true);
    }

    [ClientRpc]
    public void RPCDeactivateLootBox(GameObject obj)
    {
        obj.SetActive(false);
    }

}
