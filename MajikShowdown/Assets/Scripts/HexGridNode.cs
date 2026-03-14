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

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        button = GetComponent<Button>();
        this.GetComponent<Image>().alphaHitTestMinimumThreshold = 0.1f;
    }

    public void ConnectNode()
    {
        if (HexGrid.instance.selectedNode == null) return;
        if (!VerifyNearbyConnections(HexGrid.instance.selectedNode)) return;

        NodeInventory.instance.RemoveNodeFromInventory(HexGrid.instance.selectedNode);
        HexGrid.instance.selectedNode.rect.position = this.rect.position;
        if(HexGrid.instance.selectedNode.hexGridNode != null)
        {
            HexGrid.instance.selectedNode.hexGridNode.VerifyNearbyBreakConections(HexGrid.instance.selectedNode);
            HexGrid.instance.selectedNode.hexGridNode.spellNode = null;
            HexGrid.instance.selectedNode.hexGridNode.SetNodeButtonState(true);
        }
        spellNode = HexGrid.instance.selectedNode;
        HexGrid.instance.selectedNode.hexGridNode = this;
        HexGrid.instance.selectedNode = null;
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
        if (HexGrid.instance.hexGridNodes[0] == this)
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
