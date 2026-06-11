using Mirror;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;
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

    [NonSerialized] public Vector3 localVelocity, worldVelocity, parentVelocity, externalVelocity, lastHorizontalDirection, hDir, acceleration;
    public Vector3 HolrizontalDirection { get { return hDir; } }
    [System.NonSerialized] public Rigidbody rb;
    protected float height;
    public enum HorizontalState { moving, idle, none };
    [NonSerialized] public HorizontalState movingState;
    public enum VerticalState { falling, grounded, none };
    [NonSerialized] public VerticalState vState;

    [Header("Movement Events")]
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
    protected virtual void Awake()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        height = gameObject.GetComponent<Collider>().bounds.size.y;
        lastHorizontalDirection = transform.forward;
        externalVelocity = Vector3.zero;
        //hits = new RaycastHit[raycastNumber + 1];
    }
    protected virtual void FixedUpdate()
    {
        Gravity();
        UpdateVelocity();
    }
    //IMovingGround movingGround;
    protected RaycastHit LastHitInfo;
    protected void RaycastGround()
    {
        
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
                localVelocity.y = 0;
            }
            else
            {
                localVelocity += Vector3.up * Physics.gravity.y * gravityMultiplier * Time.fixedDeltaTime;
            }
        }
        else
        {
            localVelocity.y = 0;
        }

        if (LastHitInfo.collider != null)
        {
            if (vState == VerticalState.falling)
            {
                vState = VerticalState.grounded;
                InvokeIfHasListener(HitGround);
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
                InvokeIfHasListener(Fell);
            }
            else
            {
                vState = VerticalState.falling;
            }
        }

    }

    protected void SetVelocity(Vector3 vel)
    {
        if (movePaused) vel = new Vector3(0, vel.y, 0);

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
                InvokeIfHasListener(StartedMoving);
            }
        }
        else if (movingState != HorizontalState.idle)
        {
            movingState = HorizontalState.idle;
            InvokeIfHasListener(StoppedMoving);
        }

        /*if (affectedByMovingGround && movingGround != null)
        {
            parentVelocity = movingGround.GetVelocity();
        }
        else
        {
            parentVelocity = Vector3.zero;
        }*/
        worldVelocity = parentVelocity + Vector3.ClampMagnitude(localVelocity, maxVelocity);
        Vector3 atritionVector = externalVelocity.normalized * BaseFriction;
        if (externalVelocity.sqrMagnitude < 0.01f)
        {
            externalVelocity = Vector3.zero;
        }
        else
        {
            externalVelocity -= atritionVector;
        }
        Vector3 velocityChange = worldVelocity - rb.linearVelocity + externalVelocity;

        if (velocityChange.sqrMagnitude > 0.01f)
        {
            rb.AddForce(velocityChange, ForceMode.VelocityChange);
        }

    }
    public void InvokeIfHasListener(UnityEvent e)
    {
        if (e.GetPersistentEventCount() > 0)
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
