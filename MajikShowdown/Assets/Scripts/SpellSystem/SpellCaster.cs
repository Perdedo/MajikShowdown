using UnityEngine;


public class SpellCaster : MonoBehaviour
{
    public Player player; 
    public Spell[] equippedSpells = new Spell[4];
    public NodeInventory inventory;
    public SpellCollider ProjectilePrefab;
    public Transform CastingPoint;

    [Header("Collision Layers")]
    public LayerMask EnemyLayer;
    public LayerMask PlayerLayer;
    public LayerMask ObjectLayer;

    private void Awake()
    {
        /*foreach (var grid in SpellGrids)
        {
            grid.caster = this;
        }*/
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (equippedSpells[0] != null)
            {
                CastSpell(equippedSpells[0]);
                Debug.Log(equippedSpells[0].spellName);
            }
        }
    }
    public void CastSpell(Spell spell)
    {
        if (spell.validSpell)
        {
            InstantiateSpellCollider(spell, CastingPoint.position,transform.forward, true);
        }
        
    }
    public void InstantiateSpellCollider(Spell Spell, Vector3 pos, Vector3 lookDir, bool primary = false)
    {
        GameObject g = Instantiate(ProjectilePrefab.gameObject, pos, Quaternion.LookRotation(lookDir,Vector3.up));
        SpellCollider col = g.GetComponent<SpellCollider>();
        col.OwnerSpell = Spell;
        col.primarySpell = primary;
    }
    
}
