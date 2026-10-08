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
        dmgCtrl = new Damage(damage, element);
        leapTimer.Paused = true;
        leapTimer.SetTimer(0);
        attackTimer.Paused = true;
        attackCooldownTimer.Paused = true;
        attackTimer.SetTimer(0);
        attackCooldownTimer.SetTimer(0);
        occupiedCellNum = (int)math.ceil(size / FlowFieldManager.instance.CellSize);
        detectRadius = math.max((int)math.ceil(DetectionRadius / FlowFieldManager.instance.CellSize), 1);
        canBeKnocked = true;
        onAttackCooldown = false;
        attacked = true;
        jumpOnCooldown = false;
    }
    public override void EnemyUpdate()
    {
        if (FlowFieldManager.instance == null)
        {
            return;
        }
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
            if (leapTimer.timer(leapTime, Time.deltaTime, false, false))
            {
                Leap();
            }
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
}
