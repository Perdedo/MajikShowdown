
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.VisualScripting;
using System;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Burst;

#if UNITY_EDITOR
using UnityEditor;
#endif
public class FlowFieldManager : MonoBehaviour
{
    public FlowFieldAsset flowFieldAsset;
    public static FlowFieldManager instance;
    public Vector2 MapSize;
    [NonSerialized] public int width, depth;
    public float MinHeight;
    public float MaxHeight;
    public float CellSize = 1f;
    public Vector3 Offset = Vector3.one * 0.5f;
    public float ObstructionHeight = 2f;
    public LayerMask ObstructionLayer;
    public LayerMask ObstacleMask;
    public float SlopeThreshold = 0.5f;
    [Range(1, 2)] public float DiagonalWeight = 1;
    [Tooltip("How much the directions point to the lowest cost cells")]
    [Range(0, 20)] public float BestDirectionStrenght = 10;
    [Tooltip("How much the directions point directly to the target")]
    [Range(0, 20)] public float TargetDirectionStrenght = 0;
    [Tooltip("How much the directions point to the sum of all lower cost neighbors")]
    [Range(0, 20)] public float NeighborSumDirectionStrenght = 1;
    public float BorderCellWeight = 1;
    public bool DiagonalNeighbors = true;
    public int TargetRecalculationOffset;
    public FlowField flowField;
    public Transform Target;
    //FieldCell lastTargetPos;
    public List<Transform> Targets = new List<Transform>();
    List<FieldCell> lastTargetsPos = new List<FieldCell>();
    bool moved;
    FieldCell current;
    public float flowFieldDelay = 0.5f;
    [Header("Gizmos")]
    public bool ShowCells = true;
    public bool ShowFieldArea = true;
    public bool ShowDirections = true;
    public bool ShowTargetPos = true;

    [HideInInspector] public NativeArray<CellJobData> cellJobDatas;
    [HideInInspector] public NativeArray<int> CellNeighborID;
    [HideInInspector] public NativeArray<int> CellCollumFirst;
    [HideInInspector] public NativeArray<int> CellCollumCount;
    [HideInInspector] public NativeArray<FieldCell.NeighborContext.Context> neighborContexts;
    [HideInInspector] public NativeArray<float3> CellNeighborDir;
    [HideInInspector] public NativeArray<byte> cellNeighborDiagonal;
    void Awake()
    {
        instance = this;
        //GenerateGrid();
        InitializeGrid();
        Target = GameManager.Instance.Players[0].transform;
        //lastTargetPos = WorldToGridPosition(Target.position);
        Targets.Clear();
        lastTargetsPos.Clear();
        foreach (Player p in GameManager.Instance.Players)
        {
            Targets.Add(p.transform);
            lastTargetsPos.Add(WorldToGridPosition(p.transform.position));
            p.TargetCellID = lastTargetsPos[lastTargetsPos.Count - 1].ID;
        }
        flowField.GenerateFlowField(lastTargetsPos);
        //StartCoroutine(FlowFieldGenerator());
    }

    private void Update()
    {
        moved = false;
        for (int i = 0; i < Targets.Count; i++)
        {
            current = WorldToGridPosition(Targets[i].position);
            if (current != null && ((current.fieldPos.gridPosition - lastTargetsPos[i].fieldPos.gridPosition).magnitude > TargetRecalculationOffset || current.position.y - lastTargetsPos[i].position.y > SlopeThreshold))
            {
                lastTargetsPos[i] = current;
                moved = true;
                GameManager.Instance.Players[i].TargetCellID = current.ID;
            }
        }
        if (moved)
        {
            flowField.GenerateFlowField(lastTargetsPos);
        }
    }
    IEnumerator FlowFieldGenerator()
    {
        moved = false;
        for (int i = 0; i < Targets.Count; i++)
        {
            current = WorldToGridPosition(Targets[i].position);
            if (current != null && ((current.fieldPos.gridPosition - lastTargetsPos[i].fieldPos.gridPosition).magnitude > TargetRecalculationOffset || current.position.y - lastTargetsPos[i].position.y > SlopeThreshold))
            {
                lastTargetsPos[i] = current;
                moved = true;
                GameManager.Instance.Players[i].TargetCellID = current.ID;
            }
        }
        if (moved)
        {
            flowField.GenerateFlowFieldOld(lastTargetsPos);
        }
        yield return new WaitForSeconds(flowFieldDelay);
        StartCoroutine(FlowFieldGenerator());
    }

    public void UpdateFlowField()
    {
        Targets.Clear();
        lastTargetsPos.Clear();
        foreach (Player p in GameManager.Instance.Players)
        {
            if (!p.dead)
            {
                Targets.Add(p.transform);
                lastTargetsPos.Add(WorldToGridPosition(p.transform.position));
                p.TargetCellID = lastTargetsPos[lastTargetsPos.Count - 1].ID;
            }
        }
        flowField.GenerateFlowField(lastTargetsPos);
    }

    bool integrated = false;
    public void GenerateFlowFieldIntegrations()
    {
        integrated = false;
        StartCoroutine(GenerateFFIntegrations());
    }

    IEnumerator GenerateFFIntegrations()
    {
        if (!integrated)
        {
            integrated = flowField.GenerateIntegration();
            yield return new WaitForEndOfFrame();
            StartCoroutine(GenerateFFIntegrations());
        }
    }
    bool directed = false;
    public int cellCountAux;
    public void GenerateFlowFieldDirections()
    {
        cellCountAux = 0;
        directed = false;
        StartCoroutine(GenerateFFDirections());
    }

    IEnumerator GenerateFFDirections()
    {
        if (!directed)
        {
            directed = flowField.GenerateDirections(ref cellCountAux);
            yield return new WaitForEndOfFrame();
            StartCoroutine(GenerateFFDirections());
        }
    }

    public float maxSqrRenderDistance = 10000;

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (flowField != null && ShowCells)
        {
            foreach (var v in flowField.field)
            {
                foreach (FieldCell cell in v.Value.Layers)
                {
                    if (cell != null && (cell.position - Camera.current.transform.position).sqrMagnitude < maxSqrRenderDistance)
                    {

                        if (cellJobDatas[cell.ID].bestCost == float.MaxValue)
                        {
                            Gizmos.color = Color.red;
                        }
                        else
                        {
                            Gizmos.color = Color.green;
                        }
                        Gizmos.DrawCube(cell.position, Vector3.one * CellSize * 0.9f);
                    }
                }
            }
        }
        if (ShowFieldArea)
        {
            Gizmos.color = Color.yellow;
            Vector3 center = new Vector3(MapSize.x * 0.5f + transform.position.x, (MinHeight + MaxHeight) / 2 + transform.position.y, MapSize.y * 0.5f + transform.position.z);
            Vector3 volume = new Vector3(MapSize.x, MaxHeight - MinHeight, MapSize.y);
            Gizmos.DrawWireCube(center, volume);
        }
        if (ShowDirections)
        {
            if (flowField != null)
            {
                foreach (var v in flowField.field)
                {
                    foreach (FieldCell cell in v.Value.Layers)
                    {
                        if (cell != null && (cell.position - Camera.current.transform.position).sqrMagnitude < maxSqrRenderDistance)
                        {
                            Gizmos.color = Color.blue;
                            Gizmos.DrawRay(cell.position, cell.direction * CellSize * 0.5f);
                        }
                    }
                }
            }
        }
        if (flowField != null && ShowTargetPos)
        {
            foreach (Transform p in Targets)
            {
                FieldCell c = WorldToGridPosition(p.position);
                if (c != null)
                {
                    Gizmos.color = Color.magenta;
                    Gizmos.DrawCube(c.position, Vector3.one * CellSize * 0.9f);
                    foreach (FieldCell.NeighborContext n in c.Neighbors)
                    {
                        switch (n.context)
                        {
                            case FieldCell.NeighborContext.Context.Jumpable:
                                Gizmos.color = Color.orange;
                                break;
                            case FieldCell.NeighborContext.Context.Upper:
                                Gizmos.color = Color.red;
                                break;
                            default:
                                Gizmos.color = Color.blue;
                                break;
                        }
                        Gizmos.DrawCube(n.neighborCell.position, Vector3.one * CellSize * 0.9f);
                    }
                }
            }
        }
        /*if (flowField != null)
        {
            
            Gizmos.color = Color.red;
                Gizmos.DrawCube(flowField.GetCell(cellpos, 0).position, Vector3.one * CellSize * 0.9f);
        }*/
        /*if (flowField != null)
        {
            FieldCell c = WorldToGridPosition(Target.position);
            Debug.Log(c.BestCost);
        }*/
    }
#endif

    //public Vector2Int cellpos;
    public FieldCell WorldToGridPosition(Vector3 worldPosition, bool ToLowestLayer = true)
    {
        Vector3 localPos = worldPosition - transform.position + Offset;
        Vector2Int v = new Vector2Int(Mathf.FloorToInt(localPos.x / CellSize), Mathf.FloorToInt(localPos.z / CellSize));
        FieldCell closest = null;
        CellColumn col = flowField.GetColumn(v);
        if (col == null) return null;
        foreach (FieldCell c in col.Layers)
        {
            if (closest == null || Mathf.Abs(c.position.y - worldPosition.y) < Mathf.Abs(closest.position.y - worldPosition.y))
            {
                if (!ToLowestLayer || c.position.y <= worldPosition.y)
                {
                    closest = c;
                }
            }
        }
        return closest;
    }

    public void InitializeGrid()
    {
        width = Mathf.CeilToInt(MapSize.x / CellSize);
        depth = Mathf.CeilToInt(MapSize.y / CellSize);
        flowField = new FlowField(CellSize, this);
        flowField.GetFieldFromAsset();

        cellJobDatas = new NativeArray<CellJobData>(flowField.allCells.Count, Allocator.Persistent);
        for (int i = 0; i < cellJobDatas.Length; i++)
        {
            int neighborCount = flowField.allCells[i].lastNeighbor - flowField.allCells[i].firstNeighbor + 1;
            float baseC = 1;
            if (neighborCount < 8)
            {
                baseC = BorderCellWeight;
            }
            cellJobDatas[i] = new CellJobData()
            {
                firstNeighbor = flowField.allCells[i].firstNeighbor,
                lastNeighbor = flowField.allCells[i].lastNeighbor,
                Position = flowField.allCells[i].position,
                baseCost = baseC
            };
        }

    }
    public void OnDestroy()
    {
        if (cellJobDatas.IsCreated)
        {
            cellJobDatas.Dispose();
        }
        if (CellNeighborID.IsCreated)
        {
            CellNeighborID.Dispose();
        }
        if (CellCollumCount.IsCreated)
        {
            CellCollumCount.Dispose();
        }
        if (CellCollumFirst.IsCreated)
        {
            CellCollumFirst.Dispose();
        }
        if (neighborContexts.IsCreated)
        {
            neighborContexts.Dispose();
        }
        if (CellNeighborDir.IsCreated)
        {
            CellNeighborDir.Dispose();
        }
        if (cellNeighborDiagonal.IsCreated)
        {
            cellNeighborDiagonal.Dispose();
        }
    }

    [ContextMenu("GenerateGrid")]
    public void GenerateGrid()
    {
        flowField = new FlowField(CellSize, this);
        flowField.GenerateGrid(MapSize, MinHeight, MaxHeight);
    }

    [ContextMenu("Player Test")]
    public void PlayerTest()
    {
        flowField.GenerateFlowField(WorldToGridPosition(Target.position));
    }
}
[BurstCompile]
public struct GenerateIntegrationJob : IJob
{
    public NativeArray<int> targetCells;
    public NativeArray<CellJobData> Cells;
    public NativeArray<int> cellNeighbors;
    public NativeArray<FieldCell.NeighborContext.Context> NeighborContext;
    public NativeArray<byte> CellNeighborDiagonal;
    public int currentGeneration;
    public float borderCellWeight;
    public float diagonalWeight;
    public void Execute()
    {
        GenerateIntegration();
    }
    void GenerateIntegration()
    {
        NativeQueue<int> cellsToProcess = new NativeQueue<int>(Allocator.Temp);
        foreach (int index in targetCells)
        {
            CellJobData c = Cells[index];
            c.bestCost = 0;
            c.generation = currentGeneration;
            c.TargetID = index;
            Cells[index] = c;
            cellsToProcess.Enqueue(index);
        }
        while (cellsToProcess.Count > 0)
        {
            CellJobData currentCell = Cells[cellsToProcess.Dequeue()];
            //int neighborCount = currentCell.lastNeighbor - currentCell.firstNeighbor + 1;
            for (int i = currentCell.firstNeighbor; i <= currentCell.lastNeighbor; i++)
            {
                int neighborID = cellNeighbors[i];
                CellJobData neighborCell = Cells[neighborID];
                /*if (neighborCount < 8)
                {
                    neighborCell.baseCost = borderCellWeight;
                }*/
                if (neighborCell.generation != currentGeneration)
                {
                    neighborCell.generation = currentGeneration;
                    neighborCell.bestCost = float.MaxValue;
                    neighborCell.TargetID = -1;
                }
                if (NeighborContext[i] == FieldCell.NeighborContext.Context.Lower)
                {
                    Cells[neighborID] = neighborCell;
                    continue;
                }
                if (neighborCell.bestCost > currentCell.bestCost + neighborCell.baseCost)
                {
                    float mult = 1;
                    if (CellNeighborDiagonal[i] == 1)
                    {
                        mult = diagonalWeight;
                    }
                    neighborCell.bestCost = currentCell.bestCost + neighborCell.baseCost * mult;
                    neighborCell.TargetID = currentCell.TargetID;

                    cellsToProcess.Enqueue(neighborID);
                }
                Cells[neighborID] = neighborCell;
            }

        }
        cellsToProcess.Dispose();
    }
}
[BurstCompile]
public struct GenerateDirectionJob : IJobParallelFor
{
    [Unity.Collections.ReadOnly] public NativeArray<CellJobData> Cells;
    [Unity.Collections.ReadOnly] public NativeArray<int> cellNeighbors;
    [Unity.Collections.ReadOnly] public NativeArray<float3> cellNeighborsDir;
    [Unity.Collections.ReadOnly] public NativeArray<FieldCell.NeighborContext.Context> NeighborContext;
    [Unity.Collections.ReadOnly] public NativeParallelHashSet<int> targetCells;
    [WriteOnly]
    public NativeArray<float3> DirectionsOutput;
    public float NeighborSumDirectionStrenght, BestDirectionStrenght, TargetDirectionStrenght;
    public void Execute(int index)
    {
        DirectionsOutput[index] = GenerateDirections(index);
    }
    float3 GenerateDirections(int index)
    {
        CellJobData c = Cells[index];
        if (targetCells.Contains(index) || c.TargetID == -1)
        {
            return float3.zero;
        }
        int lowest = -1;
        float3 lowestDir = float3.zero;
        float3 dirToDestiny = Cells[c.TargetID].Position - c.Position;
        dirToDestiny.y = 0;
        dirToDestiny *= 1f / (math.abs(dirToDestiny.x) + math.abs(dirToDestiny.z) + 0.0001f);
        float bestDot = float.MinValue;
        float3 dirSum = float3.zero;
        for (int i = c.firstNeighbor; i <= c.lastNeighbor; i++)
        {
            CellJobData neighborCell = Cells[cellNeighbors[i]];
            if (NeighborContext[i] == FieldCell.NeighborContext.Context.Upper)
            {
                continue;
            }
            if (neighborCell.bestCost > c.bestCost)
            {
                continue;
            }
            dirSum += cellNeighborsDir[i] * (1 + c.bestCost - neighborCell.bestCost);
            if (lowest == -1 || neighborCell.bestCost < Cells[lowest].bestCost)
            {
                lowest = cellNeighbors[i];
                lowestDir = cellNeighborsDir[i];
                bestDot = math.dot(dirToDestiny, cellNeighborsDir[i]);
            }
            else if (neighborCell.bestCost == Cells[lowest].bestCost)
            {
                float dot = math.dot(dirToDestiny, cellNeighborsDir[i]);
                if (dot > bestDot)
                {
                    lowest = cellNeighbors[i];
                    lowestDir = cellNeighborsDir[i];
                    bestDot = dot;
                }
            }
        }
        if (lowest == -1)
        {
            return float3.zero;
        }
        //float3 dir = math.normalize(CellDistance(index, lowest));
        return math.normalizesafe(dirSum * NeighborSumDirectionStrenght + lowestDir * BestDirectionStrenght + dirToDestiny * TargetDirectionStrenght);

    }
    /*public float3 GetDistanceToClosestDestinationCell(int cellIndex)
    {
        float3 dir = float3.zero, aux;
        float sqrMag = float.MaxValue;
        foreach (int i in targetCells)
        {
            aux = CellDistance(cellIndex, i);
            float auxMagSqr = math.lengthsq(aux);
            if (auxMagSqr < sqrMag)
            {
                sqrMag = auxMagSqr;
                dir = aux;
            }
        }
        return dir;
    }
    public float3 CellDistance(int from, int to)
    {
        return new float3(Cells[to].Position.x - Cells[from].Position.x, 0, Cells[to].Position.z - Cells[from].Position.z);
    }*/
}
