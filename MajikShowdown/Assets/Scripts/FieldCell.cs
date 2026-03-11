using System.Collections.Generic;
using UnityEngine;

public class FieldCell
{
    public Vector3 position;
    public FieldPos fieldPos;
    public float BaseCost = 1;
    public float BestCost = float.MaxValue;
    public Vector3 direction;
    public int generation = 0;
    public List<NeighborContext> Neighbors;
    //public float angle;
    public FieldCell(Vector3 position, Vector2Int gridPosition, int layerIndex/*, float angle*/)
    {
        this.position = position;
        fieldPos.gridPosition = gridPosition;
        fieldPos.layerIndex = layerIndex;
        //this.angle = angle;
    }
    public FieldCell(Vector3 position, FieldPos pos/*, float angle*/)
    {
        this.position = position;
        fieldPos = pos;
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
        public enum Context { None, Lower, Upper, Jumpable }
        public Context context;
        public NeighborContext(FieldCell cell,Vector3 dir, Context c)
        {
            neighborCell = cell;
            context = c;
            neighborDir = dir.normalized;
        }
    }

}
public class CellColumn
{
    public CellColumn(Vector2Int GridPos)
    {
        gridPosition = GridPos;
    }
    public List<FieldCell> Layers = new List<FieldCell>();
    public Vector2Int gridPosition;
}
