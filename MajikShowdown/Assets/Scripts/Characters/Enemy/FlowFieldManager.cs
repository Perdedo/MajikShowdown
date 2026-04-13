
using UnityEngine;

public class FlowFieldManager : MonoBehaviour
{
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
    FieldCell lastTargetPos;
    [Header("Gizmos")]
    public bool ShowCells = true;
    public bool ShowFieldArea = true;
    public bool ShowDirections = true;
    public bool ShowTargetPos = true;
    void Awake()
    {
        instance = this;
        GenerateGrid();
        Target = GameManager.Instance.Players[0].transform;
        lastTargetPos = WorldToGridPosition(Target.position);
        flowField.GenerateFlowField(lastTargetPos);
    }
    void Update()
    {
        FieldCell current = WorldToGridPosition(Target.position);
        if (current != null && ((current.fieldPos.gridPosition - lastTargetPos.fieldPos.gridPosition).magnitude > TargetRecalculationOffset || current.position.y - lastTargetPos.position.y > SlopeThreshold))
        {
            lastTargetPos = current;
            flowField.GenerateFlowField(current);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (flowField != null && ShowCells)
        {
            foreach (var v in flowField.field)
            {
                foreach (FieldCell cell in v.Value.Layers)
                {
                    if (cell != null)
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
                        if (cell != null)
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
            FieldCell c = WorldToGridPosition(Target.position);
            Gizmos.color = Color.magenta;
            Gizmos.DrawCube(c.position, Vector3.one * CellSize * 0.9f);
            foreach (FieldCell.NeighborContext n in c.Neighbors)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawCube(n.neighborCell.position, Vector3.one * CellSize * 0.9f);
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
        Vector3 localPos = worldPosition - transform.position;
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
