using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class NodeInventory : MonoBehaviour
{
    public List<SpellNodeInterface> nodes = new List<SpellNodeInterface>();

    void Start()
    {
        GenerateInventory();
    }

    public void AddNodeToInventory(SpellNodeInterface node)
    {
        nodes.Add(node);
        node.transform.SetParent(transform);
    }

    public void RemoveNodeFromInventory(SpellNodeInterface node, HexGrid NewParent)
    {
        nodes.Remove(node);
        node.transform.SetParent(NewParent.transform);
    }

    public void GenerateInventory()
    {
        for(int i = 0; i < nodes.Count; i++)
        {
            Instantiate(nodes[i], this.transform);
        }
    }

    public void ReturnNode()
    {
        HexGrid active = GameManager.Instance.uiController.activeGrid;
        if (active.selectedNode == null || active.selectedNode.hexGridNode == null) return;
        AddNodeToInventory(active.selectedNode);
        active.RemoveSelectedFromGrid();
    }
}
