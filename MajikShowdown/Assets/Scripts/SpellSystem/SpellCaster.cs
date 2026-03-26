using UnityEngine;


public class SpellCaster : MonoBehaviour
{
    public Player player;
    public HexGrid[] SpellGrids;
    public NodeInventory inventory;
    public SpellCollider ProjectilePrefab;
    public Transform CastingPoint;

    [Header("Collision Layers")]
    public LayerMask EnemyLayer;
    public LayerMask PlayerLayer;
    public LayerMask ObjectLayer;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            CastSpell(SpellGrids[0].spell);
        }
    }
    public void CastSpell(Spell spell)
    {
        InstantiateSpellCollider(spell, CastingPoint.position, true);
    }
    public void InstantiateSpellCollider(Spell Spell, Vector3 pos, bool primary = false)
    {
        GameObject g = Instantiate(ProjectilePrefab.gameObject, pos, transform.rotation);
        SpellCollider col = g.GetComponent<SpellCollider>();
        col.OwnerSpell = Spell;
        col.primarySpell = primary;
    }
    
}
