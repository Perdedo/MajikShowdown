using Mirror;
using UnityEngine;
using UnityEngine.Events;
using System;
public class CrowdRB : NetworkBehaviour
{
    [Header("Velocity Options")]
    [SerializeField] protected float maxVelocity;
    //public bool affectedByMovingGround = false;
    [Header("Floating Options")]
    //[SerializeField] protected float floatingHeight;
    [SerializeField] protected float gravityMultiplier;
    [SerializeField] protected float terrainBuffer;
    [SerializeField] protected float BaseFriction = 0.5f;

    [NonSerialized] public Vector3 localVelocity, worldVelocity, parentVelocity, externalVelocity, lastHorizontalDirection, hDir, acceleration, verticalVelocity;
    public Vector3 HolrizontalDirection { get { return hDir; } }
    [System.NonSerialized] public Rigidbody rb;
    protected float height;
    public enum HorizontalState { moving, idle, none };
    [NonSerialized] public HorizontalState movingState;
    public enum VerticalState { falling, grounded, none };
    [NonSerialized] public VerticalState vState;

    [Header("Movement Events")]
    public bool callEvents = false;
    [SerializeField] protected UnityEvent Fell;
    [SerializeField] protected UnityEvent HitGround;
    [SerializeField] protected UnityEvent StartedMoving;
    [SerializeField] protected UnityEvent StoppedMoving;

    [Header("Ground Raycast Options")]
    [SerializeField] protected QueryTriggerInteraction RayTriggerInteraction;
    [SerializeField] protected LayerMask RayMasks;
    //[SerializeField] protected int raycastNumber;
    //[SerializeField] protected float raycastRadius;
    public GameObject OnTopOf { get; protected set; }
    [System.NonSerialized] public bool movePaused, gravityPaused;
    Timer raycastTimer = new Timer(false);
    protected virtual void Awake()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        height = gameObject.GetComponent<Collider>().bounds.size.y;
        lastHorizontalDirection = transform.forward;
        externalVelocity = Vector3.zero;
        //hits = new RaycastHit[raycastNumber + 1];
    }
    public virtual void FixedRBUpdate()
    {
        Gravity();
        UpdateVelocity();
    }
    //IMovingGround movingGround;
    protected RaycastHit LastHitInfo;
    protected void RaycastGround()
    {
        /*if(raycastTimer.timer(0.05f, Time.captureDeltaTime, false, false))
        {
            Physics.Raycast(rb.position, Vector3.down, out LastHitInfo, terrainBuffer + height / 2, RayMasks, RayTriggerInteraction);
            raycastTimer.ResetTimer();
        }*/
        Physics.Raycast(rb.position, Vector3.down, out LastHitInfo, terrainBuffer + height / 2, RayMasks, RayTriggerInteraction);
    }

    protected void Gravity()
    {
        RaycastGround();
        float groundDistance = LastHitInfo.distance - (height / 2);

        if (!gravityPaused)
        {
            if (LastHitInfo.collider != null || (groundDistance <= terrainBuffer && vState == VerticalState.grounded))
            {
                verticalVelocity.y = 0;
            }
            else
            {
                verticalVelocity += Vector3.up * Physics.gravity.y * gravityMultiplier * Time.fixedDeltaTime;
            }
        }
        else
        {
            verticalVelocity.y = 0;
        }

        if (LastHitInfo.collider != null)
        {
            if (vState == VerticalState.falling)
            {
                vState = VerticalState.grounded;
                InvokeIfAllowed(HitGround);
            }
            else
            {
                vState = VerticalState.grounded;
            }
        }
        else
        {
            if (vState == VerticalState.grounded)
            {
                vState = VerticalState.falling;
                InvokeIfAllowed(Fell);
            }
            else
            {
                vState = VerticalState.falling;
            }
        }

    }

    public void ResetAllVelocities()
    {
        localVelocity = Vector3.zero;
        acceleration = Vector3.zero;
        externalVelocity = Vector3.zero;
        verticalVelocity = Vector3.zero;
        worldVelocity = Vector3.zero;
        parentVelocity = Vector3.zero;
    }

    protected void SetVelocity(Vector3 vel)
    {
        if (movePaused) vel = Vector3.zero;

        localVelocity = vel;
    }
    /*protected void AccelerateToVelocity(Vector3 vel, float seconds)
    {
        SetAcceleration((vel - localVelocity) / seconds);
    }*/

    protected void AddVelocity(Vector3 vel)
    {
        SetVelocity(localVelocity + vel);
    }

    protected void SetAcceleration(Vector3 acc)
    {
        acceleration = acc;
    }

    protected void AddAcceleration(Vector3 acc)
    {
        acceleration += acc;
    }
    protected void AddExternalVelocity(Vector3 vel)
    {
        externalVelocity += vel;
    }
    protected void UpdateVelocity()
    {
        localVelocity += acceleration * Time.fixedDeltaTime;

        hDir = new Vector3(localVelocity.x, 0, localVelocity.z).normalized;
        if (hDir != Vector3.zero)
        {
            lastHorizontalDirection = hDir;
            if (movingState != HorizontalState.moving)
            {
                movingState = HorizontalState.moving;
                InvokeIfAllowed(StartedMoving);
            }
        }
        else if (movingState != HorizontalState.idle)
        {
            movingState = HorizontalState.idle;
            InvokeIfAllowed(StoppedMoving);
        }

        /*if (affectedByMovingGround && movingGround != null)
        {
            parentVelocity = movingGround.GetVelocity();
        }
        else
        {
            parentVelocity = Vector3.zero;
        }*/
        worldVelocity = parentVelocity + Vector3.ClampMagnitude(localVelocity, maxVelocity) + verticalVelocity;
        Vector3 atritionVector = externalVelocity.normalized * BaseFriction;
        if (externalVelocity.sqrMagnitude < 0.01f)
        {
            externalVelocity = Vector3.zero;
        }
        else
        {
            externalVelocity -= atritionVector;
        }
        //rb.Move(rb.position + ((worldVelocity+externalVelocity) * Time.fixedDeltaTime), rb.rotation);
        Vector3 velocityChange = worldVelocity - rb.linearVelocity + externalVelocity;

        if (velocityChange.sqrMagnitude > 0.01f)
        {
            rb.AddForce(velocityChange, ForceMode.VelocityChange);
        }

    }
    public void InvokeIfAllowed(UnityEvent e)
    {
        if (callEvents)
        {
            e.Invoke();
        }
    }
    ContactPoint[] contactBuffer = new ContactPoint[10];
    protected virtual void OnCollisionStay(Collision collision)
    {
        if (externalVelocity.sqrMagnitude != 0)
        {
            int contactCount = collision.GetContacts(contactBuffer);
            for (int i = 0; i < contactCount; i++)
            {
                float dot = Vector3.Dot(contactBuffer[i].normal, externalVelocity.normalized);
                if (dot < 0)
                {
                    externalVelocity += externalVelocity * dot;
                }
            }
        }

    }
}
