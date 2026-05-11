using Mirror;
using Mirror.BouncyCastle.Utilities.Encoders;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static NodeConection;

public class UICommandController : NetworkBehaviour
{
    public List<HexGrid> grids = new List<HexGrid>();
    public void ConfigurateSpell(HexGrid grid)
    {
        if(isLocalPlayer && !isServer)
        {
            CMDConfigurateSpell(grids.IndexOf(grid));
        }
    }

    [Command]
    public void CMDConfigurateSpell(int ind)
    {
        grids[ind].spell.spellNodes.Clear();
        foreach (var node in grids[ind].spellNodes)
        {
            if (node != null)
            {
                node.Node.hierarchy = node.hexGridNode.Layer;
                grids[ind].spell.spellNodes.Add(node.Node);
            }

        }
        if (grids[ind].hexGridNodes[0].spellNode != null && grids[ind].hexGridNodes[0].spellNode.Node is SpellType t)
        {
            grids[ind].spell.validSpell = true;
            grids[ind].spell.primaryNode = t;
        }
        else
        {
            grids[ind].spell.validSpell = false;
            grids[ind].spell.primaryNode = null;
        }

        grids[ind].spell.UpdateSpell();
    }


    public void ReturnAllNodesToInventory(HexGrid grid)
    {
        if (isLocalPlayer && !isServer)
        {
            CMDReturnAllNodesToInventory(grids.IndexOf(grid));
        }
    }

    [Command]
    public void CMDReturnAllNodesToInventory(int ind)
    {
        if (grids[ind].caster == null || grids[ind].caster.inventory == null) return;

        foreach (var node in grids[ind].spellNodes)
        {
            if (node != null)
            {
                if (node.hexGridNode != null)
                {
                    node.hexGridNode.spellNode = null;
                    node.hexGridNode.SetNodeButtonState(true);
                }
                node.hexGridNode = null;
                node.Node.ResetNode();
                grids[ind].caster.inventory.AddNodeToInventory(node);
            }
        }
        for (int i = 0; i < grids[ind].spellNodes.Count; i++)
        {
            grids[ind].spellNodes[i] = null;
        }
        grids[ind].ConfigurateSpell();
    }

    public void AddNodeToGrid(HexGridNode hex, SpellNodeInterface node, HexGrid grid)
    {
        if (isLocalPlayer && !isServer)
        {
            CMDAddNodeToGrid(hex.gameObject, node.gameObject, grids.IndexOf(grid));
        }
    }

    [Command]
    public void CMDAddNodeToGrid(GameObject goHex, GameObject goNode, int ind)
    {
        HexGridNode hex = goHex.GetComponent<HexGridNode>();
        SpellNodeInterface node = goNode.GetComponent<SpellNodeInterface>();
        if (node == null) return;
        if (grids[ind].caster == null || grids[ind].caster.inventory == null)
        {
            return;
        }
        grids[ind].caster.inventory.RemoveNodeFromInventory(node);
        if (node.hexGridNode != null)
        {
            node.hexGridNode.VerifyNearbyBreakConections(node);
            node.hexGridNode.spellNode = null;
            node.hexGridNode.SetNodeButtonState(true);
        }
        hex.spellNode = node;
        node.hexGridNode = hex;
        grids[ind].spellNodes[hex.index] = node;
        hex.SetNodeButtonState(false);
        RectTransform rect = node.GetComponent<RectTransform>();
        node.transform.SetParent(grids[ind].nodeContainer, false);
        rect.position = hex.rect.position;
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
        grids[ind].ConfigurateSpell();
    }

    public void InitializeSNI(SpellNodeInterface sni)
    {
        if (isLocalPlayer && !isServer)
        {
            CMDInitializeSNI(sni.gameObject);
        }
    }

    [Command]
    public void CMDInitializeSNI(GameObject go)
    {
        SpellNodeInterface sni = go.GetComponent<SpellNodeInterface>();
        sni.Node = Instantiate(sni.PrefabNode);
        sni.Node.Interface = sni;
        sni.Node.Initialize();
        sni.InitializeConections();
        sni.GetComponent<Image>().color *= sni.PrefabNode.color;
        sni.GetComponent<Image>().alphaHitTestMinimumThreshold = 0.1f;
        sni.rect = sni.GetComponent<RectTransform>();
        sni.borderImg = sni.transform.GetChild(0).GetComponent<Image>();
    }


    public void UpdateSNIConnected(SpellNodeInterface sni)
    {
        if (isLocalPlayer && !isServer)
        {
            CMDUpdateSNIConnected(sni.gameObject);
        }
    }

    [Command]
    public void CMDUpdateSNIConnected(GameObject go)
    {
        SpellNodeInterface sni = go.GetComponent<SpellNodeInterface>();
        for (int i = 0; i < sni.conections.Length; i++)
        {
            if (sni.conections[i] != null)
            {
                sni.Node.ConectedNodes[i] = sni.conections[i].GetNode();
            }
            else
            {
                sni.Node.ConectedNodes[i] = null;
            }
        }
    }
    public void BreakSNIConnection(SpellNodeInterface sni, int Index)
    {
        if (isLocalPlayer && !isServer)
        {
            CMDBreakSNIConnection(sni.gameObject, Index);
        }
    }

    [Command]
    public void CMDBreakSNIConnection(GameObject go, int Index)
    {
        SpellNodeInterface sni = go.GetComponent<SpellNodeInterface>();
        if (Index >= sni.conections.Length)
        {
            return;
        }
        //ConectedNodes[Index] = null;
        SpellNode aux = sni.conections[Index].GetNode();
        //Debug.Log(aux);
        //Debug.Log(aux.Interface);
        if (aux != null)
        {
            sni.conections[Index].RemoveConection();
            aux.Interface.UpdateConected();
            sni.UpdateConected();
            var spell = sni.Node.OwnerSpell;
            if (spell != null)
            {
                spell.UpdateSpell();
            }
        }
    }

    public void UpdateSNIConnectionPorts(SpellNodeInterface sni)
    {
        if (isLocalPlayer && !isServer)
        {
            CMDUpdateSNIConnectionPorts(sni.gameObject);
        }
    }

    [Command]
    public void CMDUpdateSNIConnectionPorts(GameObject go)
    {
        SpellNodeInterface sni = go.GetComponent<SpellNodeInterface>();
        for (int i = 0; i < sni.conections.Length; i++)
        {
            if (sni.conections[i] != null)
            {
                sni.conections[i].conectionType = sni.ConectionPorts[i];
            }
        }
    }

    public void DeleteSCUI(SpellCardUI scui)
    {
        if (isLocalPlayer && !isServer)
        {
            CMDDeleteSCUI(scui.gameObject);
        }
    }

    [Command]
    public void CMDDeleteSCUI(GameObject go)
    {
        SpellCardUI scui = go.GetComponent<SpellCardUI>();
        if (scui.boundSpell == null) return;
        for (int i = 0; i < scui.boundSpell.Caster.equippedSpells.Length; i++)
        {
            if (scui.boundSpell.Caster.equippedSpells[i] == scui.boundSpell)
            {
                scui.boundSpell.Caster.equippedSpells[i] = null;
                GameManager.Instance.uiController.playerUI.equipSlotTexts[i].text = "Spell Slot " + (i + 1);
            }
        }
        if (scui.boundSpell.grid != null)
        {
            scui.boundSpell.grid.ReturnAllNodesToInventory();
            Destroy(scui.boundSpell.grid.gameObject);
        }
        scui.boundSpell.Caster.spells.Remove(scui.boundSpell);
        scui.boundSpell.OnSpellUpdated -= scui.RefreshUI;
        Destroy(gameObject);
        scui.boundSpell = null;
    }

    public void SelectSCUI(SpellCardUI scui)
    {
        if (isLocalPlayer && !isServer)
        {
            CMDSelectSCUI(scui.gameObject);
        }
    }

    [Command]
    public void CMDSelectSCUI(GameObject go)
    {
        SpellCardUI scui = go.GetComponent<SpellCardUI>();
        GameManager.Instance.uiController.playerUI.StartEquipSpell(scui.boundSpell);
    }

    public void InitializeHex(HexGridNode hex)
    {
        if (isLocalPlayer && !isServer)
        {
            CMDInitializeHex(hex.gameObject);
        }
    }

    [Command]
    public void CMDInitializeHex(GameObject go)
    {
        HexGridNode hex = go.GetComponent<HexGridNode>();
        hex.rect = GetComponent<RectTransform>();
        hex.button = GetComponent<Button>();
        hex.GetComponent<Image>().alphaHitTestMinimumThreshold = 0.1f;
    }

    public void HexReceive(DraggableNode node, HexGridNode hex)
    {
        if (isLocalPlayer && !isServer)
        {
            CMDHexReceive(node.gameObject, hex.gameObject);
        }
    }

    [Command]
    public void CMDHexReceive(GameObject goNode, GameObject goHex)
    {
        DraggableNode node = goNode.GetComponent<DraggableNode>();
        HexGridNode hex = goHex.GetComponent<HexGridNode>();
        var spell = node.GetComponent<SpellNodeInterface>();
        if (spell == null) return;

        node.transform.SetParent(hex.transform, false);
        node.transform.localPosition = Vector3.zero;
        node.SetOriginZone(hex);
        hex.ConnectNode(spell);
    }

    public void HexRelease(HexGridNode hex)
    {
        if (isLocalPlayer && !isServer)
        {
            CMDHexRelease(hex.gameObject);
        }
    }

    [Command]
    public void CMDHexRelease(GameObject go)
    {
        HexGridNode hex = go.GetComponent<HexGridNode>();
        if (hex.spellNode == null) return;

        hex.VerifyNearbyBreakConections(hex.spellNode);
        hex.grid.spellNodes[hex.index] = null;
        hex.spellNode.hexGridNode = null;
        hex.spellNode = null;
        hex.SetNodeButtonState(true);
        hex.grid.ConfigurateSpell();
    }
}
