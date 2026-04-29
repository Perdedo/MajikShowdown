using System;
using UnityEngine;
using UnityEngine.Events;
using Mirror;
public class FloatingRigidbody : NetworkBehaviour
{
    [SerializeField] protected float maxVelocity;
    [Header("Floating Options")]
    [SerializeField] protected float floatingHeight;
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
    [SerializeField] protected int raycastNumber;
    [SerializeField] protected float raycastRadius;
    public GameObject OnTopOf { get; protected set; }
    [System.NonSerialized] public bool movePaused, gravityPaused;
    protected virtual void Awake()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        height = gameObject.GetComponent<Collider>().bounds.size.y;
        lastHorizontalDirection = transform.forward;
        externalVelocity = Vector3.zero;
    }
    protected virtual void FixedUpdate()
    {
        Float();
        UpdateVelocity();
    }
    protected RaycastHit RaycastGround()
    {
        RaycastHit hitInfo;
        RaycastHit[] hits;
        if (raycastNumber <= 1)
        {
            Physics.Raycast(rb.position, Vector3.down, out hitInfo, floatingHeight + terrainBuffer + height / 2, RayMasks, RayTriggerInteraction);
        }
        else
        {
            hits = new RaycastHit[raycastNumber + 1];
            for (int i = 0; i < raycastNumber; i++)
            {
                Quaternion rot = Quaternion.Euler(0, (360 / raycastNumber) * i, 0);
                Vector3 raypoint = rb.position + Vector3.forward * raycastRadius;
                var v = raypoint - rb.position;
                v = rot * v;
                raypoint = rb.position + v;
                Physics.Raycast(raypoint, Vector3.down, out hits[i], floatingHeight + terrainBuffer + height / 2, RayMasks, RayTriggerInteraction);
                Debug.DrawRay(raypoint, Vector3.down * (floatingHeight + terrainBuffer + height / 2), Color.red, Time.fixedDeltaTime);
            }
            Physics.Raycast(rb.position, Vector3.down, out hits[raycastNumber], floatingHeight + terrainBuffer + height / 2, RayMasks, RayTriggerInteraction);
            int shorter = 0;
            for (int i = 1; i < hits.Length; i++)
            {
                if ((hits[i].collider != null && hits[i].distance < hits[shorter].distance) || hits[shorter].collider == null)
                {
                    shorter = i;
                }
            }
            hitInfo = hits[shorter];
        }
        if (OnTopOf != hitInfo.collider?.gameObject && vState == VerticalState.grounded)
        {
            OnTopOf?.GetComponent<IMovingGround>()?.FRig.Remove(this);
            OnTopOf = hitInfo.collider?.gameObject;
            OnTopOf?.GetComponent<IMovingGround>()?.FRig.Add(this);
        }
        else if (vState != VerticalState.grounded)
        {
            OnTopOf?.GetComponent<IMovingGround>()?.FRig.Remove(this);
            OnTopOf = null;
        }
        return hitInfo;
    }

    protected void Float()
    {
        RaycastHit hitInfo = RaycastGround();
        float groundDistance = hitInfo.distance - (floatingHeight + (height / 2));

        if (hitInfo.collider == null || Mathf.Sign(groundDistance) > 0)
        {
            if (!gravityPaused)
            {
                if (hitInfo.collider != null || (groundDistance <= terrainBuffer && vState == VerticalState.grounded))
                {
                    localVelocity.y = Mathf.Clamp(groundDistance, 0, 1) * Physics.gravity.y * gravityMultiplier;
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

        }
        else
        {
            localVelocity.y = Mathf.Clamp(groundDistance, -1, 0) * Physics.gravity.y * gravityMultiplier;
        }

        if (hitInfo.collider != null)
        {
            if (vState == VerticalState.falling)
            {
                vState = VerticalState.grounded;
                HitGround.Invoke();
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
                Fell.Invoke();
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
                StartedMoving.Invoke();
            }
        }
        else if (movingState != HorizontalState.idle)
        {
            movingState = HorizontalState.idle;
            StoppedMoving.Invoke();
        }

        if (OnTopOf?.GetComponent<IMovingGround>() != null)
        {
            parentVelocity = OnTopOf.GetComponent<IMovingGround>().GetVelocity();
        }
        else
        {
            parentVelocity = Vector3.zero;
        }
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
        rb.AddForce(velocityChange, ForceMode.VelocityChange);
    }
    protected virtual void OnCollisionStay(Collision collision)
    {
        if (externalVelocity.sqrMagnitude != 0)
        {
            foreach (ContactPoint p in collision.contacts)
            {
                float dot = Vector3.Dot(p.normal, externalVelocity.normalized);
                if (dot < 0)
                {
                    externalVelocity += externalVelocity * dot;
                }
            }
        }

    }
}
