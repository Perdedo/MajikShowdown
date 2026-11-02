using Mirror;
using UnityEngine;

public class CasterEnemy : Enemy
{
    [Header("Spell Caster")]
    public EnmSpellCaster caster;
    public bool Casting;
    public float CastPoseTime = 3f;
    protected int selectedSpellIndex = 0;
    [Header("Warping")]
    protected bool Warped = false;
    public float MinWarpDistance = 5f, MaxWarpDistance = 15f, MinWarpHeight = 5f, MaxWarpHeight = 7f;
    protected Timer warpTimer = new Timer(false);
    protected bool warping = false, warpingIn = false;
    public float warpDuration = 1.8f, warpInDuration = 0.9f;
    public override void Initialize(float HealthMultiplier)
    {
        for(int i = 0; i < caster.equippedSpells.Length; i++)
        {
            if(caster.equippedSpells[i] != null && caster.equippedSpells[i].coreNode.Element == element)
            {
                selectedSpellIndex = i;
                break;
            }
        }
        base.Initialize(HealthMultiplier);
        Casting = false;
        attackCooldown = caster.equippedSpells[selectedSpellIndex].SpellCooldown + 0.1f;
        gravityPaused = true;
        Warped = false;
        warping = false;
        warpingIn = false;
        warpTimer.Paused = true;
        warpTimer.SetTimer(0);
    }
    protected override void AttackPlayer()
    {
        caster.CastSpell(selectedSpellIndex);
        Warped = false;
    }
    public void CastAnim()
    {

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
        if (transparency > 0)
        {
            transparency = Mathf.Clamp(transparency - Time.deltaTime, 0, 1);

            foreach (Material mat in mesh.materials)
            {
                mat.SetFloat("_Alpha_Threshold", transparency);
            }
            return;
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
            if (!Warped && attackCooldownTimer.Timestamp > attackCooldown * 0.5f)
            {
                Warp();
                warpTimer.SetTimer(0);
                warpTimer.Paused = false;
                attackCooldownTimer.Paused = true;
            }
        }
        else
        {
            attackCooldownTimer.Paused = true;
        }
        if (!onAttackCooldown)
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
        if(warping)
        {
            if(warpTimer.timer(warpDuration, Time.deltaTime, false, false))
            {
                warping = false;
                warpTimer.Paused = true;
                attackCooldownTimer.Paused = false;
            }
            else if(warpingIn && warpTimer.Timestamp > warpInDuration)
            {
                warpingIn = false;
                rb.interpolation = RigidbodyInterpolation.None;
                rb.position = WarpPos;
                transform.position = WarpPos;
                ResetAllVelocities();
                rb.interpolation = RigidbodyInterpolation.Interpolate;
            }
        }
        UpdateTransform();
    }
    protected Vector3 WarpPos;
    public void Warp()
    {
        Warped = true;
        bool validWarp = false;
        //Vector3 WarpPos;
        int count = 0;
        while (!validWarp)
        {
            Vector3 targetPos = target.transform.position;
            float warpDistance = Random.Range(MinWarpDistance, MaxWarpDistance);
            float warpHeight = Random.Range(MinWarpHeight, MaxWarpHeight);
            Vector3 warpDir = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
            FieldCell warpCell = FlowFieldManager.instance.WorldToGridPosition(targetPos + warpDir * warpDistance);
            if (warpCell != null && !Physics.CapsuleCast(warpCell.position + Vector3.up * 0.5f, warpCell.position + Vector3.up * 2f, 0.5f, Vector3.up, warpHeight))
            {
                if(count >= 20 || Vector3.Distance(warpCell.position, currentCell.position) > 5)
                {
                    WarpPos = warpCell.position + Vector3.up * warpHeight;
                    validWarp = true;
                    warpingIn = true;
                    warping = true;
                    PlayAnimation(EnemyAnimState.Teleport);
                }
            }
                //transform.position = WarpPos;
            
            count++;
            if (count > 20)
            {
                Debug.LogWarning("Failed to find valid warp position for CasterEnemy after 20 attempts.");
                break;
            }
        }

    }

    protected override void Gravity()
    {
        //RaycastGround();
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
                /*if (isServer)
                {
                    PlayAnimation(EnemyAnimState.Land);
                }*/
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

    protected override void Jump(bool pressed)
    {
        if (pressed && !movePaused)
        {
            if ((CvState == CharVerticalState.grounded || canJumpOnAir) && !jumpOnCooldown)
            {
                jumpTimer.SetTimer(0);
                jumpTimer.Paused = false;
                CvState = CharVerticalState.jumping;
                /*if (isServer)
                {
                    PlayAnimation(EnemyAnimState.Jump);
                }*/
                InvokeIfAllowed(Jumped);
                StartCoroutine(JumpCooldown());
            }
        }
        else if (CvState == CharVerticalState.jumping)
        {
            jumpTimer.SetTimer(jumpTime);
        }
    }

    public override void EnemyClientUpdate()
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
        if (transparency > 0)
        {
            transparency = Mathf.Clamp(transparency - Time.deltaTime, 0, 1);

            foreach (Material mat in mesh.materials)
            {
                mat.SetFloat("_Alpha_Threshold", transparency);
            }
            return;
        }
        timePred = (float)NetworkTime.time - GameManager.Instance.hordeController.lastTime;
        predTarget = GameManager.Instance.hordeController.enemiesInfo[instanceIndex].pos + GameManager.Instance.hordeController.enemiesInfo[instanceIndex].vel * timePred;
        if ((predTarget - transform.position).sqrMagnitude < 100)
        {
            transform.position = Vector3.Lerp(transform.position, predTarget, Time.deltaTime * 15);
        }
        else
        {
            transform.position = GameManager.Instance.hordeController.enemiesInfo[instanceIndex].pos;
        }
        Vector3 dir = predTarget - transform.position;
        dir.y = 0;
        if (dir.sqrMagnitude >= 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }
    }
}
