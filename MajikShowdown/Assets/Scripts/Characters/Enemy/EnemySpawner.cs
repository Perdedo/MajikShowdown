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
    float spawnTime, spawnerStartTime, randDifficulty, randElemental, totalSelection, sum, elementalChance, hordeStartTime, hordeDurationTime;
    bool isSpawning;
    GameObject aux;
    Enemy auxEnemy;
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

    public void Initialize(List<EnemySelection> enemyList, float baseSpawn, float minSpawn, float lifetime, float baseDiff, float maxDiff, float baseEl, float maxEl, float hordeStart, float hordeDuration)
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
            hordeStartTime = hordeStart;
            hordeDurationTime = hordeDuration;
            foreach (EnemySelection e in enemies)
            {
                if (e.difficult)
                {
                    e.chance = e.spawnCurve.Evaluate(0) * 100 * Mathf.Lerp(baseDifficultyMult, maxDifficultyMult, difficultyCurve.Evaluate(0));
                }
                else
                {
                    e.chance = e.spawnCurve.Evaluate(0) * 100;
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
        if(GameManager.Instance.hordeController.enemies.Count < GameManager.Instance.hordeController.maxEnemyCount)
        {
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
                    if (GameManager.Instance.hordeController.enemiesByType[i].Count <= GameManager.Instance.hordeController.usedEnemiesByType[i].Count)
                    {
                        aux = Instantiate(enemies[i].enemy, spawnPos.position, Quaternion.identity);
                        NetworkServer.Spawn(aux);
                        GameManager.Instance.hordeController.enemiesByType[i].Add(aux.GetComponent<Enemy>());
                        auxEnemy = aux.GetComponent<Enemy>();
                        auxEnemy.instanceIndex = GameManager.Instance.hordeController.enemiesInfo.Count;
                        EnemyTransformInfo auxTrInfo = new EnemyTransformInfo(aux, spawnPos.position, aux.transform.localScale, Quaternion.identity, Time.time, Vector3.zero);
                        GameManager.Instance.hordeController.enemiesInfo.Add(auxTrInfo);
                        auxEnemy.transformInfo = auxTrInfo;
                    }
                    else
                    {
                        foreach(Enemy e in GameManager.Instance.hordeController.enemiesByType[i])
                        {
                            if (!GameManager.Instance.hordeController.usedEnemiesByType[i].Contains(e))
                            {
                                aux = e.gameObject;
                                aux.transform.position = spawnPos.position;
                                aux.SetActive(true);
                                ActivateEnemy(aux);
                                auxEnemy = aux.GetComponent<Enemy>();
                                break;
                            }
                        }
                    }
                    auxEnemy.ResetAllVelocities();
                    GameManager.Instance.hordeController.enemies.Add(aux);
                    GameManager.Instance.hordeController.UpdateEnemyText(GameManager.Instance.hordeController.enemies.Count);
                    GameManager.Instance.hordeController.usedEnemiesByType[i].Add(aux.GetComponent<Enemy>());
                    randElemental = UnityEngine.Random.Range(0, 100);
                    if(randElemental < elementalChance)
                    {
                        auxEnemy.element = (Elements)UnityEngine.Random.Range(0, Enum.GetNames(typeof(Elements)).Length);
                    }
                    aux.GetComponent<CharacterDamageHandler>().enemyIndex = i;
                    auxEnemy.Initialize();
                    i = enemies.Count;
                }
            }
        }
        if(isSpawning)
        {
            foreach (EnemySelection e in enemies)
            {
                if (e.difficult)
                {
                    e.chance = e.spawnCurve.Evaluate(Mathf.Clamp((Time.time - hordeStartTime) / hordeDurationTime, 0, 1)) * 100 * Mathf.Lerp(baseDifficultyMult, maxDifficultyMult, difficultyCurve.Evaluate(Mathf.Clamp((Time.time - hordeStartTime) / hordeDurationTime, 0, 1)));
                }
                else
                {
                    e.chance = e.spawnCurve.Evaluate(Mathf.Clamp((Time.time - hordeStartTime) / hordeDurationTime, 0, 1)) * 100;
                }
                /*if (e.difficult)
                { 
                    e.chance = e.baseChance * (Mathf.Lerp(baseDifficultyMult, maxDifficultyMult, difficultyCurve.Evaluate(Mathf.Clamp((Time.time - spawnerStartTime) / spawnerLifeTime, 0, 1))));
                }*/
            }
            elementalChance = Mathf.Lerp(baseElemental, maxElemental, elementalCurve.Evaluate(Mathf.Clamp((Time.time - hordeStartTime) / hordeDurationTime, 0, 1)));
            spawnTime = Mathf.Lerp(baseSpawnTime, minSpawnTime, spawnRateCurve.Evaluate(Mathf.Clamp((Time.time - hordeStartTime)/hordeDurationTime, 0, 1)));
            StartCoroutine(SpawnEnemy());
        }
    }

    [ClientRpc]
    public void ActivateEnemy(GameObject obj)
    {
        obj.SetActive(true);
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
        GameManager.Instance.hordeController.usedSpawners.Remove(this.gameObject);
        Disable();
        this.gameObject.SetActive(false);
        //NetworkServer.Destroy(this.gameObject);
    }

    [ClientRpc]
    public void Disable()
    {
        this.gameObject.SetActive(false);
    }
}

[Serializable]
public class EnemySelection
{
    public GameObject enemy;
    //public float baseChance;
    public float chance;
    public bool difficult;
    public AnimationCurve spawnCurve;
}
