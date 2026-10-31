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
    float spawnTime, randDifficulty, randElemental, totalSelection, sum, elementalChance, hordeStartTime, hordeDurationTime, maxSpawnRadius;
    bool isSpawning;
    GameObject aux;

    Enemy auxEnemy;
    HordeController controller;
    /*public AnimationCurve spawnRateCurve;
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

    public void Initialize(List<EnemySelection> enemyList, float baseSpawn, float minSpawn, float lifetime, float baseEl, float maxEl, float hordeStart, float hordeDuration, float spawnRadius)
    {
        if (isServer)
        {
            controller = GameManager.Instance.hordeController;
            enemies = enemyList;
            baseSpawnTime = baseSpawn;
            minSpawnTime = minSpawn;
            spawnerLifeTime = lifetime;
            baseElemental = baseEl;
            maxElemental = maxEl;
            isSpawning = true;
            spawnTime = baseSpawnTime;
            elementalChance = baseElemental;
            hordeStartTime = hordeStart;
            hordeDurationTime = hordeDuration;
            maxSpawnRadius = spawnRadius;
            foreach (EnemySelection e in enemies)
            {
                /*if (e.difficult)
                {
                    e.chance = e.spawnCurve.Evaluate(0) * 100 * Mathf.Lerp(baseDifficultyMult, maxDifficultyMult, difficultyCurve.Evaluate(0));
                }
                else
                {
                    e.chance = e.spawnCurve.Evaluate(0) * 100;
                }*/
                e.chance = e.spawnCurve.Evaluate(0) * 100;
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
        if(controller.enemies.Count < controller.maxEnemyCount)
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

                    if (controller.enemiesByType[i].Count <= controller.usedEnemiesByType[i].Count)
                    {
                        if(i != controller.hordes[controller.hordeIndex].casterEnemyIndex)
                        {
                            aux = Instantiate(enemies[i].enemy, spawnPos.position + GetSpawnDirection(), Quaternion.identity);
                        }
                        else
                        {
                            Vector3 auxPos = spawnPos.position + GetSpawnDirection();
                            if (!Physics.CapsuleCast(auxPos - (Vector3.up * controller.spawnerHeight) + Vector3.up * 0.5f, auxPos - (Vector3.up * controller.spawnerHeight) + Vector3.up * 2f, 0.5f, Vector3.up, controller.spawnerHeight))
                            {
                                aux = Instantiate(enemies[i].enemy, auxPos + Vector3.up * (controller.casterEnemySpawnHeight - controller.spawnerHeight), Quaternion.identity);
                            }
                            else
                            {
                                aux = Instantiate(enemies[i].enemy, auxPos, Quaternion.identity);
                            }
                        }
                        NetworkServer.Spawn(aux);
                        auxEnemy = aux.GetComponent<Enemy>();
                        auxEnemy.transparency = 1;
                        foreach (Material mat in auxEnemy.mesh.materials)
                        {
                            mat.SetFloat("_Alpha_Threshold", 1);
                        }
                        controller.enemiesByType[i].Add(auxEnemy);
                        auxEnemy.instanceIndex = controller.enemiesInfo.Count;
                        EnemyTransformInfo auxTrInfo = new EnemyTransformInfo(/*aux, */spawnPos.position, 0,/* (float)NetworkTime.time,*/ Vector3.zero);
                        controller.enemiesInfo.Add(auxTrInfo);
                        auxEnemy.transformInfo = auxTrInfo;
                        //auxEnemy.GameID = GameManager.Instance.hordeController.GameEnemies.Count;
                        //Debug.Log(auxEnemy.GameID);
                        controller.GameEnemies.Add(auxEnemy);
                    }
                    else
                    {
                        foreach(Enemy e in controller.enemiesByType[i])
                        {
                            if (!controller.usedEnemiesByType[i].Contains(e))
                            {
                                aux = e.gameObject;
                                Vector3 auxPos = spawnPos.position + GetSpawnDirection();
                                if(i != controller.hordes[controller.hordeIndex].casterEnemyIndex)
                                {
                                    aux.transform.position = auxPos;
                                }
                                else
                                {
                                    if (!Physics.CapsuleCast(auxPos - (Vector3.up * controller.spawnerHeight) + Vector3.up * 0.5f, auxPos - (Vector3.up * controller.spawnerHeight) + Vector3.up * 2f, 0.5f, Vector3.up, controller.spawnerHeight))
                                    {
                                        aux.transform.position = auxPos + Vector3.up * (controller.casterEnemySpawnHeight - controller.spawnerHeight);
                                    }
                                    else
                                    {
                                        aux.transform.position = auxPos;
                                    }
                                }
                                auxEnemy = aux.GetComponent<Enemy>();
                                auxEnemy.ResetTransform(auxPos);
                                if(controller.DyingEnemies.Contains(auxEnemy))
                                {
                                    controller.DyingEnemies.Remove(auxEnemy);
                                }
                                auxEnemy.transparency = 1;
                                foreach (Material mat in auxEnemy.mesh.materials)
                                {
                                    mat.SetFloat("_Alpha_Threshold", 1);
                                }
                                aux.SetActive(true);
                                ActivateEnemy(aux, auxPos);
                                break;
                            }
                        }
                    }
                    auxEnemy.DamageHandler.dying = false;
                    auxEnemy.ResetAllVelocities();
                    controller.enemies.Add(auxEnemy);
                    controller.UpdateEnemyText(controller.enemies.Count);
                    controller.usedEnemiesByType[i].Add(auxEnemy);
                    auxEnemy.DamageHandler.Resistances.Clear();
                    randElemental = UnityEngine.Random.Range(0, 100);
                    if(randElemental < elementalChance)
                    {
                        auxEnemy.element = (Elements)UnityEngine.Random.Range(1, Enum.GetNames(typeof(Elements)).Length);
                        auxEnemy.DamageHandler.Resistances.Add(new Resistance(auxEnemy.element, auxEnemy.DamageHandler.resistancePercentValue));
                        switch (auxEnemy.element)
                        {
                            case Elements.Ice:
                                auxEnemy.DamageHandler.Resistances.Add(new Resistance(Elements.Fire, auxEnemy.DamageHandler.weaknessPercentValue));
                                auxEnemy.DamageHandler.Resistances.Add(new Resistance(Elements.Radiance, auxEnemy.DamageHandler.weaknessPercentValue));
                                auxEnemy.DamageHandler.Resistances.Add(new Resistance(Elements.Earth, auxEnemy.DamageHandler.resistancePercentValue));
                                auxEnemy.DamageHandler.Resistances.Add(new Resistance(Elements.Decay, auxEnemy.DamageHandler.resistancePercentValue));
                                auxEnemy.colorIndex = 0;
                                break;
                            case Elements.Fire:
                                auxEnemy.DamageHandler.Resistances.Add(new Resistance(Elements.Darkness, auxEnemy.DamageHandler.weaknessPercentValue));
                                auxEnemy.DamageHandler.Resistances.Add(new Resistance(Elements.Earth, auxEnemy.DamageHandler.weaknessPercentValue));
                                auxEnemy.DamageHandler.Resistances.Add(new Resistance(Elements.Ice, auxEnemy.DamageHandler.resistancePercentValue));
                                auxEnemy.DamageHandler.Resistances.Add(new Resistance(Elements.Decay, auxEnemy.DamageHandler.resistancePercentValue));
                                auxEnemy.colorIndex = 1;
                                break;
                            case Elements.Lightning:
                                auxEnemy.DamageHandler.Resistances.Add(new Resistance(Elements.Earth, auxEnemy.DamageHandler.weaknessPercentValue));
                                auxEnemy.DamageHandler.Resistances.Add(new Resistance(Elements.Ice, auxEnemy.DamageHandler.resistancePercentValue));
                                auxEnemy.DamageHandler.Resistances.Add(new Resistance(Elements.Radiance, auxEnemy.DamageHandler.resistancePercentValue));
                                auxEnemy.colorIndex = 2;
                                break;
                            case Elements.Radiance:
                                auxEnemy.DamageHandler.Resistances.Add(new Resistance(Elements.Darkness, auxEnemy.DamageHandler.weaknessPercentValue));
                                auxEnemy.DamageHandler.Resistances.Add(new Resistance(Elements.Decay, auxEnemy.DamageHandler.weaknessPercentValue));
                                auxEnemy.colorIndex = 3;
                                break;
                            case Elements.Decay:
                                auxEnemy.DamageHandler.Resistances.Add(new Resistance(Elements.Fire, auxEnemy.DamageHandler.weaknessPercentValue));
                                auxEnemy.DamageHandler.Resistances.Add(new Resistance(Elements.Radiance, auxEnemy.DamageHandler.resistancePercentValue));
                                auxEnemy.DamageHandler.Resistances.Add(new Resistance(Elements.Darkness, auxEnemy.DamageHandler.resistancePercentValue));
                                auxEnemy.colorIndex = 4;
                                break;
                            case Elements.Earth:
                                auxEnemy.DamageHandler.Resistances.Add(new Resistance(Elements.Ice, auxEnemy.DamageHandler.weaknessPercentValue));
                                auxEnemy.DamageHandler.Resistances.Add(new Resistance(Elements.Fire, auxEnemy.DamageHandler.resistancePercentValue));
                                auxEnemy.DamageHandler.Resistances.Add(new Resistance(Elements.Lightning, auxEnemy.DamageHandler.resistancePercentValue));
                                auxEnemy.DamageHandler.Resistances.Add(new Resistance(Elements.Decay, auxEnemy.DamageHandler.resistancePercentValue));
                                auxEnemy.colorIndex = 5;
                                break;
                            case Elements.Darkness:
                                auxEnemy.DamageHandler.Resistances.Add(new Resistance(Elements.Lightning, auxEnemy.DamageHandler.weaknessPercentValue));
                                auxEnemy.DamageHandler.Resistances.Add(new Resistance(Elements.Radiance, auxEnemy.DamageHandler.weaknessPercentValue));
                                auxEnemy.DamageHandler.Resistances.Add(new Resistance(Elements.Fire, auxEnemy.DamageHandler.resistancePercentValue));
                                auxEnemy.colorIndex = 6;
                                break;
                            default:
                                auxEnemy.colorIndex = -1;
                                break;
                        }

                        if(auxEnemy is CasterEnemy)
                        {
                            foreach (SpellNode n in ((CasterEnemy)auxEnemy).caster.runtimeNodes)
                            {
                                if (n is SpellCore)
                                {
                                    if(auxEnemy.element != Elements.Lightning)
                                    {
                                        ((SpellCore)n).Element = auxEnemy.element;
                                    }
                                    else
                                    {
                                        //Provisório até ter os VFX de Lightning
                                        ((SpellCore)n).Element = Elements.Radiance;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        auxEnemy.element = Elements.None;
                        auxEnemy.colorIndex = -1;
                    }
                    aux.GetComponent<CharacterDamageHandler>().enemyIndex = i;
                    controller.UsedEnemies.Add(auxEnemy);
                    auxEnemy.Initialize(Mathf.Lerp(auxEnemy.DamageHandler.minHealthMultiplier, auxEnemy.DamageHandler.maxHealthMultiplier, controller.hordes[controller.hordeIndex].baseHPMultiplierCurve.Evaluate(Mathf.Clamp((Time.time - hordeStartTime) / hordeDurationTime, 0, 1))));
                    i = enemies.Count;
                    controller.UpdateEnemyActiveID();
                }
            }
        }
        if(isSpawning)
        {
            foreach (EnemySelection e in enemies)
            {
                /*if (e.difficult)
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
                e.chance = e.spawnCurve.Evaluate(Mathf.Clamp((Time.time - hordeStartTime) / hordeDurationTime, 0, 1)) * 100;
            }
            elementalChance = Mathf.Lerp(baseElemental, maxElemental, controller.hordes[controller.hordeIndex].elementalCurve.Evaluate(Mathf.Clamp((Time.time - hordeStartTime) / hordeDurationTime, 0, 1)));
            spawnTime = Mathf.Lerp(baseSpawnTime, minSpawnTime, controller.hordes[controller.hordeIndex].enemySpawnRateCurve.Evaluate(Mathf.Clamp((Time.time - hordeStartTime)/hordeDurationTime, 0, 1)));
            StartCoroutine(SpawnEnemy());
        }
    }

    [ClientRpc]
    public void ActivateEnemy(GameObject obj, Vector3 pos)
    {
        obj.GetComponent<Enemy>().ResetTransform(pos);
        obj.SetActive(true);
    }

    Vector3 GetSpawnDirection()
    {
        Vector3 randDir = new Vector3(UnityEngine.Random.Range(-maxSpawnRadius, maxSpawnRadius), 0, UnityEngine.Random.Range(-maxSpawnRadius, maxSpawnRadius));
        return randDir;
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
        controller.usedSpawners.Remove(this.gameObject);
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
    [HideInInspector]public float chance;
    //public bool difficult;
    public AnimationCurve spawnCurve;
}
