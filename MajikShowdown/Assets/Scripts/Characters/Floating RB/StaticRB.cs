using UnityEngine;

public class StaticRB : MonoBehaviour
{
    public Vector3 Velocity;
    public Vector3 Acceleration;
    public Rigidbody rb;
    Vector3 velocityGoal;
    Vector3 previousGoal;
    bool lerpingVelocity = false;
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    void FixedUpdate()
    {
        if (lerpingVelocity)
        {
            if (Vector3.Distance(Velocity, velocityGoal) < 0.1f)
            {
                Velocity = velocityGoal;
                lerpingVelocity = false;
                Acceleration = Vector3.zero;
                velocityGoal = Vector3.zero;
                previousGoal = Vector3.zero;
            }
        }
        Velocity += Acceleration * Time.fixedDeltaTime;
        rb.MovePosition(transform.position + Velocity * Time.fixedDeltaTime);
        //transform.Translate(Velocity * Time.deltaTime, Space.World);
    }
    public void LerpToVelocity(Vector3 targetVelocity, float acceleration)
    {
        lerpingVelocity = true;
        velocityGoal = targetVelocity;
        if (velocityGoal != previousGoal)
        {
            Acceleration = (velocityGoal - Velocity).normalized * acceleration;
            previousGoal = velocityGoal;
        }

    }
    public void CancelLerp()
    {
        if (lerpingVelocity)
        {
            lerpingVelocity = false;
            Acceleration = Vector3.zero;
            velocityGoal = Vector3.zero;
            previousGoal = Vector3.zero;
        }
    }
}
