using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class HordeController : NetworkBehaviour
{
    public List<EnemySelection> enemyChances;
    List<EnemySelection> selections = new List<EnemySelection>();
    public List<DifficultySetting> difficulties;
    public int difficulty;
    public AnimationCurve spawnerFrequencyCurve;
    public float minSpawnRadius, maxSpawnRadius, hordeDuration = 300, pauseDuration = 300, maxSpawnTime = 30, minSpawnTime = 10, heightCheckPoint = 5, checkHeight = 15, spawnerHeight = 2;
    float hordeStartTime, hordeEndTime, spawnTime, timer, pauseStartTime, pauseEndTime, randEnemy;
    public GameObject spawner;
    GameObject aux;
    Vector3 spawnPos;
    [HideInInspector][SyncVar] public bool inHorde = false;
    [HideInInspector][SyncVar] public bool inHordeTime = false;
    [HideInInspector][SyncVar] public bool inPause = false;
    public TextMeshProUGUI timerTxt;
    [HideInInspector] public List<GameObject> enemies = new List<GameObject>();
    [HideInInspector] public List<EnemyTransformInfo> enemiesInfo = new List<EnemyTransformInfo>();
    [HideInInspector] public List<List<Enemy>> enemiesByType = new List<List<Enemy>>();
    [HideInInspector] public List<HashSet<Enemy>> usedEnemiesByType = new List<HashSet<Enemy>>();
    [HideInInspector] public List<GameObject> spawners = new List<GameObject>();
    [HideInInspector] public HashSet<GameObject> usedSpawners = new HashSet<GameObject>();
    [HideInInspector][SyncVar] public bool running = false;
    public LayerMask spawnableLocations;
    Vector2 dir;
    bool possiblePos;
    public TextMeshProUGUI enemyCounterTxt;
    public int maxEnemyCount = 500;
    public int hordesToWin = 3;
    int hordeCount;
    private void Awake()
    {
        GameManager.Instance.hordeController = this;
        hordeCount = 0;
        spawners.Clear();
        enemiesByType.Clear();
        usedEnemiesByType.Clear();
        for (int i = 0; i < enemyChances.Count; i++)
        {
            enemiesByType.Add(new List<Enemy>());
            usedEnemiesByType.Add(new HashSet<Enemy>());
        }
        if(isServer)
        {
            UpdateEnemyText(0);
        }
    }



    private void Update()
    {
        if (!isServer || !running)
        {
            return;
        }
        if (inHorde)
        {
            timer = Mathf.Round(hordeEndTime - Time.time);
            if (timer > 0)
            {
                if ((int)timer % 60 >= 10)
                {
                    UpdateTimerText(((int)timer / 60) + ":" + ((int)timer % 60));
                }
                else
                {
                    UpdateTimerText(((int)timer / 60) + ":0" + ((int)timer % 60));
                }
            }
            else
            {
                UpdateTimerText("0:00");
            }
            for (int i = 0; i < usedEnemiesByType.Count; i++)
            {
                foreach (Enemy e in usedEnemiesByType[i])
                {
                    if (e != null)
                    {
                        e.EnemyUpdate();
                    }
                }
            }
            //UpdateEnemiesPos(enemiesInfo);
        }
        else
        {
            timer = Mathf.Round(pauseEndTime - Time.time);
            if ((int)timer % 60 >= 10)
            {
                UpdateTimerText(((int)timer / 60) + ":" + ((int)timer % 60));
            }
            else
            {
                UpdateTimerText(((int)timer / 60) + ":0" + ((int)timer % 60));
            }
        }
    }
    void FixedUpdate()
    {
        if (!isServer || !running)
        {
            return;
        }
        for (int i = 0; i < usedEnemiesByType.Count; i++)
        {
            foreach (Enemy e in usedEnemiesByType[i])
            {
                if (e != null)
                {
                    e.FixedRBUpdate();
                }
            }
        }
    }

    public void Initialize()
    {
        if (isServer)
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
        foreach(Player p in GameManager.Instance.Players)
        {
            if(p.dead)
            {
                p.GetComponent<PlayerDamageHandler>().Respawn();
            }
        }
        StartCoroutine(EndPause());
    }

    [Server]
    public void StartHorde()
    {
        hordeCount++;
        hordeStartTime = Time.time;
        hordeEndTime = hordeStartTime + hordeDuration;
        spawnTime = Mathf.Lerp(maxSpawnTime, minSpawnTime, spawnerFrequencyCurve.Evaluate(0));
        inHorde = true;
        inHordeTime = true;
        usedSpawners.Clear();
        enemies.Clear();
        foreach (HashSet<Enemy> hs in usedEnemiesByType)
        {
            hs.Clear();
        }
        StartCoroutine(StopSpawning());
        StartCoroutine(SpawnSpawner());
        StartCoroutine(DelayUpdateEnemiesPos());
    }
    IEnumerator DelayUpdateEnemiesPos()
    {
        yield return new WaitForSeconds(0.25f);
        UpdateEnemiesPos(enemiesInfo);
        if(inHorde)
        {
            StartCoroutine(DelayUpdateEnemiesPos());
        }
    }

    [ClientRpc]
    public void UpdateEnemiesPos(List<EnemyTransformInfo> aux)
    {
        for(int i = 0; i < aux.Count; i++)
        {
            if (aux[i].enemy != null && aux[i].enemy.activeSelf)
            {
                if (enemiesInfo.Count == i)
                {
                    enemiesInfo.Add(new EnemyTransformInfo(aux[i].enemy, aux[i].pos, aux[i].rot, Time.time, aux[i].vel));
                }
                else
                {
                    enemiesInfo[i] = new EnemyTransformInfo(aux[i].enemy, aux[i].pos, aux[i].rot, Time.time, aux[i].vel);
                }
                //aux[i].enemy.transform.position = aux[i].pos;
                aux[i].enemy.transform.rotation = Quaternion.Euler(0, aux[i].rot, 0);
            }
        }
    }

    [ClientRpc]
    public void UpdateTimerText(string txt)
    {
        timerTxt.text = txt;
    }

    [ClientRpc]
    public void UpdateEnemyText(int ammount)
    {
        enemyCounterTxt.text = ammount.ToString();
    }

    [Server]
    IEnumerator SpawnSpawner()
    {
        foreach (Player p in GameManager.Instance.Players)
        {
            spawnPos = GetSpawnPos(p.transform.position);
            if (spawners.Count <= usedSpawners.Count)
            {
                aux = Instantiate(spawner, spawnPos, Quaternion.identity);
                spawners.Add(aux);
                NetworkServer.Spawn(aux);
            }
            else
            {
                foreach (GameObject s in spawners)
                {
                    if (!usedSpawners.Contains(s))
                    {
                        aux = s;
                        aux.transform.position = spawnPos;
                        aux.SetActive(true);
                        break;
                    }
                }
            }
            //selections.Clear();
            /*foreach(int i in difficulties[difficulty].indexes)
            {
                selections.Add(enemyChances[i]);
            }*/
            aux.GetComponent<EnemySpawner>().Initialize(enemyChances, difficulties[difficulty].baseSpawnTime, difficulties[difficulty].minSpawnTime, Mathf.Min(difficulties[difficulty].maxLifeTime, hordeEndTime - Time.time), difficulties[difficulty].baseDifficultyMult, difficulties[difficulty].maxDifficultyMult, difficulties[difficulty].baseElemental, difficulties[difficulty].maxElemental, hordeStartTime, hordeDuration);
            usedSpawners.Add(aux);
        }
        if (inHordeTime)
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
        //StopAllCoroutines();
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
        float radius;
        while (!possiblePos)
        {
            do
            {
                dir = new Vector2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f)).normalized;
            }
            while (dir == Vector2.zero);
            radius = UnityEngine.Random.Range(minSpawnRadius, maxSpawnRadius);
            pos = playerPos + new Vector3(dir.x * radius, heightCheckPoint, dir.y * radius);
            if (Physics.Raycast(pos, Vector3.down, out hit, checkHeight, spawnableLocations))
            {
                possiblePos = true;
            }
        }
        return hit.point + Vector3.up * spawnerHeight;
    }

    public void CheckEnemyCount()
    {
        if (!isServer)
        {
            return;
        }
        if (enemies.Count == 0)
        {
            inHorde = false;
            if(hordeCount < hordesToWin)
            {
                StartPause();
            }
            else
            {
                running = false;
                Victory();
            }
        }
    }

    [ClientRpc]
    public void Victory()
    {
        GameManager.Instance.uiController.sharedUI.SetActive(false);
        GameManager.Instance.uiController.playerUI.victoryPanel.SetActive(true);
        GameManager.Instance.uiController.playerUI.EnableUICursor();
    }

    public void CheckDeadPlayers()
    {
        if(!isServer)
        {
            return;
        }
        bool allDead = true;
        foreach (Player p in GameManager.Instance.Players)
        {
            if (!p.dead)
            {
                allDead = false;
                break;
            }
        }
        if (allDead)
        {
            running = false;
            Defeat();
        }
    }

    [ClientRpc]
    public void Defeat()
    {
        GameManager.Instance.uiController.sharedUI.SetActive(false);
        GameManager.Instance.uiController.playerUI.defeatPanel.SetActive(true);
        GameManager.Instance.uiController.playerUI.EnableUICursor();
    }

    public void CheckReadyPlayers()
    {
        if (!isServer)
        {
            return;
        }

        bool everyoneReady = true;
        foreach (Player p in GameManager.Instance.Players)
        {
            if (!p.readyForHorde)
            {
                everyoneReady = false;
            }
        }
        if (everyoneReady)
        {
            StopAllCoroutines();
            foreach (Player p in GameManager.Instance.Players)
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

public struct EnemyTransformInfo
{
    public GameObject enemy;
    public Vector3 pos;
    public byte rot;
    public float lastTime;
    public Vector3 vel;

    public EnemyTransformInfo(GameObject enemy, Vector3 pos, byte rot, float lastTime, Vector3 vel)
    {
        this.enemy = enemy;
        this.pos = pos;
        this.rot = rot;
        this.lastTime = lastTime;
        this.vel = vel;
    }
}
