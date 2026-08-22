using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;
using Mirror;
using Unity.Jobs;
//using UnityEngine.Jobs;
using Unity.Burst;
using System;
using Unity.Collections.LowLevel.Unsafe;
using System.Threading;

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
    [NonSerialized]public float FlowfieldActivationDistance = 20;
    public int priority = 1;

    [Header("DropConfig")]
    [Range(0, 100)] public float DropChance;
    public List<RuneLootPool> AvailablePools;
    public ProbabilitySlider<int> PoolProbability = new ProbabilitySlider<int>();

    [HideInInspector] public float size;
    public Player target;
    HashSet<Enemy> neighbors = new HashSet<Enemy>();
    //Collider[] neighborBuffer = new Collider[32];
    //Vector3[] Directions = new Vector3[8];
    float[] Danger = new float[8];
    float[] Interest = new float[8];
    [NonSerialized]public  Vector3 targetVector, attackedTargetVector/*, targetLastSeen*/;
    bool detectedObstacle = false, detectedHigherPriority = false;
    [NonSerialized] public Vector3 MoveDirection;
    [NonSerialized] public Vector3 interestDirection;
    Vector3 priorityAvoidDirection;
    public bool canSeeTarget;
    [NonSerialized] public FieldCell currentCell, forwardCell;
    HashSet<FieldCell> OccupiedCells = new HashSet<FieldCell>();
    [NonSerialized] public int occupiedCellNum;

    bool attacked = true, onAttackCooldown = false;
    Timer attackTimer = new Timer(false), attackCooldownTimer = new Timer(false);
    public float attackDuration = 0.3f, attackCooldown = 0.5f;
    public float damage = 1;
    public Elements element = Elements.None;
    Damage dmgCtrl;
    [HideInInspector][SyncVar] public int instanceIndex;
    [HideInInspector][SyncVar] public IdWrapper ActiveID;
    [Serializable]
    public struct IdWrapper
    {
        public int ID;
    }
    public EnemyTransformInfo transformInfo;
    Player attackedPlayer;
    float timePred;
    Vector3 predTarget;
    int detectRadius;
    public float maxDistanceFromPlayer = 100, repositionRange = 20;

    public Animator animator;
    bool prevMoving = false, moving = false;
    public enum EnemyAnimState : byte { None, Attack, Jump, Land };
    [SyncVar (hook = "OnAnimStateChange")] public EnemyAnimState animState;

    public void Initialize()
    {
        currentCell = FlowFieldManager.instance.flowField.allCells[0];
        DamageHandler.Initialize(this);
        size = GetComponent<CapsuleCollider>().radius * transform.localScale.x;
        /*for (int i = 0; i < Directions.Length; i++)
        {
            float angle = i * Mathf.PI * 2f / Directions.Length;
            Directions[i] = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
        }*/
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
    public void UpdateIdWrapper(int value)
    {
        IdWrapper aux = ActiveID;
        aux.ID = value;
        ActiveID = aux;
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
        if (dir.sqrMagnitude >= 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }
        prevMoving = moving;
        moving = rb.linearVelocity != Vector3.zero;
        if (prevMoving != moving)
        {
            animator.SetBool("Moving", moving);
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
        prevMoving = moving;
        moving = rb.linearVelocity != Vector3.zero;
        if(prevMoving != moving)
        {
            animator.SetBool("Moving", moving);
        }
        UpdateTransform();
    }

    public void OnAnimStateChange(EnemyAnimState oldVal, EnemyAnimState newVal)
    {
        switch(newVal)
        {
            case EnemyAnimState.Attack:
                animator.ResetTrigger("Attack");
                animator.SetTrigger("Attack");
                break;
            case EnemyAnimState.Jump:
                animator.ResetTrigger("Jump");
                animator.SetTrigger("Jump");
                break;
            case EnemyAnimState.Land:
                animator.ResetTrigger("Land");
                animator.SetTrigger("Land");
                break;
        }
    }

    public void PlayAnimation(EnemyAnimState state)
    {
        animState = state;
        animState = EnemyAnimState.None;
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
            if (targetVector.sqrMagnitude > maxDistanceFromPlayer * maxDistanceFromPlayer)
            {
                Vector3 reposition = CheckReposition();
                if (reposition != Vector3.zero)
                {
                    rb.interpolation = RigidbodyInterpolation.None;
                    rb.position = reposition;
                    transform.position = reposition;
                    ResetAllVelocities();
                    rb.interpolation = RigidbodyInterpolation.Interpolate;
                }
            }
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

                /*FindObstacles();
                CalculateDanger();
                CalculateInterest();
                MoveDirection = GetBestDirection();*/
            }
        }
    }
    Vector3 CheckReposition()
    {
        RaycastHit hit;
        bool canReposition = false;
        Vector3 repos = target.gameObject.transform.position + targetVector.normalized * repositionRange;
        if (Physics.Raycast(repos + Vector3.up * GameManager.Instance.hordeController.heightCheckPoint, Vector3.down, out hit, GameManager.Instance.hordeController.checkHeight, GameManager.Instance.hordeController.spawnableLocations))
        {
            Collider[] obstacles = Physics.OverlapSphere(repos, GameManager.Instance.hordeController.maxEnemySpawnRadius, ~GameManager.Instance.hordeController.spawnableLocations);
            if (obstacles.Length <= 0)
            {
                canReposition = true;
            }
        }
        if (canReposition)
        {
            return hit.point + Vector3.up * GameManager.Instance.hordeController.spawnerHeight;
        }
        else
        {
            return Vector3.zero;
        }
    }
    Queue<FieldCell> ocupiedQueue = new Queue<FieldCell>();
    public void CheckFieldLocation()
    {
        FieldCell temp = FlowFieldManager.instance.WorldToGridPosition(transform.position);
        if (temp != null)
        {
            currentCell = temp;
        }
        int aux = 1;

        foreach (FieldCell c in OccupiedCells)
        {
            c.ContainedEnemies.Remove(ActiveID);
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
            c.ContainedEnemies.Add(ActiveID);
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

    protected override void Jump(bool pressed)
    {
        if (pressed && !movePaused)
        {
            if ((CvState == CharVerticalState.grounded || canJumpOnAir) && !jumpOnCooldown)
            {
                jumpTimer.SetTimer(0);
                jumpTimer.Paused = false;
                CvState = CharVerticalState.jumping;
                if(isServer)
                {
                    PlayAnimation(EnemyAnimState.Jump);
                }
                InvokeIfAllowed(Jumped);
                StartCoroutine(JumpCooldown());
            }
        }
        else if (CvState == CharVerticalState.jumping)
        {
            jumpTimer.SetTimer(jumpTime);
        }
    }

    protected override void Gravity()
    {
        RaycastGround();
        float groundDistance = LastHitInfo.distance - (height / 2);

        if (!gravityPaused)
        {
            if (LastHitInfo.collider == null || (normalDot > 0.9f && groundDistance > 0.05f && !IgnoreSlope))
            {
                verticalVelocity += Vector3.up * Physics.gravity.y * gravityMultiplier * Time.fixedDeltaTime;
            }
            else if (groundDistance <= terrainBuffer && vState == VerticalState.grounded)
            {
                verticalVelocity.y = 0;
            }
        }
        else
        {
            verticalVelocity.y = 0;
        }

        if (LastHitInfo.collider != null)
        {
            if (vState == VerticalState.falling)
            {
                vState = VerticalState.grounded;
                InvokeIfAllowed(HitGround);
                if(isServer)
                {
                    PlayAnimation(EnemyAnimState.Land);
                }
            }
            else
            {
                vState = VerticalState.grounded;
            }
        }
        else
        {
            if (vState == VerticalState.grounded)
            {
                vState = VerticalState.falling;
                InvokeIfAllowed(Fell);
            }
            else
            {
                vState = VerticalState.falling;
            }
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
                    PlayAnimation(EnemyAnimState.Attack);
                }
            }
            /*else
            {
                attacked = true;
                attackTimer.Paused = true;
            }*/
        }
        else
        {
            //attackTimer.Paused = true;
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
                for (int i = 0; i < GameManager.Instance.hordeController.Directions.Length; i++)
                {
                    float dot = Vector3.Dot(toEnemy.normalized, GameManager.Instance.hordeController.Directions[i]);
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
            for (int i = 0; i < GameManager.Instance.hordeController.Directions.Length; i++)
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position, GameManager.Instance.hordeController.Directions[i], out hit, DetectionRadius, ObstacleMask))
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
            for (int i = 0; i < GameManager.Instance.hordeController.Directions.Length; i++)
            {
                Interest[i] = 0.01f;
                float dot = Vector3.Dot(interestDirection.normalized, GameManager.Instance.hordeController.Directions[i]);
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

        for (int i = 0; i < GameManager.Instance.hordeController.Directions.Length; i++)
        {
            add += (Vector3)GameManager.Instance.hordeController.Directions[i] * Mathf.Clamp01(Interest[i] - Danger[i]);
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
            foreach (IdWrapper eID in c.ContainedEnemies)
            {
                Enemy e = GameManager.Instance.hordeController.GameEnemies[eID.ID];
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
            c.ContainedEnemies.Remove(ActiveID);
        }
        OccupiedCells.Clear();
        base.Die();
    }
}
[BurstCompile]
public unsafe struct EnemyFieldLocation : IJobParallelFor
{
    //prompted
    [Unity.Collections.ReadOnly] public NativeArray<float3> PlayerPositions;
    //[Unity.Collections.ReadOnly] public NativeArray<float3> EnemyPositions;
    [Unity.Collections.ReadOnly] public NativeArray<int> CellNeighbors;
    public int maxEnemiesPerCell;
    public int maxEnemyOccupiedCells;
    public NativeArray<CellJobData> Cells;

    //Prompted && output
    public NativeArray<EnemyJobData> EnemyData;

    //Output
    public NativeArray<int> TargetIndices;
    [NativeDisableParallelForRestriction] public NativeArray<int> enemiesInField;
    [NativeDisableParallelForRestriction] public NativeArray<int> enemyOcupiedCells;
    [NativeDisableParallelForRestriction] public NativeArray<int> cellEnemiesNum;
    //public NativeArray<float3> interestDirections;

    //calculated
    [NativeDisableParallelForRestriction] public NativeArray<int> OccupiedCellsToCheck;
    public void Execute(int index)
    {
        //EnemyData[index].CurrentCell = FlowFieldManager.instance.WorldToGridPosition(EnemyData[index].Position).ID;
        DefineTarget(index);
        CheckFieldLocation(index);
        CalculateTargetVectors(index);
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
    public int WorldToGridPosition(float3 worldPosition)
    {
        int closest = -1;
        float closestDist = float.MaxValue;
        for (int i = 0; i < Cells.Length; i++)
        {
            float newDistance = math.distance(worldPosition, Cells[i].Position);
            if (newDistance < closestDist)
            {
                closestDist = newDistance;
                closest = i;
            }
        }
        return closest;
    }
    public void CalculateTargetVectors(int index)
    {
        EnemyJobData e = EnemyData[index];
        e.targetVector = PlayerPositions[TargetIndices[index]] - e.Position;
        EnemyData[index] = e;
    }
    public void CheckFieldLocation(int index)
    {
        int currentCell = WorldToGridPosition(EnemyData[index].Position);
        int depth = 1;
        int occupiedCellNum = 0;
        int queueCount = 0;
        int processedCount = 0;
        bool reachedLimit = false;
        //int checkedAmount = 0;

        /*foreach (FieldCell c in OccupiedCells)
        {
            c.ContainedEnemies.Remove(GameID);
        }
        OccupiedCells.Clear();*/

        if (currentCell == -1) return;
        EnemyJobData EJD = EnemyData[index];
        EJD.CurrentCell = currentCell;
        int fC = WorldToGridPosition(EJD.Position + Cells[currentCell].Direction * EJD.Size);
        if(fC > -1)
        {
            EJD.fowardCell = fC;
        }
        EnemyData[index] = EJD;

        int OccupiedCellsOffset = maxEnemyOccupiedCells * index;
        //ocupiedQueue.Enqueue(currentCell);
        OccupiedCellsToCheck[OccupiedCellsOffset + queueCount] = currentCell;
        queueCount++;

        //OccupiedCells.Add(currentCell);
        /*enemyOcupiedCells[OccupiedCellsOffset + occupiedCellNum] = currentCell;
        //currentCell.ContainedEnemies.Add(GameID);
        enemiesInField[(currentCell * maxEnemiesPerCell) + Cells[currentCell].EnemiesNum] = index;
        aux++;
        occupiedCellNum++;*/
        while (queueCount > processedCount && depth <= EnemyData[index].occupiedCellDepth)
        {

            int nodesThisDepth = queueCount - processedCount;
            for (int i = 0; i < nodesThisDepth; i++)
            {
                if (reachedLimit)
                {
                    break;
                }
                int CellIndex = OccupiedCellsToCheck[OccupiedCellsOffset + processedCount];

                int* cellCounters = (int*)NativeArrayUnsafeUtility.GetUnsafePtr(cellEnemiesNum);

                int* counter = cellCounters + CellIndex;

                int enemyNumSlot = -1;
                bool calculateSlot = true;

                while (calculateSlot)
                {
                    int current = cellEnemiesNum[CellIndex];

                    if (current >= maxEnemiesPerCell)
                    {
                        enemyNumSlot = -1;
                        calculateSlot = false;
                    }
                    else if (Interlocked.CompareExchange(ref *counter,current + 1,current) == current)
                    {
                        enemyNumSlot = current;
                        calculateSlot = false;
                    }
                }


                if (enemyNumSlot > -1)
                {
                    int enemyInFieldOffset = CellIndex * maxEnemiesPerCell;
                    enemyOcupiedCells[OccupiedCellsOffset + occupiedCellNum] = CellIndex;
                    enemiesInField[enemyInFieldOffset + enemyNumSlot] = index;
                    //enemyNumSlot++;
                    occupiedCellNum++;
                    if (occupiedCellNum >= maxEnemyOccupiedCells)
                    {
                        reachedLimit = true;
                    }
                }
                processedCount++;

                if (depth < EnemyData[index].occupiedCellDepth)
                {
                    for (int j = Cells[CellIndex].firstNeighbor; j <= Cells[CellIndex].lastNeighbor; j++)
                    {
                        int neighborID = CellNeighbors[j];
                        bool alreadyChecked = false;
                        for (int k = OccupiedCellsOffset; k < OccupiedCellsOffset + queueCount; k++)
                        {
                            if (OccupiedCellsToCheck[k] == neighborID)
                            {
                                alreadyChecked = true;
                            }
                        }
                        if (!alreadyChecked)
                        {
                            OccupiedCellsToCheck[OccupiedCellsOffset + queueCount] = neighborID;
                            queueCount++;
                        }

                    }

                }
            }
            depth++;

        }
    }
}
[BurstCompile]
public struct AvoidanceCalculation : IJobParallelFor
{
    //prompted

    [Unity.Collections.ReadOnly] public NativeArray<float3> Directions;
    public float CellSize;
    [Unity.Collections.ReadOnly] public NativeArray<CellJobData> Cells;
    [Unity.Collections.ReadOnly] public NativeArray<EnemyJobData> EnemyData;

    public int MaxCellsChecked;
    public int MaxEnemyNeighbors;
    public int maxEnemiesPerCell;
    [Unity.Collections.ReadOnly] public NativeArray<int> CellNeighbors;
    [Unity.Collections.ReadOnly] public NativeArray<int> enemiesInField;
    public NativeArray<int> cellEnemiesNum;

    //calculated
    [NativeDisableParallelForRestriction] public NativeArray<int> EnemyNeighbors;
    public NativeArray<int> EnemyNeighborCounts;
    [NativeDisableParallelForRestriction] public NativeArray<int> cellsToCheck;
    [NativeDisableParallelForRestriction] public NativeArray<float> enemiesInterest;
    [NativeDisableParallelForRestriction] public NativeArray<float> enemiesDanger;

    //Output
    public NativeArray<float3> DirectionsOutput;



    public void Execute(int index)
    {
        //DefineTarget(index);
        //CheckFieldLocation(index);


        FindObstacles(index);
        //CalculateDanger(index);
        DirectionsOutput[index] = CalculateInterest(index);
        //GetBestDirection();
    }

    public bool FindObstacles(int index)
    {
        int offset = index * MaxCellsChecked;
        int eOffset = index * MaxEnemyNeighbors;
        int queueCount = 0;
        int processedCount = 0;
        //NativeHashSet<int> checkedCells = new NativeHashSet<int>();
        //NativeQueue<int> cellsToCheck = new NativeQueue<int>(Allocator.Temp);
        bool detectedHigherPriority = false;
        //detectedObstacle = false;
        int detectRadius = math.max((int)math.ceil(EnemyData[index].DetectionRadius / CellSize), 1);
        int Depth = 0;
        bool ReachedLimit = false;


        //HashSet<FieldCell> cellsToCheck = new HashSet<FieldCell>()
        int startCell = EnemyData[index].CurrentCell;

        //checkedCells[queueCount + offset] = startCell;
        cellsToCheck[queueCount + offset] = startCell;
        queueCount++;

        while (queueCount > processedCount && Depth <= detectRadius /*&& queueCount <= MaxCellsChecked*/)
        {
            int nodesThisDepth = queueCount - processedCount;
            for (int i = 0; i < nodesThisDepth; i++)
            {
                if (ReachedLimit)
                {
                    break;
                }
                int cInd = cellsToCheck[processedCount + offset];
                processedCount++;
                int EnemiesInFieldOffset = maxEnemiesPerCell * cInd;
                //Check enemies in cell
                for (int j = EnemiesInFieldOffset; j < EnemiesInFieldOffset + cellEnemiesNum[cInd]; j++)
                {
                    int eID = enemiesInField[j];
                    EnemyJobData e = EnemyData[eID];
                    //EnemyJobData currentEnemy = EnemyData[index];
                    if (eID != index && e.Priority >= EnemyData[index].Priority)
                    {
                        if (EnemyNeighborCounts[index] >= MaxEnemyNeighbors)
                        {
                            ReachedLimit = true;
                            break;
                        }
                        if (e.Priority > EnemyData[index].Priority)
                        {
                            detectedHigherPriority = true;
                        }
                        EnemyNeighbors[EnemyNeighborCounts[index] + eOffset] = eID;
                        /*if (currentEnemy.neighborNum == 0)
                        {
                            currentEnemy.firstNeighbor = offset;
                        }*/
                        EnemyNeighborCounts[index]++;
                        //EnemyData[index].Neighbors.Add(eID);
                    }
                }
                /*foreach (int eID in Cells[cInd].ContainedEnemies)
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
                }*/
                if (Depth == detectRadius)
                {
                    continue;
                }
                for (int j = Cells[cInd].firstNeighbor; j <= Cells[cInd].lastNeighbor; j++)
                {
                    int neighborID = CellNeighbors[j];

                    bool alreadyChecked = false;
                    for (int k = 0; k < queueCount; k++)
                    {
                        if (cellsToCheck[offset + k] == neighborID)
                        {
                            alreadyChecked = true;
                            break;
                        }
                    }
                    if (!alreadyChecked)
                    {
                        if (queueCount >= MaxCellsChecked)
                        {
                            ReachedLimit = true;
                            break;
                        }
                        //checkedCells[queueCount + offset] = neighborID;
                        cellsToCheck[queueCount + offset] = neighborID;
                        queueCount++;
                    }
                }
            }
            Depth++;
        }
        //checkedCells.Dispose();
        //cellsToCheck.Dispose();
        return detectedHigherPriority;
    }
    /*public void CalculateDanger(int index)
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
    }*/
    public float3 CalculateInterest(int index)
    {
        int InterestOffset = Directions.Length * index;
        //NativeArray<float> Interest = new NativeArray<float>(Directions.Length, Allocator.Temp);
        //NativeArray<float> Danger = new NativeArray<float>(Directions.Length, Allocator.Temp);
        int eOffset = index * MaxEnemyNeighbors;
        //Danger Calculation
        float3 priorityAvoidDirection = float3.zero;
        for (int i = 0; i < Directions.Length; i++)
        {
            enemiesDanger[i + InterestOffset] = 0;
        }
        for (int i = eOffset; i < eOffset + EnemyNeighborCounts[index]; i++)
        {
            int eID = EnemyNeighbors[i];
            EnemyJobData e = EnemyData[eID];
            float3 toEnemy = e.Position - EnemyData[index].Position;
            float distance = math.distance(toEnemy, float3.zero) - e.Size;
            if (distance < EnemyData[index].EnemyAvoidanceRadius && distance != 0)
            {
                float strength = Mathf.Pow(2 - (distance / EnemyData[index].EnemyAvoidanceRadius), 2) - 1;
                for (int j = 0; j < Directions.Length; j++)
                {
                    float dot = math.dot(math.normalize(toEnemy), Directions[j]);
                    if (dot > 0)
                    {
                        enemiesDanger[j + InterestOffset] += strength * dot * EnemyData[index].SeparationForce * (e.Priority / EnemyData[index].Priority);
                    }
                }
                if (e.Priority > EnemyData[index].Priority)
                {
                    priorityAvoidDirection -= toEnemy * (e.Priority / EnemyData[index].Priority);
                }
            }
        }
        /*foreach (int eID in EnemyData[index].Neighbors)
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
        }*/

        //Interest Calculation
        for (int i = 0; i < Directions.Length; i++)
        {
            enemiesInterest[i + InterestOffset] = 0.01f;
            //float dot = math.dot(Cells[EnemyData[index].CurrentCell].Direction, Directions[i]);
            float dot = math.dot(EnemyData[index].interestDirection, Directions[i]);
            if (dot > 0)
            {
                enemiesInterest[i + InterestOffset] += dot;
            }
        }

        //Get Best Direction
        float3 add = float3.zero;
        for (int i = 0; i < Directions.Length; i++)
        {
            add += Directions[i] * math.clamp(enemiesInterest[i + InterestOffset] - enemiesDanger[i + InterestOffset], 0, 1);
        }
        add.y = 0;
        //Interest.Dispose();
        //Danger.Dispose();
        if (add.x == 0 && add.z == 0)
        {
            return float3.zero;
        }
        return math.normalize(add);
    }
    /*public float3 GetBestDirection()
    {
        float3 add = float3.zero;
        for (int i = 0; i < Directions.Length; i++)
        {
            add += Directions[i] * math.clamp(Interest[i] - Danger[i], 0, 1);
        }
        add.y = 0;
        return math.normalize(add);
    }*/
}
public struct EnemyJobData
{
    //Imutable
    public float Size;
    public float EnemyAvoidanceRadius;
    public float SeparationForce;
    public int Priority;
    public float DetectionRadius;
    public int occupiedCellDepth;
    public float activationDistance;
    //public float TargetStoppingDistance;

    //Prompted
    public float3 Position;
    //public float3 Velocity;
    public int CurrentCell;
    public int fowardCell;
    public float3 interestDirection;
    public float3 targetVector;
    public bool canSeePlayer;

}
public struct CellJobData
{
    public float3 Position;
    public float3 Direction;

    //public int EnemiesNum;
    //public int firstEnemy;
    public int firstNeighbor, lastNeighbor;
    //public int ID;
}

