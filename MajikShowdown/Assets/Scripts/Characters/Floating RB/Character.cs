using Mirror;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Character : FloatingRigidbody
{
    [Header("Movement Options")]
    [SerializeField] protected float speed;
    [SerializeField] protected bool accelerate;
    [SerializeField] protected float accelerationTime;
    protected float accelerationSpeed;

    [Header("Jump Options")]
    [SerializeField] protected float jumpForce;
    [SerializeField] protected float jumpTime;
    [SerializeField] protected float jumpingSpeedMultiplier;
     [SerializeField]protected float  jumpCooldown;
    protected bool jumpOnCooldown = false;
    protected bool canJumpOnAir = false;
    protected Timer jumpTimer = new Timer();
    [Header("Jump Events")]
    [SerializeField] protected UnityEvent Jumped;
    [SerializeField] protected UnityEvent FellOnJump;
    public enum CharVerticalState { falling, grounded, jumping };
    CharVerticalState cvState; // N�O USE ESTA VARIAVEL use PvState ao inv�s
    public CharVerticalState CvState
    {
        get
        {
            switch (vState)
            {
                case VerticalState.falling: cvState = CharVerticalState.falling; break;
                case VerticalState.grounded: cvState = CharVerticalState.grounded; break;
                default: cvState = CharVerticalState.jumping; break;
            }
            return cvState;
        }
        set
        {
            cvState = value;
            switch (value)
            {
                case CharVerticalState.falling: vState = VerticalState.falling; break;
                case CharVerticalState.grounded: vState = VerticalState.grounded; break;
                default: vState = VerticalState.none; break;
            }
        }
    }
    protected override void Awake()
    {
        base.Awake();
        jumpTimer.timedEvent.AddListener(JumpForce);
        accelerationSpeed = speed / accelerationTime;
    }

    protected override void FixedUpdate()
    {
        if (CvState != CharVerticalState.jumping)
        {
            Float();
        }
        else if (!jumpTimer.timer(jumpTime, Time.fixedDeltaTime, true, false))
        {
            stopJump();
        }
        HandleRotation();
        UpdateVelocity();
    }
    protected virtual void HandleRotation()
    {
        RotateFoward();
    }

    protected void Jump(bool pressed)
    {
        if (pressed && !movePaused)
        {
            if ((CvState == CharVerticalState.grounded|| canJumpOnAir) && !jumpOnCooldown)
            {
                jumpTimer.SetTimer(0);
                jumpTimer.Paused = false;
                CvState = CharVerticalState.jumping;
                Jumped.Invoke();
                StartCoroutine(JumpCooldown());
            }
        }
        else if (CvState == CharVerticalState.jumping)
        {
            jumpTimer.SetTimer(jumpTime);
        }
    }
    protected void stopJump()
    {
        CvState = CharVerticalState.falling;
        jumpTimer.SetTimer(0);
        FellOnJump.Invoke();
        jumpTimer.Paused = true;
    }
    protected void JumpForce()
    {
        //pular
        localVelocity.y = 0;
        localVelocity += Vector3.up * Mathf.Sqrt(jumpForce * -Physics.gravity.y * gravityMultiplier);
        //parar de pular se bater a cabe�a
        if (Physics.Raycast(rb.position, Vector3.up, (height / 2) * 1.1f, Physics.AllLayers, QueryTriggerInteraction.Ignore))
        {
            Jump(false);
        }
    }
    protected IEnumerator JumpCooldown()
    {
        jumpOnCooldown = true;
        yield return new WaitForSeconds(jumpCooldown);
        jumpOnCooldown = false;
    }
    protected float rotationAux;
    protected void RotateFoward()
    {
        float angle = Vector3.SignedAngle(Vector3.forward, lastHorizontalDirection, Vector3.up);
        if (Mathf.Abs(Mathf.Abs(transform.eulerAngles.y) - angle) > 0.1f)
        {
            float SmoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, angle, ref rotationAux, 0.05f);
            transform.rotation = Quaternion.Euler(0, SmoothAngle, 0);
        }
    }
    protected void RotateTowards(Vector3 direction)
    {
        float angle = Vector3.SignedAngle(Vector3.forward, direction, Vector3.up);
        if (Mathf.Abs(Mathf.Abs(transform.eulerAngles.y) - angle) > 0.1f)
        {
            float SmoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, angle, ref rotationAux, 0.05f);
            transform.rotation = Quaternion.Euler(0, SmoothAngle, 0);
        }
    }
    protected void Move(Vector3 dir, float speed)
    {
        if (movePaused) dir = Vector3.zero;
        dir.y = 0;
        dir = Vector3.ClampMagnitude(dir, 1);
        if(accelerate && accelerationTime > 0)
        {
            accelerationSpeed = speed / accelerationTime;
            Vector3 velocityChange = new Vector3(dir.x * speed, localVelocity.y, dir.z * speed) - localVelocity;
            SetAcceleration(velocityChange * accelerationSpeed);
        }
        else
        {
            SetAcceleration(Vector3.zero);
            if (CvState != CharVerticalState.grounded)
            {
                speed *= jumpingSpeedMultiplier;
            }
            SetVelocity(new Vector3(dir.x * speed, localVelocity.y, dir.z * speed));
        }
        
    }
    protected void Move(Vector3 vel)
    {
        Vector3 dir = vel;
        dir.y = 0;
        dir = Vector3.ClampMagnitude(dir, 1);
        float speed = vel.magnitude;
        if (movePaused) dir = Vector3.zero;
        if (accelerate && accelerationTime > 0)
        {
            accelerationSpeed = speed / accelerationTime;
            Vector3 velocityChange = new Vector3(dir.x * speed, localVelocity.y, dir.z * speed) - localVelocity;
            SetAcceleration(velocityChange.normalized * accelerationSpeed);
        }
        else
        {
            SetAcceleration(Vector3.zero);
            if (CvState != CharVerticalState.grounded)
            {
                speed *= jumpingSpeedMultiplier;
            }
            SetVelocity(new Vector3(dir.x * speed, localVelocity.y, dir.z * speed));
        }

    }
    public void KnockBack(Vector3 direction, float strenght)
    {
        AddExternalVelocity(direction*strenght);
    }

}
