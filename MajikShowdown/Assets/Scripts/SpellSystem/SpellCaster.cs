using Mirror;
using System.Collections.Generic;
using UnityEngine;


public class SpellCaster : NetworkBehaviour, IGameCharacter
{
    public CharacterDamageHandler DamageHandler { get; private set; }
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
    public SpellCollider ProjectilePrefab;
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
        DamageHandler = GetComponent<CharacterDamageHandler>();
        equippedSpells = new Spell[4];
        foreach (var nodeData in ownedNodes)
        {
            SpellNode runtimeNode = Instantiate(nodeData);
            runtimeNode.Initialize();
            runtimeNodes.Add(runtimeNode);
        }
        
        /*foreach (var grid in SpellGrids)
        {
            grid.caster = this;
        }*/
    }

    public override void OnStartAuthority()
    {
        if (!isServer)
        {
            CMDInitialize();
        }
    }

    [Command]
    public void CMDInitialize()
    {
        DamageHandler = GetComponent<CharacterDamageHandler>();
        equippedSpells = new Spell[4];
        foreach (var nodeData in ownedNodes)
        {
            SpellNode runtimeNode = Instantiate(nodeData);
            runtimeNode.Initialize();
            runtimeNodes.Add(runtimeNode);
        }
    }

    private void Update()
    {
        if(!isLocalPlayer && network)
        {
            return;
        }
        if(!canCast)
        {
            return;
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (equippedSpells[0] != null)
            {
                /*if(network)
                {
                    CMDCastSpell(0);
                }
                else
                {
                    CastSpell(0);
                }*/
                Debug.Log("Cast");
                CMDCastSpell(0);
                //Debug.Log(equippedSpells[0].spellName);
            }
        }
    }

    [Command]
    public void CMDCastSpell(int spellInd)
    {
        Spell spell = equippedSpells[spellInd];

        if (spell.validSpell)
        {
            ServerInstantiateSpellCollider(spell, CastingPoint.position,transform.forward, true);
        }
        
    }

    [Server]
    public void ServerInstantiateSpellCollider(Spell Spell, Vector3 pos, Vector3 lookDir, bool primary = false)
    {
        GameObject g = Instantiate(ProjectilePrefab.gameObject, pos, Quaternion.LookRotation(lookDir,Vector3.up));
        SpellCollider col = g.GetComponent<SpellCollider>();
        //col.OwnerSpell = Spell;
        //col.primarySpell = primary;
        col.Initialize(Spell, primary);
        NetworkServer.Spawn(g);
    }
    public void CastSpell(int spellInd)
    {
        Spell spell = equippedSpells[spellInd];

        if (spell.validSpell)
        {
            InstantiateSpellCollider(spell, CastingPoint,transform.forward, true);
        }
        
    }

    public void InstantiateSpellCollider(Spell Spell, Vector3 pos, Vector3 lookDir, bool primary = false)
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
        GameObject g = Instantiate(ProjectilePrefab.gameObject, castPoint.position, Quaternion.LookRotation(lookDir,Vector3.up));
        SpellCollider col = g.GetComponent<SpellCollider>();
        col.SpawnTransform = castPoint;
        col.SpawnPoint = castPoint.position;
        col.Initialize(Spell, primary);
        //NetworkServer.Spawn(g);
    }

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
}
