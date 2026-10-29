using UnityEngine;

public class CasterEnemy : Enemy
{
    [Header("Spell Caster")]
    public EnmSpellCaster caster;
    public bool Casting;
    public float CastPoseTime = 3f;
    [Header("Warping")]
    bool Warped = false;
    public float MinWarpDistance = 5f, MaxWarpDistance = 15f, MinWarpHeight = 5f, MaxWarpHeight = 7f;
    public override void Initialize(float HealthMultiplier)
    {
        base.Initialize(HealthMultiplier);
        Casting = false;
        attackCooldown = caster.equippedSpells[0].SpellCooldown + 0.1f;
        gravityPaused = true;
        Warped = false;
    }
    protected override void AttackPlayer()
    {
        caster.CastSpell(0);
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
        UpdateTransform();
    }
    public void Warp()
    {
        Warped = true;
        bool validWarp = false;
        Vector3 WarpPos;
        int count = 0;
        while (!validWarp)
        {
            Vector3 targetPos = target.transform.position;
            float warpDistance = Random.Range(MinWarpDistance, MaxWarpDistance);
            float warpHeight = Random.Range(MinWarpHeight, MaxWarpHeight);
            Vector3 warpDir = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
            FieldCell warpCell = FlowFieldManager.instance.WorldToGridPosition(targetPos + warpDir * warpDistance);
            if (!Physics.CapsuleCast(warpCell.position + Vector3.up * 0.5f, warpCell.position + Vector3.up * 2f, 0.5f, Vector3.up, warpHeight))
            {
                WarpPos = warpCell.position + Vector3.up * warpHeight;
                validWarp = true;
                transform.position = WarpPos;
            }
            count++;
            if (count > 20)
            {
                Debug.LogWarning("Failed to find valid warp position for CasterEnemy after 20 attempts.");
                break;
            }
        }

    }
}
