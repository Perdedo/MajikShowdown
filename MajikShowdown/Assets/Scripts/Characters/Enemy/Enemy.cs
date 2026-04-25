using System.Collections.Generic;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : Character
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
    float size;
    Player target;
    List<Enemy> neighbors = new List<Enemy>();
    Collider[] neighborBuffer = new Collider[32];
    Vector3[] Directions = new Vector3[8];
    float[] Danger = new float[8];
    float[] Interest = new float[8];
    Vector3 targetVector/*, targetLastSeen*/;
    bool detectedObstacle = false, detectedHigherPriority = false;
    Vector3 MoveDirection;
    Vector3 interestDirection;
    Vector3 priorityAvoidDirection;
    bool canSeeTarget;
    float updateRate;
    FieldCell currentCell, forwardCell;

    bool attacked = true, onAttackCooldown = false;
    Timer attackTimer = new Timer(), attackCooldownTimer = new Timer();
    public float attackDuration = 0.3f, attackCooldown = 0.5f;
    public float damage = 1;
    public Elements element = Elements.None;
    Damage dmgCtrl;
    void Start()
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
        StartCoroutine(AICalculation());
        //targetLastSeen = targetVector;
    }
    void Update()
    {
        //target = GetClosestPlayer();
        if(target == null)
        {
            return;
        }
        if(currentCell == null)
        {
            return;
        }
        /*targetVector = target.transform.position - transform.position;
        if(targetVector.sqrMagnitude > 2500)
        {
            updateRate = 1f / 15f;
        }
        else if(targetVector.sqrMagnitude > 625)
        {
            updateRate = 1f / 20f;
        }
        else if(targetVector.sqrMagnitude > 225)
        {
            updateRate = 1f / 25f;
        }
        else
        {
            updateRate = 1f / 30f;
        }*/
        /*if (!Physics.Raycast(transform.position, targetVector.normalized, targetVector.magnitude, ~CanSeeTargetThrough))
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
        FieldCell currentCell = FlowFieldManager.instance.WorldToGridPosition(transform.position);
        if (currentCell == null)
        {
            return;
        }
        if (canSeeTarget && targetVector.magnitude < FlowfieldActivationDistance && Vector3.Dot(targetVector.normalized, currentCell.direction.normalized) > 0.5)
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
        MoveDirection = GetBestDirection();*/
        if(forwardCell != null)
        {
            CheckForJump(currentCell);
        }
        //Debug.DrawRay(transform.position, targetVector, Color.red);
        //Debug.DrawRay(transform.position, targetLastSeen, Color.blue);
        if(!attacked)
        {
            attacked = attackTimer.timer(attackDuration, Time.deltaTime, false, false);
            if(attacked)
            {
                attackCooldownTimer.SetTimer(0);
                attackCooldownTimer.Paused = false;
                onAttackCooldown = true;
            }
        }
        if(onAttackCooldown)
        {
            onAttackCooldown = attackCooldownTimer.timer(attackCooldown, Time.deltaTime, true, false);
        }
        else
        {
            attackCooldownTimer.Paused = true;
        }
        PathToTarget(currentCell);
    }

    IEnumerator AICalculation()
    {
        target = GetClosestPlayer();
        if (target != null)
        {
            targetVector = target.transform.position - transform.position;
            if (targetVector.sqrMagnitude > 2500)
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
            currentCell = FlowFieldManager.instance.WorldToGridPosition(transform.position);
            forwardCell = FlowFieldManager.instance.WorldToGridPosition(transform.position + currentCell.direction * size);
            if (currentCell != null)
            {
                if (canSeeTarget && targetVector.magnitude < FlowfieldActivationDistance && Vector3.Dot(targetVector.normalized, currentCell.direction.normalized) > 0.5)
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
        yield return new WaitForSeconds(updateRate);
        StartCoroutine(AICalculation());
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
        foreach(FieldCell.NeighborContext n in forwardCell.Neighbors)
        {
            if(n.context == FieldCell.NeighborContext.Context.Jumpable && Vector3.Dot(forwardCell.direction, FlowField.CellDistance(forwardCell, n.neighborCell)) > 0.75f)
            {
                needsJump = true; 
                break;
            }
        }
        if(needsJump)
        {
            Jump(true);
        }
    }
    public void PathToTarget(FieldCell currentCell)
    {
        if (targetVector.magnitude <= TargetStoppingDistance || (MoveDirection == Vector3.zero && canSeeTarget))
        {
            if(detectedHigherPriority)
            {
                Move(priorityAvoidDirection.normalized, speed);
            }
            else
            {
                SetVelocity(new Vector3(0, localVelocity.y, 0));
                SetAcceleration(Vector3.zero);
            }

            if(targetVector.magnitude <= TargetStoppingDistance && !onAttackCooldown)
            {
                if(attacked)
                {
                    attacked = false;
                    attackTimer.SetTimer(0);
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
            if(detectedObstacle)
            {
                Vector3 navDir = GetNavMeshDir(currentCell);
                if(Vector3.Dot(targetVector, navDir) < -0.75f)
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
                if(detectedHigherPriority)
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
        if (targetVector.magnitude <= TargetStoppingDistance)
        {
            target.damageHandler.TakeDamage(dmgCtrl);
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
                        Danger[i] += strength * dot * SeparationForce * (e.priority/priority);
                    }
                }
                if(e.priority > priority)
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
        return add.normalized;
    }
    public void FindObstacles()
    {
        neighbors.Clear();
        detectedHigherPriority = false;
        detectedObstacle = false;
        int count = Physics.OverlapSphereNonAlloc(transform.position, DetectionRadius, neighborBuffer, ObstacleMask | EnemyMask);
        for (int i = 0; i < count; i++)
        {
            if (neighborBuffer[i].TryGetComponent(out Enemy e))
            {
                if (e != this && e.priority >= priority)
                {
                    if(e.priority > priority)
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
        }
    }
    public Player GetClosestPlayer()
    {
        Player closest = null;
        float closestDistance = Mathf.Infinity;
        foreach (Player p in GameManager.Instance.Players)
        {
            float distance = Vector3.Distance(transform.position, p.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = p;
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
}

