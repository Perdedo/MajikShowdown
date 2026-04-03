using Mirror;
using System;
using System.Collections;
using Unity.Cinemachine;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
public class Player : Character
{
    [Header("Camera Options")]
    public CinemachineCamera playerCamera;
    //CinemachineThirdPersonAim cameraAim;
    //public float CameraClampAngle = 80;
    //public CameraSensitivity cameraSensitivity = new CameraSensitivity { vertical = 50, horizontal = 50 };
    //CameraRotation cameraRotation;
    [Header("Anchors")]
    public Transform directionAnchor;
    public Transform lookAnchor;
    [System.NonSerialized] public Vector2 directionInput;
    [Header("Jump Input")]
    [SerializeField] protected float coyoteTime;
    [SerializeField] protected float jumpBuffering;
    bool jumpBuffer;

    [Header("Push Events")]
    public UnityEvent StartedPushing;
    public UnityEvent StoppedPushing;
    public PushableObject pushing;
    PlayerInput input;
    bool Casting;
    [Header("Network")]
    public bool network = true;
    //public PlayerData data;

    /*[Serializable]
    public struct CameraSensitivity
    {
        public float vertical;
        public float horizontal;
    }
    public struct CameraRotation
    {
        public float x;
        public float y;
    }*/
    protected override void Awake()
    {
        base.Awake();
        GameManager.Instance.AddPlayer(this);
        //GameManager.Instance.Players.Add(this);
        Fell.AddListener(CoyoteTime);
        HitGround.AddListener(StopCoyoteTime);
        HitGround.AddListener(PeformJumpBuffering);
        //cameraRotation = new CameraRotation { x = lookAnchor.localRotation.eulerAngles.x, y = transform.localRotation.eulerAngles.y };
        //cameraAim = playerCamera.GetComponent<CinemachineThirdPersonAim>();
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        playerCamera.Priority = 2;
        input = GetComponent<PlayerInput>();
        input.enabled = true;
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
        if (!isServer && network)
        {
            CMDUpdateVelocity();
        }
    }
    private void Update()
    {
        Move(directionAnchor.forward * directionInput.y + directionAnchor.right * directionInput.x, speed);
        
        //RotateCamera();

        
    }
    /*void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(playerCamera.GetComponent<CinemachineThirdPersonAim>().AimTarget, 0.5f);
    }*/
    void CoyoteTime()
    {
        StartCoroutine(CoyoteTimer());
    }
    void StopCoyoteTime()
    {
        StopCoroutine(CoyoteTimer());
        canJumpOnAir = false;
    }
    void PeformJumpBuffering()
    {
        if (jumpBuffer)
        {
            Jump(true);
            jumpBuffer = false;
        }
    }
    public void MoveInput(InputAction.CallbackContext context)
    {
        if(!isLocalPlayer && network)
        {
            return;
        }if (!movePaused)
        {
            directionInput = Vector2.ClampMagnitude(context.ReadValue<Vector2>(), 1);
        }
        else
        {
            directionInput = Vector2.zero;
        }
    }

    public void JumpInput(InputAction.CallbackContext context)
    {
        if(!isLocalPlayer && network)
        {
            return;
        }
        if (context.phase == InputActionPhase.Started)
        {
            if (!movePaused)
            {
                if (vState != VerticalState.grounded && canJumpOnAir == false)
                {
                    StartCoroutine(JumpBuffer());
                }
                Jump(true);
                if (canJumpOnAir == true)
                {
                    canJumpOnAir = false;
                }
            }
        }
        else if (context.phase == InputActionPhase.Canceled)
        {
            Jump(false);
            jumpBuffer = false;
        }
    }
    /*Vector2 lookInput;
    public void LookInput(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
        
    }
    void RotateCamera()
    {
        cameraRotation.x -= lookInput.y * cameraSensitivity.vertical * Time.deltaTime;
        cameraRotation.x = Mathf.Clamp(cameraRotation.x, -CameraClampAngle, CameraClampAngle);
        cameraRotation.y += lookInput.x * cameraSensitivity.horizontal * Time.deltaTime;
        lookAnchor.localRotation = Quaternion.Euler(cameraRotation.x, cameraRotation.y, 0);
        //transform.localRotation = Quaternion.Euler(0, cameraRotation.y, 0);
        //lookAnchor.Rotate(Vector3.right, -lookInput.y * cameraSensitivity* Time.deltaTime);
        //transform.Rotate(Vector3.up, lookInput.x * cameraSensitivity* Time.deltaTime);
    }*/
    protected override void HandleRotation()
    {
        if (Casting)
        {
            RotateTowards(directionAnchor.forward);
        }
        else
        {
            RotateFoward();
        }
        
        //transform.eulerAngles = new Vector3(0, lookAnchor.eulerAngles.y, 0);
    }
    IEnumerator CoyoteTimer()
    {
        canJumpOnAir = true;
        yield return new WaitForSeconds(coyoteTime);
        canJumpOnAir = false;
    }
    IEnumerator JumpBuffer()
    {
        jumpBuffer = true;
        yield return new WaitForSeconds(jumpBuffering);
        jumpBuffer = false;
    }

    private void OnDestroy()
    {
        GameManager.Instance.RemovePlayer(this);
    }

    [Command]
    protected void CMDUpdateVelocity()
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
        Vector3 velocityChange = worldVelocity - rb.linearVelocity;
        rb.AddForce(velocityChange, ForceMode.VelocityChange);
    }
}
