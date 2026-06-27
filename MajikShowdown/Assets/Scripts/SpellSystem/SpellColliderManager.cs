using UnityEngine;
using System.Collections.Generic;
using Mirror;

public class SpellColliderManager : NetworkBehaviour
{
    public static SpellColliderManager Instance { get; private set; }
    public SpellCollider SpellColliderPrefab;
    public List<SpellCollider> spellColliders = new List<SpellCollider>();
    public List<SpellCollider> activeSpellColliders = new List<SpellCollider>();
    public List<SpellCollider> inactiveSpellColliders = new List<SpellCollider>();
    public bool network = true;
    void Awake()
    {
        Instance = this;
    }
    void Update()
    {
        if (!isServer && network)
        {
            return;
        }
        for(int i =0; i < activeSpellColliders.Count; i++)
        {
            activeSpellColliders[i].UpdateCollider();
        }
        /*foreach (SpellCollider collider in activeSpellColliders)
        {
            collider.UpdateCollider();
        }*/
    }
    void InstantiateNewCollider()
    {
        SpellCollider collider = Instantiate(SpellColliderPrefab.gameObject, transform.position, transform.rotation).GetComponent<SpellCollider>();
        collider.gameObject.SetActive(false);
        spellColliders.Add(collider);
        inactiveSpellColliders.Add(collider);
    }
    [Server]
    void ServerInstantiateNewCollider()
    {
        GameObject colliderGO = Instantiate(SpellColliderPrefab.gameObject, transform.position, transform.rotation);
        SpellCollider collider = colliderGO.GetComponent<SpellCollider>();
        collider.isVisible = false;
        NetworkServer.Spawn(colliderGO);
        colliderGO.SetActive(false);
        spellColliders.Add(collider);
        inactiveSpellColliders.Add(collider);
        RPCInstantiateNewCollider(colliderGO);
    }
    [ClientRpc]
    void RPCInstantiateNewCollider(GameObject collider)
    {
        collider.SetActive(false);
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
        collider.gameObject.SetActive(true);
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
        collider.gameObject.SetActive(true);
    }
    [Server]
    public void ServerInitializeSpellCollider(Spell Spell, Vector3 pos, Vector3 lookDir, bool primary = false)
    {
        if (inactiveSpellColliders.Count == 0)
        {
            ServerInstantiateNewCollider();
        }
        SpellCollider collider = inactiveSpellColliders[0];
        inactiveSpellColliders.RemoveAt(0);
        activeSpellColliders.Add(collider);
        collider.transform.position = pos;
        collider.transform.rotation = Quaternion.LookRotation(lookDir, Vector3.up);
        collider.SpawnPoint = pos;
        collider.Initialize(Spell, primary);
        collider.gameObject.SetActive(true);
        RPCInitializeSpellCollider(collider.gameObject);
    }
    [Server]
    public void ServerInitializeSpellCollider(Spell Spell, Transform castPoint, Vector3 lookDir, bool primary = false)
    {
        if (inactiveSpellColliders.Count == 0)
        {
            ServerInstantiateNewCollider();
        }
        SpellCollider collider = inactiveSpellColliders[0];
        inactiveSpellColliders.RemoveAt(0);
        activeSpellColliders.Add(collider);
        collider.transform.position = castPoint.position;
        collider.transform.rotation = Quaternion.LookRotation(lookDir, Vector3.up);
        collider.SpawnTransform = castPoint;
        collider.SpawnPoint = castPoint.position;
        collider.Initialize(Spell, primary);
        collider.gameObject.SetActive(true);
        RPCInitializeSpellCollider(collider.gameObject);
    }
    [ClientRpc]
    public void RPCInitializeSpellCollider(GameObject collider)
    {
        collider.SetActive(true);
    }
    
    public void DeactivateSpellCollider(SpellCollider collider)
    {
        collider.gameObject.SetActive(false);
        activeSpellColliders.Remove(collider);
        inactiveSpellColliders.Add(collider);
        collider.ResetCollider();
        collider.transform.position = transform.position;
        collider.transform.rotation = Quaternion.identity;

    }
    [Server]
    public void ServerDeactivateSpellCollider(SpellCollider collider)
    {
        collider.gameObject.SetActive(false);
        activeSpellColliders.Remove(collider);
        inactiveSpellColliders.Add(collider);
        collider.ResetCollider();
        collider.transform.position = transform.position;
        collider.transform.rotation = Quaternion.identity;
        RPCDeactivateSpellCollider(collider);

    }
    [ClientRpc]
    public void RPCDeactivateSpellCollider(SpellCollider collider)
    {
        if(isServer)
        {
            return;
        }
        collider.gameObject.SetActive(false);
        collider.ResetCollider();
    }
}
