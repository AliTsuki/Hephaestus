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

        public float TargetSpeed;

        public void UpdateTargetSpeed(Vector2 inputAxis)
        {
            if(inputAxis.x == 0 && inputAxis.y == 0)
                return;

            if(inputAxis.x > 0 || inputAxis.x < 0)
                TargetSpeed = SidewaysSpeed;

            if(inputAxis.y < 0)
                TargetSpeed = BackwardSpeed;

            if(inputAxis.y > 0)
                TargetSpeed = ForwardSpeed;

        }
    }

    MovementSettings settings = new MovementSettings();
    PlayerLookAt lookAt = new PlayerLookAt();

    Camera cam;
    Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        cam = GetComponentInChildren<Camera>();

        lookAt.Initialize(transform, cam.transform);
    }

    // Update is called once per frame
    void Update()
    {
        lookAt.LookRotation(transform, cam.transform);

        lookAt.LockUpdate();
    }

    void FixedUpdate()
    {
        CheckIfLanded();

        Vector2 input = GetInput();

        Vector3 moveDirection = cam.transform.forward * input.y + cam.transform.right * input.x;

        moveDirection.x = moveDirection.x * settings.TargetSpeed;
        moveDirection.z = moveDirection.z * settings.TargetSpeed;
        moveDirection.y = moveDirection.y * settings.TargetSpeed;

        if(rb.velocity.sqrMagnitude < (settings.TargetSpeed * settings.TargetSpeed))
            rb.AddForce(moveDirection, ForceMode.Impulse);
    }

    void CheckIfLanded()
    {

    }

    Vector2 GetInput()
    {
        Vector2 value = new Vector2
        {
            x = Input.GetAxis("Horizontal"),
            y = Input.GetAxis("Vertical")
        };
        settings.UpdateTargetSpeed(value);

        return value;
    }
}
