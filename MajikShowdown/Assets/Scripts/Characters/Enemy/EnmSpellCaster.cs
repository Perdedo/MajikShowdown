using Mirror;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class EnmSpellCaster : SpellCaster
{
    public CasterEnemy enemy;

    protected override void Awake()
    {
        if (!network)
        {
            enemy.caster = this;
            //DamageHandler = GetComponent<CharacterDamageHandler>();
            //equippedSpells = new Spell[4];
            foreach (var nodeData in ownedNodes)
            {
                InstantiateNode(nodeData);
            }
        }


        /*foreach (var grid in SpellGrids)
        {
            grid.caster = this;
        }*/
    }

    public override void OnStartAuthority()
    {
        enemy.caster = this;
        //DamageHandler = GetComponent<CharacterDamageHandler>();
        equippedSpells = new Spell[4];
        foreach (var nodeData in ownedNodes)
        {
            InstantiateNode(nodeData);
        }
        if (!isServer)
        {
            CMDInitialize();
        }
    }

    [Command]
    public override void CMDInitialize()
    {
        enemy.caster = this;
        //DamageHandler = GetComponent<CharacterDamageHandler>();
        equippedSpells = new Spell[4];
        foreach (var nodeData in ownedNodes)
        {
            InstantiateNode(nodeData);
        }
    }
    protected override SpellNode InstantiateNode(SpellNode nodePrefab)
    {
        SpellNode runtimeNode = Instantiate(nodePrefab);
        //runtimeNode.instanceIndex = commander.runtimeNodes.Count;
        runtimeNode.owner = this;
        //commander.runtimeNodes.Add(runtimeNode);
        if(isOwned || !network)
        {
            runtimeNode.ClientInitialize();
        }
        else
        {
            runtimeNode.ServerInitialize();
        }
        runtimeNodes.Add(runtimeNode);
        return runtimeNode;
    }


    protected override void Update()
    {
        if (!isLocalPlayer && network)
        {
            return;
        }
        if (!canCast)
        {
            return;
        }
        for (int i = 0; i < equippedSpells.Length; i++)
        {
            if (equippedSpells[i] != null && equippedSpells[i].onCooldown)
            {
                if (equippedSpells[i].cooldownTimer.timer(equippedSpells[i].SpellCooldown, Time.deltaTime, false, true))
                {
                    equippedSpells[i].onCooldown = false;
                    if(!isServer && network)
                    {
                        RemoveCooldown(i);
                    }
                }
            }
        }
        if(enemy != null && enemy.Casting)
        {
            if(castPoseTimer.timer(enemy.CastPoseTime,Time.deltaTime, false, false))
            {
                enemy.Casting = false;
            }
        }
    }



    [Command]
    public override void CMDCastSpell(int spellInd, Vector3 aimPoint)
    {
        Spell spell = equippedSpells[spellInd];
        if (spell.onCooldown) return;
        spell.onCooldown = true;
        AddCooldown(this.connectionToClient, spellInd);

        if (spell.validSpell)
        {
            enemy.CastAnim();
            //ServerInstantiateSpellCollider(spell, CastingPoint.position,transform.forward, true);
            if (spell.coreNode.castPoint == null)
            {
                SpellColliderManager.Instance.ServerInitializeSpellCollider(spell, CastingPoint, (aimPoint - CastingPoint.position).normalized, true);
                //ServerInstantiateSpellCollider(spell, CastingPoint, (AimController.AimPoint - CastingPoint.position).normalized, true);
            }
            else
            {
                Vector3 castPos = spell.coreNode.castPoint.GetCastPoint(transform, aimPoint);
                Vector3 dir = (aimPoint - castPos).normalized;
                //Vector3 castPos = spell.coreNode.castPoint.GetCastPoint(transform, AimController.AimPoint);
                //Vector3 dir = (AimController.AimPoint - castPos).normalized;
                if (dir == Vector3.zero)
                {
                    dir = aimPoint - transform.position;
                    //dir = AimController.AimPoint - transform.position;
                }
                SpellColliderManager.Instance.ServerInitializeSpellCollider(spell, castPos, dir, true);
            }
            if(enemy != null)
            {
                enemy.Casting = true;
                castPoseTimer.SetTimer(0);
            }
        }
    }



    public override void CastSpell(int spellInd)
    {
        if (network)
        {
            CMDCastSpell(spellInd, enemy.target.transform.position);
            return;
        }
        Spell spell = equippedSpells[spellInd];
        if (spell.onCooldown) return;
        if (spell.validSpell)
        {
            enemy.CastAnim();
            spell.onCooldown = true;
            if (spell.coreNode.castPoint == null)
            {
                SpellColliderManager.Instance.InitializeSpellCollider(spell, CastingPoint, enemy.targetVector.normalized, true);
            }
            else
            {
                Vector3 castPos = spell.coreNode.castPoint.GetCastPoint(transform, enemy.target.transform.position);
                Vector3 dir = (enemy.target.transform.position - castPos).normalized;
                if (dir == Vector3.zero)
                {
                    dir = enemy.targetVector;
                }
                SpellColliderManager.Instance.InitializeSpellCollider(spell, castPos, dir, true);
            }
            if(enemy != null)
            {
                enemy.Casting = true;
                castPoseTimer.SetTimer(0);
            }
        }

    }
}
