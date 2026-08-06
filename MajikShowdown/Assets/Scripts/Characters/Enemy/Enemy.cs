using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;
using Mirror;
using Unity.Jobs;
using UnityEngine.Jobs;
using Unity.Burst;

public class Enemy : CrowdCharacter
{
    public float rotationSpeed = 1;
    [Header("Target Avoidance Options")]
    public LayerMask ObstacleMask;
    public LayerMask EnemyMask;
    public LayerMask CanSeeTargetThrough;
    public float DetectionRadius;
    public float EnemyAvoidanceRadius;
    public float TargetStoppingDistance;
    public float SeparationForce = 1;
    public float FlowfieldActivationDistance = 20;
    public int priority = 1;

    [Header("DropConfig")]
    [Range(0, 100)] public float DropChance;
    public List<RuneLootPool> AvailablePools;
    public ProbabilitySlider<int> PoolProbability = new ProbabilitySlider<int>();

    float size;
    Player target;
    HashSet<Enemy> neighbors = new HashSet<Enemy>();
    Collider[] neighborBuffer = new Collider[32];
    Vector3[] Directions = new Vector3[8];
    float[] Danger = new float[8];
    float[] Interest = new float[8];
    Vector3 targetVector, attackedTargetVector/*, targetLastSeen*/;
    bool detectedObstacle = false, detectedHigherPriority = false;
    Vector3 MoveDirection;
    Vector3 interestDirection;
    Vector3 priorityAvoidDirection;
    bool canSeeTarget;
    FieldCell currentCell, forwardCell;
    HashSet<FieldCell> OccupiedCells = new HashSet<FieldCell>();
    int occupiedCellNum;

    bool attacked = true, onAttackCooldown = false;
    Timer attackTimer = new Timer(false), attackCooldownTimer = new Timer(false);
    public float attackDuration = 0.3f, attackCooldown = 0.5f;
    public float damage = 1;
    public Elements element = Elements.None;
    Damage dmgCtrl;
    [HideInInspector][SyncVar] public int instanceIndex;
    [HideInInspector] public int GameID;
    public EnemyTransformInfo transformInfo;
    Player attackedPlayer;
    float timePred;
    Vector3 predTarget;
    int detectRadius;
    public void Initialize()
    {
        DamageHandler.Initialize(this);
        size = GetComponent<CapsuleCollider>().radius * transform.localScale.x;
        for (int i = 0; i < Directions.Length; i++)
        {
            float angle = i * Mathf.PI * 2f / Directions.Length;
            Directions[i] = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
        }
        //updateRate = 1f / 30f;
        dmgCtrl = new Damage(damage, element);
        //aiCalcTimer.timedEvent.AddListener(AICalculation);
        //aiCalcTimer.SetTimer(0);
        //attackTimer.timedEvent.AddListener(AttackPlayer);
        attackTimer.Paused = true;
        attackCooldownTimer.Paused = true;
        attackTimer.SetTimer(0);
        attackCooldownTimer.SetTimer(0);
        occupiedCellNum = (int)math.ceil(size / FlowFieldManager.instance.CellSize);
        detectRadius = math.max((int)math.ceil(DetectionRadius / FlowFieldManager.instance.CellSize), 1);
        RigidbodySetting();
        //CheckFieldLocation();
    }

    [ClientRpc]
    public void RigidbodySetting()
    {
        rb.isKinematic = !isServer;
    }
    /*void Start()
    {
        size = GetComponent<CapsuleCollider>().radius * transform.localScale.x;
        for (int i = 0; i < Directions.Length; i++)
        {
            float angle = i * Mathf.PI * 2f / Directions.Length;
            Directions[i] = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
        }
        updateRate = 1f / 30f;
        dmgCtrl = new Damage(damage, element);
        attackTimer.timedEvent.AddListener(AttackPlayer);
        attackTimer.Paused = true;
        attackCooldownTimer.Paused = true;
        StartCoroutine(StartAICalc());
        targetLastSeen = targetVector;
    }*/
#if UNITY_EDITOR
    protected override void OnValidate()
    {
        int c = PoolProbability.Entries.Count;
        if (AvailablePools.Count > c)
        {
            PoolProbability.AddEntry(c.ToString(), 1 / (c + 1), c);
        }
        else if (AvailablePools.Count < c)
        {
            PoolProbability.RemoveEntry(c - 1);
        }
    }
#endif

    [ClientRpc]
    public void EnemyClientUpdate()
    {
        if (isServer)
        {
            return;
        }
        if (GameManager.Instance.hordeController == null)
        {
            return;
        }
        if (GameManager.Instance.hordeController.enemiesInfo.Count <= instanceIndex)
        {
            return;
        }
        timePred = Time.time - GameManager.Instance.hordeController.enemiesInfo[instanceIndex].lastTime;
        predTarget = GameManager.Instance.hordeController.enemiesInfo[instanceIndex].pos + GameManager.Instance.hordeController.enemiesInfo[instanceIndex].vel * timePred;
        transform.position = Vector3.Lerp(transform.position, predTarget, Time.deltaTime * 15);
        Vector3 dir = predTarget - transform.position;
        dir.y = 0;
        if (dir.sqrMagnitude >= 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }
    }

    public void EnemyUpdate()
    {
        if (FlowFieldManager.instance == null)
        {
            return;
        }
        EnemyClientUpdate();
        /*if(aiCalcTimer.timer(updateRate, Time.deltaTime, false, true))
        {
            AICalculation();
        }*/
        if (target == null)
        {
            return;
        }
        if (currentCell == null)
        {
            return;
        }

        if (forwardCell != null)
        {
            CheckForJump(currentCell);
        }
        if (attackedPlayer != null)
        {
            attackedTargetVector = attackedPlayer.gameObject.transform.position - transform.position;
        }
        if (!attacked)
        {
            attacked = attackTimer.timer(attackDuration, Time.deltaTime, false, false);
            if (attacked)
            {
                AttackPlayer();
                attackCooldownTimer.SetTimer(0);
                attackCooldownTimer.Paused = false;
                onAttackCooldown = true;
            }
        }
        if (onAttackCooldown)
        {
            onAttackCooldown = attackCooldownTimer.timer(attackCooldown, Time.deltaTime, true, false);
        }
        else
        {
            attackCooldownTimer.Paused = true;
        }
        PathToTarget(currentCell);
        UpdateTransform();
    }

    public void UpdateTransform()
    {
        transformInfo.pos = transform.position;
        transformInfo.rot = (byte)transform.rotation.eulerAngles.y;
        //transformInfo.scale = transform.localScale;
        transformInfo.vel = rb.linearVelocity;
        //transformInfo.vel = worldVelocity - rb.linearVelocity + externalVelocity;
        GameManager.Instance.hordeController.enemiesInfo[instanceIndex] = transformInfo;
    }

    public void AICalculation()
    {
        target = GetClosestPlayer();
        if (target != null)
        {
            targetVector = target.transform.position - transform.position;
            /*if (targetVector.sqrMagnitude > 2500)
            {
                updateRate = 1f / 15f;
            }
            else if (targetVector.sqrMagnitude > 625)
            {
                updateRate = 1f / 20f;
            }
            else if (targetVector.sqrMagnitude > 225)
            {
                updateRate = 1f / 25f;
            }
            else
            {
                updateRate = 1f / 30f;
            }*/
            if (!Physics.Raycast(transform.position, targetVector.normalized, targetVector.magnitude, ~CanSeeTargetThrough))
            {
                //targetLastSeen = targetVector;
                canSeeTarget = true;
                //Debug.Log("cant see");
            }
            else
            {
                canSeeTarget = false;
                //Debug.Log("can see");
            }
            CheckFieldLocation();
            if (currentCell != null)
            {
                forwardCell = FlowFieldManager.instance.WorldToGridPosition(transform.position + currentCell.direction * size);
                if (canSeeTarget && targetVector.magnitude < FlowfieldActivationDistance /*&& Vector3.Dot(targetVector.normalized, currentCell.direction.normalized) > 0.5*/)
                {
                    interestDirection = targetVector.normalized;
                }
                else
                {
                    interestDirection = currentCell.direction;
                }
                FindObstacles();
                CalculateDanger();
                CalculateInterest();
                MoveDirection = GetBestDirection();
            }
        }
    }
    Queue<FieldCell> ocupiedQueue = new Queue<FieldCell>();
    public void CheckFieldLocation()
    {
        currentCell = FlowFieldManager.instance.WorldToGridPosition(transform.position);
        int aux = 1;

        foreach (FieldCell c in OccupiedCells)
        {
            c.ContainedEnemies.Remove(GameID);
        }
        OccupiedCells.Clear();
        if (currentCell == null) return;
        ocupiedQueue.Enqueue(currentCell);
        //OccupiedCells.Add(currentCell);
        //currentCell.ContainedEnemies.Add(GameID);
        while (ocupiedQueue.Count > 0)
        {
            FieldCell c = ocupiedQueue.Dequeue();
            OccupiedCells.Add(c);
            c.ContainedEnemies.Add(GameID);
            if (aux < occupiedCellNum)
            {
                foreach (FieldCell.NeighborContext n in c.Neighbors)
                {
                    if (!OccupiedCells.Contains(n.neighborCell))
                    {
                        ocupiedQueue.Enqueue(n.neighborCell);
                    }
                }
                aux++;
            }
        }
        /*for (int i = 1; i < occupiedCellNum; i++)
        {
            HashSet<FieldCell> tempCells = new HashSet<FieldCell>(OccupiedCells);
            foreach (FieldCell c in tempCells)
            {
                foreach (FieldCell.NeighborContext n in c.Neighbors)
                {
                    OccupiedCells.Add(n.neighborCell);
                    n.neighborCell.ContainedEnemies.Add(GameID);
                }
            }
        }*/
    }

    public Vector3 GetNavMeshDir(FieldCell c)
    {
        NavMeshPath path = new NavMeshPath();
        if (NavMesh.CalculatePath(c.position, new Vector3(target.transform.position.x, c.position.y, target.transform.position.z), NavMesh.AllAreas, path))
        {
            if (path.corners.Length > 1)
            {
                Vector3 navDir = path.corners[1] - c.position;
                return navDir.normalized;
            }
            else
            {
                return Vector3.zero;
            }
        }
        else
        {
            return Vector3.zero;
        }
    }

    public void CheckForJump(FieldCell currentCell)
    {
        bool needsJump = false;
        foreach (FieldCell.NeighborContext n in forwardCell.Neighbors)
        {
            if (n.context == FieldCell.NeighborContext.Context.Jumpable && Vector3.Dot(forwardCell.direction, FlowField.CellDistance(forwardCell, n.neighborCell)) > 0.75f)
            {
                needsJump = true;
                break;
            }
        }
        if (needsJump)
        {
            Jump(true);
        }
    }
    public void PathToTarget(FieldCell currentCell)
    {
        if (targetVector.magnitude <= TargetStoppingDistance || (MoveDirection == Vector3.zero && canSeeTarget))
        {
            if (detectedHigherPriority)
            {
                Move(priorityAvoidDirection.normalized, speed);
            }
            else
            {
                SetVelocity(new Vector3(0, localVelocity.y, 0));
                SetAcceleration(Vector3.zero);
            }

            if (targetVector.magnitude <= TargetStoppingDistance && !onAttackCooldown)
            {
                if (attacked)
                {
                    attacked = false;
                    attackTimer.SetTimer(0);
                    attackedPlayer = target;
                    attackTimer.Paused = false;
                }
            }
            else
            {
                attackTimer.Paused = true;
            }
        }
        else
        {
            attackTimer.Paused = true;
            //Move(MoveDirection, speed);
            if (detectedObstacle)
            {
                Vector3 navDir = GetNavMeshDir(currentCell);
                if (Vector3.Dot(targetVector, navDir) < -0.75f)
                {
                    Move(navDir, speed);
                }
                else
                {
                    Move((MoveDirection + navDir).normalized, speed);
                }
            }
            else
            {
                if (detectedHigherPriority)
                {
                    Move((MoveDirection + priorityAvoidDirection).normalized, speed);
                }
                else
                {

                    Move(MoveDirection, speed);
                }
            }
        }
    }

    void AttackPlayer()
    {
        //if (targetVector.magnitude <= TargetStoppingDistance)
        if (attackedTargetVector.magnitude <= TargetStoppingDistance)
        {
            attackedPlayer.DamageHandler.TakeDamage(dmgCtrl);
        }
    }
    public void CalculateDanger()
    {
        priorityAvoidDirection = Vector3.zero;
        for (int i = 0; i < Danger.Length; i++)
        {
            Danger[i] = 0;
        }
        foreach (Enemy e in neighbors)
        {
            Vector3 toEnemy = e.transform.position - transform.position;
            float distance = toEnemy.magnitude - e.size;
            if (distance < EnemyAvoidanceRadius)
            {
                float strength = Mathf.Pow(2 - (distance / EnemyAvoidanceRadius), 2) - 1;
                for (int i = 0; i < Directions.Length; i++)
                {
                    float dot = Vector3.Dot(toEnemy.normalized, Directions[i]);
                    if (dot > 0)
                    {
                        Danger[i] += strength * dot * SeparationForce * (e.priority / priority);
                    }
                }
                if (e.priority > priority)
                {
                    priorityAvoidDirection -= toEnemy * (e.priority / priority);
                }
            }
        }
        if (detectedObstacle)
        {
            for (int i = 0; i < Directions.Length; i++)
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position, Directions[i], out hit, DetectionRadius, ObstacleMask))
                {
                    //float dot = Mathf.Clamp01(Vector3.Dot(Directions[i], targetVector.normalized));
                    float strength = 1 - (hit.distance / DetectionRadius);
                    Danger[i] += strength;
                }
            }
        }
    }
    public void CalculateInterest()
    {
        if (target != null)
        {
            for (int i = 0; i < Directions.Length; i++)
            {
                Interest[i] = 0.01f;
                float dot = Vector3.Dot(interestDirection.normalized, Directions[i]);
                if (dot > 0)
                {
                    Interest[i] += dot;
                }
            }
        }
    }
    public Vector3 GetBestDirection()
    {
        Vector3 add = Vector3.zero;
        for (int i = 0; i < Directions.Length; i++)
        {
            add += Directions[i] * Mathf.Clamp01(Interest[i] - Danger[i]);
        }
        add.y = 0;
        return add.normalized;
    }
    Queue<FieldCell> cellsToCheck = new Queue<FieldCell>();
    HashSet<FieldCell> checkedCells = new HashSet<FieldCell>();
    public void FindObstacles()
    {
        neighbors.Clear();
        checkedCells.Clear();
        detectedHigherPriority = false;
        detectedObstacle = false;
        int aux = 0;


        //HashSet<FieldCell> cellsToCheck = new HashSet<FieldCell>();
        cellsToCheck.Enqueue(currentCell);
        while (cellsToCheck.Count > 0)
        {
            FieldCell c = cellsToCheck.Dequeue();
            checkedCells.Add(c);
            foreach (int eID in c.ContainedEnemies)
            {
                Enemy e = GameManager.Instance.hordeController.GameEnemies[eID];
                if (e != this && e.priority >= priority)
                {
                    if (e.priority > priority)
                    {
                        detectedHigherPriority = true;
                    }
                    neighbors.Add(e);
                }
            }
            if (aux < detectRadius)
            {
                foreach (FieldCell.NeighborContext n in c.Neighbors)
                {
                    if (!checkedCells.Contains(n.neighborCell) && !cellsToCheck.Contains(n.neighborCell))
                    {
                        cellsToCheck.Enqueue(n.neighborCell);
                    }
                }
                aux++;
            }
        }
        /*for (int i = 0; i < detectRadius; i++)
        {
            HashSet<FieldCell> tempCells = new HashSet<FieldCell>(cellsToCheck);
            foreach (FieldCell c in tempCells)
            {
                foreach (FieldCell.NeighborContext n in c.Neighbors)
                {
                    cellsToCheck.Add(n.neighborCell);
                }
            }
        }

        foreach (FieldCell c in cellsToCheck)
        {
            foreach (int eID in c.ContainedEnemies)
            {
                Enemy e = GameManager.Instance.hordeController.GameEnemies[eID];
                if (e != this && e.priority >= priority)
                {
                    if (e.priority > priority)
                    {
                        detectedHigherPriority = true;
                    }
                    neighbors.Add(e);
                }
            }
        }*/
        /*int count = Physics.OverlapSphereNonAlloc(transform.position, DetectionRadius, neighborBuffer, ObstacleMask | EnemyMask);
        for (int i = 0; i < count; i++)
        {
            if (neighborBuffer[i].TryGetComponent(out Enemy e))
            {
                if (e != this && e.priority >= priority)
                {
                    if (e.priority > priority)
                    {
                        detectedHigherPriority = true;
                    }
                    neighbors.Add(e);
                }
            }
            else if (((1 << neighborBuffer[i].gameObject.layer) & ObstacleMask) != 0)
            {
                detectedObstacle = true;
            }
        }*/
    }
    public Player GetClosestPlayer()
    {
        Player closest = null;
        float closestDistance = Mathf.Infinity;
        foreach (Player p in GameManager.Instance.Players)
        {
            if (!p.dead)
            {
                float distance = Vector3.Distance(transform.position, p.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = p;
                }
            }
        }
        return closest;
    }
    protected override void HandleRotation()
    {
        //Vector3 lookDir = new Vector3(transform.position.x +interestDirection.normalized.x, transform.position.y, interestDirection.normalized.z + transform.position.z);
        //transform.LookAt(lookDir);
        //transform.Rotate(Vector3.up, Vector3.SignedAngle(transform.forward, new Vector3(interestDirection.x, 0, interestDirection.z), Vector3.up) * Time.fixedDeltaTime * rotationSpeed);
        if (Time.frameCount % 2 == 0)
        {
            Vector3 dir = interestDirection;
            dir.y = 0;

            if (dir.sqrMagnitude < 0.001f)
                return;

            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime);
        }

    }
    public override void Die()
    {
        foreach (FieldCell c in OccupiedCells)
        {
            c.ContainedEnemies.Remove(GameID);
        }
        OccupiedCells.Clear();
        base.Die();
    }
}
[BurstCompile]
public struct AvoidanceCalculation : IJobParallelFor
{
    public NativeArray<float3> PlayerPositions;
    public NativeArray<int> TargetIndices;
    public NativeArray<float3> Directions;
    public float CellSize;
    public NativeArray<CellJobData> Cells;


    NativeArray<float> Interest;
    NativeArray<float> Danger;
    NativeHashSet<int> checkedCells;
    NativeQueue<int> cellsToCheck;
    bool detectedHigherPriority;

    public NativeArray<EnemyJobData> EnemyData;
    public void Execute(int index)
    {
        DefineTarget(index);
        CheckFieldLocation(index);
        FindObstacles(index);
        CalculateDanger(index);
        CalculateInterest(index);
        GetBestDirection();
    }
    public void DefineTarget(int index)
    {
        int closestPlayerIndex = -1;
        float closestDistance = float.MaxValue;
        for (int i = 0; i < PlayerPositions.Length; i++)
        {
            float distance = math.distance(EnemyData[index].Position, PlayerPositions[i]);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPlayerIndex = i;
            }
        }
        TargetIndices[index] = closestPlayerIndex;
    }
    public void CheckFieldLocation(int index)
    {
        
    }
    public void FindObstacles(int index)
    {
        //EnemyData[index].Neighbors.Dispose();
        checkedCells.Clear();
        detectedHigherPriority = false;
        //detectedObstacle = false;
        int detectRadius = math.max((int)math.ceil(EnemyData[index].DetectionRadius / CellSize), 1);
        int aux = 0;


        //HashSet<FieldCell> cellsToCheck = new HashSet<FieldCell>();
        cellsToCheck.Enqueue(EnemyData[index].CurrentCell);
        while (cellsToCheck.Count > 0)
        {
            int cInd = cellsToCheck.Dequeue();
            if (checkedCells.Contains(cInd))
            {
                continue;
            }
            checkedCells.Add(cInd);
            foreach (int eID in Cells[cInd].ContainedEnemies)
            {
                EnemyJobData e = EnemyData[eID];
                if (eID != index && e.Priority >= EnemyData[index].Priority)
                {
                    if (e.Priority > EnemyData[index].Priority)
                    {
                        detectedHigherPriority = true;
                    }
                    EnemyData[index].Neighbors.Add(eID);
                }
            }
            if (aux < detectRadius)
            {
                foreach (int n in Cells[cInd].neighbors)
                {
                    if (!checkedCells.Contains(n))
                    {
                        cellsToCheck.Enqueue(n);
                    }
                }
                aux++;
            }
        }
    }
    public void CalculateDanger(int index)
    {
        float3 priorityAvoidDirection = float3.zero;
        for (int i = 0; i < Danger.Length; i++)
        {
            Danger[i] = 0;
        }
        foreach (int eID in EnemyData[index].Neighbors)
        {
            EnemyJobData e = EnemyData[eID];
            float3 toEnemy = e.Position - EnemyData[index].Position;
            float distance = math.distance(toEnemy, float3.zero) - e.Size;
            if (distance < EnemyData[index].EnemyAvoidanceRadius)
            {
                float strength = Mathf.Pow(2 - (distance / EnemyData[index].EnemyAvoidanceRadius), 2) - 1;
                for (int i = 0; i < Directions.Length; i++)
                {
                    float dot = math.dot(math.normalize(toEnemy), Directions[i]);
                    if (dot > 0)
                    {
                        Danger[i] += strength * dot * EnemyData[index].SeparationForce * (e.Priority / EnemyData[index].Priority);
                    }
                }
                if (e.Priority > EnemyData[index].Priority)
                {
                    priorityAvoidDirection -= toEnemy * (e.Priority / EnemyData[index].Priority);
                }
            }
        }
        /*if (detectedObstacle)
        {
            for (int i = 0; i < Directions.Length; i++)
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position, Directions[i], out hit, DetectionRadius, ObstacleMask))
                {
                    //float dot = Mathf.Clamp01(Vector3.Dot(Directions[i], targetVector.normalized));
                    float strength = 1 - (hit.distance / DetectionRadius);
                    Danger[i] += strength;
                }
            }
        }*/
    }
    public void CalculateInterest(int index)
    {
        for (int i = 0; i < Directions.Length; i++)
        {
            Interest[i] = 0.01f;
            float dot = math.dot(Cells[EnemyData[index].CurrentCell].Direction, Directions[i]);
            if (dot > 0)
            {
                Interest[i] += dot;
            }
        }
    }
    public float3 GetBestDirection()
    {
        float3 add = float3.zero;
        for (int i = 0; i < Directions.Length; i++)
        {
            add += Directions[i] * math.clamp(Interest[i] - Danger[i], 0, 1);
        }
        add.y = 0;
        return math.normalize(add);
    }
}
public struct EnemyJobData
{
    //Imutable
    public float Size;
    public float EnemyAvoidanceRadius;
    public float SeparationForce;
    public int Priority;
    public float DetectionRadius;
    public float TargetStoppingDistance;

    //Prompted
    public float3 Position;
    public float3 Velocity;

    //Calculated
    public int CurrentCell;
    public NativeList<int> Neighbors;

}
public struct CellJobData
{
    public float3 Position;
    public float3 Direction;
    public NativeArray<int> neighbors;
    public int ContainedEnemiesCount;
    public NativeArray<int> ContainedEnemies;
}

