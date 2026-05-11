using Mirror.BouncyCastle.Asn1.Mozilla;
using Mirror.BouncyCastle.Math.Field;
using UnityEngine;
using UnityEngine.UI;

public class SpellNodeInterface : MonoBehaviour
{
    [HideInInspector] public RectTransform rect;
    [HideInInspector] public HexGridNode hexGridNode;

    public SpellNode Node;

    public NodeConection.Conections[] ConectionPorts = new NodeConection.Conections[6];
    public NodeConection[] conections;

    public SpellNodeInfos info;

    GameObject usedNodeImg;
    Image borderImg;

    [HideInInspector] public int acquisitionOrder;
    [HideInInspector] public SpellNodeDescription linkedDescription;

    void Awake()
    {
        rect = GetComponent<RectTransform>();

        borderImg = transform.GetChild(0).GetComponent<Image>();

        usedNodeImg = transform.GetChild(1).gameObject;
    }

    public void Setup(SpellNode nodeData)
    {
        Node = nodeData;
        Node.Interface = this;
        InitializeConections();
        GetComponent<Image>().color = Node.color;
        GetComponent<Image>().alphaHitTestMinimumThreshold = 0.1f;
        SetNodeBorder(borderImg);
        usedNodeImg.SetActive(Node.IsInUse);
    }

    [ContextMenu("Initialize")]
    public void InitializeConections()
    {
        conections = new NodeConection[]
        {
            new(Node),
            new(Node),
            new(Node),
            new(Node),
            new(Node),
            new(Node)
        };

        UpdateConectionPorts();
    }

    public bool TryConectNode(SpellNodeInterface con, int index)
    {
        int mirrorIndex = (index + 3) % 6;

        if (index < conections.Length)
        {
            if (conections[index].TryConect(con.conections[mirrorIndex]))
            {
                UpdateConected();

                con.UpdateConected();

                return true;
            }
        }

        return false;
    }

    public bool CheckConectNode(SpellNodeInterface con, int index)
    {
        int mirrorIndex = (index + 3) % 6;

        if (index < conections.Length)
        {
            if (conections[index].CheckConection(con.conections[mirrorIndex]))
            {
                return true;
            }
        }

        return false;
    }

    public void BreakConection(int Index)
    {
        if (Index >= conections.Length)
        {
            return;
        }

        SpellNode aux = conections[Index].GetNode();

        if (aux != null)
        {
            conections[Index].RemoveConection();

            aux.Interface.UpdateConected();

            UpdateConected();

            var spell = Node.OwnerSpell;

            if (spell != null)
            {
                spell.UpdateSpell();
            }
        }
    }

    public void UpdateConected()
    {
        for (int i = 0; i < conections.Length; i++)
        {
            if (conections[i] != null)
            {
                Node.ConectedNodes[i] = conections[i].GetNode();
            }
            else
            {
                Node.ConectedNodes[i] = null;
            }
        }
    }

    public void UpdateConectionPorts()
    {
        for (int i = 0; i < conections.Length; i++)
        {
            if (conections[i] != null)
            {
                conections[i].conectionType = Node.ConectionPorts[i];
            }
        }
    }

    public void SelectNode()
    {
        var description = linkedDescription ?? GameManager.Instance.uiController.playerUI.spellNodeDescription;

        var ui = GameManager.Instance.uiController.playerUI;

        if (ui.selectedNode == this)
        {
            description.HideDescription();

            ui.selectedNode = null;
        }
        else
        {
            ui.selectedNode = this;

            description.ShowDescription(Node);
        }
    }

    public void SelectOnly()
    {
        var ui = GameManager.Instance.uiController.playerUI;

        if (ui.selectedNode == this) return;

        ui.selectedNode = this;

        var description = linkedDescription
            ?? ui.spellNodeDescription;

        description.ShowDescription(Node);
    }

    public void SetNodeBorder(Image img)
    {
        if (Node.ConectionPorts[0] == NodeConection.Conections.Circle && Node.ConectionPorts[1] == NodeConection.Conections.Square)
        {
            img.sprite = info.borderSprite[0];
            return;
        }
        else if (Node.ConectionPorts[0] == NodeConection.Conections.None && Node.ConectionPorts[3] == NodeConection.Conections.Circle)
        {
            img.sprite = info.borderSprite[1];
            return;
        }
        else if (Node.ConectionPorts[0] == NodeConection.Conections.None && Node.ConectionPorts[2] == NodeConection.Conections.Triangle)
        {
            img.sprite = info.borderSprite[2];
            return;
        }
        else if (Node.ConectionPorts[0] == NodeConection.Conections.Square && Node.ConectionPorts[1] == NodeConection.Conections.None)
        {
            img.sprite = info.borderSprite[3];
            return;
        }
        else if (Node.ConectionPorts[0] == NodeConection.Conections.None && Node.ConectionPorts[1] == NodeConection.Conections.Penta)
        {
            img.sprite = info.borderSprite[4];
            return;
        }
        else
        {
            img.sprite = info.borderSprite[2];
            return;
        }
    }

    public bool IsUsed()
    {
        return Node.IsInUse;
    }

    public void SetUsed(bool used)
    {
        Node.IsInUse = used;

        usedNodeImg.SetActive(used);
    }

    public NodeCategory GetCategory()
    {
        if (Node is SpellEffect) return NodeCategory.Effect;

        if (Node is SpellStat) return NodeCategory.Stat;

        if (Node is SpellTrajectory) return NodeCategory.Trajectory;

        if (Node is SpellTrigger) return NodeCategory.Trigger;

        if (Node is SpellType) return NodeCategory.Type;

        return NodeCategory.All;
    }
}