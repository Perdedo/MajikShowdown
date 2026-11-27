using Mirror;
using System.Collections;
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
    public float collectTime = 1.2f;

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
            if(network) RPCActivateLootBox(g, position);
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
        ActiveLootBoxes.Remove(box);
        InactiveLootBoxes.Add(box);
        StartCoroutine(WaitCollectAnim(box));
        //box.gameObject.SetActive(false);
        //if(network) RPCDeactivateLootBox(box.gameObject);
    }

    IEnumerator WaitCollectAnim(RuneLootBox box)
    {
        yield return new WaitForSeconds(collectTime);
        box.gameObject.SetActive(false);
        if (network) RPCDeactivateLootBox(box.gameObject);
    }

    [ClientRpc]
    public void RPCActivateLootBox(GameObject obj, Vector3 pos)
    {
        obj.transform.position = pos;
        obj.SetActive(true);
    }

    [ClientRpc]
    public void RPCDeactivateLootBox(GameObject obj)
    {
        obj.SetActive(false);
    }

}
