using Mirror;
using Mirror.BouncyCastle.Utilities.Encoders;
using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using static NodeConection;

public class UICommandController : NetworkBehaviour
{
    public List<HexGrid> grids = new List<HexGrid>();
    public List<SpellCardUI> cards = new List<SpellCardUI>();
    public List<DraggableNode> drags = new List<DraggableNode>();
    public List<SpellNodeInterface> interfaces = new List<SpellNodeInterface>();


    public void ConfigurateSpell(HexGrid grid)
    {
        if(isLocalPlayer && !isServer)
        {
            /*if(NetworkClient.ready)
            {
                CMDConfigurateSpell(grids.IndexOf(grid));
            }
            else
            {
                StartCoroutine(WaitConfigurateSpell(grid));
            }*/
            StartCoroutine(WaitConfigurateSpell(grid));
        }
    }
    IEnumerator WaitConfigurateSpell(HexGrid grid)
    {
        yield return new WaitUntil(() => grids.Contains(grid));
        yield return new WaitUntil(() => NetworkClient.ready);
        CMDConfigurateSpell(grids.IndexOf(grid));
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
            /*if(NetworkClient.ready)
            {
                CMDReturnAllNodesToInventory(grids.IndexOf(grid));
            }
            else
            {
                StartCoroutine(WaitReturnAllNodesToInventory(grid));
            }*/
            StartCoroutine(WaitReturnAllNodesToInventory(grid));
        }
    }

    IEnumerator WaitReturnAllNodesToInventory(HexGrid grid)
    {
        yield return new WaitUntil(() => grids.Contains(grid));
        yield return new WaitUntil(() => NetworkClient.ready);
        CMDReturnAllNodesToInventory(grids.IndexOf(grid));
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
            /*if (NetworkClient.ready)
            {
                CMDAddNodeToGrid(grid.hexGridNodes.IndexOf(hex), interfaces.IndexOf(node), grids.IndexOf(grid));
            }
            else
            {
                StartCoroutine(WaitAddNodeToGrid(hex, node, grid));
            }*/
            StartCoroutine(WaitAddNodeToGrid(hex, node, grid));
        }
    }
    IEnumerator WaitAddNodeToGrid(HexGridNode hex, SpellNodeInterface node, HexGrid grid)
    {
        yield return new WaitUntil(() => interfaces.Contains(node));
        yield return new WaitUntil(() => NetworkClient.ready);
        CMDAddNodeToGrid(grid.hexGridNodes.IndexOf(hex), interfaces.IndexOf(node), grids.IndexOf(grid));
    }

    [Command]
    public void CMDAddNodeToGrid(int hexInd, int nodeInd, int gridInd)
    {
        HexGridNode hex = grids[gridInd].hexGridNodes[hexInd];
        SpellNodeInterface node = interfaces[nodeInd];
        if (node == null) return;
        if (grids[gridInd].caster == null || grids[gridInd].caster.inventory == null)
        {
            return;
        }
        grids[gridInd].caster.inventory.RemoveNodeFromInventory(node);
        if (node.hexGridNode != null)
        {
            node.hexGridNode.VerifyNearbyBreakConections(node);
            node.hexGridNode.spellNode = null;
            node.hexGridNode.SetNodeButtonState(true);
        }
        hex.spellNode = node;
        node.hexGridNode = hex;
        grids[gridInd].spellNodes[hex.index] = node;
        hex.SetNodeButtonState(false);
        RectTransform rect = node.GetComponent<RectTransform>();
        node.transform.SetParent(grids[gridInd].nodeContainer, false);
        rect.position = hex.rect.position;
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
        grids[gridInd].ConfigurateSpell();
    }

    /*public void InitializeSNI(SpellNodeInterface sni)
    {
        if (isLocalPlayer && !isServer)
        {
            CMDInitializeSNI(interfaces.IndexOf(sni));
        }
    }

    [Command]
    public void CMDInitializeSNI(int index)
    {
        SpellNodeInterface sni = interfaces[index];
        sni.rect = GetComponent<RectTransform>();

        sni.borderImg = transform.GetChild(0).GetComponent<Image>();

        sni.usedNodeImg = transform.GetChild(1).gameObject;
        /*sni.Node = Instantiate(sni.PrefabNode);
        sni.Node.Interface = sni;
        sni.Node.Initialize();
        sni.InitializeConections();
        sni.GetComponent<Image>().color *= sni.PrefabNode.color;
        sni.GetComponent<Image>().alphaHitTestMinimumThreshold = 0.1f;
        sni.rect = sni.GetComponent<RectTransform>();
        sni.borderImg = sni.transform.GetChild(0).GetComponent<Image>();
    }*/


    public void UpdateSNIConnected(SpellNodeInterface sni)
    {
        if (isLocalPlayer && !isServer)
        {
            /*if (NetworkClient.ready)
            {
                CMDUpdateSNIConnected(interfaces.IndexOf(sni));
            }
            else
            {
                StartCoroutine(WaitUpdateSNIConnected(sni));
            }*/
            StartCoroutine(WaitUpdateSNIConnected(sni));
        }
    }
    IEnumerator WaitUpdateSNIConnected(SpellNodeInterface sni)
    {
        yield return new WaitUntil(() => interfaces.Contains(sni));
        yield return new WaitUntil(() => NetworkClient.ready);
        CMDUpdateSNIConnected(interfaces.IndexOf(sni));
    }

    [Command]
    public void CMDUpdateSNIConnected(int index)
    {
        SpellNodeInterface sni = interfaces[index];
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
            /*if (NetworkClient.ready)
            {
                CMDBreakSNIConnection(interfaces.IndexOf(sni), Index);
            }
            else
            {
                StartCoroutine(WaitBreakSNIConnection(sni, Index));
            }*/
            StartCoroutine(WaitBreakSNIConnection(sni, Index));
        }
    }
    IEnumerator WaitBreakSNIConnection(SpellNodeInterface sni, int Index)
    {
        yield return new WaitUntil(() => interfaces.Contains(sni));
        yield return new WaitUntil(() => NetworkClient.ready);
        CMDBreakSNIConnection(interfaces.IndexOf(sni), Index);
    }

    [Command]
    public void CMDBreakSNIConnection(int nodeInd, int Index)
    {
        SpellNodeInterface sni = interfaces[nodeInd];
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

    /*public void UpdateSNIConnectionPorts(SpellNodeInterface sni)
    {
        if (isLocalPlayer && !isServer)
        {
            CMDUpdateSNIConnectionPorts(interfaces.IndexOf(sni));
        }
    }
    [Command]
    public void CMDUpdateSNIConnectionPorts(int index)
    {
        SpellNodeInterface sni = interfaces[index];
        for (int i = 0; i < sni.conections.Length; i++)
        {
            if (sni.conections[i] != null)
            {
                sni.conections[i].conectionType = sni.Node.ConectionPorts[i];
            }
        }
    }*/

    public void SetUsedSNI(SpellNodeInterface sni, bool used)
    {
        if (isLocalPlayer && !isServer)
        {
            /*if (NetworkClient.ready)
            {
                CMDSetUsedSNI(interfaces.IndexOf(sni), used);
            }
            else
            {
                StartCoroutine(WaitSetUsedSNI(sni, used));
            }*/
            StartCoroutine(WaitSetUsedSNI(sni, used));
        }
    }
    IEnumerator WaitSetUsedSNI(SpellNodeInterface sni, bool used)
    {
        yield return new WaitUntil(() => interfaces.Contains(sni));
        yield return new WaitUntil(() => NetworkClient.ready);
        CMDSetUsedSNI(interfaces.IndexOf(sni), used);
    }

    [Command]
    public void CMDSetUsedSNI(int index, bool used)
    {
        SpellNodeInterface sni = interfaces[index];
        sni.Node.IsInUse = used;

        sni.usedNodeImg.SetActive(used);
    }

    public void DeleteSCUI(SpellCardUI scui)
    {
        if (isLocalPlayer && !isServer)
        {
            /*if (NetworkClient.ready)
            {
                CMDDeleteSCUI(cards.IndexOf(scui));
            }
            else
            {
                StartCoroutine(WaitDeleteSCUI(scui));
            }*/
            StartCoroutine(WaitDeleteSCUI(scui));
        }
    }
    IEnumerator WaitDeleteSCUI(SpellCardUI scui)
    {
        yield return new WaitUntil(() => cards.Contains(scui));
        yield return new WaitUntil(() => NetworkClient.ready);
        CMDDeleteSCUI(cards.IndexOf(scui));
    }

    [Command]
    public void CMDDeleteSCUI(int index)
    {
        SpellCardUI scui = cards[index];
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
            /*if (NetworkClient.ready)
            {
                CMDSelectSCUI(cards.IndexOf(scui));
            }
            else
            {
                StartCoroutine(WaitSelectSCUI(scui));
            }*/
            StartCoroutine(WaitSelectSCUI(scui));
        }
    }
    IEnumerator WaitSelectSCUI(SpellCardUI scui)
    {
        yield return new WaitUntil(() => cards.Contains(scui));
        yield return new WaitUntil(() => NetworkClient.ready);
        CMDSelectSCUI(cards.IndexOf(scui));
    }

    [Command]
    public void CMDSelectSCUI(int index)
    {
        SpellCardUI scui = cards[index];
        GameManager.Instance.uiController.playerUI.StartEquipSpell(scui.boundSpell);
    }

    public void InitializeHex(HexGridNode hex)
    {
        if (isLocalPlayer && !isServer)
        {
            /*if (NetworkClient.ready)
            {
                CMDInitializeHex(hex.grid.hexGridNodes.IndexOf(hex), grids.IndexOf(hex.grid));
            }
            else
            {
                StartCoroutine(WaitInitializeHex(hex));
            }*/
            StartCoroutine(WaitInitializeHex(hex));
        }
    }
    IEnumerator WaitInitializeHex(HexGridNode hex)
    {
        //yield return new WaitUntil(() => grids.Contains(hex.grid));
        yield return new WaitUntil(() => grids.Contains(hex.grid));
        yield return new WaitUntil(() => NetworkClient.ready);
        CMDInitializeHex(hex.grid.hexGridNodes.IndexOf(hex), grids.IndexOf(hex.grid));
    }

    [Command]
    public void CMDInitializeHex(int hexInd, int gridInd)
    {
        HexGridNode hex = grids[gridInd].hexGridNodes[hexInd];
        hex.rect = GetComponent<RectTransform>();
        hex.button = GetComponent<Button>();
        hex.GetComponent<Image>().alphaHitTestMinimumThreshold = 0.1f;
    }

    public void HexReceive(DraggableNode node, HexGridNode hex)
    {
        if (isLocalPlayer && !isServer)
        {
            /*if (NetworkClient.ready)
            {
                CMDHexReceive(drags.IndexOf(node), hex.grid.hexGridNodes.IndexOf(hex), grids.IndexOf(hex.grid));
            }
            else
            {
                StartCoroutine(WaitHexReceive(node, hex));
            }*/
            StartCoroutine(WaitHexReceive(node, hex));
        }
    }
    IEnumerator WaitHexReceive(DraggableNode node, HexGridNode hex)
    {
        yield return new WaitUntil(() => drags.Exists(d => d.GetInstanceID() == node.GetInstanceID()));
        yield return new WaitUntil(() => NetworkClient.ready);
        CMDHexReceive(drags.IndexOf(node), hex.grid.hexGridNodes.IndexOf(hex), grids.IndexOf(hex.grid));
    }

    [Command]
    public void CMDHexReceive(int nodeInd, int hexInd, int gridInd)
    {
        DraggableNode node = drags[nodeInd];
        HexGridNode hex = grids[gridInd].hexGridNodes[hexInd];
        var spell = node.GetComponent<SpellNodeInterface>();
        if (spell == null) return;

        node.transform.SetParent(hex.transform, false);
        node.transform.localPosition = Vector3.zero;
        node.SetOriginZone(hex);
        hex.ConnectNode(spell);
    }

    public void HexRelease(HexGridNode hex, DraggableNode node)
    {
        if (isLocalPlayer && !isServer)
        {
            /*if (NetworkClient.ready)
            {
                CMDHexRelease(hex.grid.hexGridNodes.IndexOf(hex), grids.IndexOf(hex.grid), drags.IndexOf(node));
            }
            else
            {
                StartCoroutine(WaitHexRelease(hex,node));
            }*/
            StartCoroutine(WaitHexRelease(hex,node));
        }
    }
    IEnumerator WaitHexRelease(HexGridNode hex, DraggableNode node)
    {
        yield return new WaitUntil(() => drags.Contains(node));
        yield return new WaitUntil(() => NetworkClient.ready);
        CMDHexRelease(hex.grid.hexGridNodes.IndexOf(hex), grids.IndexOf(hex.grid), drags.IndexOf(node));
    }

    [Command]
    public void CMDHexRelease(int hexInd, int gridInd, int nodeInd)
    {
        HexGridNode hex = grids[gridInd].hexGridNodes[hexInd];
        DraggableNode node = drags[nodeInd];
        if (hex.spellNode == null) return;

        hex.VerifyNearbyBreakConections(hex.spellNode);
        hex.grid.spellNodes[hex.index] = null;
        hex.spellNode.hexGridNode = null;
        hex.spellNode = null;
        hex.SetNodeButtonState(true);
        hex.grid.ConfigurateSpell();
        if (node.isClone && node.inventorySource != null)
        {
            var inventory = node.inventorySource.OriginZone as NodeInventory;
            var spellNode = node.inventorySource.GetComponent<SpellNodeInterface>();
            if (spellNode != null)
                GameManager.Instance.uiController.playerUI.caster.SetNodeInUse(spellNode.Node, false);
            //Destroy(node.gameObject);
        }
    }
}
