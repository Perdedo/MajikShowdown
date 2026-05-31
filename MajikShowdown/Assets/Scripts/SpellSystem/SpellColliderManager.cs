using UnityEngine;
using System.Collections.Generic;

public class SpellColliderManager : MonoBehaviour
{
    public static SpellColliderManager Instance { get; private set; }
    public SpellCollider SpellColliderPrefab;
    public List<SpellCollider> spellColliders;
    public List<SpellCollider> activeSpellColliders;
    public List<SpellCollider> inactiveSpellColliders;
    void Awake()
    {
        Instance = this;
    }
    void Update()
    {

    }
    void InstantiateNewCollider()
    {
        SpellCollider collider = Instantiate(SpellColliderPrefab.gameObject, transform.position, transform.rotation, transform).GetComponent<SpellCollider>();
        collider.gameObject.SetActive(false);
        spellColliders.Add(collider);
        inactiveSpellColliders.Add(collider);
    }
    public void InitializeSpellCollider(Spell Spell, Vector3 pos, Vector3 lookDir, bool primary = false)
    {
        if (inactiveSpellColliders.Count == 0)
        {
            InstantiateNewCollider();
        }
        SpellCollider collider = inactiveSpellColliders[0];
        inactiveSpellColliders.RemoveAt(0);
        activeSpellColliders.Add(collider);
        collider.transform.position = pos;
        collider.transform.rotation = Quaternion.LookRotation(lookDir, Vector3.up);
        collider.SpawnPoint = pos;
        collider.Initialize(Spell, primary);
    }
    public void InitializeSpellCollider(Spell Spell, Transform castPoint, Vector3 lookDir, bool primary = false)
    {
        if (inactiveSpellColliders.Count == 0)
        {
            InstantiateNewCollider();
        }
        SpellCollider collider = inactiveSpellColliders[0];
        inactiveSpellColliders.RemoveAt(0);
        activeSpellColliders.Add(collider);
        collider.transform.position = castPoint.position;
        collider.transform.rotation = Quaternion.LookRotation(lookDir, Vector3.up);
        collider.SpawnTransform = castPoint;
        collider.SpawnPoint = castPoint.position;
        collider.Initialize(Spell, primary);
    }
    public void DeactivateSpellCollider(SpellCollider collider)
    {
        collider.gameObject.SetActive(false);
        activeSpellColliders.Remove(collider);
        inactiveSpellColliders.Add(collider);
        collider.ResetCollider();
        collider.transform.position = transform.position;
        collider.transform.rotation = Quaternion.identity;
        collider.transform.localScale = Vector3.one;

    }
}
