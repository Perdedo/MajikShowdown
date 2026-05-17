using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Linq;
using Mirror;
public class HexGrid : MonoBehaviour
{
    public List<HexGridNode> hexGridNodes = new List<HexGridNode>();
    public List<SpellNodeInterface> spellNodes;
    public RectTransform hexPrefab;
    public int hexGridRadius;
    private float hexNodeSize;
    public float maxDistance;
    public SpellCaster caster;
    public Spell spell;
    public Transform hexContainer;
    public Transform nodeContainer;
    public int instanceIndex;

    Vector2Int[] directions =
    {
        new Vector2Int(1, 0),
        new Vector2Int(1, -1),
        new Vector2Int(0, -1),
        new Vector2Int(-1, 0),
        new Vector2Int(-1, 1),
        new Vector2Int(0, 1)
    };

    public void Initialize()
    {
        if (hexGridNodes.Count > 0) return;
        InitGrid();
        ConfigurateSpell();
    }

    public void InitGrid()
    {
        hexNodeSize = hexPrefab.rect.height / 2f;
        GenerateGrid();
        SetNeighbours();
    }

    public void SetSpell(Spell newSpell)
    {
        spell = newSpell;
        //ConfigurateSpell();
    }
    void OnEnable()
    {
        /*if(GameManager.Instance.uiController.activeGrid != null && GameManager.Instance.uiController.activeGrid != this)
        {
            GameManager.Instance.uiController.activeGrid.gameObject.SetActive(false);
        }*/
        GameManager.Instance.uiController.playerUI.activeGrid = this;
    }
    void OnDisable()
    {
        if (GameManager.Instance.uiController.playerUI.activeGrid == this)
        {
            GameManager.Instance.uiController.playerUI.activeGrid = null;
        }

    }

    void GenerateGrid()
    {
        CreateHex(0, 0, 0);

        for (int layer = 1; layer <= hexGridRadius; layer++)
        {
            GenerateRing(layer);
        }
        spellNodes = Enumerable.Repeat<SpellNodeInterface>(null, hexGridNodes.Count).ToList();
    }

    void GenerateRing(int layer)
    {
        Vector2Int hex = directions[4] * layer;

        for (int side = 0; side < 6; side++)
        {
            for (int step = 0; step < layer; step++)
            {
                CreateHex(hex.x, hex.y, layer);
                hex += directions[side];
            }
        }
    }

    void CreateHex(int q, int r, int Layer)
    {
        Vector2 pos = HexToPixel(q, r);
        RectTransform hex = Instantiate(hexPrefab, hexContainer);
        hex.anchoredPosition = pos;
        HexGridNode node = hex.GetComponent<HexGridNode>();
        node.index = hexGridNodes.Count;
        node.Layer = Layer;
        hexGridNodes.Add(node);
        node.SetGrid(this);
    }

    Vector2 HexToPixel(int q, int r)
    {
        float width = Mathf.Sqrt(3) * hexNodeSize;
        float height = 2f * hexNodeSize;
        float x = width * (q + r * 0.5f);
        float y = height * 0.75f * r;
        return new Vector2(x, y);
    }

    public void SetNeighbours()
    {
        Vector2 dist;
        float angle;
        int index;
        for (int i = 0; i < hexGridNodes.Count; i++)
        {
            for (int j = 0; j < hexGridNodes.Count; j++)
            {
                if (i != j)
                {
                    dist = (Vector2)hexGridNodes[j].transform.localPosition - (Vector2)hexGridNodes[i].transform.localPosition;
                    if (dist.sqrMagnitude <= Mathf.Pow(maxDistance, 2))
                    {
                        angle = Vector2.SignedAngle(Vector2.up, dist.normalized);
                        index = (int)angle / 60;
                        if (angle < 0)
                        {
                            index += 5;
                        }
                        hexGridNodes[i].AddNeighbours(hexGridNodes[j], index);
                    }
                }
            }
        }
    }
    
    public void ConfigurateSpell()
    {
        spell.spellNodes.Clear();
        foreach (var node in spellNodes)
        {
            if (node != null)
            {
                node.Node.hierarchy = node.hexGridNode.Layer;
                spell.spellNodes.Add(node.Node);
            }

        }
        if (hexGridNodes[0].spellNode != null && hexGridNodes[0].spellNode.Node is SpellType t)
        {
            spell.validSpell = true;
            spell.primaryNode = t;
        }
        else
        {
            spell.validSpell = false;
            spell.primaryNode = null;
        }

        spell.UpdateSpell();
        caster.commander.ConfigurateSpell(this);
    }

    public void ReturnAllNodesToInventory()
    {
        if (caster == null || caster.inventory == null) return;

        foreach (var node in spellNodes)
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
                caster.inventory.AddNodeToInventory(node);
            }
        }
        for (int i = 0; i < spellNodes.Count; i++)
        {
            spellNodes[i] = null;
        }
        ConfigurateSpell();
        caster.commander.ReturnAllNodesToInventory(this);
    }

    public void AddNodeToGrid(HexGridNode hex, SpellNodeInterface node)
    {
        Debug.Log("Add");
        if (node == null) return;
        if (!VerifyNode(node)) return;
        if (caster == null || caster.inventory == null)
        {
            return;
        }
        caster.inventory.RemoveNodeFromInventory(node);
        if (node.hexGridNode != null)
        {
            Debug.Log("Quebrou aqui");
            node.hexGridNode.VerifyNearbyBreakConections(node);
            node.hexGridNode.spellNode = null;
            node.hexGridNode.SetNodeButtonState(true);
        }
        hex.spellNode = node;
        node.hexGridNode = hex;
        spellNodes[hex.index] = node;
        hex.SetNodeButtonState(false);
        RectTransform rect = node.GetComponent<RectTransform>();
        node.transform.SetParent(nodeContainer, false);
        rect.position = hex.rect.position;
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
        ConfigurateSpell();
        caster.commander.AddNodeToGrid(hex, node, this);
    }

    public bool VerifyNode(SpellNodeInterface node)
    {
        bool found = false;
        for(int i = 0; i < spellNodes.Count; i++)
        {
            if(spellNodes[i] != null)
            {
                if (spellNodes[i].acquisitionOrder == node.acquisitionOrder)
                {
                    found = true;
                    i = spellNodes.Count;
                }
            }
        }
        return found;
    }
}