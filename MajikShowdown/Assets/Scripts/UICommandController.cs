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
    public bool network = true;

    public void ConfigurateSpell(HexGrid grid)
    {
        if(!network)
        {
            return;
        }
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
        //yield return new WaitUntil(() => grids.Contains(grid));
        yield return new WaitUntil(() => grids.Exists(g => g.instanceIndex == grid.instanceIndex));
        yield return new WaitUntil(() => NetworkClient.ready);
        CMDConfigurateSpell(grid.instanceIndex);
        //CMDConfigurateSpell(grids.IndexOf(grid));
    }

    [Command]
    public void CMDConfigurateSpell(int ind)
    {
        Debug.Log("Config cmd");
        HexGrid grid = grids.Find(g => g.instanceIndex == ind);
        grid.spell.spellNodes.Clear();
        foreach (var node in grid.spellNodes)
        {
            if (node != null)
            {
                node.Node.hierarchy = node.hexGridNode.Layer;
                grid.spell.spellNodes.Add(node.Node);
            }

        }
        if (grid.hexGridNodes[0].spellNode != null && grid.hexGridNodes[0].spellNode.Node is SpellType t)
        {
            grid.spell.validSpell = true;
            grid.spell.coreNode = t;
        }
        else
        {
            grid.spell.validSpell = false;
            grid.spell.coreNode = null;
        }
        grid.spell.UpdateSpell();
    }


    public void ReturnAllNodesToInventory(HexGrid grid)
    {
        if (!network)
        {
            return;
        }
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
        yield return new WaitUntil(() => grids.Exists(g => g.instanceIndex == grid.instanceIndex));
        yield return new WaitUntil(() => NetworkClient.ready);
        CMDReturnAllNodesToInventory(grid.instanceIndex);
    }

    [Command]
    public void CMDReturnAllNodesToInventory(int ind)
    {
        HexGrid grid = grids.Find(g => g.instanceIndex == ind);
        if (grid.caster == null || grid.caster.inventory == null) return;

        foreach (var node in grid.spellNodes)
        {
            if (node != null)
            {
                if (node.hexGridNode != null)
                {
                    node.hexGridNode.spellNode = null;
                    //node.hexGridNode.SetNodeButtonState(true);
                }
                node.hexGridNode = null;
                node.Node.ResetNode();
                grid.caster.inventory.AddNodeToInventory(node);
            }
        }
        for (int i = 0; i < grid.spellNodes.Count; i++)
        {
            grid.spellNodes[i] = null;
        }
        grid.ConfigurateSpell();
    }

    public void AddNodeToGrid(HexGridNode hex, SpellNodeInterface node, HexGrid grid)
    {
        if (!network)
        {
            return;
        }
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
        yield return new WaitUntil(() => grids.Exists(g => g.instanceIndex == grid.instanceIndex) && hex.grid.hexGridNodes.Exists(h => h.index == hex.index));
        yield return new WaitUntil(() => interfaces.Exists(i => i.acquisitionOrder == node.acquisitionOrder));
        yield return new WaitUntil(() => NetworkClient.ready);
        CMDAddNodeToGrid(hex.index, node.acquisitionOrder, grid.instanceIndex);
        //CMDAddNodeToGrid(grid.hexGridNodes.IndexOf(hex), node.acquisitionOrder, grids.IndexOf(grid));
    }

    [Command]
    public void CMDAddNodeToGrid(int hexInd, int nodeInd, int gridInd)
    {
        Debug.Log("Add cmd");
        //HexGridNode hex = grids[gridInd].hexGridNodes[hexInd];
        HexGrid grid = grids.Find(g => g.instanceIndex == gridInd);
        HexGridNode hex = grids.Find(g => g.instanceIndex == gridInd).hexGridNodes.Find(h => h.index == hexInd);
        SpellNodeInterface node = interfaces.Find(i => i.acquisitionOrder == nodeInd);
        if (node == null) return;
        if (grid.VerifyNode(node)) return;
        if (grid.caster == null || grid.caster.inventory == null)
        {
            return;
        }
        grid.caster.inventory.RemoveNodeFromInventory(node);
        if (node.hexGridNode != null)
        {
            node.hexGridNode.VerifyNearbyBreakConections(node);
            node.hexGridNode.spellNode = null;
            //node.hexGridNode.SetNodeButtonState(true);
        }
        hex.spellNode = node;
        node.hexGridNode = hex;
        grid.spellNodes[hex.index] = node;
        //hex.SetNodeButtonState(false);
        RectTransform rect = node.GetComponent<RectTransform>();
        node.transform.SetParent(grid.nodeContainer, false);
        rect.position = hex.rect.position;
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
        grid.ConfigurateSpell();
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
        if (!network)
        {
            return;
        }
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
        //yield return new WaitUntil(() => interfaces.Contains(sni));
        yield return new WaitUntil(() => interfaces.Exists(i => i.acquisitionOrder == sni.acquisitionOrder));
        yield return new WaitUntil(() => NetworkClient.ready);
        CMDUpdateSNIConnected(sni.acquisitionOrder);
    }

    [Command]
    public void CMDUpdateSNIConnected(int index)
    {
        SpellNodeInterface sni = interfaces.Find(i => i.acquisitionOrder == index);
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
        if (sni.hexGridNode != null)
        {
            sni.hexGridNode.grid.ConfigurateSpell();
        }
    }
    public void BreakSNIConnection(SpellNodeInterface sni, int Index)
    {
        if (!network)
        {
            return;
        }
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
        //yield return new WaitUntil(() => interfaces.Contains(sni));
        yield return new WaitUntil(() => interfaces.Exists(i => i.acquisitionOrder == sni.acquisitionOrder));
        yield return new WaitUntil(() => NetworkClient.ready);
        CMDBreakSNIConnection(sni.acquisitionOrder, Index);
        //CMDBreakSNIConnection(interfaces.IndexOf(sni), Index);
    }

    [Command]
    public void CMDBreakSNIConnection(int nodeInd, int Index)
    {
        SpellNodeInterface sni = interfaces.Find(i => i.acquisitionOrder == nodeInd);
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
        if (!network)
        {
            return;
        }
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
        //yield return new WaitUntil(() => interfaces.Contains(sni));
        yield return new WaitUntil(() => interfaces.Exists(i => i.acquisitionOrder == sni.acquisitionOrder));
        yield return new WaitUntil(() => NetworkClient.ready);
        CMDSetUsedSNI(sni.acquisitionOrder, used);
        //CMDSetUsedSNI(interfaces.IndexOf(sni), used);
    }

    [Command]
    public void CMDSetUsedSNI(int index, bool used)
    {
        SpellNodeInterface sni = interfaces.Find(i => i.acquisitionOrder == index);
        sni.Node.IsInUse = used;

        sni.usedNodeImg.SetActive(used);
    }

    public void DeleteSCUI(SpellCardUI scui)
    {
        if (!network)
        {
            return;
        }
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
        //yield return new WaitUntil(() => cards.Contains(scui));
        yield return new WaitUntil(() => cards.Exists(c => c.instanceIndex == scui.instanceIndex));
        yield return new WaitUntil(() => NetworkClient.ready);
        CMDDeleteSCUI(scui.instanceIndex);
        //CMDDeleteSCUI(cards.IndexOf(scui));
    }

    [Command]
    public void CMDDeleteSCUI(int index)
    {
        //SpellCardUI scui = cards[index];
        SpellCardUI scui = cards.Find(c => c.instanceIndex == index);
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
        if (!network)
        {
            return;
        }
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
        //yield return new WaitUntil(() => cards.Contains(scui));
        yield return new WaitUntil(() => cards.Exists(c => c.instanceIndex == scui.instanceIndex));
        yield return new WaitUntil(() => NetworkClient.ready);
        CMDSelectSCUI(scui.instanceIndex);
        //CMDSelectSCUI(cards.IndexOf(scui));
    }

    [Command]
    public void CMDSelectSCUI(int index)
    {
        //SpellCardUI scui = cards[index];
        SpellCardUI scui = cards.Find(c => c.instanceIndex == index);
        if (scui.spellInventory != null)
        {
            scui.spellInventory.DeselectAllCards();
        }
        scui.isSelected = true;
        scui.editButton.gameObject.SetActive(true);
        scui.deleteButton.gameObject.SetActive(true);
        scui.cardColor.color = Color.cyan;
        GameManager.Instance.uiController.playerUI.StartEquipSpell(scui.boundSpell);
    }
    public void DeselectSCUI(SpellCardUI scui)
    {
        if (!network)
        {
            return;
        }
        if (isLocalPlayer && !isServer)
        {
            StartCoroutine(WaitDeselectSCUI(scui));
        }
    }
    IEnumerator WaitDeselectSCUI(SpellCardUI scui)
    {
        //yield return new WaitUntil(() => cards.Contains(scui));
        yield return new WaitUntil(() => cards.Exists(c => c.instanceIndex == scui.instanceIndex));
        yield return new WaitUntil(() => NetworkClient.ready);
        CMDDeselectSCUI(scui.instanceIndex);
        //CMDSelectSCUI(cards.IndexOf(scui));
    }

    [Command]
    public void CMDDeselectSCUI(int index)
    {
        //SpellCardUI scui = cards[index];
        SpellCardUI scui = cards.Find(c => c.instanceIndex == index);
        scui.isSelected = false;
        scui.editButton.gameObject.SetActive(false);
        scui.deleteButton.gameObject.SetActive(false);
        scui.cardColor.color = Color.white;
    }

    public void InitializeHex(HexGridNode hex)
    {
        if (!network)
        {
            return;
        }
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
        yield return new WaitUntil(() => grids.Exists(g => g.instanceIndex == hex.grid.instanceIndex) && hex.grid.hexGridNodes.Exists(h => h.index == hex.index));
        yield return new WaitUntil(() => NetworkClient.ready);
        CMDInitializeHex(hex.index, hex.grid.instanceIndex);
        //CMDInitializeHex(hex.grid.hexGridNodes.IndexOf(hex), grids.IndexOf(hex.grid));
    }

    [Command]
    public void CMDInitializeHex(int hexInd, int gridInd)
    {
        HexGridNode hex = grids.Find(g => g.instanceIndex == gridInd).hexGridNodes.Find(h => h.index == hexInd);
        //HexGridNode hex = grids[gridInd].hexGridNodes[hexInd];
        hex.rect = GetComponent<RectTransform>();
        hex.button = GetComponent<Button>();
        hex.GetComponent<Image>().alphaHitTestMinimumThreshold = 0.1f;
    }

    public void HexReceive(DraggableNode node, HexGridNode hex)
    {
        if (!network)
        {
            return;
        }
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
        //yield return new WaitUntil(() => drags.Contains(node));
        yield return new WaitUntil(() => grids.Exists(g => g.instanceIndex == hex.grid.instanceIndex) && hex.grid.hexGridNodes.Exists(h => h.index == hex.index));
        yield return new WaitUntil(() => drags.Exists(d => d.acquisitionOrder == node.acquisitionOrder));
        yield return new WaitUntil(() => NetworkClient.ready);
        CMDHexReceive(node.acquisitionOrder, hex.index, hex.grid.instanceIndex);
        //CMDHexReceive(drags.IndexOf(node), hex.grid.hexGridNodes.IndexOf(hex), grids.IndexOf(hex.grid));
    }

    [Command]
    public void CMDHexReceive(int nodeInd, int hexInd, int gridInd)
    {
        Debug.Log("receive cmd");
        //DraggableNode node = drags[nodeInd];
        DraggableNode node = drags.Find(d => d.acquisitionOrder == nodeInd);
        HexGridNode hex = grids.Find(g => g.instanceIndex == gridInd).hexGridNodes.Find(h => h.index == hexInd);
        //HexGridNode hex = grids[gridInd].hexGridNodes[hexInd];
        var spell = node.GetComponent<SpellNodeInterface>();
        if (spell == null) return;

        node.transform.SetParent(hex.transform, false);
        node.transform.localPosition = Vector3.zero;
        node.SetOriginZone(hex);
        hex.ConnectNode(spell);
    }

    public void HexRelease(HexGridNode hex, DraggableNode node)
    {
        if (!network)
        {
            return;
        }
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
        //yield return new WaitUntil(() => drags.Contains(node));
        yield return new WaitUntil(() => grids.Exists(g => g.instanceIndex == hex.grid.instanceIndex) && hex.grid.hexGridNodes.Exists(h => h.index == hex.index));
        yield return new WaitUntil(() => drags.Exists(d => d.acquisitionOrder == node.acquisitionOrder));
        yield return new WaitUntil(() => NetworkClient.ready);
        //CMDHexRelease(hex.grid.hexGridNodes.IndexOf(hex), grids.IndexOf(hex.grid), drags.IndexOf(node));
        CMDHexRelease(hex.index, hex.grid.instanceIndex, node.acquisitionOrder);
    }

    [Command]
    public void CMDHexRelease(int hexInd, int gridInd, int nodeInd)
    {
        //HexGridNode hex = grids[gridInd].hexGridNodes[hexInd];
        HexGridNode hex = grids.Find(g => g.instanceIndex == gridInd).hexGridNodes.Find(h => h.index == hexInd);
        DraggableNode node = drags.Find(d => d.acquisitionOrder == nodeInd);
        //DraggableNode node = drags[nodeInd];
        if (hex.spellNode == null) return;

        hex.VerifyNearbyBreakConections(hex.spellNode);
        hex.grid.spellNodes[hex.index] = null;
        hex.spellNode.hexGridNode = null;
        hex.spellNode = null;
        //hex.SetNodeButtonState(true);
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
