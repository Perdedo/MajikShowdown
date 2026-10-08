//using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Mirror;
public class ExplosiveEnemy : Enemy
{
    public float explosionRadius = 10, knockbackStrength = 100, knockbackLift = 0.5f;
    public GameObject explosionVFX;
    public LayerMask affectedByExplosion;
    Collider[] hits;
    protected bool exploded = false;
    Vector3 knockbackDir, leapDir;
    protected Timer leapTimer = new Timer(false);
    public float leapTime = 0.8f, leapSpeed = 50, leapForce = 2f, baseJumpForce = 5f;
    public override void Initialize(float HealthMultiplier)
    {
        base.Initialize(HealthMultiplier);
        exploded = false;
        leapTimer.Paused = true;
        leapTimer.SetTimer(0);
        jumpForce = baseJumpForce;
    }
    protected override void AttackPlayer()
    {
        this.DamageHandler.Die();
    }

    void Explode()
    {
        exploded = true;
        GameObject inst = Instantiate(explosionVFX, transform.position, Quaternion.identity);
        if(DamageHandler.network)
        {
            NetworkServer.Spawn(inst);
        }
        hits = Physics.OverlapSphere(transform.position, explosionRadius, affectedByExplosion);
        foreach (Collider collider in hits)
        {
            if (collider.gameObject != this.gameObject)
            {
                collider.gameObject.GetComponent<IGameCharacter>().DamageHandler.TakeDamage(dmgCtrl);
                knockbackDir = collider.transform.position - transform.position;
                knockbackDir.y = 0;
                collider.gameObject.GetComponent<IGameCharacter>().Knockback(knockbackDir.normalized + Vector3.up * knockbackLift, knockbackStrength);
            }
        }
    }

    public override void Die()
    {
        if(!exploded)
        {
            Explode();
        }
        base.Die();
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
            if(leapTimer.timer(leapTime, Time.deltaTime, false, false))
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
        UpdateTransform();
    }

    public override void PathToTarget(FieldCell currentCell)
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
                    leapDir = targetVector.normalized;
                    jumpForce = leapForce;
                    attacked = false;
                    leapTimer.SetTimer(0);
                    leapTimer.Paused = false;
                    attackTimer.SetTimer(0);
                    attackedPlayer = target;
                    attackTimer.Paused = false;
                    PlayAnimation(EnemyAnimState.Attack);
                }
            }
        }
        else if(attacked)
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


    protected void Leap()
    {
        Move(leapDir, leapSpeed);
        if (!movePaused)
        {
            if ((CvState == CharVerticalState.grounded || canJumpOnAir) && !jumpOnCooldown)
            {
                jumpTimer.SetTimer(0);
                jumpTimer.Paused = false;
                CvState = CharVerticalState.jumping;
                InvokeIfAllowed(Jumped);
                StartCoroutine(JumpCooldown());
            }
        }
        else if (CvState == CharVerticalState.jumping)
        {
            jumpTimer.SetTimer(jumpTime);
        }
    }

    protected override void HandleRotation()
    {
        if (Time.frameCount % 2 == 0)
        {
            if(attacked)
            {
                Vector3 dir = interestDirection;
                dir.y = 0;

                if (dir.sqrMagnitude < 0.001f)
                    return;

                Quaternion targetRot = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime);
            }
            else
            {
                Vector3 dir = leapDir;
                dir.y = 0;

                if (dir.sqrMagnitude < 0.001f)
                    return;

                Quaternion targetRot = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime);
            }
        }

    }
}
