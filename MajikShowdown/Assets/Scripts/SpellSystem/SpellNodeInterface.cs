using UnityEngine;

public class SpellNodeInterface : MonoBehaviour
{
    //COLOCAR CONECXÕES NESSE SCRIPT
    public SpellNode PrefabNode;
    public SpellNode Node;
    public NodeConection.Conections[] ConectionPorts = new NodeConection.Conections[6];
    public NodeConection[] conections;
    void Awake()
    {
        Node = Instantiate(PrefabNode);
        Node.Interface = this;
        Node.Initialize();
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
        conections[Index].RemoveConection();
        aux.Interface.UpdateConected();
        UpdateConected();
    }
    public void UpdateConected()
    {
        for (int i = 0; 0 < conections.Length; i++)
        {
            Node.ConectedNodes[i] = conections[i].GetNode();
        }
    }
    public void UpdateConectionPorts()
    {
        for (int i = 0; 0 < conections.Length; i++)
        {
            conections[i].conectionType = ConectionPorts[i];
        }
    }
}
