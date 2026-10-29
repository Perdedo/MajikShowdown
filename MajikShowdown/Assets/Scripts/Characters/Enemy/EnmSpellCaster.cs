using Mirror;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class EnmSpellCaster : NetworkBehaviour, ISpellCaster
{
    public List<SavedSpellAsset> startingSpells = new List<SavedSpellAsset>();

    [Header("Generic Node")]
    public SpellNodeInterface genericNodePrefab;
    [Header("Enemy Nodes")]
    public List<SpellNode> ownedNodes = new();

    public List<SpellNode> runtimeNodes = new();
    public List<NodeInventory> inventories = new();
    public CasterEnemy enemy;
    public List<Spell> spells = new List<Spell>();
    public List<Spell> equippedSpells = new List<Spell>();
    public NodeInventory inventory;
    //public SpellCollider ProjectilePrefab;
    public Transform CastingPoint;
    //public UICommandController commander;

    [Header("Collision Layers")]
    public LayerMask EnemyLayer;
    public LayerMask PlayerLayer;
    public LayerMask ObjectLayer;

    [HideInInspector] public bool canCast = true;
    Timer castPoseTimer = new Timer(false);

    public bool startingSpellsReady = false;

    [Header("Network")]
    public bool network = true;

    private void Awake()
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
        equippedSpells = new List<Spell>();
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
    public void CMDInitialize()
    {
        enemy.caster = this;
        //DamageHandler = GetComponent<CharacterDamageHandler>();
        equippedSpells = new List<Spell>();
        foreach (var nodeData in ownedNodes)
        {
            InstantiateNode(nodeData);
        }
    }
    SpellNode InstantiateNode(SpellNode nodePrefab)
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

    public SpellNode AddRune(SpellNode nodePrefab, bool syncInventory = true)
    {
        ownedNodes.Add(nodePrefab);
        SpellNode runtimeNode = InstantiateNode(nodePrefab);

        if (syncInventory)
        {
            foreach (var inv in inventories)
            {
                inv.SyncFromCaster();
            }
        }

        return runtimeNode;
    }

    private string GetPath(Transform current)
    {
        if (current.parent == null)
            return current.name;

        return GetPath(current.parent) + "/" + current.name;
    }

    private void Update()
    {
        if (!isLocalPlayer && network)
        {
            return;
        }
        if (!canCast)
        {
            return;
        }
        for (int i = 0; i < equippedSpells.Count; i++)
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
    public void RemoveCooldown(int index)
    {
        equippedSpells[index].onCooldown = false;
    }
    

    [TargetRpc]
    public void AddCooldown(NetworkConnection target, int index)
    {
        equippedSpells[index].onCooldown = true;
    }


    [Command]
    public void CMDCastSpell(int spellInd, Vector3 aimPoint)
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



    public void CastSpell(int spellInd)
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

    /*public bool IsSlotValid(int index)
    {
        if (index < 0 || index >= equippedSpells.Count) return false;
        var spell = equippedSpells[index];
        return spell != null && !string.IsNullOrEmpty(spell.spellName);
    }

    public bool IsNodeInUse(SpellNode node)
    {
        return node.IsInUse;
    }

    public void SetNodeInUse(SpellNode node, bool value)
    {
        node.IsInUse = value;
        foreach (var inventory in inventories)
            inventory.RefreshNodeState(node);
    }*/
}
