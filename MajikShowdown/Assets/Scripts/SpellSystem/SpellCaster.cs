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
        InstantiateSpellCollider(spell.SubSpells[0], CastingPoint.position);
    }
    public void InstantiateSpellCollider(SubSpell subSpell, Vector3 pos)
    {
        GameObject g = Instantiate(ProjectilePrefab.gameObject, pos, transform.rotation);
        g.GetComponent<SpellCollider>().subSpell = subSpell;
    }
    
}
