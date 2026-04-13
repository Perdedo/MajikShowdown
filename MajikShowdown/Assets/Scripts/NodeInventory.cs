using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NodeInventory : MonoBehaviour, IDropZone
{
    [Header("Prefab Nodes")]
    public List<SpellNodeInterface> nodePrefabs = new List<SpellNodeInterface>();
    private List<SpellNodeInterface> activeNodes = new List<SpellNodeInterface>();
    public ContentSizeFitter contentSizeFitter;
    public LayoutGroup layoutGroup;

    void Start()
    {
        GenerateInventory();
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
    }

    public void RemoveNodeFromInventory(SpellNodeInterface node)
    {
        if (activeNodes.Contains(node))
            activeNodes.Remove(node);
    }
}