using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
public class NodeInventory : NetworkBehaviour, IDropZone
{
    [Header("Prefab Nodes")]
    public List<SpellNodeInterface> nodePrefabs = new List<SpellNodeInterface>();
    private List<SpellNodeInterface> activeNodes = new List<SpellNodeInterface>();
    public ContentSizeFitter contentSizeFitter;
    public LayoutGroup layoutGroup;
    public UICommandController commander;

    void Start()
    {
        if(isLocalPlayer)
        {
            GenerateInventory();
            if(!isServer)
            {
                CMDGenerateInventory();
            }
        }
    }

    public bool CanReceive(DraggableNode node) => true;

    public void Release(DraggableNode node) { }

    public void Receive(DraggableNode node)
    {
        node.SetOriginZone(this as IDropZone);
        var spellNode = node.GetComponent<SpellNodeInterface>();
        if (spellNode != null)
            AddNodeToInventory(spellNode);
    }

    [Command]
    public void CMDReceive(GameObject go)
    {
        DraggableNode node = go.GetComponent<DraggableNode>();
        node.SetOriginZone(this as IDropZone);
        var spellNode = node.GetComponent<SpellNodeInterface>();
        if (spellNode != null)
            AddNodeToInventory(spellNode);
    }

    public void Freeze()
    {
        if (contentSizeFitter != null) contentSizeFitter.enabled = false;
        if (layoutGroup != null) layoutGroup.enabled = false;
    }

    public void Unfreeze()
    {
        if (layoutGroup != null) layoutGroup.enabled = true;
        if (contentSizeFitter != null) contentSizeFitter.enabled = true;
    }


    public void GenerateInventory()
    {
        foreach (var prefab in nodePrefabs)
        {
            var instance = Instantiate(prefab, transform);
            activeNodes.Add(instance);
            instance.GetComponent<SpellNodeInterface>().inventory = this;
            var draggable = instance.GetComponent<DraggableNode>();
            if (draggable != null)
                draggable.SetOriginZone(this as IDropZone);
        }
    }

    [Command]
    public void CMDGenerateInventory()
    {
        foreach (var prefab in nodePrefabs)
        {
            var instance = Instantiate(prefab, transform);
            activeNodes.Add(instance);
            instance.GetComponent<SpellNodeInterface>().inventory = this;
            var draggable = instance.GetComponent<DraggableNode>();
            if (draggable != null)
                draggable.SetOriginZone(this as IDropZone);
        }
    }

    public void AddNodeToInventory(SpellNodeInterface node)
    {
        if (!activeNodes.Contains(node))
            activeNodes.Add(node);

        node.transform.SetParent(transform, false);

        RectTransform rect = node.GetComponent<RectTransform>();
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
        if(!isServer)
        {
            CMDAddNodeToInventory(node.gameObject);
        }
    }

    [Command]
    public void CMDAddNodeToInventory(GameObject go)
    {
        SpellNodeInterface node = go.GetComponent<SpellNodeInterface>();
        if (!activeNodes.Contains(node))
            activeNodes.Add(node);

        node.transform.SetParent(transform, false);

        RectTransform rect = node.GetComponent<RectTransform>();
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
    }

    public void RemoveNodeFromInventory(SpellNodeInterface node)
    {
        if (activeNodes.Contains(node))
        {
            if(!isServer)
            {
                CMDRemoveNodeFromInventory(activeNodes.IndexOf(node));
            }
            activeNodes.Remove(node);
        }
    }

    [Command]
    public void CMDRemoveNodeFromInventory(int ind)
    {
        activeNodes.RemoveAt(ind);
    }
}