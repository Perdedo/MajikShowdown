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

    void Awake()
    {
        Node = Instantiate(PrefabNode);
        Node.Interface = this;
        Node.Initialize();
        InitializeConections();
        this.GetComponent<Image>().color *= PrefabNode.color;
        this.GetComponent<Image>().alphaHitTestMinimumThreshold = 0.1f;
        rect = GetComponent<RectTransform>();
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
    public void BreakConection(int Index)
    {
        if (Index >= conections.Length)
        {
            return;
        }
        //ConectedNodes[Index] = null;
        SpellNode aux = conections[Index].GetNode();
        Debug.Log(aux);
        Debug.Log(aux.Interface);
        conections[Index].RemoveConection();
        aux.Interface.UpdateConected();
        UpdateConected();
    }
    public void UpdateConected()
    {
        for (int i = 0; i < conections.Length; i++)
        {
            if(Node.ConectedNodes[i] != null)
            {
                Node.ConectedNodes[i] = conections[i].GetNode();
            }
        }
    }
    public void UpdateConectionPorts()
    {
        for (int i = 0; i < conections.Length; i++)
        {
            if(conections[i] != null)
            {
                conections[i].conectionType = ConectionPorts[i];
            }
        }
    }

    public void SelectNode()
    {
        if (GameManager.Instance.uiController.activeGrid.selectedNode == this)
        {
            Debug.Log(GameManager.Instance.uiController.activeGrid.selectedNode.name + " Deselected");
            GameManager.Instance.uiController.activeGrid.selectedNode = null;
        }
        else
        {
            /*if(HexGrid.instance.selectedNode != null)
            {
                //Remover qqr visual do outro nodo que está selecionado
            }*/
            GameManager.Instance.uiController.activeGrid.selectedNode = this;
            Debug.Log(GameManager.Instance.uiController.activeGrid.selectedNode.name + " Selected");
        }
    }
}
