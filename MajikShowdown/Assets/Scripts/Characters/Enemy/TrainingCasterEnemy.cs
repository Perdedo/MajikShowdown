using UnityEngine;
using Unity.Mathematics;
public class TrainingCasterEnemy : CasterEnemy
{
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
        onAttackCooldown = false;
        attacked = true;
        jumpOnCooldown = false;
        prevMoving = false;
        //CheckFieldLocation();
        Casting = false;
        attackCooldown = caster.equippedSpells[selectedSpellIndex].SpellCooldown + 0.1f;
        gravityPaused = true;
        Warped = false;
        warping = false;
        warpingIn = false;
        warpTimer.Paused = true;
        warpTimer.SetTimer(0);
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
        if (warping)
        {
            if (warpTimer.timer(warpDuration, Time.deltaTime, false, false))
            {
                warping = false;
                warpTimer.Paused = true;
                attackCooldownTimer.Paused = false;
            }
            else if (warpingIn && warpTimer.Timestamp > warpInDuration)
            {
                //Debug.Log("Warping to position: " + WarpPos);
                warpingIn = false;
                rb.interpolation = RigidbodyInterpolation.None;
                rb.position = WarpPos;
                transform.position = WarpPos;
                ResetAllVelocities();
                rb.interpolation = RigidbodyInterpolation.Interpolate;
            }
        }
    }
}
