using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class NodeInventory : MonoBehaviour
{
    public List<SpellNodeInterface> nodes = new List<SpellNodeInterface>();
    public static NodeInventory instance;

    void Start()
    {
        instance = this;
        GenerateInventory();
    }

    public void AddNodeToInventory(SpellNodeInterface node)
    {
        nodes.Add(node);
        node.transform.parent = this.transform;
    }

    public void RemoveNodeFromInventory(SpellNodeInterface node)
    {
        nodes.Remove(node);
        node.transform.parent = HexGrid.instance.transform;
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

        if (HexGrid.instance.selectedNode == null || HexGrid.instance.selectedNode.hexGridNode == null) return;

        HexGrid.instance.selectedNode.hexGridNode.SetNodeButtonState(true);
        HexGrid.instance.selectedNode.hexGridNode.VerifyNearbyBreakConections(HexGrid.instance.selectedNode);
        HexGrid.instance.selectedNode.hexGridNode.spellNode = null;
        HexGrid.instance.selectedNode.hexGridNode = null;
        AddNodeToInventory(HexGrid.instance.selectedNode);
        HexGrid.instance.selectedNode = null;
    }
}
