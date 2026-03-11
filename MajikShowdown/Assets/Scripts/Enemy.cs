using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;

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
    Player target;
    List<Enemy> neighbors = new List<Enemy>();
    Collider[] neighborBuffer = new Collider[32];
    Vector3[] Directions = new Vector3[16];
    float[] Danger = new float[16];
    float[] Interest = new float[16];
    Vector3 targetVector/*, targetLastSeen*/;
    bool detectedObstacle = false;
    Vector3 MoveDirection;
    Vector3 interestDirection;
    bool canSeeTarget;

    void Start()
    {
        for (int i = 0; i < Directions.Length; i++)
        {
            float angle = i * Mathf.PI * 2f / Directions.Length;
            Directions[i] = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
        }
        //targetLastSeen = targetVector;
    }

    // Update is called once per frame
    void Update()
    {
        target = GetClosestPlayer();
        if(target == null)
        {
            return;
        }
        targetVector = target.transform.position - transform.position;
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
        MoveDirection = GetBestDirection();
        //Debug.DrawRay(transform.position, targetVector, Color.red);
        //Debug.DrawRay(transform.position, targetLastSeen, Color.blue);
        PathToTarget();
    }
    public void PathToTarget()
    {
        if (targetVector.magnitude <= TargetStoppingDistance)
        {
            SetVelocity(new Vector3(0, localVelocity.y, 0));
            SetAcceleration(Vector3.zero);
        }
        else
        {
            Move(MoveDirection, speed);
        }
    }
    public void CalculateDanger()
    {
        for (int i = 0; i < Danger.Length; i++)
        {
            Danger[i] = 0;
        }
        foreach (Enemy e in neighbors)
        {
            Vector3 toEnemy = e.transform.position - transform.position;
            float distance = toEnemy.magnitude;
            if (distance < EnemyAvoidanceRadius)
            {
                float strength = Mathf.Pow(2 - (distance / EnemyAvoidanceRadius), 2) - 1;
                for (int i = 0; i < Directions.Length; i++)
                {
                    float dot = Vector3.Dot(toEnemy.normalized, Directions[i]);
                    if (dot > 0)
                    {
                        Danger[i] += strength * dot * SeparationForce;
                    }
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
                Interest[i] = 0;
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
        detectedObstacle = false;
        int count = Physics.OverlapSphereNonAlloc(transform.position, DetectionRadius, neighborBuffer, ObstacleMask | EnemyMask);
        for (int i = 0; i < count; i++)
        {
            if (neighborBuffer[i].TryGetComponent(out Enemy e))
            {
                if (e != this)
                {
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

