using Cinemachine;

using UnityEngine;
using UnityEngine.InputSystem;


/// <summary>
/// Class that controls the player character.
/// </summary>
[RequireComponent(typeof(CharacterController), typeof(Player))]
public class PlayerController : MonoBehaviour
{
    private enum MovementModes
    {
        Normal,
        Crouch,
        Sprint,
        Climb
    }

    public PlayerControls Controls { get; private set; }
    private bool captureInput = true;
    // Movement
    [Header("Movement")]
    public Transform GroundCheck;
    public Transform HeadCheck;
    public float CrouchSpeed = 2f;
    public float NormalSpeed = 4f;
    public float SprintSpeed = 6f;
    public float Gravity = -9.81f;
    public float JumpHeight = 1f;
    public float JumpLateMarginTime = 0.1f;
    public float AirMoveSpeed = 0.2f;
    public float CollisionCheckDistance = 0.15f;
    public bool isGrounded = false;
    public bool isHeadColliding = false;
    public float timeSinceLastGrounded = 0.0f;
    private MovementModes currentMoveMode = MovementModes.Normal;
    private Vector3 horizontalMoveInput;
    private float verticalMoveInput = 0.0f;
    private Vector3 moveDelta;
    private Vector3 moveDeltaLast;
    private float moveDeltaLastMagnitudeGrounded = 0.0f;
    // Aim
    [Header("Aim")]
    public Camera Cam;
    public CinemachineVirtualCamera VCam;
    public float MouseSensitivity = 10f;
    public bool InvertY = true;
    private float xRotation = 0f;
    private Vector2 aimInput;
    private Vector2 aimDelta;
    
    // Block selection
    private GameObject BlockSelectorGO;
    public RaycastHit BlockSelectorHit { get; private set; }
    public Vector3Int CurrentBlockSelectedPos { get; private set; } = new Vector3Int();
    public Block CurrentBlockSelected { get; private set; }
    public bool IsBlockSelected { get; private set; } = false;

    // References
    private CharacterController cController;


    // This function is called when the object becomes enabled and active.
    private void OnEnable()
    {
        this.Controls.Gameplay.Enable();
    }

    // This function is called when the behaviour becomes disabled or inactive.
    private void OnDisable()
    {
        this.Controls.Gameplay.Disable();
    }

    // Awake is called when the script instance is being loaded.
    private void Awake()
    {
        this.InitializeReferencesAndControls();
    }

    // Start is called before the first frame update.
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame.
    private void Update()
    {
        
    }

    // FixedUpdate is called a fixed number of times per second. Used for physics calculations irrespective of frame update speed.
    private void FixedUpdate()
    {
        this.ApplyAim();
        this.ApplyMovement();
        this.ModifyCollider();
        this.ModifyFOV();
        this.UpdateBlockSelection();
    }

    // OnTriggerEnter is called when the collider enters a trigger collider.
    private void OnTriggerEnter(Collider other)
    {

    }

    // OnTriggerExit is called when the collider exits a trigger collider.
    private void OnTriggerExit(Collider other)
    {

    }

    /// <summary>
    /// Initializes references and controls for player.
    /// </summary>
    private void InitializeReferencesAndControls()
    {
        this.cController = this.GetComponent<CharacterController>();
        this.BlockSelectorGO = GameObject.Instantiate(GameManager.Instance.BlockSelectorPrefab);
        this.Controls = new PlayerControls();
        this.Controls.Gameplay.Move.performed += this.Move_performed;
        this.Controls.Gameplay.Move.canceled += this.Move_canceled;
        this.Controls.Gameplay.Aim.performed += this.Aim_performed;
        this.Controls.Gameplay.Aim.canceled += this.Aim_canceled;
        this.Controls.Gameplay.Attack.performed += this.Attack_performed;
        this.Controls.Gameplay.Attack.canceled += this.Attack_canceled;
        this.Controls.Gameplay.Interact.performed += this.Interact_performed;
        this.Controls.Gameplay.Interact.canceled += this.Interact_canceled;
        this.Controls.Gameplay.Sprint.performed += this.Sprint_performed;
        this.Controls.Gameplay.Sprint.canceled += this.Sprint_canceled;
        this.Controls.Gameplay.Crouch.performed += this.Crouch_performed;
        this.Controls.Gameplay.Crouch.canceled += this.Crouch_canceled;
        this.Controls.Gameplay.Jump.performed += this.Jump_performed;
        this.Controls.Gameplay.ToggleMenu.performed += this.ToggleMenu_performed;
        this.Controls.Gameplay.DEBUGLOCKCURSORTOGGLE.performed += this.DEBUGLOCKCURSORTOGGLE_performed;
        Cursor.visible = false;
    }

    /// <summary>
    /// Applies aim from input to the player character and camera.
    /// </summary>
    private void ApplyAim()
    {
        float aimFOVMod = 1.0f;
        this.aimDelta = this.aimInput * this.MouseSensitivity * aimFOVMod * Time.fixedDeltaTime;
        if(this.InvertY == true)
        {
            this.xRotation -= -this.aimDelta.y;
        }
        else
        {
            this.xRotation -= this.aimDelta.y;
        }
        this.xRotation = Mathf.Clamp(this.xRotation, -89f, 89f);
        this.Cam.transform.localRotation = Quaternion.Euler(this.xRotation, 0f, 0f);
        this.transform.Rotate(Vector3.up * this.aimDelta.x);
    }

    /// <summary>
    /// Applies movement from input to player character.
    /// </summary>
    private void ApplyMovement()
    {
        this.CheckCollisions();
        this.ApplyGravity();
        this.VerticalVelocityCorrection();
        if(this.isGrounded == false)
        {
            Vector3 horizontalMovement = new Vector3(this.moveDeltaLast.x, 0, this.moveDeltaLast.z);
            Vector3 airMovement = this.horizontalMoveInput * this.AirMoveSpeed * Time.fixedDeltaTime;
            airMovement = airMovement.ReorientAlongTransform(this.transform);
            Vector3 newMovement = Vector3.ClampMagnitude(horizontalMovement + airMovement, Mathf.Max(this.moveDeltaLastMagnitudeGrounded, this.AirMoveSpeed * Time.fixedDeltaTime));
            this.moveDelta = newMovement + (new Vector3(0, this.verticalMoveInput, 0) * Time.fixedDeltaTime);
            this.moveDeltaLast = this.moveDelta;
            this.timeSinceLastGrounded += Time.fixedDeltaTime;
        }
        else
        {
            float speed = this.GetCurrentMoveSpeed();
            this.moveDelta = (this.horizontalMoveInput * speed * Time.fixedDeltaTime) + (new Vector3(0, this.verticalMoveInput, 0) * Time.fixedDeltaTime);
            this.moveDelta = this.moveDelta.ReorientAlongTransform(this.transform);
            this.moveDeltaLast = this.moveDelta;
            this.moveDeltaLastMagnitudeGrounded = new Vector3(this.moveDeltaLast.x, 0, this.moveDeltaLast.z).magnitude;
            this.timeSinceLastGrounded = 0.0f;
        }
        this.cController.Move(this.moveDelta);
    }

    /// <summary>
    /// Checks if the players head or feet are colliding with the world geometry.
    /// </summary>
    private void CheckCollisions()
    {
        this.isGrounded = Physics.CheckSphere(this.GroundCheck.position, this.CollisionCheckDistance, 1<<GameManager.Instance.LevelGeometryLayerMask);
        if(this.isGrounded == false)
        {
            this.isHeadColliding = Physics.CheckSphere(this.HeadCheck.position, this.CollisionCheckDistance, 1<<GameManager.Instance.LevelGeometryLayerMask);
        }
        else
        {
            this.isHeadColliding = false;
        }
    }

    /// <summary>
    /// Corrects vertical velocity when player is grounded.
    /// </summary>
    private void VerticalVelocityCorrection()
    {
        if((this.isGrounded == true && this.verticalMoveInput < 0) || (this.isHeadColliding == true && this.verticalMoveInput >= 0f))
        {
            this.verticalMoveInput = -0.5f;
        }
    }

    /// <summary>
    /// Applies gravity to player character.
    /// </summary>
    private void ApplyGravity()
    {
        this.verticalMoveInput += this.Gravity * Time.fixedDeltaTime;
    }

    /// <summary>
    /// Updates movement speed to reflect current character movement mode.
    /// </summary>
    /// <returns>Returns the current movement speed.</returns>
    private float GetCurrentMoveSpeed()
    {
        switch(this.currentMoveMode)
        {
            case MovementModes.Crouch:
            {
                return this.CrouchSpeed;
            }
            case MovementModes.Normal:
            {
                return this.NormalSpeed;
            }
            case MovementModes.Sprint:
            {
                return this.SprintSpeed;
            }
            default:
            {
                return this.NormalSpeed;
            }
        }
    }

    /// <summary>
    /// Performs actions on left click.
    /// </summary>
    /// <param name="AttackDown">Is left mouse button pressed?</param>
    private void ApplyAttack(bool AttackDown)
    {
        if(AttackDown == true)
        {
            if(this.IsBlockSelected == true)
            {
                if(World.TryGetBlockFromWorldPos(this.CurrentBlockSelectedPos, out _) == true)
                {
                    World.AddBlockUpdateToQueue(new Block.BlockUpdateParameters(this.CurrentBlockSelectedPos, Block.Air));
                }
            }
        }
        else
        {
            
        }
    }

    /// <summary>
    /// Perfroms actions on right click.
    /// </summary>
    /// <param name="InteractDown">Is right mouse button pressed?</param>
    private void ApplyInteract(bool InteractDown)
    {
        if(InteractDown == true)
        {
            if(this.IsBlockSelected == true)
            {
                Vector3Int blockSelectedPos = this.CurrentBlockSelectedPos + this.BlockSelectorHit.normal.ToInt();
                if(blockSelectedPos != GameManager.Instance.Player.transform.position.RoundToInt() && blockSelectedPos != GameManager.Instance.Player.transform.position.RoundToInt() + new Vector3Int(0, 1, 0))
                {
                    if(World.TryGetBlockFromWorldPos(blockSelectedPos, out _) == true)
                    {
                        World.AddBlockUpdateToQueue(new Block.BlockUpdateParameters(blockSelectedPos, Block.Stone));
                    }
                }
            }
        }
        else
        {
            
        }
    }

    /// <summary>
    /// Applies sprint changes to character movement.
    /// </summary>
    /// <param name="sprinting">Is sprint button pressed?</param>
    private void ApplySprint(bool sprinting)
    {
        if(sprinting == true)
        {
            this.currentMoveMode = MovementModes.Sprint;
        }
        else
        {
            this.currentMoveMode = MovementModes.Normal;
        }
    }

    /// <summary>
    /// Applies crouch changes to character movement.
    /// </summary>
    /// <param name="crouching">Is crouch button pressed?</param>
    private void ApplyCrouch(bool crouching)
    {
        if(crouching == true)
        {
            this.currentMoveMode = MovementModes.Crouch;
        }
        else
        {
            this.currentMoveMode = MovementModes.Normal;
        }
    }

    /// <summary>
    /// Performs a jump for player character.
    /// </summary>
    private void ApplyJump()
    {
        if(this.isGrounded == true || this.timeSinceLastGrounded <= this.JumpLateMarginTime)
        {
            this.verticalMoveInput = Mathf.Sqrt(this.JumpHeight * -2f * this.Gravity);
        }
    }

    /// <summary>
    /// Toggles in game menu screen.
    /// </summary>
    private void ApplyToggleMenu()
    {
        // TODO: Add pause menu
    }

    /// <summary>
    /// Toggles mouse capture for testing purposes.
    /// </summary>
    private void ToggleCaptureInput()
    {
        if(this.captureInput == true)
        {
            this.captureInput = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            this.horizontalMoveInput = Vector3.zero;
            this.aimInput = Vector2.zero;
        }
        else
        {
            this.captureInput = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    /// <summary>
    /// Modifies player collider size when crouching.
    /// </summary>
    private void ModifyCollider()
    {
        this.cController.center = new Vector3(0, this.cController.height / 2f, 0);
        if(this.currentMoveMode == MovementModes.Crouch)
        {
            this.cController.height = Mathf.Lerp(this.cController.height, GameManager.CrouchCharacterHeight, GameManager.CrouchRate);
            this.Cam.transform.localPosition = new Vector3(0f, (this.cController.height / 2f) + this.cController.center.y - 0.3f, 0f);
            this.GroundCheck.localPosition = new Vector3(0f, -(this.cController.height / 2f) + this.cController.center.y + 0.05f, 0f);
            this.HeadCheck.localPosition = new Vector3(0f, (this.cController.height / 2f) + this.cController.center.y - 0.05f, 0f);
        }
        else
        {
            this.cController.height = Mathf.Lerp(this.cController.height, GameManager.DefaultCharacterHeight, GameManager.CrouchRate);
            this.Cam.transform.localPosition = new Vector3(0f, (this.cController.height / 2f) + this.cController.center.y - 0.3f, 0f);
            this.GroundCheck.localPosition = new Vector3(0f, -(this.cController.height / 2f) + this.cController.center.y + 0.05f, 0f);
            this.HeadCheck.localPosition = new Vector3(0f, (this.cController.height / 2f) + this.cController.center.y - 0.05f, 0f);
        }
    }

    /// <summary>
    /// Modifies player camera field of view while sprinting.
    /// </summary>
    private void ModifyFOV()
    {
        if(this.currentMoveMode == MovementModes.Sprint && this.moveDeltaLastMagnitudeGrounded > 0f)
        {
            this.VCam.m_Lens.FieldOfView = Mathf.Lerp(this.Cam.fieldOfView, GameManager.SprintFOV, 0.2f);
        }
        else
        {
            this.VCam.m_Lens.FieldOfView = Mathf.Lerp(this.Cam.fieldOfView, GameManager.DefaultFOV, 0.2f);
        }
    }

    /// <summary>
    /// Casts a ray forward from the camera, checks if ray hits terrain, gets position of hit in world coordinates and gets the current block that is present there.
    /// </summary>
    private void UpdateBlockSelection()
    {
        if(Physics.Raycast(new Ray(this.Cam.transform.position, this.Cam.transform.forward), out RaycastHit hit, GameManager.Instance.BlockSelectionMaxDistance, 1 << GameManager.Instance.LevelGeometryLayerMask))
        {
            this.BlockSelectorHit = hit;
            this.CurrentBlockSelectedPos = (this.BlockSelectorHit.point - (this.BlockSelectorHit.normal * 0.5f)).RoundToInt();
            if(World.TryGetBlockFromWorldPos(this.CurrentBlockSelectedPos, out Block block))
            {
                this.CurrentBlockSelected = block;
                this.IsBlockSelected = true;
                this.BlockSelectorGO.SetActive(true);
                this.BlockSelectorGO.transform.position = this.CurrentBlockSelectedPos;
            }
        }
        else
        {
            this.IsBlockSelected = false;
            this.BlockSelectorGO.SetActive(false);
        }
    }

    #region RawInputs
    private void Move_performed(InputAction.CallbackContext context)
    {
        if(this.captureInput == true)
        {
            this.horizontalMoveInput = new Vector3(context.ReadValue<Vector2>().x, 0, context.ReadValue<Vector2>().y);
        }
    }

    private void Move_canceled(InputAction.CallbackContext context)
    {
        if(this.captureInput == true)
        {
            this.horizontalMoveInput = new Vector3(context.ReadValue<Vector2>().x, 0, context.ReadValue<Vector2>().y);
        }
    }

    private void Aim_performed(InputAction.CallbackContext context)
    {
        if(this.captureInput == true)
        {
            this.aimInput = context.ReadValue<Vector2>();
        }
    }

    private void Aim_canceled(InputAction.CallbackContext context)
    {
        if(this.captureInput == true)
        {
            this.aimInput = context.ReadValue<Vector2>();
        }
    }

    private void Attack_performed(InputAction.CallbackContext context)
    {
        if(this.captureInput == true)
        {
            this.ApplyAttack(true);
        }
    }

    private void Attack_canceled(InputAction.CallbackContext context)
    {
        if(this.captureInput == true)
        {
            this.ApplyAttack(false);
        }
    }

    private void Interact_performed(InputAction.CallbackContext context)
    {
        if(this.captureInput == true)
        {
            this.ApplyInteract(true);
        }
    }

    private void Interact_canceled(InputAction.CallbackContext context)
    {
        if(this.captureInput == true)
        {
            this.ApplyInteract(false);
        }
    }

    private void Sprint_performed(InputAction.CallbackContext context)
    {
        if(this.captureInput == true)
        {
            this.ApplySprint(true);
        }
    }

    private void Sprint_canceled(InputAction.CallbackContext context)
    {
        if(this.captureInput == true)
        {
            this.ApplySprint(false);
        }
    }

    private void Crouch_performed(InputAction.CallbackContext context)
    {
        if(this.captureInput == true)
        {
            this.ApplyCrouch(true);
        }
    }

    private void Crouch_canceled(InputAction.CallbackContext context)
    {
        if(this.captureInput == true)
        {
            this.ApplyCrouch(false);
        }
    }

    private void Jump_performed(InputAction.CallbackContext context)
    {
        if(this.captureInput == true)
        {
            this.ApplyJump();
        }
    }

    private void ToggleMenu_performed(InputAction.CallbackContext context)
    {
        if(this.captureInput == true)
        {
            this.ApplyToggleMenu();
        }
    }

    private void DEBUGLOCKCURSORTOGGLE_performed(InputAction.CallbackContext context)
    {
        this.ToggleCaptureInput();
    }
    #endregion
}
