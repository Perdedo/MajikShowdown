using Mirror;
using System.Collections.Generic;
using UnityEngine;


public class SpellCaster : NetworkBehaviour, IGameCharacter
{
    public CharacterDamageHandler DamageHandler { get; private set; }

    public Player player;
    public List<Spell> spells = new List<Spell>();
    public Spell[] equippedSpells;
    public NodeInventory inventory;
    public SpellCollider ProjectilePrefab;
    public Transform CastingPoint;

    [Header("Collision Layers")]
    public LayerMask EnemyLayer;
    public LayerMask PlayerLayer;
    public LayerMask ObjectLayer;

    private void Awake()
    {
        DamageHandler = GetComponent<CharacterDamageHandler>();
        equippedSpells = new Spell[4];
        /*foreach (var grid in SpellGrids)
        {
            grid.caster = this;
        }*/
    }

    private void Update()
    {
        if(!isLocalPlayer)
        {
            return;
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (equippedSpells[0] != null)
            {
                CastSpell(0);
                //Debug.Log(equippedSpells[0].spellName);
            }
        }
    }

    [Command]
    public void CastSpell(int spellInd)
    {
        Spell spell = equippedSpells[spellInd];

        if (spell.validSpell)
        {
            InstantiateSpellCollider(spell, CastingPoint.position,transform.forward, true);
        }
        
    }

    [Server]
    public void InstantiateSpellCollider(Spell Spell, Vector3 pos, Vector3 lookDir, bool primary = false)
    {
        GameObject g = Instantiate(ProjectilePrefab.gameObject, pos, Quaternion.LookRotation(lookDir,Vector3.up));
        SpellCollider col = g.GetComponent<SpellCollider>();
        //col.OwnerSpell = Spell;
        //col.primarySpell = primary;
        col.Initialize(Spell, primary);
        NetworkServer.Spawn(g);
    }

    public bool IsSlotValid(int index)
    {
        if (index < 0 || index >= equippedSpells.Length) return false;
        var spell = equippedSpells[index];
        return spell != null && !string.IsNullOrEmpty(spell.spellName);
    }
}
