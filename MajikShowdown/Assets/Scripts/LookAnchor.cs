using UnityEngine;

public class LookAnchor : MonoBehaviour
{
    public Transform PlayerFollow;
    void Update()
    {
        transform.position = PlayerFollow.position;
    }
}
