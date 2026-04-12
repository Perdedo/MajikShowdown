using UnityEngine;

public class StaticRB : MonoBehaviour
{
    public Vector3 Velocity;
    public Vector3 Acceleration;
    public Rigidbody rb;
    private void Awake() 
    {
        rb = GetComponent<Rigidbody>();
    }
    void FixedUpdate()
    {
        Velocity += Acceleration * Time.fixedDeltaTime;
        rb.MovePosition(transform.position + Velocity * Time.fixedDeltaTime);
        //transform.Translate(Velocity * Time.deltaTime, Space.World);
    }
}
