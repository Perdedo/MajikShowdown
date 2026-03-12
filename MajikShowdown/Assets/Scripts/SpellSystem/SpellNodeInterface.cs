using UnityEngine;

public class SpellNodeInterface : MonoBehaviour
{
    //COLOCAR CONECXÕES NESSE SCRIPT
    public SpellNode PrefabNode;
    public SpellNode Node;
    void Awake()
    {
        Node = Instantiate(PrefabNode);
        Node.Initialize();
    }
}
