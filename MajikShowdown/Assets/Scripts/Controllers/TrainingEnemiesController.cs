using Mirror;
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class TrainingEnemiesController : MonoBehaviour
{
    public List<TrainingEnemySpawner> spawners = new List<TrainingEnemySpawner>();
    NativeArray<float3> JobDirectionData;
    public float3[] Directions = new float3[8];
    public List<Enemy> GameEnemies = new List<Enemy>();
    public List<Enemy> UsedEnemies = new List<Enemy>();
    [HideInInspector] public List<List<Enemy>> enemiesByType = new List<List<Enemy>>();
    [HideInInspector] public List<HashSet<Enemy>> usedEnemiesByType = new List<HashSet<Enemy>>();
    public float spawnerHeight = 2;
    public int maxEnemyCount = 500;
    Timer aiCalcTimer = new Timer(false);
    float enemyAIupdateRate = 1f / 10f;
    public float elementalChance = 5;
    [SerializeField] protected LayerMask RayMasks;
    private void Awake()
    {
        GameManager.Instance.trainingController = this;
        for (int i = 0; i < Directions.Length; i++)
        {
            float angle = i * Mathf.PI * 2f / Directions.Length;
            Directions[i] = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
        }
        JobDirectionData = new NativeArray<float3>(Directions.Length, Allocator.Persistent);
        JobDirectionData.CopyFrom(Directions);
        enemiesByType.Clear();
        usedEnemiesByType.Clear();
        for (int i = 0; i < spawners.Count; i++)
        {
            enemiesByType.Add(new List<Enemy>());
            usedEnemiesByType.Add(new HashSet<Enemy>());
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
        for (int i = 0; i < UsedEnemies.Count; i++)
        {
            UsedEnemies[i].UpdateIdWrapper(i);
        }
    }
    private void Update()
    {
        if(UsedEnemies.Count <= 0)
        {
            return;
        }
        bool aux = aiCalcTimer.timer(enemyAIupdateRate, Time.deltaTime, false, true);
        if (aux)
        {
            Vector3[] results = StartAvoidanceJob();
            for (int i = 0; i < UsedEnemies.Count; i++)
            {
                if (UsedEnemies[i] != null)
                {
                    UsedEnemies[i].MoveDirection = results[i];
                    UsedEnemies[i].Reposition();
                    UsedEnemies[i].ResetKnockbackCooldown();
                    UsedEnemies[i].EnemyUpdate();
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
    }

    GameObject aux;
    Enemy auxEnemy;
    public void SpawnEnemy(TrainingEnemySpawner spawner, GameObject enemy, Vector3 pos, int quantity, Vector3 distanceBetweenSpawns)
    {
        for(int i = 0; i < quantity; i++)
        {
            if (UsedEnemies.Count < maxEnemyCount)
            {
                int ind = spawners.IndexOf(spawner);
                if (enemiesByType[ind].Count <= usedEnemiesByType[ind].Count)
                {
                    aux = Instantiate(enemy, pos + distanceBetweenSpawns * i, Quaternion.identity);
                    auxEnemy = aux.GetComponent<Enemy>();
                    enemiesByType[ind].Add(auxEnemy);
                    GameEnemies.Add(auxEnemy);
                }
                else
                {
                    foreach (Enemy e in enemiesByType[ind])
                    {
                        if (!usedEnemiesByType[ind].Contains(e))
                        {
                            aux = e.gameObject;
                            aux.transform.position = pos + distanceBetweenSpawns * i;
                            aux.SetActive(true);
                            auxEnemy = aux.GetComponent<Enemy>();
                            break;
                        }
                    }
                }
                auxEnemy.ResetAllVelocities();
                usedEnemiesByType[ind].Add(auxEnemy);
                float randElemental = UnityEngine.Random.Range(0, 100);
                if (randElemental < elementalChance)
                {
                    auxEnemy.element = (Elements)UnityEngine.Random.Range(0, Enum.GetNames(typeof(Elements)).Length);
                }
                aux.GetComponent<CharacterDamageHandler>().enemyIndex = ind;
                UsedEnemies.Add(auxEnemy);
                auxEnemy.Initialize();
                UpdateEnemyActiveID();
            }
        }
    }

    public Vector3[] StartAvoidanceJob()
    {
        Vector3[] finalDirections;
        NativeArray<float3> results = new NativeArray<float3>(UsedEnemies.Count, Allocator.TempJob);
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
        /*for (int i = 0; i < FlowFieldManager.instance.cellJobDatas.Length; i++)
        {
            CellJobData cell = FlowFieldManager.instance.cellJobDatas[i];
            cell.Direction = FlowFieldManager.instance.flowField.allCells[i].direction;
            FlowFieldManager.instance.cellJobDatas[i] = cell;
        }*/
        NativeArray<float3> playerPos = new NativeArray<float3>(GameManager.Instance.Players.Count, Allocator.TempJob);
        for (int i = 0; i < GameManager.Instance.Players.Count; i++)
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
            OccupiedCellsToCheck = new NativeArray<int>(9 * UsedEnemies.Count, Allocator.TempJob),

            flowfieldOffset = FlowFieldManager.instance.Offset - FlowFieldManager.instance.transform.position,
            CellSize = FlowFieldManager.instance.CellSize,
            CellCollumCount = FlowFieldManager.instance.CellCollumCount,
            CellCollumFirst = FlowFieldManager.instance.CellCollumFirst,
            ColumWidthValue = FlowFieldManager.instance.depth
        };
        JobHandle handle = enemyLocation.Schedule(UsedEnemies.Count, 64);
        handle.Complete();

        //ProcessResults
        for (int i = 0; i < UsedEnemies.Count; i++)
        {
            Enemy e = UsedEnemies[i];
            EnemyJobData EJD = enemyLocation.EnemyData[i];
            e.target = GameManager.Instance.Players[enemyLocation.TargetIndices[i]];
            e.targetVector = EJD.targetVector;
            e.currentCell = FlowFieldManager.instance.flowField.allCells[EJD.CurrentCell];
            e.forwardCell = FlowFieldManager.instance.flowField.allCells[EJD.fowardCell];

            //if (!Physics.Raycast(transform.position, math.normalize(EJD.targetVector),  math.length(EJD.targetVector), ~e.CanSeeTargetThrough))
            if (math.dot(enemyLocation.Cells[EJD.CurrentCell].Direction, enemyLocation.Cells[e.target.TargetCellID].Position) > 0.9f || EJD.CurrentCell == e.target.TargetCellID)
            {
                EJD.canSeePlayer = true;
            }
            else
            {
                EJD.canSeePlayer = false;
            }
            if (EJD.canSeePlayer && math.length(EJD.targetVector) < EJD.activationDistance)
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
            NeighborContexts = FlowFieldManager.instance.neighborContexts,
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
        if (UsedEnemies.Count <= 0)
        {
            return;
        }
        RaycastHit[] hit;
        float[] dot;
        StartEnemyRaycastJob(out hit, out dot);
        for (int i = 0; i < usedEnemiesByType.Count; i++)
        {
            foreach (Enemy e in usedEnemiesByType[i])
            {
                if (e != null)
                {
                    e.normalDot = dot[e.ActiveID.ID];
                    e.LastHitInfo = hit[e.ActiveID.ID];
                    e.FixedRBUpdate();
                }
            }
        }
    }
    void StartEnemyRaycastJob(out RaycastHit[] results, out float[] normalDot)
    {
        NativeArray<RaycastCommand> commands = new NativeArray<RaycastCommand>(UsedEnemies.Count, Allocator.TempJob);
        NativeArray<float> nDot = new NativeArray<float>(UsedEnemies.Count, Allocator.TempJob);

        NativeArray<EnemyJobData> enemiesInfo = new NativeArray<EnemyJobData>(UsedEnemies.Count, Allocator.TempJob);
        NativeArray<RaycastHit> Results = new NativeArray<RaycastHit>(UsedEnemies.Count, Allocator.TempJob);
        for (int i = 0; i < UsedEnemies.Count; i++)
        {
            if (UsedEnemies[i] != null)
            {
                enemiesInfo[i] = new EnemyJobData()
                {
                    Position = UsedEnemies[i].transform.position,
                    height = UsedEnemies[i].height,
                    terrainBuffer = UsedEnemies[i].terrainBuffer
                };
            }
        }
        EnemyGroundRaycastJob raycastJob = new EnemyGroundRaycastJob()
        {
            EnemyData = enemiesInfo,
            GroundMask = RayMasks,
            Commands = commands,
        };
        JobHandle handle = raycastJob.Schedule(UsedEnemies.Count, 64);
        JobHandle RaycastHandle = RaycastCommand.ScheduleBatch(raycastJob.Commands, Results, 64, handle);
        EnemyCalculateDotJob dotJob = new EnemyCalculateDotJob()
        {
            Res = Results,
            NormalDot = nDot
        };
        JobHandle dotHandle = dotJob.Schedule(UsedEnemies.Count, 64, RaycastHandle);
        dotHandle.Complete();
        results = Results.ToArray();
        normalDot = dotJob.NormalDot.ToArray();

        enemiesInfo.Dispose();
        Results.Dispose();
        nDot.Dispose();
        commands.Dispose();

    }
}
