using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using UnityEngine;
using Unity.Jobs;
using Unity.Mathematics;

public class HordeController : NetworkBehaviour
{
    public List<EnemySelection> enemyChances;
    [NonSerialized]public float3[] Directions = new float3[8];
    NativeArray<float3> JobDirectionData;
    List<EnemySelection> selections = new List<EnemySelection>();
    public List<DifficultySetting> difficulties;
    public int difficulty;
    public AnimationCurve spawnerFrequencyCurve;
    public float minSpawnRadius, maxSpawnRadius, radiusStepIncrease = 2, radiusLimit = 100, maxEnemySpawnRadius = 3, hordeDuration = 300, pauseDuration = 300, maxSpawnTime = 30, minSpawnTime = 10, heightCheckPoint = 5, checkHeight = 15, spawnerHeight = 2;
    float hordeStartTime, hordeEndTime, spawnTime, timer, pauseStartTime, pauseEndTime, randEnemy;
    public GameObject spawner;
    GameObject aux;
    Vector3 spawnPos;
    [HideInInspector][SyncVar] public bool inHorde = false;
    [HideInInspector][SyncVar] public bool inHordeTime = false;
    [HideInInspector][SyncVar] public bool inPause = false;
    public TextMeshProUGUI timerTxt;
    [HideInInspector] public List<Enemy> enemies = new List<Enemy>();
    [HideInInspector] public Enemy[] clientEnemies;
    [HideInInspector] public List<Enemy> GameEnemies = new List<Enemy>();
    [HideInInspector] public List<Enemy> UsedEnemies = new List<Enemy>();
    [HideInInspector] public List<EnemyTransformInfo> enemiesInfo = new List<EnemyTransformInfo>();
    [HideInInspector] public List<List<Enemy>> enemiesByType = new List<List<Enemy>>();
    [HideInInspector] public List<HashSet<Enemy>> usedEnemiesByType = new List<HashSet<Enemy>>();
    [HideInInspector] public List<GameObject> spawners = new List<GameObject>();
    [HideInInspector] public HashSet<GameObject> usedSpawners = new HashSet<GameObject>();
    [HideInInspector][SyncVar] public bool running = false;
    public LayerMask spawnableLocations, ignoredObstacles;
    Vector2 dir;
    bool possiblePos;
    public TextMeshProUGUI enemyCounterTxt;
    public int maxEnemyCount = 500;
    public int hordesToWin = 3;
    int hordeCount;
    public float lastTime;
    Timer aiCalcTimer = new Timer(false);
    float enemyAIupdateRate = 1f / 10f; // Hz
    private void Awake()
    {
        clientEnemies = new Enemy[maxEnemyCount];
        GameManager.Instance.hordeController = this;
        for (int i = 0; i < Directions.Length; i++)
        {
            float angle = i * Mathf.PI * 2f / Directions.Length;
            Directions[i] = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
        }
        JobDirectionData = new NativeArray<float3>(Directions.Length, Allocator.Persistent);
        JobDirectionData.CopyFrom(Directions);
        hordeCount = 0;
        spawners.Clear();
        enemiesByType.Clear();
        usedEnemiesByType.Clear();
        for (int i = 0; i < enemyChances.Count; i++)
        {
            enemiesByType.Add(new List<Enemy>());
            usedEnemiesByType.Add(new HashSet<Enemy>());
        }
        if (isServer)
        {
            UpdateEnemyText(0);
        }
    }
    private void OnDestroy()
    {
        if (JobDirectionData.IsCreated)
        {
            JobDirectionData.Dispose();
        }
    }
    public void UpdateEnemyActiveID()
    {
        if (!isServer) return;
        for (int i = 0; i < UsedEnemies.Count; i++)
        {
            UsedEnemies[i].UpdateIdWrapper(i);
        }
    }


    private void Update()
    {
        if (!running)
        {
            return;
        }
        if(isServer)
        {
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
                bool aux = aiCalcTimer.timer(enemyAIupdateRate, Time.deltaTime, false, true);
                /*for (int i = 0; i < usedEnemiesByType.Count; i++)
                {
                    foreach (Enemy e in usedEnemiesByType[i])
                    {
                        if (e != null)
                        {
                            if (aux)
                            {
                                e.AICalculation();
                            }
                            //e.EnemyUpdate();
                        }
                    }
                }*/
                if (aux)
                {
                    Vector3[] results = StartAvoidanceJob();
                    for (int i = 0; i < UsedEnemies.Count; i++)
                    {
                        if (UsedEnemies[i] != null)
                        {
                            UsedEnemies[i].MoveDirection = results[i];
                            UsedEnemies[i].EnemyUpdate();
                            UsedEnemies[i].Reposition();
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < UsedEnemies.Count; i++)
                    {
                        if (UsedEnemies[i] != null)
                        {
                            UsedEnemies[i].EnemyUpdate();
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
        else
        {
            if (inHorde)
            {
                bool aux = aiCalcTimer.timer(enemyAIupdateRate, Time.deltaTime, false, true);
                foreach (Enemy e in clientEnemies)
                {
                    if (e != null && e.gameObject.activeSelf)
                    {
                        e.EnemyClientUpdate();
                        if(aux)
                        {
                            e.Reposition();
                        }
                    }
                }
            }
        }
        lastTime = (float)NetworkTime.time;
    }
    public Vector3[] StartAvoidanceJob()
    {
        Vector3[] finalDirections;
        NativeArray<float3> results = new NativeArray<float3>(GameManager.Instance.hordeController.UsedEnemies.Count, Allocator.TempJob);
        NativeArray<EnemyJobData> enemiesInfo = new NativeArray<EnemyJobData>(UsedEnemies.Count, Allocator.TempJob);
        for (int i = 0; i < UsedEnemies.Count; i++)
        {
            //Debug.Log(UsedEnemies[i].currentCell.ID);
            if (UsedEnemies[i] != null)
            {
                enemiesInfo[i] = new EnemyJobData()
                {
                    Position = UsedEnemies[i].transform.position,
                    Size = UsedEnemies[i].size,
                    EnemyAvoidanceRadius = UsedEnemies[i].EnemyAvoidanceRadius,
                    SeparationForce = UsedEnemies[i].SeparationForce,
                    Priority = UsedEnemies[i].priority,
                    DetectionRadius = UsedEnemies[i].DetectionRadius,
                    //CurrentCell = UsedEnemies[i].currentCell.ID,
                    occupiedCellDepth = UsedEnemies[i].occupiedCellNum,
                    activationDistance = UsedEnemies[i].FlowfieldActivationDistance,
                    canSeePlayer = UsedEnemies[i].canSeeTarget,
                    CurrentCell = UsedEnemies[i].currentCell.ID

                };
            }
        }
        //NativeArray<CellJobData> cellInfo = new NativeArray<CellJobData>(FlowFieldManager.instance.flowField.allCells.Count, Allocator.TempJob);
        //NativeList<int> EnemyFieldData = new NativeList<int>(Allocator.TempJob);
        for (int i = 0; i < FlowFieldManager.instance.cellJobDatas.Length; i++)
        {
            CellJobData cell = FlowFieldManager.instance.cellJobDatas[i];
            //cell.firstEnemy = EnemyFieldData.Length;
            cell.Direction = FlowFieldManager.instance.flowField.allCells[i].direction;
            //cell.EnemiesNum = FlowFieldManager.instance.flowField.allCells[i].ContainedEnemies.Count;
            FlowFieldManager.instance.cellJobDatas[i] = cell;

            /*for (int j = 0; j < cell.EnemiesNum; j++)
            {
                EnemyFieldData.Add(FlowFieldManager.instance.flowField.allCells[i].ContainedEnemies[j].ID);
            }*/


        }
        NativeArray<float3> playerPos = new NativeArray<float3>(GameManager.Instance.Players.Count, Allocator.TempJob);
        for(int i =0; i<GameManager.Instance.Players.Count; i++)
        {
            playerPos[i] = GameManager.Instance.Players[i].transform.position;
        }
        EnemyFieldLocation enemyLocation = new EnemyFieldLocation()
        {
            PlayerPositions = playerPos,
            CellNeighbors = FlowFieldManager.instance.CellNeighborID,
            maxEnemiesPerCell = 5,
            maxEnemyOccupiedCells = 9,
            Cells = FlowFieldManager.instance.cellJobDatas,
            EnemyData = enemiesInfo,

            cellEnemiesNum = new NativeArray<int>(FlowFieldManager.instance.cellJobDatas.Length, Allocator.TempJob),
            TargetIndices = new NativeArray<int>(UsedEnemies.Count, Allocator.TempJob),
            enemiesInField = new NativeArray<int>(5 * FlowFieldManager.instance.cellJobDatas.Length, Allocator.TempJob),
            enemyOcupiedCells = new NativeArray<int>(9 * UsedEnemies.Count, Allocator.TempJob),
            OccupiedCellsToCheck = new NativeArray<int>(9 * UsedEnemies.Count, Allocator.TempJob)
        };
        JobHandle handle = enemyLocation.Schedule(UsedEnemies.Count, 64);
        handle.Complete();

        //ProcessResults
        for(int i = 0; i< UsedEnemies.Count; i++)
        {
            Enemy e = UsedEnemies[i];
            EnemyJobData EJD = enemyLocation.EnemyData[i];
            e.target = GameManager.Instance.Players[enemyLocation.TargetIndices[i]];
            e.targetVector = EJD.targetVector;
            e.currentCell = FlowFieldManager.instance.flowField.allCells[EJD.CurrentCell];
            e.forwardCell = FlowFieldManager.instance.flowField.allCells[EJD.fowardCell];

            //if (!Physics.Raycast(transform.position, math.normalize(EJD.targetVector),  math.length(EJD.targetVector), ~e.CanSeeTargetThrough))
            if(math.dot(enemyLocation.Cells[EJD.CurrentCell].Direction, enemyLocation.Cells[e.target.TargetCellID].Position) > 0.9f || EJD.CurrentCell== e.target.TargetCellID)
            {
                EJD.canSeePlayer = true;
            }
            else
            {
                EJD.canSeePlayer = false;
            }
            if(EJD.canSeePlayer && math.length(EJD.targetVector) < EJD.activationDistance)
            {
                EJD.interestDirection = math.normalize(EJD.targetVector);
            }
            else
            {
                EJD.interestDirection = enemyLocation.Cells[EJD.CurrentCell].Direction;
            }
            enemyLocation.EnemyData[i] = EJD;

            e.interestDirection = EJD.interestDirection;
            e.canSeeTarget = EJD.canSeePlayer;
        }

        AvoidanceCalculation calculation = new AvoidanceCalculation()
        {
            Directions = JobDirectionData,
            Cells = FlowFieldManager.instance.cellJobDatas,
            CellSize = FlowFieldManager.instance.CellSize,
            MaxCellsChecked = 64,
            MaxEnemyNeighbors = 32,
            maxEnemiesPerCell = enemyLocation.maxEnemiesPerCell,
            CellNeighbors = FlowFieldManager.instance.CellNeighborID,
            //enemiesInField = EnemyFieldData.AsArray(),
            enemiesInField = enemyLocation.enemiesInField,
            EnemyData = enemyLocation.EnemyData,
            DirectionsOutput = results,
            cellEnemiesNum = enemyLocation.cellEnemiesNum,

            EnemyNeighbors = new NativeArray<int>(UsedEnemies.Count * 32, Allocator.TempJob),
            EnemyNeighborCounts = new NativeArray<int>(UsedEnemies.Count, Allocator.TempJob),
            cellsToCheck = new NativeArray<int>(UsedEnemies.Count * 64, Allocator.TempJob),
            enemiesInterest = new NativeArray<float>(UsedEnemies.Count * Directions.Length, Allocator.TempJob),
            enemiesDanger = new NativeArray<float>(UsedEnemies.Count * Directions.Length, Allocator.TempJob)

        };

        handle = calculation.Schedule(UsedEnemies.Count, 64);
        handle.Complete();

        finalDirections = new Vector3[results.Length];
        for (int i = 0; i < finalDirections.Length; i++)
        {
            finalDirections[i] = results[i];
        }
        enemyLocation.TargetIndices.Dispose();
        enemyLocation.enemiesInField.Dispose();
        enemyLocation.enemyOcupiedCells.Dispose();
        enemyLocation.OccupiedCellsToCheck.Dispose();
        enemyLocation.cellEnemiesNum.Dispose();

        enemiesInfo.Dispose();
        playerPos.Dispose();
        //cellNData.Dispose();
        //cellInfo.Dispose();
        //EnemyFieldData.Dispose();
        results.Dispose();
        //calculation.Directions.Dispose();
        calculation.EnemyNeighbors.Dispose();
        calculation.EnemyNeighborCounts.Dispose();
        calculation.cellsToCheck.Dispose();
        calculation.enemiesInterest.Dispose();
        calculation.enemiesDanger.Dispose();
        return finalDirections;
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
        if (!isServer || running) return;

        StartPause();
        running = true;
    }

    [Server]
    public void StartPause()
    {
        pauseStartTime = Time.time;
        pauseEndTime = pauseStartTime + pauseDuration;
        inPause = true;
        foreach (Player p in GameManager.Instance.Players)
        {
            if (p.dead)
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
        /*yield return new WaitForSeconds(0.2f);
        UpdateEnemiesPos(enemiesInfo);
        if (inHorde)
        {
            StartCoroutine(DelayUpdateEnemiesPos());
        }*/
        while (inHorde)
        {
            yield return new WaitForSeconds(0.2f);
            UpdateEnemiesPos(enemiesInfo);
        }
    }

    [ClientRpc]
    public void UpdateEnemiesPos(List<EnemyTransformInfo> aux)
    {
        if (isServer)
        {
            return;
        }
        for (int i = 0; i < aux.Count; i++)
        {
            /*if (aux[i].enemy != null && aux[i].enemy.activeSelf)
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
                //aux[i].enemy.transform.rotation = Quaternion.Euler(0, aux[i].rot, 0);
            }*/
            if (enemiesInfo.Count == i)
            {
                enemiesInfo.Add(new EnemyTransformInfo(aux[i].pos, aux[i].rot, /*aux[i].lastTime,*/ aux[i].vel));
            }
            else
            {
                enemiesInfo[i] = new EnemyTransformInfo(aux[i].pos, aux[i].rot,/* aux[i].lastTime, */aux[i].vel);
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
            if(spawnPos != Vector3.zero)
            {
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
                            ActivateSpawner(aux);
                            break;
                        }
                    }
                }
                //selections.Clear();
                /*foreach(int i in difficulties[difficulty].indexes)
                {
                    selections.Add(enemyChances[i]);
                }*/
                aux.GetComponent<EnemySpawner>().Initialize(enemyChances, difficulties[difficulty].baseSpawnTime, difficulties[difficulty].minSpawnTime, Mathf.Min(difficulties[difficulty].maxLifeTime, hordeEndTime - Time.time), difficulties[difficulty].baseDifficultyMult, difficulties[difficulty].maxDifficultyMult, difficulties[difficulty].baseElemental, difficulties[difficulty].maxElemental, hordeStartTime, hordeDuration, maxEnemySpawnRadius);
                usedSpawners.Add(aux);
            }
        }
        if (inHordeTime)
        {
            yield return new WaitForSeconds(spawnTime);
            spawnTime = Mathf.Lerp(maxSpawnTime, minSpawnTime, spawnerFrequencyCurve.Evaluate(Mathf.Clamp((Time.time - hordeStartTime) / hordeDuration, 0, 1)));
            StartCoroutine(SpawnSpawner());
        }
    }
    [ClientRpc]
    public void ActivateSpawner(GameObject obj)
    {
        obj.SetActive(true);
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
        FieldCell auxCell = null;
        //RaycastHit hit = new RaycastHit();
        Vector3 pos, aux;
        possiblePos = false;
        bool oppositeOccupied = false;
        float radius, auxMaxRadius = maxSpawnRadius, auxMinRadius = minSpawnRadius;
        while (!possiblePos && auxMaxRadius < radiusLimit)
        {
            if(usedSpawners.Count > 0 && !oppositeOccupied)
            {
                aux = GetOppositeDirection(playerPos);
                dir = new Vector2(aux.x, aux.z).normalized;
            }
            else
            {
                do
                {
                    dir = new Vector2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f)).normalized;
                }
                while (dir == Vector2.zero);
            }
            radius = UnityEngine.Random.Range(auxMinRadius, auxMaxRadius);
            pos = playerPos + new Vector3(dir.x * radius, heightCheckPoint, dir.y * radius);
            /*if (Physics.Raycast(pos, Vector3.down, out hit, checkHeight, spawnableLocations))
            {
                Collider[] obstacles = Physics.OverlapSphere(pos, maxEnemySpawnRadius, ignoredObstacles);
                if(obstacles.Length <= 0)
                {
                    possiblePos = true;
                }
            }*/
            auxCell = FlowFieldManager.instance.WorldToGridPosition(pos);
            if(auxCell != null && auxCell.Neighbors.Count >= 8)
            {
                possiblePos = true;
            }
            oppositeOccupied = true;
            auxMinRadius += radiusStepIncrease;
            auxMaxRadius += radiusStepIncrease;
        }
        if(possiblePos)
        {
            return auxCell.position + Vector3.up * spawnerHeight;
            //return hit.point + Vector3.up * spawnerHeight;
        }
        else
        {
            return Vector3.zero;
        }
    }

    Vector3 GetOppositeDirection(Vector3 playerPos)
    {
        Vector3 aux = Vector3.zero;
        foreach(GameObject go in usedSpawners)
        {
            aux += playerPos - go.transform.position;
        }
        return aux;
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
            if (hordeCount < hordesToWin)
            {
                difficulty++;
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
        GameManager.Instance.uiController.playerUI.pausePanel.SetActive(false);
        GameManager.Instance.uiController.playerUI.spellPanel.SetActive(false);
        GameManager.Instance.uiController.playerUI.deathPanel.SetActive(false);
        GameManager.Instance.uiController.playerUI.victoryPanel.SetActive(true);
        GameManager.Instance.uiController.playerUI.EnableUICursor();
    }

    public void CheckDeadPlayers()
    {
        if (!isServer)
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
        GameManager.Instance.uiController.playerUI.pausePanel.SetActive(false);
        GameManager.Instance.uiController.playerUI.spellPanel.SetActive(false);
        GameManager.Instance.uiController.playerUI.deathPanel.SetActive(false);
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

    [Server]
    public void CheckGameplayLoaded()
    {
        int expectedPlayers = NetworkManager.singleton.GetComponent<RoomManager>().playerList.Count;
        if (GameManager.Instance.Players.Count < expectedPlayers) return;

        foreach (Player player in GameManager.Instance.Players)
        {
            if (!player.gameplayLoaded) return;
        }
        Initialize();
        RPCStartGameplay();
    }

    [ClientRpc]
    private void RPCStartGameplay()
    {
        PlayerUI playerUI = GameManager.Instance.uiController.playerUI;
        if (playerUI != null)
        {
            playerUI.FinishLoading();
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
    //public GameObject enemy;
    public Vector3 pos;
    public byte rot;
    //public float lastTime;
    public Vector3 vel;

    public EnemyTransformInfo(/*GameObject enemy, */Vector3 pos, byte rot/*, float lastTime*/, Vector3 vel)
    {
        //this.enemy = enemy;
        this.pos = pos;
        this.rot = rot;
        //this.lastTime = lastTime;
        this.vel = vel;
    }
}
