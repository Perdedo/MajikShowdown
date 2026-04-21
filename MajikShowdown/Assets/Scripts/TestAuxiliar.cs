using System.Collections.Generic;
using UnityEngine;

public class TestAuxiliar : MonoBehaviour
{
    public List<GameObject> objectsToTurnOn = new List<GameObject>();
    void Start()
    {
        foreach(GameObject obj in objectsToTurnOn)
        {
            obj.SetActive(true);
        }
    }
}
