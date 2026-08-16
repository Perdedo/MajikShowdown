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
    public GameObject mesh;
    public Collider col;
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
    [Header("Dash Options")]
    [SerializeField] protected float DashForce;
    [SerializeField] protected float DashCooldown;
    [SerializeField] protected float GravityNegationTime;
    Timer dashTimer = new Timer(false), gravityTimer = new Timer(false);
    bool dashOnCooldown;

    [Header("Push Events")]
    public UnityEvent StartedPushing;
    public UnityEvent StoppedPushing;
    public PushableObject pushing;
    [HideInInspector] public PlayerInput input;
    
    [SyncVar(hook = "GetReady")]public bool readyForHorde = false;
    [Header("Network")]
    public bool network = true;

    [Header("Spellcasting")]
    public SpellCaster caster;
    [HideInInspector]public bool Casting;
    [HideInInspector][SyncVar (hook = nameof(OnDeathValueChange))] public bool dead = false;
    public float CastPoseTime = 3f;

    public InteractableObject currentInteraction;

    public Animator animator;
    [SerializeField] float speedChangeRate = 10;
    float xAux = 0, yAux = 0;
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
        input = GetComponent<PlayerInput>();
        DamageHandler = GetComponent<PlayerDamageHandler>();
        //cameraRotation = new CameraRotation { x = lookAnchor.localRotation.eulerAngles.x, y = transform.localRotation.eulerAngles.y };
        //cameraAim = playerCamera.GetComponent<CinemachineThirdPersonAim>();
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        playerCamera.Priority = 2;
        input.enabled = true;
        readyForHorde = false;
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
        if (isLocalPlayer || !network)
        {
            UpdateVelocity();
            if (!isServer && network)
            {
                CMDUpdateVelocity();
            }
        }
    }
    private void Update()
    {
        Move(directionAnchor.forward * directionInput.y + directionAnchor.right * directionInput.x, speed);
        if (dashOnCooldown)
        {
            if (gravityTimer.timer(GravityNegationTime, Time.deltaTime, false, false))
            {
                gravityPaused = false;
            }
            if (dashTimer.timer(DashCooldown, Time.deltaTime, false, true))
            {
                dashOnCooldown = false;
                gravityPaused = false;
                gravityTimer.SetTimer(0);
            }
        }

        xAux = (float)System.Math.Round(Mathf.MoveTowards(xAux, directionInput.x, Time.deltaTime * speedChangeRate), 2);
        yAux = (float)System.Math.Round(Mathf.MoveTowards(yAux, directionInput.y, Time.deltaTime * speedChangeRate), 2);
        animator.SetFloat("InputX", xAux);
        animator.SetFloat("InputY", yAux);
        /*if(isLocalPlayer && GameManager.Instance.hordeController.inPause)
        {
            if(Input.GetKeyDown(KeyCode.R))
            {
                readyForHorde = true;
            }
        }
        /*if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            Dash(directionInput);
        }*/
        //RotateCamera();
    }

    public void OnDeathValueChange(bool oldVal, bool newVal)
    {
        /*if(input != null && isLocalPlayer)
        {
            input.enabled = !newVal;
        }*/
        if(mesh != null)
        {
            mesh.SetActive(!newVal);
        }
        if(col != null)
        {
            col.enabled = !newVal;
        }
        directionInput = Vector2.zero;
    }

    public void ReadyInput(InputAction.CallbackContext context)
    {
        if (!isLocalPlayer && network) return;
        if (!context.started) return;
        if (!GameManager.Instance.hordeController.inPause) return;
        if (!GameManager.Instance.uiController.playerUI.inGame) return;
        //readyForHorde = true;
        CMDReadyInput();
    }

    [Command]
    void CMDReadyInput()
    {
        readyForHorde = true;
    }

    public void GetReady(bool oldVal, bool newVal)
    {
        if(newVal)
        {
            Debug.Log("Ready");
            GameManager.Instance.hordeController.CheckReadyPlayers();
        }
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

    public void JumpAnim()
    {
        animator.ResetTrigger("Jump");
        animator.SetTrigger("Jump");
    }

    public void LandAnim()
    {
        animator.ResetTrigger("Land");
        animator.SetTrigger("Land");
    }

    public void CastAnim()
    {
        animator.ResetTrigger("Casting");
        animator.SetTrigger("Casting");
    }

    public void MoveInput(InputAction.CallbackContext context)
    {
        if (!isLocalPlayer && network) return;
        if (dead) return;
        if (!GameManager.Instance.uiController.playerUI.inGame) return;

        if (!movePaused)
        {
            directionInput = Vector2.ClampMagnitude(context.ReadValue<Vector2>(), 1);
            
        }
        else
        {
            directionInput = Vector2.zero;
            animator.SetFloat("InputX", 0);
            animator.SetFloat("InputY", 0);
        }
    }

    public void JumpInput(InputAction.CallbackContext context)
    {
        if (!isLocalPlayer && network) return;
        if (dead) return;
        if (!GameManager.Instance.uiController.playerUI.inGame) return;

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
    public void DashInput(InputAction.CallbackContext context)
    {
        if (!isLocalPlayer && network) return;
        if (dead) return;
        if (!GameManager.Instance.uiController.playerUI.inGame) return;

        if (context.phase == InputActionPhase.Started)
        {
            if (!movePaused)
            {
                Dash(directionInput);
            }
        }
    }
    public void Dash(Vector2 dir)
    {
        if (!dashOnCooldown)
        {
            Vector3 v;
            if (dir.sqrMagnitude != 0)
            {
                v = (directionAnchor.transform.right * dir.x) + (directionAnchor.transform.forward * dir.y);
            }
            else
            {
                v = directionAnchor.transform.forward;
            }
            AddExternalVelocity(v.normalized * DashForce);
            animator.ResetTrigger("Dash");
            animator.SetTrigger("Dash");
            dashOnCooldown = true;
            gravityPaused = true;
        }
    }
    public void InteractInput(InputAction.CallbackContext context)
    {
        if (!isLocalPlayer && network) return;
        if (dead) return;
        if (!GameManager.Instance.uiController.playerUI.inGame) return;

        if (context.phase == InputActionPhase.Started)
        {
            Interact();
        }
    }
    public void Interact()
    {
        if (currentInteraction != null)
        {
            currentInteraction.Interact(this);
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
        /*if (Casting)
        {
            RotateTowards(directionAnchor.forward);
        }
        else
        {
            RotateFoward();
        }*/

        RotateTowards(directionAnchor.forward);
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
