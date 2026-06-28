using System.Collections.Generic;
using UnityEngine;

public class LootSpawner : MonoBehaviour
{
    public static LootSpawner Instance;
    public RuneLootBox prefab;
    public List<RuneLootBox> runeLootBoxes;
    public List<RuneLootBox> ActiveLootBoxes;
    public List<RuneLootBox> InactiveLootBoxes;
    public RuneLootPool test;
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
    public void SpawnLootBox(Vector3 position, RuneLootPool pool)
    {
        RuneLootBox inst;
        GameObject g;
        if (InactiveLootBoxes.Count > 0)
        {
            inst = InactiveLootBoxes[0];
            InactiveLootBoxes.Remove(inst);
            g = inst.gameObject;
            inst.transform.position = position;
            g.SetActive(true);
        }
        else
        {
            g = Instantiate(prefab.gameObject, position, Quaternion.identity);
            inst = g.GetComponent<RuneLootBox>();
            runeLootBoxes.Add(inst);
        }
        inst.lootPool = pool;
        ActiveLootBoxes.Add(inst);
    }
    public void DespawnLootBox(RuneLootBox box)
    {
        box.lootPool = null;
        box.gameObject.SetActive(false);
        ActiveLootBoxes.Remove(box);
        InactiveLootBoxes.Add(box);
    }
}
