using UnityEngine;

public class StaticRB : MonoBehaviour
{
    public Vector3 Velocity;
    public Vector3 Acceleration;
    void Update()
    {
        Velocity += Acceleration * Time.deltaTime;
        transform.position += Velocity * Time.deltaTime;
    }
}
