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
        if (this == grid.hexGridNodes[0] && !(grid.selectedNode.Node is SpellType)) return;
        if (!VerifyNearbyConnections(grid.selectedNode)) return;
        MakeNearbyConnections(grid.selectedNode);
        grid.AddSelectedToGrid(this);
        
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
        /*if (grid.hexGridNodes[0] == this)
        {
            return true;
        }*/
        /*if (neighbours.Length == 0)
        {
            return true;
        }*/
        for (int i = 0; i < neighbours.Length; i++)
        {
            if (neighbours[i] != null && neighbours[i].spellNode != null && neighbours[i].spellNode != spell)
            {
                if (!spell.CheckConectNode(neighbours[i].spellNode, i))
                {
                    return false;
                }
            }
        }
        return true;
    }
    public void MakeNearbyConnections(SpellNodeInterface spell)
    {
        /*if (grid.hexGridNodes[0] == this)
        {
            return true;
        }*/
        /*if (neighbours.Length == 0)
        {
            return true;
        }*/
        for (int i  = 0; i < neighbours.Length; i++)
        {
            if (neighbours[i] != null && neighbours[i].spellNode != null && neighbours[i].spellNode != spell)
            {
                if(!spell.TryConectNode(neighbours[i].spellNode, i))
                {
                    Debug.LogError("Failed to conect");
                }
            }
        }
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
