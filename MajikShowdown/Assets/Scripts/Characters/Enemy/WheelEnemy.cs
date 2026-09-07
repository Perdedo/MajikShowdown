using UnityEngine;

public class WheelEnemy : Enemy
{
    public float chargeStartDistance = 30;
    public float chargeSpeed = 30, baseRotationSpeed = 180, chargeRotationSpeed = 15, knockbackStrength = 50;
    Vector3 chargeDir, knockbackDir;
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
                //AttackPlayer();
                rotationSpeed = baseRotationSpeed;
                PlayAnimation(EnemyAnimState.Stop);
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
        UpdateTransform();
    }
    public override void PathToTarget(FieldCell currentCell)
    {
        if(targetVector.magnitude <= chargeStartDistance)
        {
            if(attacked)
            {
                if(!onAttackCooldown)
                {
                    attacked = false;
                    attackTimer.SetTimer(0);
                    attackedPlayer = target;
                    attackTimer.Paused = false;
                    PlayAnimation(EnemyAnimState.Attack);
                    rotationSpeed = chargeRotationSpeed;
                    chargeDir = MoveDirection;
                    Move(chargeDir, chargeSpeed);
                }
                else
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
                    }
                    else
                    {
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
            }
            else
            {
                rotationSpeed = chargeRotationSpeed;
                Move(chargeDir, chargeSpeed);
            }
        }
        else
        {
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

    private void OnCollisionEnter(Collision collision)
    {
        if(!isServer)
        {
            return;
        }
        if(attacked)
        {
            return;
        }
        if(collision.gameObject.TryGetComponent<IGameCharacter>(out IGameCharacter c))
        {
            if(c is Character)
            {
                c.DamageHandler.TakeDamage(dmgCtrl);
            }
            knockbackDir = (collision.transform.position - transform.position).normalized;
            c.Knockback((knockbackDir + transform.right + Vector3.up).normalized, knockbackStrength);
        }
    }
}
