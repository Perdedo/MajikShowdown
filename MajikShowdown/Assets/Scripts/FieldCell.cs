using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class FieldCell
{
    public Vector3 position;
    public FieldPos fieldPos;
    public float BaseCost = 1;
    public float BestCost = float.MaxValue;
    public bool closeToObstacle = false;
    public Vector3 direction;
    public Vector3 directionToDestiny;
    public int generation = 0;
    public int ID;
    public int firstNeighbor, lastNeighbor;
    public List<NeighborContext> Neighbors;
    [NonSerialized]
    private HashSet<int> containedEnemies;
    public HashSet<int> ContainedEnemies => containedEnemies ??= new HashSet<int>();
    //public float angle;
    public FieldCell(Vector3 position, Vector2Int gridPosition, int layerIndex, int id/*, float angle*/)
    {
        this.position = position;
        fieldPos.gridPosition = gridPosition;
        fieldPos.layerIndex = layerIndex;
        this.ID = id;
        //this.angle = angle;
    }
    public FieldCell(Vector3 position, FieldPos pos, int id/*, float angle*/)
    {
        this.position = position;
        fieldPos = pos;
        this.ID = id;
        //this.angle = angle;
    }
    public void SetDirection(Vector3 direction)
    {
        this.direction = direction.normalized;
    }
    public void ResetCost()
    {
        BestCost = float.MaxValue;
    }
    [Serializable]
    public struct FieldPos
    {
        public Vector2Int gridPosition;
        public int layerIndex;
        public FieldPos(Vector2Int GridPosition, int LayerIndex)
        {
            gridPosition = GridPosition;
            layerIndex = LayerIndex;
        }
    }
    public struct NeighborContext
    {
        public FieldCell neighborCell;
        public Vector3 neighborDir;
        public enum Context { None, Lower, ABitLower, Upper, Jumpable }
        public Context context;
        public NeighborContext(FieldCell cell,Vector3 dir, Context c)
        {
            neighborCell = cell;
            context = c;
            neighborDir = dir.normalized;
        }
    }

}

[Serializable]
public class CellColumn
{
    public CellColumn(Vector2Int GridPos)
    {
        gridPosition = GridPos;
    }
    public List<FieldCell> Layers = new List<FieldCell>();
    public Vector2Int gridPosition;
}

[Serializable]
public class FlowFieldDivision
{
    public CellColumn column;
    public Vector2Int key;
    public FlowFieldDivision(CellColumn column, Vector2Int key)
    {
        this.column = column;
        this.key = key;
    }
}
