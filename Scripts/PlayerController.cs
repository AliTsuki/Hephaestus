using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [System.Serializable]
    public class MovementSettings
    {
        public float ForwardSpeed = 8.0f;
        public float BackwardSpeed = 4.0f;
        public float SidewaysSpeed = 6.0f;
        public float RunMultiplier = 2.0f;
        public KeyCode RunButton = KeyCode.LeftShift;
        public KeyCode JumpButton = KeyCode.Space;
        public float JumpForce = 30f;

        public AnimationCurve SlopeCurveModifier = new AnimationCurve(new Keyframe(-90f, 1.0f), new Keyframe(0.0f, 1.0f), new Keyframe(90f, 0.0f));

        [HideInInspector]
        public float CurrentTargetSpeed = 8f;

        bool m_running;

        public void UpdateDesiredTargetSpeed(Vector2 input)
        {
            if (input.x == 0 && input.y == 0)
            {
                return;
            }

            if (input.x > 0 || input.x < 0)
                CurrentTargetSpeed = SidewaysSpeed;
            if (input.y < 0)
                CurrentTargetSpeed = BackwardSpeed;
            if (input.y > 0)
                CurrentTargetSpeed = ForwardSpeed;

            if (Input.GetKey(RunButton))
            {
                CurrentTargetSpeed *= RunMultiplier;
                m_running = true;
            }
            else m_running = false;
        }

        public bool Running
        {
            get
            {
                return m_running;
            }
        }
    }

    [System.Serializable]
    public class AdvancedSettings
    {
        public float groundCheckDistance = 0.01f; //Distance away from ground to be considered "grounded"
        public float stickToGroundHelperDistance = 0.5f; //Stops character
        public float slowDownRate = 20f;
        public bool controlWhenInAir;
        public float airSpeedMultiplier = 0.25f;

        [Tooltip("Set to 0.1 or more if stuck in wall")]
        public float shellOffset;
    }

    public Camera cam;
    public MovementSettings movementSettings = new MovementSettings();
    public PlayerLookAt lookAt = new PlayerLookAt();
    public AdvancedSettings advancedSettings = new AdvancedSettings();
    public PlayerChunkLoader loader = new PlayerChunkLoader();
    public PlayerInteraction interaction = new PlayerInteraction();

    public int chunkRenderDistance = 2;

    Rigidbody rb;
    BoxCollider box;

    float yRotation;
    Vector3 groundContactNormal;
    bool jump, previouslyGrounded, jumping, isGrounded;

    Vector3Int currentChunkUnderPlayer;

    public Vector3 Velocity
    {
        get
        {
            return rb.velocity;
        }
    }

    public bool Grounded
    {
        get
        {
            return isGrounded;
        }
    }

    public bool Jumping
    {
        get
        {
            return jumping;
        }
    }

    public bool Running
    {
        get
        {
            return movementSettings.Running;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        box = GetComponent<BoxCollider>();
        lookAt.Initialize(transform, cam.transform);

        currentChunkUnderPlayer = World.WorldCoordsToChunkCoords(transform.position.x, transform.position.y, transform.position.z);

        loader.Initialize(currentChunkUnderPlayer, chunkRenderDistance);
    }

    IEnumerator WaitForWorld()
    {
        Chunk chunk = null;

        yield return new WaitUntil(() => World.instance.GetChunkAt(currentChunkUnderPlayer.x, 0, currentChunkUnderPlayer.z, out chunk));
        if(chunk != null)
        {
            yield return new WaitUntil(() => chunk.ready);
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        RotateView();
        lookAt.LockUpdate();

        if(Input.GetKeyDown(movementSettings.JumpButton) && !jump)
            jump = true;
    }

    void LateUpdate()
    {
        Vector3Int previousChunkUnderPlayer = World.WorldCoordsToChunkCoords(transform.position.x, transform.position.y, transform.position.z);

        if(previousChunkUnderPlayer != currentChunkUnderPlayer)
        {
            GameController.instance.MoveCollider(previousChunkUnderPlayer.x, previousChunkUnderPlayer.y, previousChunkUnderPlayer.z);

            currentChunkUnderPlayer.x = previousChunkUnderPlayer.x;
            currentChunkUnderPlayer.y = previousChunkUnderPlayer.y;
            currentChunkUnderPlayer.z = previousChunkUnderPlayer.z;
        }
    }

    void FixedUpdate()
    {
        if (interaction.InteractWithBlocks(cam.transform))
            GameController.instance.MoveCollider(currentChunkUnderPlayer.x, currentChunkUnderPlayer.y, currentChunkUnderPlayer.z);

        GroundCheck();
        Vector2 input = GetInput();

        if((Mathf.Abs(input.x) > float.Epsilon || Mathf.Abs(input.y) > float.Epsilon) && (advancedSettings.controlWhenInAir || isGrounded))
        {
            // Always move along the camera forward as it is the direction taht is being aimed at
            Vector3 desiredMove = cam.transform.forward * input.y + cam.transform.right * input.x;
            desiredMove = Vector3.ProjectOnPlane(desiredMove, groundContactNormal).normalized;

            if(!isGrounded && advancedSettings.controlWhenInAir)
            {
                desiredMove.x = desiredMove.x * movementSettings.CurrentTargetSpeed * advancedSettings.airSpeedMultiplier;
                desiredMove.y = desiredMove.y * movementSettings.CurrentTargetSpeed * advancedSettings.airSpeedMultiplier;
                desiredMove.z = desiredMove.z * movementSettings.CurrentTargetSpeed * advancedSettings.airSpeedMultiplier;
            }
            else
            {
                desiredMove.x = desiredMove.x * movementSettings.CurrentTargetSpeed;
                desiredMove.y = desiredMove.y * movementSettings.CurrentTargetSpeed;
                desiredMove.z = desiredMove.z * movementSettings.CurrentTargetSpeed;
            }

            if(rb.velocity.sqrMagnitude < (movementSettings.CurrentTargetSpeed * movementSettings.CurrentTargetSpeed))
            {
                rb.AddForce(desiredMove * SlopeMultiplier(), ForceMode.Impulse);
            }
        }

        if(!isGrounded)
        {
            rb.drag = 2f;

            if(jump)
            {
                rb.drag = 0f;
                rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
                rb.AddForce(new Vector3(0f, movementSettings.JumpForce, 0f), ForceMode.Impulse);
                jumping = true;
            }

            if(!jumping && Mathf.Abs(input.x) < float.Epsilon && Mathf.Abs(input.y) < float.Epsilon && rb.velocity.magnitude < 1f)
            {
                rb.Sleep();
            }
        }
        else
        {
            rb.drag = 0f;
            if(previouslyGrounded && !jumping)
            {
                StickToGroundHelper();
            }
        }
        jump = false;
    }

    private float SlopeMultiplier()
    {
        float angle = Vector3.Angle(groundContactNormal, Vector3.up);
        return movementSettings.SlopeCurveModifier.Evaluate(angle);
    }

    private void StickToGroundHelper()
    {
        RaycastHit hitInfo;
        if(Physics.SphereCast(transform.position, box.size.x * (1.0f - advancedSettings.shellOffset), Vector3.down, out hitInfo,
            ((box.size.y / 2f) - box.size.x) +
            advancedSettings.stickToGroundHelperDistance, Physics.AllLayers, QueryTriggerInteraction.Ignore))
        {
            if(Mathf.Abs(Vector3.Angle(hitInfo.normal, Vector3.up)) < 85f)
            {
                rb.velocity = Vector3.ProjectOnPlane(rb.velocity, hitInfo.normal);
            }
        }
    }

    private Vector2 GetInput()
    {
        Vector2 input = new Vector2
        {
            x = Input.GetAxis("Horizontal"),
            y = Input.GetAxis("Vertical")
        };
        movementSettings.UpdateDesiredTargetSpeed(input);
        return input;
    }

    private void RotateView()
    {
        if (Mathf.Abs(Time.timeScale) < float.Epsilon) return;

        float oldYRotation = transform.eulerAngles.y;

        lookAt.LookRotation(transform, cam.transform);

        if(isGrounded || advancedSettings.controlWhenInAir)
        {
            Quaternion velRotation = Quaternion.AngleAxis(transform.eulerAngles.y - oldYRotation, Vector3.up);
            rb.velocity = velRotation * rb.velocity;
        }
    }

    private void GroundCheck()
    {
        previouslyGrounded = isGrounded;
        RaycastHit hitInfo;
        if(Physics.SphereCast(transform.position, box.size.x * (1.0f - advancedSettings.shellOffset), Vector3.down, out hitInfo,
            ((box.size.y / 2f) - box.size.x) + advancedSettings.groundCheckDistance, Physics.AllLayers, QueryTriggerInteraction.Ignore))
        {
            isGrounded = true;
            groundContactNormal = hitInfo.normal;
        }
        else
        {
            isGrounded = false;
            groundContactNormal = Vector3.up;
        }

        if(!previouslyGrounded && isGrounded && jumping)
        {
            jumping = false;
        }
    }
}
