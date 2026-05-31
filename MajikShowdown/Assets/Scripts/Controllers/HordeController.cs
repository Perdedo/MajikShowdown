using Mirror;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using TMPro;
using System;

public class HordeController : NetworkBehaviour
{
    public List<EnemySelection> enemyChances;
    List<EnemySelection> selections = new List<EnemySelection>();
    public List<DifficultySetting> difficulties;
    public int difficulty;
    public AnimationCurve spawnerFrequencyCurve;
    public float spawnRadius, hordeDuration = 300, pauseDuration = 300, maxSpawnTime = 30, minSpawnTime = 10, heightCheckPoint = 5, checkHeight = 15, spawnerHeight = 2;
    float hordeStartTime, hordeEndTime, spawnTime, timer, pauseStartTime, pauseEndTime;
    public GameObject spawner;
    GameObject aux;
    Vector3 spawnPos;
    [HideInInspector]public bool inHorde = false, inHordeTime = false, inPause = false;
    public TextMeshProUGUI timerTxt;
    [HideInInspector]public List<GameObject> enemies;
    bool running = false;
    public LayerMask spawnableLocations;
    Vector2 dir;
    bool possiblePos;

    private void Awake()
    {
        GameManager.Instance.hordeController = this;
    }

    private void Update()
    {
        if(!isServer || !running)
        {
            return;
        }
        if(inHorde)
        {
            timer = Mathf.Round(hordeEndTime - Time.time);
            if(timer > 0)
            {
                UpdateTimerText(((int)timer / 60) + ":" + ((int)timer % 60));
            }
            else
            {
                UpdateTimerText("FINISH THEM!");
            }
        }
        else
        {
            timer = Mathf.Round(pauseEndTime - Time.time);
            UpdateTimerText(((int)timer / 60) + ":" + ((int)timer % 60));
        }
    }

    public void Initialize()
    {
        if(isServer)
        {
            StartPause();
            running = true;
        }
    }

    [Server]
    public void StartPause()
    {
        pauseStartTime = Time.time;
        pauseEndTime = pauseStartTime + pauseDuration;
        inPause = true;
        StartCoroutine(EndPause());
    }

    [Server]
    public void StartHorde()
    {
        hordeStartTime = Time.time;
        hordeEndTime = hordeStartTime + hordeDuration;
        spawnTime = Mathf.Lerp(maxSpawnTime, minSpawnTime, spawnerFrequencyCurve.Evaluate(0));
        inHorde = true;
        inHordeTime = true;
        StartCoroutine(StopSpawning());
        StartCoroutine(SpawnSpawner());
    }

    [ClientRpc]
    public void UpdateTimerText(string txt)
    {
        timerTxt.text = txt;
    }

    [Server]
    IEnumerator SpawnSpawner()
    {
        foreach(Player p in GameManager.Instance.Players)
        {
            spawnPos = GetSpawnPos(p.transform.position);
            aux = Instantiate(spawner, spawnPos, Quaternion.identity);
            NetworkServer.Spawn(aux);
            selections.Clear();
            foreach(int i in difficulties[difficulty].indexes)
            {
                selections.Add(enemyChances[i]);
            }
            aux.GetComponent<EnemySpawner>().Initialize(selections, difficulties[difficulty].baseSpawnTime, difficulties[difficulty].minSpawnTime, Mathf.Min(difficulties[difficulty].maxLifeTime, hordeEndTime - Time.time), difficulties[difficulty].baseDifficultyMult, difficulties[difficulty].maxDifficultyMult, difficulties[difficulty].baseElemental, difficulties[difficulty].maxElemental);
        }
        if (inHorde)
        {
            yield return new WaitForSeconds(spawnTime);
            spawnTime = Mathf.Lerp(maxSpawnTime, minSpawnTime, spawnerFrequencyCurve.Evaluate(Mathf.Clamp((Time.time - hordeStartTime) / hordeDuration, 0, 1)));
            StartCoroutine(SpawnSpawner());
        }
    }


    [Server]
    IEnumerator StopSpawning()
    {
        yield return new WaitForSeconds(hordeDuration);
        inHordeTime = false;
        StopAllCoroutines();
        CheckEnemyCount();
    }


    [Server]
    IEnumerator EndPause()
    {
        yield return new WaitForSeconds(pauseDuration);
        inPause = false;
        StartHorde();
    }

    public Vector3 GetSpawnPos(Vector3 playerPos)
    {
        RaycastHit hit = new RaycastHit();
        Vector3 pos;
        possiblePos = false;
        while(!possiblePos)
        {
            do
            {
                dir = new Vector2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f)).normalized;
            }
            while (dir == Vector2.zero);
            pos = playerPos + new Vector3(dir.x * spawnRadius, heightCheckPoint, dir.y * spawnRadius);
            if(Physics.Raycast(pos, Vector3.down, out hit, checkHeight, spawnableLocations))
            {
                possiblePos = true;
            }
        }
        return hit.point + Vector3.up * spawnerHeight; 
    }

    public void CheckEnemyCount()
    {
        if(!isServer)
        {
            return;
        }
        if(enemies.Count == 0)
        {
            inHorde = false;
            StartPause();
        }
    }

    public void CheckReadyPlayers()
    {
        if(!isServer)
        {
            return;
        }

        bool everyoneReady = true;
        foreach(Player p in GameManager.Instance.Players)
        {
            if(!p.readyForHorde)
            {
                everyoneReady = false;
            }
        }
        if(everyoneReady)
        {
            StopAllCoroutines();
            foreach(Player p in GameManager.Instance.Players)
            {
                p.readyForHorde = false;
            }
            inPause = false;
            StartHorde();
        }
    }
}

[Serializable]
public class DifficultySetting
{
    public string difficulty;
    public List<int> indexes;
    public float baseSpawnTime, minSpawnTime, maxLifeTime, baseDifficultyMult, maxDifficultyMult, baseElemental, maxElemental;
}
