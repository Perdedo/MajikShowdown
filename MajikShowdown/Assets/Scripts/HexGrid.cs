using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Linq;

public class HexGrid : MonoBehaviour
{
    public List<HexGridNode> hexGridNodes = new List<HexGridNode>();
    public List<SpellNodeInterface> spellNodes;
    public RectTransform hexPrefab;
    public int hexGridRadius;
    private float hexNodeSize;
    //public static HexGrid instance;
    public SpellNodeInterface selectedNode;
    public float maxDistance;
    public SpellCaster caster;
    public Spell spell;

    Vector2Int[] directions =
    {
        new Vector2Int(1, 0),
        new Vector2Int(1, -1),
        new Vector2Int(0, -1),
        new Vector2Int(-1, 0),
        new Vector2Int(-1, 1),
        new Vector2Int(0, 1)
    };

    void Start()
    {
        spell = new Spell(caster);
        hexNodeSize = hexPrefab.rect.height / 2f;
        GenerateGrid();
        SetNeighbours();
        ConfigurateSpell();
    }
    void OnEnable()
    {
        /*if(GameManager.Instance.uiController.activeGrid != null && GameManager.Instance.uiController.activeGrid != this)
        {
            GameManager.Instance.uiController.activeGrid.gameObject.SetActive(false);
        }*/
        GameManager.Instance.uiController.activeGrid = this;
    }
    void OnDisable()
    {
        selectedNode = null;
        if (GameManager.Instance.uiController.activeGrid == this)
        {
            GameManager.Instance.uiController.activeGrid = null;
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
        RectTransform hex = Instantiate(hexPrefab, transform);
        hex.anchoredPosition = pos;
        HexGridNode node = hex.GetComponent<HexGridNode>();
        node.SetGrid(this);
        node.index = hexGridNodes.Count;
        node.Layer = Layer;
        hexGridNodes.Add(node);
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
                    dist = hexGridNodes[j].rect.localPosition - hexGridNodes[i].rect.localPosition;
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
    public void AddSelectedToGrid(HexGridNode node)
    {
        NodeInventory.instance.RemoveNodeFromInventory(selectedNode, this);
        selectedNode.rect.position = node.rect.position;
        if (selectedNode.hexGridNode != null)
        {
            selectedNode.hexGridNode.VerifyNearbyBreakConections(selectedNode);
            selectedNode.hexGridNode.spellNode = null;
            selectedNode.hexGridNode.SetNodeButtonState(true);
        }
        node.spellNode = selectedNode;
        selectedNode.hexGridNode = node;
        spellNodes.Insert(node.index, selectedNode);
        selectedNode = null;
        node.SetNodeButtonState(false);
        ConfigurateSpell();
    }
    public void RemoveSelectedFromGrid()
    {
        selectedNode.hexGridNode.SetNodeButtonState(true);
        selectedNode.hexGridNode.VerifyNearbyBreakConections(selectedNode);
        selectedNode.hexGridNode.spellNode = null;
        selectedNode.hexGridNode = null;
        //AddNodeToInventory(selectedNode);
        spellNodes.Remove(selectedNode);
        selectedNode.Node.ResetNode();
        selectedNode = null;
        ConfigurateSpell();
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
            spell.UpdateSpell();
        }
        else
        {
            spell.validSpell = false;
        }

    }

}
