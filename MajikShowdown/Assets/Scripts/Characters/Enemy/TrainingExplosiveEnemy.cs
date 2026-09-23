using UnityEngine;
using Unity.Mathematics;
using static Enemy;

public class TrainingExplosiveEnemy : ExplosiveEnemy
{
    public override void Initialize(float HealthMultiplier)
    {
        exploded = false;
        currentCell = FlowFieldManager.instance.flowField.allCells[0];
        DamageHandler.Initialize(this, HealthMultiplier);
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
        //RigidbodySetting();
        canBeKnocked = true;
        //CheckFieldLocation();
    }
    public override void EnemyUpdate()
    {
        if (FlowFieldManager.instance == null)
        {
            return;
        }
        //EnemyClientUpdate();
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
            CheckForJump();
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
        moving = worldVelocity.sqrMagnitude > 0.01;
        if (prevMoving != moving)
        {
            animator.SetBool("Moving", moving);
        }
    }
    protected override Vector3 CheckReposition()
    {
        FieldCell auxCell = null;
        bool canReposition = false;
        Vector3 repos = target.gameObject.transform.position + targetVector.normalized * repositionRange;
        auxCell = FlowFieldManager.instance.WorldToGridPosition(repos);
        if (auxCell != null && auxCell.Neighbors.Count >= 8)
        {
            canReposition = true;
        }
        if (canReposition)
        {
            return auxCell.position + Vector3.up * GameManager.Instance.trainingController.spawnerHeight;
        }
        else
        {
            return Vector3.zero;
        }
    }

    public override void CalculateDanger()
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
                for (int i = 0; i < GameManager.Instance.trainingController.Directions.Length; i++)
                {
                    float dot = Vector3.Dot(toEnemy.normalized, GameManager.Instance.trainingController.Directions[i]);
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
            for (int i = 0; i < GameManager.Instance.trainingController.Directions.Length; i++)
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position, GameManager.Instance.trainingController.Directions[i], out hit, DetectionRadius, ObstacleMask))
                {
                    //float dot = Mathf.Clamp01(Vector3.Dot(Directions[i], targetVector.normalized));
                    float strength = 1 - (hit.distance / DetectionRadius);
                    Danger[i] += strength;
                }
            }
        }
    }
    public override void CalculateInterest()
    {
        if (target != null)
        {
            for (int i = 0; i < GameManager.Instance.trainingController.Directions.Length; i++)
            {
                Interest[i] = 0.01f;
                float dot = Vector3.Dot(interestDirection.normalized, GameManager.Instance.trainingController.Directions[i]);
                if (dot > 0)
                {
                    Interest[i] += dot;
                }
            }
        }
    }
    public override Vector3 GetBestDirection()
    {
        Vector3 add = Vector3.zero;

        for (int i = 0; i < GameManager.Instance.trainingController.Directions.Length; i++)
        {
            add += (Vector3)GameManager.Instance.trainingController.Directions[i] * Mathf.Clamp01(Interest[i] - Danger[i]);
        }
        add.y = 0;
        return add.normalized;
    }

    public override void FindObstacles()
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
                Enemy e = GameManager.Instance.trainingController.GameEnemies[eID.ID];
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
    }
}
