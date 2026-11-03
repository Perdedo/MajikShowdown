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
        if (AudioController.instance != null)
        {
            int sfxIndex = AudioController.instance.GetRandomSpellSFXIndex(Spell.coreNode.Element, Spell.coreNode.Type);
            collider.PlaySpellSFX(Spell.coreNode.Element, Spell.coreNode.Type, sfxIndex);
        }
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
        if (AudioController.instance != null)
        {
            int sfxIndex = AudioController.instance.GetRandomSpellSFXIndex(Spell.coreNode.Element, Spell.coreNode.Type);
            collider.PlaySpellSFX(Spell.coreNode.Element, Spell.coreNode.Type, sfxIndex);
        }
    }
    [Server]
    public void ServerInitializeSpellCollider(Spell Spell, Vector3 pos, Vector3 lookDir, bool primary = false)
    {
        if (inactiveSpellColliders.Count == 0)
        {
            ServerInstantiateNewCollider();
        }

        Elements element = Spell.coreNode.Element;
        SpellTypes type = Spell.coreNode.Type;
        int sfxIndex = AudioController.instance != null ? AudioController.instance.GetRandomSpellSFXIndex(element, type) : -1;

        SpellCollider collider = inactiveSpellColliders[0];
        inactiveSpellColliders.RemoveAt(0);
        activeSpellColliders.Add(collider);
        collider.transform.position = pos;
        collider.transform.rotation = Quaternion.LookRotation(lookDir, Vector3.up);
        collider.SpawnPoint = pos;
        collider.Initialize(Spell, primary);
        collider.gameObject.SetActive(true);

        RPCInitializeSpellCollider(collider.gameObject, element, type, sfxIndex);
    }
    [Server]
    public void ServerInitializeSpellCollider(Spell Spell, Transform castPoint, Vector3 lookDir, bool primary = false)
    {
        if (inactiveSpellColliders.Count == 0)
        {
            ServerInstantiateNewCollider();
        }

        Elements element = Spell.coreNode.Element;
        SpellTypes type = Spell.coreNode.Type;
        int sfxIndex = AudioController.instance != null ? AudioController.instance.GetRandomSpellSFXIndex(element, type) : -1;

        SpellCollider collider = inactiveSpellColliders[0];
        inactiveSpellColliders.RemoveAt(0);
        activeSpellColliders.Add(collider);
        collider.transform.position = castPoint.position;
        collider.transform.rotation = Quaternion.LookRotation(lookDir, Vector3.up);
        collider.SpawnTransform = castPoint;
        collider.SpawnPoint = castPoint.position;
        collider.Initialize(Spell, primary);
        collider.gameObject.SetActive(true);

        RPCInitializeSpellCollider(collider.gameObject, element, type, sfxIndex);
    }
    [ClientRpc]
    public void RPCInitializeSpellCollider(GameObject collider, Elements element, SpellTypes type, int sfxIndex)
    {
        collider.SetActive(true);

        SpellCollider spellCollider = collider.GetComponent<SpellCollider>();

        if (spellCollider != null)
        {
            spellCollider.PlaySpellSFX(element, type, sfxIndex);
        }
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
