using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HexGridNode : MonoBehaviour, IDropZone, IDropHandler
{
    public RectTransform rect;
    public Button button;
    public HexGridNode[] neighbours = new HexGridNode[6];
    public SpellNodeInterface spellNode;
    public HexGrid grid;
    public int index;
    public int Layer;

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

    public bool CanReceive(DraggableNode node)
    {
        if (spellNode != null) return false;
        var spell = node.GetComponent<SpellNodeInterface>();
        if (spell == null) return false;
        return CanConnect(spell);
    }

    public void Receive(DraggableNode node)
    {
        var spell = node.GetComponent<SpellNodeInterface>();
        if (spell == null) return;

        node.transform.SetParent(transform, false);
        node.transform.localPosition = Vector3.zero;
        node.SetOriginZone(this);
        ConnectNode(spell);
    }

    public void Release(DraggableNode node)
    {
        if (spellNode == null) return;

        VerifyNearbyBreakConections(spellNode);
        grid.spellNodes[index] = null;
        spellNode.hexGridNode = null;
        spellNode = null;
        SetNodeButtonState(true);
        grid.ConfigurateSpell();

        if (node.isClone && node.inventorySource != null)
        {
            var inventory = node.inventorySource.OriginZone as NodeInventory;
            var spellNode = node.inventorySource.GetComponent<SpellNodeInterface>();
            if (spellNode != null)
                GameManager.Instance.uiController.playerUI.caster.SetNodeInUse(spellNode.Node, false);
            Destroy(node.gameObject);
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        var node = eventData.pointerDrag?.GetComponent<DraggableNode>();
        if (node == null) return;
        if (!CanReceive(node)) return;
        node.RegisterDrop(this);
    }

    public void SetNodeButtonState(bool state)
    {
        button.interactable = state;
    }

    public void AddNeighbours(HexGridNode node, int index)
    {
        neighbours[index] = node;
    }

    bool CanConnect(SpellNodeInterface node)
    {
        bool isRoot = this == grid.hexGridNodes[0];
        bool isSpellType = node.Node is SpellType;
        if (isRoot != isSpellType) return false;
        if (!VerifyNearbyConnections(node)) return false;
        return true;
    }

    public void ConnectNode(SpellNodeInterface node)
    {
        if (node == null) return;
        MakeNearbyConnections(node);
        grid.AddNodeToGrid(this, node);
        spellNode = node;
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
                spell.TryConectNode(neighbours[i].spellNode, i);
                /*if (!spell.TryConectNode(neighbours[i].spellNode, i))
                {
                    Debug.LogError("Failed to conect");
                }*/
            }
        }
    }

    public void VerifyNearbyBreakConections(SpellNodeInterface spellNode)
    {
        for (int i = 0; i < neighbours.Length;i++)
        {
            if(neighbours[i] != null && neighbours[i].spellNode != null)
            {
                spellNode.BreakConection(i);
            }
        }
    }
}
