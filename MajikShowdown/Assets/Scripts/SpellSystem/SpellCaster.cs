using Mirror;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;


public class SpellCaster : NetworkBehaviour, IGameCharacter
{
    public CharacterDamageHandler DamageHandler { get; private set; }
    public AimController AimController;
    [Header("Generic Node")]
    public SpellNodeInterface genericNodePrefab;
    [Header("Player Nodes")]
    public List<SpellNode> ownedNodes = new();

    public List<SpellNode> runtimeNodes = new();
    public List<NodeInventory> inventories = new();
    public Player player;
    public List<Spell> spells = new List<Spell>();
    public Spell[] equippedSpells;
    public NodeInventory inventory;
    //public SpellCollider ProjectilePrefab;
    public Transform CastingPoint;
    public UICommandController commander;

    [Header("Collision Layers")]
    public LayerMask EnemyLayer;
    public LayerMask PlayerLayer;
    public LayerMask ObjectLayer;

    [HideInInspector] public bool canCast = true;

    [Header("Network")]
    public bool network = true;

    private void Awake()
    {
        if (!network)
        {
            player.caster = this;
            DamageHandler = GetComponent<CharacterDamageHandler>();
            equippedSpells = new Spell[4];
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
        player.caster = this;
        DamageHandler = GetComponent<CharacterDamageHandler>();
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
    public void CMDInitialize()
    {
        player.caster = this;
        DamageHandler = GetComponent<CharacterDamageHandler>();
        equippedSpells = new Spell[4];
        foreach (var nodeData in ownedNodes)
        {
            InstantiateNode(nodeData);
        }
    }
    void InstantiateNode(SpellNode nodePrefab)
    {
        SpellNode runtimeNode = Instantiate(nodePrefab);
        runtimeNode.Initialize();
        runtimeNodes.Add(runtimeNode);
    }
    public void AddRune(SpellNode nodePrefab)
    {
        ownedNodes.Add(nodePrefab);
        InstantiateNode(nodePrefab);
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
        for (int i = 0; i < equippedSpells.Length; i++)
        {
            Debug.LogWarning("For");
            if (equippedSpells[i] != null && equippedSpells[i].onCooldown)
            {
                Debug.LogWarning("Cooldown");
                if (equippedSpells[i].cooldownTimer.timer(equippedSpells[i].SpellCooldown, Time.deltaTime, false, true))
                {
                    Debug.LogWarning("Timer");
                    equippedSpells[i].onCooldown = false;
                    if(!isServer)
                    {
                        Debug.LogWarning("Client");
                        RemoveCooldown(i);
                    }
                }
            }
        }
        /*if (Input.GetKeyDown(KeyCode.E))
        {
            if (equippedSpells[0] != null)
            {
                Debug.Log("Cast");
                if (network)
                {
                    CMDCastSpell(0, AimController.AimPoint);
                }
                else
                {
                    CastSpell(0);
                }
                //CMDCastSpell(0, AimController.AimPoint);
                //Debug.Log(equippedSpells[0].spellName);
            }
        }*/
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
        }
    }

    /*[Server]
    public void ServerInstantiateSpellCollider(Spell Spell, Vector3 pos, Vector3 lookDir, bool primary = false)
    {
        GameObject g = Instantiate(ProjectilePrefab.gameObject, pos, Quaternion.LookRotation(lookDir, Vector3.up));
        SpellCollider col = g.GetComponent<SpellCollider>();
        //col.OwnerSpell = Spell;
        //col.primarySpell = primary;
        col.Initialize(Spell, primary);
        NetworkServer.Spawn(g);
    }

    [Server]
    public void ServerInstantiateSpellCollider(Spell Spell, Transform castPoint, Vector3 lookDir, bool primary = false)
    {
        GameObject g = Instantiate(ProjectilePrefab.gameObject, castPoint.position, Quaternion.LookRotation(lookDir, Vector3.up));
        SpellCollider col = g.GetComponent<SpellCollider>();
        col.SpawnTransform = castPoint;
        col.SpawnPoint = castPoint.position;
        col.Initialize(Spell, primary);
        NetworkServer.Spawn(g);
    }*/

    public void CastSpell(int spellInd)
    {
        Spell spell = equippedSpells[spellInd];
        if (spell.onCooldown) return;
        if (spell.validSpell)
        {
            spell.onCooldown = true;
            if (spell.coreNode.castPoint == null)
            {
                SpellColliderManager.Instance.InitializeSpellCollider(spell, CastingPoint, (AimController.AimPoint - CastingPoint.position).normalized, true);
            }
            else
            {
                Vector3 castPos = spell.coreNode.castPoint.GetCastPoint(transform, AimController.AimPoint);
                Vector3 dir = (AimController.AimPoint - castPos).normalized;
                if (dir == Vector3.zero)
                {
                    dir = AimController.AimPoint - transform.position;
                }
                SpellColliderManager.Instance.InitializeSpellCollider(spell, castPos, dir, true);
            }

        }

    }

    /*public void InstantiateSpellCollider(Spell Spell, Vector3 pos, Vector3 lookDir, bool primary = false)
    {
        GameObject g = Instantiate(ProjectilePrefab.gameObject, pos, Quaternion.LookRotation(lookDir, Vector3.up));
        SpellCollider col = g.GetComponent<SpellCollider>();
        col.SpawnPoint = pos;
        //col.OwnerSpell = Spell;
        //col.primarySpell = primary;
        col.Initialize(Spell, primary);
        //NetworkServer.Spawn(g);
    }
    public void InstantiateSpellCollider(Spell Spell, Transform castPoint, Vector3 lookDir, bool primary = false)
    {
        GameObject g = Instantiate(ProjectilePrefab.gameObject, castPoint.position, Quaternion.LookRotation(lookDir, Vector3.up));
        SpellCollider col = g.GetComponent<SpellCollider>();
        col.SpawnTransform = castPoint;
        col.SpawnPoint = castPoint.position;
        col.Initialize(Spell, primary);
        //NetworkServer.Spawn(g);
    }*/

    public bool IsSlotValid(int index)
    {
        if (index < 0 || index >= equippedSpells.Length) return false;
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
    }

    public void CastFirstSpellInput(InputAction.CallbackContext context)
    {
        if (!isLocalPlayer && network) return;
        if (!context.started) return;
        if (!canCast) return;
        if (equippedSpells[0] == null) return;

        if (network)
        {
            CMDCastSpell(0, AimController.AimPoint);
        }
        else
        {
            CastSpell(0);
        }
    }

    public void CastSecondSpellInput(InputAction.CallbackContext context)
    {
        if (!isLocalPlayer && network) return;
        if (!context.started) return;
        if (!canCast) return;
        if (equippedSpells[1] == null) return;
        if (network)
        {
            CMDCastSpell(1, AimController.AimPoint);
        }
        else
        {
            CastSpell(1);
        }
    }

    public void CastThirdSpellInput(InputAction.CallbackContext context)
    {
        if (!isLocalPlayer && network) return;
        if (!context.started) return;
        if (!canCast) return;
        if (equippedSpells[2] == null) return;
        if (network)
        {
            CMDCastSpell(2, AimController.AimPoint);
        }
        else
        {
            CastSpell(2);
        }
    }

    public void CastFourthSpellInput(InputAction.CallbackContext context)
    {
        if (!isLocalPlayer && network) return;
        if (!context.started) return;
        if (!canCast) return;
        if (equippedSpells[3] == null) return;
        if (network)
        {
            CMDCastSpell(3, AimController.AimPoint);
        }
        else
        {
            CastSpell(3);
        }
    }
}
