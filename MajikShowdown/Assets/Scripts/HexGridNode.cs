using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class HexGridNode : MonoBehaviour
{
    public RectTransform rect;
    public Button button;
    public HexGridNode[] neighbours = new HexGridNode[6];
    public SpellNodeInterface spellNode;
    public HexGrid grid;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        button = GetComponent<Button>();
        this.GetComponent<Image>().alphaHitTestMinimumThreshold = 0.1f;
    }
    public void SetGrid(HexGrid Grid)
    {
        grid = Grid;
    }

    public void ConnectNode()
    {
        if (grid.selectedNode == null) return;
        if (!VerifyNearbyConnections(grid.selectedNode)) return;

        NodeInventory.instance.RemoveNodeFromInventory(grid.selectedNode,grid);
        grid.selectedNode.rect.position = this.rect.position;
        if(grid.selectedNode.hexGridNode != null)
        {
            grid.selectedNode.hexGridNode.VerifyNearbyBreakConections(grid.selectedNode);
            grid.selectedNode.hexGridNode.spellNode = null;
            grid.selectedNode.hexGridNode.SetNodeButtonState(true);
        }
        spellNode = grid.selectedNode;
        grid.selectedNode.hexGridNode = this;
        grid.selectedNode = null;
        SetNodeButtonState(false);
    }

    public void SetNodeButtonState(bool state)
    {
        button.interactable = state;
    }

    public void AddNeighbours(HexGridNode node, int index)
    {
        neighbours[index] = node;
    }

    public bool VerifyNearbyConnections(SpellNodeInterface spell)
    {
        if (grid.hexGridNodes[0] == this)
        {
            return true;
        }
        for (int i  = 0; i < neighbours.Length; i++)
        {
            if (neighbours[i] != null && neighbours[i].spellNode != null)
            {
                if(spell.TryConectNode(neighbours[i].spellNode, i))
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void VerifyNearbyBreakConections(SpellNodeInterface spell)
    {
        for (int i = 0; i < neighbours.Length;i++)
        {
            if(neighbours[i] != null && neighbours[i].spellNode != null)
            {
                spell.BreakConection(i);
            }
        }
    }
}
