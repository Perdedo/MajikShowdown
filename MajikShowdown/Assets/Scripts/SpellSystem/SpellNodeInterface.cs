using Mirror.BouncyCastle.Asn1.Mozilla;
using Mirror.BouncyCastle.Math.Field;
using UnityEngine;
using UnityEngine.UI;

public class SpellNodeInterface : MonoBehaviour
{
    //COLOCAR CONECXÕES NESSE SCRIPT
    [HideInInspector] public RectTransform rect;
    [HideInInspector] public HexGridNode hexGridNode;
    public SpellNode PrefabNode;
    public SpellNode Node;
    public NodeConection.Conections[] ConectionPorts = new NodeConection.Conections[6];
    public NodeConection[] conections;
    public SpellNodeInfos info;
    Image borderImg;

    void Awake()
    {
        Node = Instantiate(PrefabNode);
        Node.Interface = this;
        Node.Initialize();
        InitializeConections();
        this.GetComponent<Image>().color *= PrefabNode.color;
        this.GetComponent<Image>().alphaHitTestMinimumThreshold = 0.1f;
        rect = GetComponent<RectTransform>();
        borderImg = transform.GetChild(0).GetComponent<Image>();
    }

    private void Start()
    {
        SetNodeBorder(borderImg);
    }

    [ContextMenu("Initialize")]
    public void InitializeConections()
    {
        conections = new NodeConection[] { new(Node), new(Node), new(Node), new(Node), new(Node), new(Node) };
        UpdateConectionPorts();
    }
    public bool TryConectNode(SpellNodeInterface con, int index)
    {
        int mirrorIndex = (index + 3) % 6;
        if (index < conections.Length)
        {
            if (conections[index].TryConect(con.conections[mirrorIndex]))
            {
                //Node.ConectedNodes[index] = con.Node;
                //con.Node.ConectedNodes[mirrorIndex] = Node;
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
        //ConectedNodes[Index] = null;
        SpellNode aux = conections[Index].GetNode();
        //Debug.Log(aux);
        //Debug.Log(aux.Interface);
        if (aux != null)
        {
            conections[Index].RemoveConection();
            aux.Interface.UpdateConected();
            UpdateConected();
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
                conections[i].conectionType = ConectionPorts[i];
            }
        }
    }

    public void SelectNode()
    {
        var ui = GameManager.Instance.uiController.playerUI;

        if (ui.selectedNode == this)
        {
            Debug.Log(name + " Deselected");
            ui.spellNodeDescription.HideDescription();
            ui.selectedNode = null;
        }
        else
        {
            ui.selectedNode = this;
            ui.spellNodeDescription.ShowDescription(Node);
            Debug.Log(name + " Selected");
        }
    }

    public void SetNodeBorder(Image img)
    {

        if (ConectionPorts[0] == NodeConection.Conections.Circle && ConectionPorts[1] == NodeConection.Conections.Square)
        {
            img.sprite = info.borderSprite[0];
            return;
        }
        else if (ConectionPorts[0] == NodeConection.Conections.None && ConectionPorts[3] == NodeConection.Conections.Circle)
        {
            img.sprite = info.borderSprite[1];
            return;
        }
        else if (ConectionPorts[0] == NodeConection.Conections.None && ConectionPorts[2] == NodeConection.Conections.Triangle)
        {
            img.sprite = info.borderSprite[2];
            return;
        }
        else if (ConectionPorts[0] == NodeConection.Conections.Square && ConectionPorts[1] == NodeConection.Conections.None)
        {
            img.sprite = info.borderSprite[3];
            return;
        }
        else if (ConectionPorts[0] == NodeConection.Conections.None && ConectionPorts[1] == NodeConection.Conections.Penta)
        {
            img.sprite = info.borderSprite[4];
            return;
        }
        else
        {
            Debug.Log("No Border Found");
        }
    }
}
