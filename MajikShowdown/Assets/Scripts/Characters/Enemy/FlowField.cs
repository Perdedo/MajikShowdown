using System.Collections.Generic;
using UnityEngine;
using Unity.AI.Navigation;
using UnityEngine.AI;

public class FlowField
{
    public Dictionary<Vector2Int, CellColumn> field = new Dictionary<Vector2Int, CellColumn>();
    public Vector2Int fieldSize;
    public float cellSize = 1f;
    public float maxStepOffset = 0.5f;
    public float maxJumpHeight = 5f;
    public FieldCell DestinationCell;
    public List<FieldCell> DestinationCells;
    public bool DiagonalNeighbors = true;
    public FlowFieldManager manager;
    public int CurrentGeneration = 0;
    public FlowField(float CellSize, FlowFieldManager Manager, bool diagonalNeighbors = true)
    {
        //field = new FieldCell[width, depth];
        cellSize = CellSize;
        DiagonalNeighbors = diagonalNeighbors;
        manager = Manager;
    }
    public void GenerateGrid(Vector2 mapSize, float minY, float maxY)
    {
        field.Clear();
        int width = Mathf.CeilToInt(mapSize.x / cellSize);
        int depth = Mathf.CeilToInt(mapSize.y / cellSize);
        fieldSize = new Vector2Int(width, depth);

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < depth; z++)
            {
                Vector2Int gridPos = new Vector2Int(x, z);
                CellColumn column = new CellColumn(gridPos);

                for (float y = maxY; y >= minY; y -= 1f)
                {
                    Vector3 samplePos = new Vector3(x * cellSize, y, z * cellSize) + manager.transform.position + manager.Offset;

                    if (NavMesh.SamplePosition(samplePos, out NavMeshHit hit, 0.5f, NavMesh.AllAreas))
                    {
                        bool alreadyExists = column.Layers.Exists(l => Mathf.Abs(l.position.y - hit.position.y) < 0.5f);

                        if (!alreadyExists)
                        {
                            column.Layers.Add(new FieldCell(hit.position, gridPos, column.Layers.Count));
                        }
                    }
                }

                if (column.Layers.Count > 0)
                    field.Add(gridPos, column);
            }
        }
        foreach (var v in field)
        {
            foreach (FieldCell cell in v.Value.Layers)
            {
                cell.Neighbors = GetNeighbors(cell);
                cell.closeToObstacle = CheckForObstacles(cell);
            }
        }
    }

    float DetectionRadius = 4;
    public bool CheckForObstacles(FieldCell c)
    {
        Collider[] buffer = new Collider[32];
        int count = Physics.OverlapSphereNonAlloc(c.position, DetectionRadius, buffer, manager.ObstacleMask);
        return count > 0;
    }

    public FieldCell GetCell(Vector2Int gridPos, int layerIndex)
    {
        if (field.TryGetValue(gridPos, out CellColumn column) && layerIndex >= 0 && layerIndex < column.Layers.Count)
        {
            return column.Layers[layerIndex];
        }
        else
        {
            return null;
        }
    }
    public CellColumn GetColumn(Vector2Int gridPos)
    {
        if (field.TryGetValue(gridPos, out CellColumn column))
        {
            return column;
        }
        else
        {
            return null;
        }
    }
    List<FieldCell.NeighborContext> GetNeighbors(FieldCell cell) // baseado em posição e não em layer
    {
        List<FieldCell.NeighborContext> neighbors = new List<FieldCell.NeighborContext>(8);
        Vector2Int pos = cell.fieldPos.gridPosition;
        Vector2Int[] directions;
        if (DiagonalNeighbors)
        {
            directions = allDirections;
        }
        else
        {
            directions = Directions;
        }
        foreach (var dir in directions)
        {
            Vector2Int neighborPos = new Vector2Int(pos.x + dir.x, pos.y + dir.y);
            //FieldCell neighborCell = GetCell(neighborPos, cell.LayerIndex);
            CellColumn neighborColumn = GetColumn(neighborPos);
            if (neighborColumn != null)
            {
                float closestY = Mathf.Infinity;
                foreach (FieldCell c in neighborColumn.Layers)
                {
                    if (c.position.y < cell.position.y + maxStepOffset) //checa se o vizinho é menor que o step Offset
                    {
                        if(Mathf.Abs(c.position.y - cell.position.y) < Mathf.Abs(closestY - cell.position.y)) //checa se o vizinho é o mais perto no y na sua coluna
                        {
                            closestY = c.position.y;
                        }
                        if(c.position.y >= closestY) //checa se o vizinho é mais alto que o y mais proximo em sua coluna
                        {
                            if(c.position.y < cell.position.y - maxStepOffset)
                            {
                                if(c.position.y < cell.position.y - maxJumpHeight)
                                {
                                    neighbors.Add(new FieldCell.NeighborContext(c, CellDistance(cell,c),FieldCell.NeighborContext.Context.Lower));
                                }
                                else
                                {
                                    neighbors.Add(new FieldCell.NeighborContext(c, CellDistance(cell,c),FieldCell.NeighborContext.Context.ABitLower));
                                }
                            }
                            else
                            {
                                neighbors.Add(new FieldCell.NeighborContext(c,CellDistance(cell,c), FieldCell.NeighborContext.Context.None));
                            }
                        }
                    }
                    else
                    {
                        if (c.position.y < cell.position.y + maxJumpHeight)
                        {
                            neighbors.Add(new FieldCell.NeighborContext(c, CellDistance(cell, c), FieldCell.NeighborContext.Context.Jumpable));
                        }
                        else
                        {
                            neighbors.Add(new FieldCell.NeighborContext(c, CellDistance(cell, c), FieldCell.NeighborContext.Context.Upper));
                        }
                    }
                }
            }

        }

        return neighbors;
    }
    public void GenerateFlowField(Vector2Int targetCellPos, int targetCellLayer)
    {
        //ResetCost();
        CurrentGeneration++;
        GenerateIntegration(targetCellPos, targetCellLayer);
        GenerateDirections();
    }
    public void GenerateFlowField(FieldCell target)
    {
        //ResetCost();
        CurrentGeneration++;
        GenerateIntegration(target);
        GenerateDirections();
    }
    public void GenerateFlowField(List<FieldCell> targets)
    {
        //ResetCost();
        CurrentGeneration++;
        GenerateIntegration(targets);
        GenerateDirections();
    }
    void GenerateIntegration(Vector2Int targetCellPos, int targetCellLayer)
    {
        DestinationCell = GetCell(targetCellPos, targetCellLayer);
        if (DestinationCell == null) return;
        Queue<FieldCell> cellsToProcess = new Queue<FieldCell>();
        DestinationCell.BestCost = 0;
        DestinationCell.generation = CurrentGeneration;
        cellsToProcess.Enqueue(DestinationCell);
        while (cellsToProcess.Count > 0)
        {
            FieldCell currentCell = cellsToProcess.Dequeue();
            foreach (FieldCell.NeighborContext n in currentCell.Neighbors)
            {
                if(currentCell.Neighbors.Count < 8)
                {
                    n.neighborCell.BaseCost = manager.BorderCellWeight;
                }
                if(n.neighborCell.generation != CurrentGeneration)
                {
                    n.neighborCell.generation = CurrentGeneration;
                    n.neighborCell.ResetCost();
                }
                if(n.context == FieldCell.NeighborContext.Context.Lower)
                {
                    continue;
                }
                if (n.neighborCell.BestCost > currentCell.BestCost + n.neighborCell.BaseCost)
                {
                    float mult = 1;
                    Vector2 dir = new Vector2(n.neighborCell.position.x - currentCell.position.x, n.neighborCell.position.z - currentCell.position.z).normalized;
                    if(dir.x != 0 && dir.y != 0)
                    {
                        mult = manager.DiagonalWeight;
                    }
                    n.neighborCell.BestCost = currentCell.BestCost + n.neighborCell.BaseCost *mult;
                    cellsToProcess.Enqueue(n.neighborCell);
                }

            }
        }
    }
    void GenerateIntegration(FieldCell target)
    {
        DestinationCell = target;
        if (DestinationCell == null) return;
        Queue<FieldCell> cellsToProcess = new Queue<FieldCell>();
        DestinationCell.BestCost = 0;
        DestinationCell.generation = CurrentGeneration;
        cellsToProcess.Enqueue(DestinationCell);
        while (cellsToProcess.Count > 0)
        {
            FieldCell currentCell = cellsToProcess.Dequeue();
            foreach (FieldCell.NeighborContext n in currentCell.Neighbors)
            {
                if(currentCell.Neighbors.Count < 8)
                {
                    n.neighborCell.BaseCost = manager.BorderCellWeight;
                }
                if(n.neighborCell.generation != CurrentGeneration)
                {
                    n.neighborCell.generation = CurrentGeneration;
                    n.neighborCell.ResetCost();
                }
                if (n.context == FieldCell.NeighborContext.Context.Lower)
                {
                    continue;
                }
                if (n.neighborCell.BestCost > currentCell.BestCost + n.neighborCell.BaseCost)
                {
                    float mult = 1;
                    Vector2 dir = new Vector2(n.neighborCell.position.x - currentCell.position.x, n.neighborCell.position.z - currentCell.position.z);
                    //Vector2 dir = new Vector2(n.neighborCell.position.x - currentCell.position.x, n.neighborCell.position.z - currentCell.position.z).normalized;
                    if(dir.x != 0 && dir.y != 0)
                    {
                        mult = manager.DiagonalWeight;
                    }
                    n.neighborCell.BestCost = currentCell.BestCost + n.neighborCell.BaseCost * mult;
                    
                    cellsToProcess.Enqueue(n.neighborCell);
                }

            }
        }
    }
    void GenerateIntegration(List<FieldCell> targets)
    {
        DestinationCells = targets;
        if (DestinationCells.Count <= 0) return;
        Queue<FieldCell> cellsToProcess = new Queue<FieldCell>();
        foreach(FieldCell cell in DestinationCells)
        {
            cell.BestCost = 0;
            cell.generation = CurrentGeneration;
            cellsToProcess.Enqueue(cell);
        }
        while (cellsToProcess.Count > 0)
        {
            FieldCell currentCell = cellsToProcess.Dequeue();
            foreach (FieldCell.NeighborContext n in currentCell.Neighbors)
            {
                if(currentCell.Neighbors.Count < 8)
                {
                    n.neighborCell.BaseCost = manager.BorderCellWeight;
                }
                if(n.neighborCell.generation != CurrentGeneration)
                {
                    n.neighborCell.generation = CurrentGeneration;
                    n.neighborCell.ResetCost();
                }
                if (n.context == FieldCell.NeighborContext.Context.Lower)
                {
                    continue;
                }
                if (n.neighborCell.BestCost > currentCell.BestCost + n.neighborCell.BaseCost)
                {
                    float mult = 1;
                    Vector2 dir = new Vector2(n.neighborCell.position.x - currentCell.position.x, n.neighborCell.position.z - currentCell.position.z);
                    //Vector2 dir = new Vector2(n.neighborCell.position.x - currentCell.position.x, n.neighborCell.position.z - currentCell.position.z).normalized;
                    if(dir.x != 0 && dir.y != 0)
                    {
                        mult = manager.DiagonalWeight;
                    }
                    n.neighborCell.BestCost = currentCell.BestCost + n.neighborCell.BaseCost * mult;
                    
                    cellsToProcess.Enqueue(n.neighborCell);
                }

            }
        }
    }
    void GenerateDirections()
    {
        foreach (var v in field)
        {
            foreach (FieldCell c in v.Value.Layers)
            {
                if (c == null) { continue; }
                if (DestinationCells.Contains(c))
                {
                    c.SetDirection(Vector3.zero);
                    continue;
                }
                /*if (c == DestinationCell)
                {
                    DestinationCell.SetDirection(Vector3.zero);
                    continue;
                }*/
                /*if(c.closeToObstacle)
                {
                    NavMeshPath path = new NavMeshPath();
                    if (NavMesh.CalculatePath(c.position, new Vector3(manager.Target.position.x, c.position.y, manager.Target.position.z), NavMesh.AllAreas, path))
                    {
                        if (path.corners.Length > 1)
                        {
                            Vector3 navDir = path.corners[1] - c.position;
                            c.SetDirection(navDir.normalized);
                        }
                        else
                        {
                            c.SetDirection(Vector3.zero);
                        }
                    }
                    else
                    {
                        c.SetDirection(Vector3.zero);
                    }
                    continue;
                }*/
                FieldCell lowest = null;
                //Vector3 dirToDestiny = CellDistance(c, DestinationCell).normalized;
                //Vector3 dirToDestiny = CellDistance(c, DestinationCell);
                Vector3 dirToDestiny = GetDistanceToClosestDestinationCell(c);
                dirToDestiny *= 1f / (Mathf.Abs(dirToDestiny.x) + Mathf.Abs(dirToDestiny.z) + 0.0001f);
                float bestDot = float.MinValue;
                Vector3 dirSum = Vector3.zero;
                foreach (FieldCell.NeighborContext n in c.Neighbors)
                {
                    if (n.context == FieldCell.NeighborContext.Context.Upper)
                    {
                        continue;
                    }
                    if (n.neighborCell.BestCost > c.BestCost)
                    {
                        continue;
                    }
                    dirSum += n.neighborDir * (1+c.BestCost - n.neighborCell.BestCost);
                    if (lowest == null || n.neighborCell.BestCost < lowest.BestCost)
                    {
                        lowest = n.neighborCell;
                        bestDot = Vector3.Dot(dirToDestiny, n.neighborDir);
                    }
                    else if (n.neighborCell.BestCost == lowest.BestCost)
                    {
                        //Vector3 dirToLowest = CellDistance(c, lowest).normalized;
                        float dot = Vector3.Dot(dirToDestiny, n.neighborDir);
                        if (dot > bestDot)
                        {
                            lowest = n.neighborCell;
                            bestDot = dot;
                        }
                    }
                }
                if (lowest == null)
                {

                    c.SetDirection(Vector3.zero);
                    continue;
                }
                //c.SetDirection((new Vector3(lowest.position.x - c.position.x, 0, lowest.position.z - c.position.z).normalized + lowest.direction).normalized);
                Vector3 dir = CellDistance(c,lowest).normalized;
                /*Vector3 dist = DestinationCell.position - c.position;
                float dot = Vector3.Dot(dir, dist.normalized);
                if( dot> 0.3f)
                {
                    dir += dist.normalized *Mathf.Clamp(20/dist.magnitude,0,10);
                    dir = dir.normalized;
                }*/
                c.SetDirection((dirSum*manager.NeighborSumDirectionStrenght + dir*manager.BestDirectionStrenght + dirToDestiny*manager.TargetDirectionStrenght).normalized);
            }
        }
    }

    public Vector3 GetDistanceToClosestDestinationCell(FieldCell c)
    {
        Vector3 dir = Vector3.zero, aux;
        float sqrMag = float.MaxValue;
        foreach (FieldCell cell in DestinationCells)
        {
            aux = CellDistance(c, cell);
            if(aux.sqrMagnitude < sqrMag)
            {
                sqrMag = aux.sqrMagnitude;
                dir = aux;
            }
        }
        return dir;
    }

    public static Vector3 CellDistance(FieldCell from, FieldCell to)
    {
        return new Vector3(to.position.x - from.position.x, 0, to.position.z - from.position.z);
    }
    public void ResetCost()
    {
        foreach (var v in field)
        {
            foreach (FieldCell cell in v.Value.Layers)
            {
                if (cell == null) { continue; }
                cell.BestCost = float.MaxValue;
            }
        }
    }
    static readonly Vector2Int[] Directions =
{
    new Vector2Int(1,0),
    new Vector2Int(-1,0),
    new Vector2Int(0,1),
    new Vector2Int(0,-1)
};
    static readonly Vector2Int[] diagonalDirections =
    {
    new Vector2Int(1,1),
    new Vector2Int(-1,-1),
    new Vector2Int(-1,1),
    new Vector2Int(1,-1)
    };
    static readonly Vector2Int[] allDirections =
    {
    new Vector2Int(1,0),
    new Vector2Int(-1,0),
    new Vector2Int(0,1),
    new Vector2Int(0,-1),
    new Vector2Int(1,1),
    new Vector2Int(-1,-1),
    new Vector2Int(-1,1),
    new Vector2Int(1,-1)
    };
}

