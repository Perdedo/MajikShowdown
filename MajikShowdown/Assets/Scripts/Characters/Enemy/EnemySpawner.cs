using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EnemySpawner : NetworkBehaviour
{
    //public GameObject enemy;
    public List<EnemySelection> enemies = new List<EnemySelection>();
    public Transform spawnPos;
    public float baseSpawnTime = 1.25f, minSpawnTime = 0.25f, spawnerLifeTime = 300f, baseDifficultyMult = 1, maxDifficultyMult = 1.5f, baseElemental = 5, maxElemental = 30;
    float spawnTime, spawnerStartTime, randDifficulty, randElemental, totalSelection, sum, elementalChance;
    bool isSpawning;
    GameObject aux;
    public AnimationCurve spawnRateCurve;
    public AnimationCurve difficultyCurve;
    public AnimationCurve elementalCurve;

    /*
    private void Start()
    {
        if(isServer)
        {
            spawnerStartTime = Time.time;
            isSpawning = true;
            spawnTime = baseSpawnTime;
            elementalChance = baseElemental;
            StartCoroutine(SpawnEnemy());
            //StartCoroutine(IncreaseSpawnRate());
            StartCoroutine(StopSpawning());
        }
    }*/

    public void Initialize(List<EnemySelection> enemyList, float baseSpawn, float minSpawn, float lifetime, float baseDiff, float maxDiff, float baseEl, float maxEl)
    {
        if (isServer)
        {
            enemies = enemyList;
            baseSpawnTime = baseSpawn;
            minSpawnTime = minSpawn;
            spawnerLifeTime = lifetime;
            baseDifficultyMult = baseDiff;
            maxDifficultyMult = maxDiff;
            baseElemental = baseEl;
            maxElemental = maxEl;
            spawnerStartTime = Time.time;
            isSpawning = true;
            spawnTime = baseSpawnTime;
            elementalChance = baseElemental;
            foreach (EnemySelection e in enemies)
            {
                if (e.difficult)
                {
                    e.chance = e.baseChance * Mathf.Lerp(baseDifficultyMult, maxDifficultyMult, difficultyCurve.Evaluate(0));
                }
                else
                {
                    e.chance = e.baseChance;
                }
            }
            StartCoroutine(SpawnEnemy());
            //StartCoroutine(IncreaseSpawnRate());
            StartCoroutine(StopSpawning());
        }
    }

    [Server]
    IEnumerator SpawnEnemy()
    {
        yield return new WaitForSeconds(spawnTime);
        totalSelection = 0;
        foreach(EnemySelection e in enemies)
        {
            totalSelection += e.chance;
        }
        randDifficulty = UnityEngine.Random.Range(0, totalSelection);
        sum = 0;
        for(int i = 0; i < enemies.Count; i++)
        {
            sum += enemies[i].chance;
            if(randDifficulty <= sum)
            {
                aux = Instantiate(enemies[i].enemy, spawnPos.position, Quaternion.identity);
                GameManager.Instance.hordeController.enemies.Add(aux);
                randElemental = UnityEngine.Random.Range(0, 100);
                if(randElemental < elementalChance)
                {
                    aux.GetComponent<Enemy>().element = (Elements)UnityEngine.Random.Range(0, Enum.GetNames(typeof(Elements)).Length);
                }
                NetworkServer.Spawn(aux);
                i = enemies.Count;
            }
        }
        if(isSpawning)
        {
            foreach (EnemySelection e in enemies)
            {
                if(e.difficult)
                { 
                    e.chance = e.baseChance * (Mathf.Lerp(baseDifficultyMult, maxDifficultyMult, difficultyCurve.Evaluate(Mathf.Clamp((Time.time - spawnerStartTime) / spawnerLifeTime, 0, 1))));
                }
            }
            elementalChance = Mathf.Lerp(baseElemental, maxElemental, elementalCurve.Evaluate(Mathf.Clamp((Time.time - spawnerStartTime) / spawnerLifeTime, 0, 1)));
            spawnTime = Mathf.Lerp(baseSpawnTime, minSpawnTime, spawnRateCurve.Evaluate(Mathf.Clamp((Time.time - spawnerStartTime)/spawnerLifeTime, 0, 1)));
            StartCoroutine(SpawnEnemy());
        }
    }

    /*[Server]
    IEnumerator IncreaseSpawnRate()
    {
        yield return new WaitForSeconds(spawnRateIncreaseTime);
        spawnTime = Mathf.Clamp(spawnTime - spawnRateIncrease, minSpawnTime, baseSpawnTime);
        if(isSpawning)
        {
            StartCoroutine(IncreaseSpawnRate());
        }
    }*/

    [Server]
    IEnumerator StopSpawning()
    {
        yield return new WaitForSeconds(spawnerLifeTime);
        isSpawning = false;
        StopAllCoroutines();
        NetworkServer.Destroy(this.gameObject);
    }
}

[Serializable]
public class EnemySelection
{
    public GameObject enemy;
    public float baseChance;
    public float chance;
    public bool difficult;
}
