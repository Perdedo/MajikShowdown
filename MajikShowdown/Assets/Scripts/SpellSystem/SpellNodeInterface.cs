using Mirror.BouncyCastle.Asn1.Mozilla;
using Mirror.BouncyCastle.Math.Field;
using UnityEngine;
using UnityEngine.UI;

public class SpellNodeInterface : MonoBehaviour
{
    //COLOCAR CONECXÕES NESSE SCRIPT
    [HideInInspector] public RectTransform rect;
    [HideInInspector] public HexGridNode hexGridNode;
    //public SpellNode PrefabNode;
    public SpellNode Node;
    //public NodeConection.Conections[] ConectionPorts = new NodeConection.Conections[6];
    public NodeConection[] conections;
    public SpellNodeInfos info;
    public NodeInventory inventory;
    public GameObject usedNodeImg;
    public Image borderImg;
    [HideInInspector] public int acquisitionOrder;
    [HideInInspector] public SpellNodeDescription linkedDescription;

    void Awake()
    {
        rect = GetComponent<RectTransform>();

        borderImg = transform.GetChild(0).GetComponent<Image>();

        usedNodeImg = transform.GetChild(1).gameObject;
        //Initialize();
        /*Node = Instantiate(PrefabNode);
        Node.Interface = this;
        Node.Initialize();
        InitializeConections();
        this.GetComponent<Image>().color *= PrefabNode.color;
        this.GetComponent<Image>().alphaHitTestMinimumThreshold = 0.1f;
        rect = GetComponent<RectTransform>();
        borderImg = transform.GetChild(0).GetComponent<Image>();*/
    }

    public void Setup(SpellNode nodeData)
    {
        rect = GetComponent<RectTransform>();

        borderImg = transform.GetChild(0).GetComponent<Image>();

        usedNodeImg = transform.GetChild(1).gameObject;

        Node = nodeData;
        Node.Interface = this;
        InitializeConections();
        GetComponent<Image>().color = Node.color;
        GetComponent<Image>().alphaHitTestMinimumThreshold = 0.1f;
        SetNodeBorder(borderImg);
        usedNodeImg.SetActive(Node.IsInUse);
    }
    /*private void Start()
    {
        SetNodeBorder(borderImg);
    }*/

    [ContextMenu("Initialize")]

    /*public void Initialize()
    {
        rect = GetComponent<RectTransform>();

        borderImg = transform.GetChild(0).GetComponent<Image>();

        usedNodeImg = transform.GetChild(1).gameObject;
        GameManager.Instance.uiController.playerUI.caster.commander.InitializeSNI(this);
    }*/
    public void InitializeConections()
    {
        conections = new NodeConection[] { new(Node), new(Node), new(Node), new(Node), new(Node), new(Node) };
        UpdateConectionPorts();
    }
    public bool TryConectNode(SpellNodeInterface con, int index)
    {
        Debug.Log("tryCon");
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
        Debug.Log("break");
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
            var spell = Node.OwnerSpell;
            if (spell != null)
            {
                spell.UpdateSpell();
            }
        }
        //inventory.commander.BreakSNIConnection(this, Index);
        GameManager.Instance.uiController.playerUI.caster.commander.BreakSNIConnection(this, Index);
    }
    public void UpdateConected()
    {
        Debug.Log("updatecon");
        for (int i = 0; i < conections.Length; i++)
        {
            if (conections[i] != null)
            {
                Debug.Log(conections[i].GetNode());
                Node.ConectedNodes[i] = conections[i].GetNode();
            }
            else
            {
                Debug.Log("else");
                Node.ConectedNodes[i] = null;
            }
        }
        //inventory.commander.UpdateSNIConnected(this);
        GameManager.Instance.uiController.playerUI.caster.commander.UpdateSNIConnected(this);
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
        //inventory.commander.UpdateSNIConnectionPorts(this);
        //GameManager.Instance.uiController.playerUI.caster.commander.UpdateSNIConnectionPorts(this);
    }

    public void SelectNode()
    {
        var description = linkedDescription ?? GameManager.Instance.uiController.playerUI.spellNodeDescription;
        var ui = GameManager.Instance.uiController.playerUI;

        if (ui.selectedNode == this)
        {
            description.HideDescription();
            //ui.spellNodeDescription.HideDescription();
            ui.selectedNode = null;
        }
        else
        {
            ui.selectedNode = this;
            //ui.spellNodeDescription.ShowDescription(Node);
            description.ShowDescription(Node);
        }
    }

    public void SelectOnly()
    {
        var ui = GameManager.Instance.uiController.playerUI;
        if (ui.selectedNode == this) return;
        ui.selectedNode = this;
        var description = linkedDescription ?? ui.spellNodeDescription;
        description.ShowDescription(Node);
        //ui.spellNodeDescription.ShowDescription(Node);
    }

    public void SetNodeBorder(Image img)
    {
        switch (GetCategory())
        {
            case NodeCategory.Type:
                img.sprite = info.coreBorder;
                break;

            case NodeCategory.Effect:
                img.sprite = info.effectBorder;
                break;

            case NodeCategory.Trajectory:
                img.sprite = info.trajectoryBorder;
                break;

            case NodeCategory.Stat:
                img.sprite = info.statBorder;
                break;

            case NodeCategory.Trigger:
                img.sprite = info.triggerBorder;
                break;
                /*case NodeCategory.CastingPoint:
                    img.sprite = info.castingPointBorder;
                    break;*/
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

        GameManager.Instance.uiController.playerUI.caster.commander.SetUsedSNI(this, used);
    }

    public NodeCategory GetCategory()
    {
        if (Node is SpellEffect) return NodeCategory.Effect;

        if (Node is SpellStat) return NodeCategory.Stat;

        if (Node is SpellTrajectory) return NodeCategory.Trajectory;

        if (Node is SpellTrigger) return NodeCategory.Trigger;

        if (Node is SpellType) return NodeCategory.Type;

        //if (Node as SpellCastingPoint) return NodeCategory.CastingPoint;

        return NodeCategory.All;
    }
}
