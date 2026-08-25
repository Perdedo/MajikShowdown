
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.VisualScripting;


#if UNITY_EDITOR
using UnityEditor;
#endif
public class FlowFieldManager : MonoBehaviour
{
    public FlowFieldAsset flowFieldAsset;
    public static FlowFieldManager instance;
    public Vector2 MapSize;
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
    [HideInInspector] public NativeArray<int>CellNeighborID;
    void Awake()
    {
        instance = this;
        //GenerateGrid();
        InitializeGrid();
        Target = GameManager.Instance.Players[0].transform;
        //lastTargetPos = WorldToGridPosition(Target.position);
        Targets.Clear();
        lastTargetsPos.Clear();
        foreach(Player p in GameManager.Instance.Players)
        {
            Targets.Add(p.transform);
            lastTargetsPos.Add(WorldToGridPosition(p.transform.position));
            p.TargetCellID = lastTargetsPos[lastTargetsPos.Count-1].ID;
        }
        flowField.GenerateFlowField(lastTargetsPos);
        StartCoroutine(FlowFieldGenerator());
    }
    IEnumerator FlowFieldGenerator()
    {
        moved = false;
        for(int i = 0; i < Targets.Count; i++)
        {
            current = WorldToGridPosition(Targets[i].position);
            if (current != null && ((current.fieldPos.gridPosition - lastTargetsPos[i].fieldPos.gridPosition).magnitude > TargetRecalculationOffset || current.position.y - lastTargetsPos[i].position.y > SlopeThreshold))
            {
                lastTargetsPos[i] = current;
                moved = true;
                GameManager.Instance.Players[i].TargetCellID = current.ID;
            }
        }
        if(moved)
        {
            flowField.GenerateFlowField(lastTargetsPos);
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
            if(!p.dead)
            {
                Targets.Add(p.transform);
                lastTargetsPos.Add(WorldToGridPosition(p.transform.position));
                p.TargetCellID = lastTargetsPos[lastTargetsPos.Count-1].ID;
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
        if(!integrated)
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
        if(!directed)
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
                        Gizmos.color = Color.green;
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
            foreach(Transform p in Targets)
            {
                FieldCell c = WorldToGridPosition(p.position);
                if(c != null)
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
        Vector3 localPos = worldPosition - transform.position +Offset;
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
        flowField = new FlowField(CellSize, this);
        flowField.GetFieldFromAsset();

        cellJobDatas = new NativeArray<CellJobData>(flowField.allCells.Count,Allocator.Persistent);
        for(int i = 0; i< cellJobDatas.Length; i++)
        {
            cellJobDatas[i] = new CellJobData()
            {
                firstNeighbor = flowField.allCells[i].firstNeighbor,
                lastNeighbor = flowField.allCells[i].lastNeighbor,
                Position = flowField.allCells[i].position
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
