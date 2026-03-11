using UnityEngine;

public class NetworkAuxiliarControl : MonoBehaviour
{
    public FlowFieldManager ffManager;
    private void Awake()
    {
        GameManager.Instance.netCtrl = this;
    }
}
