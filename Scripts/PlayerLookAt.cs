using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLookAt
{
    public Vector2 mouseSensitivity = new Vector2(2f, 2f);
    public Vector2 range = new Vector2(-90f, 90f);

    Quaternion playerTargetRotation;
    Quaternion cameraTargetRotation;

    bool lockCursor = true;

    public void Initialize(Transform player, Transform camera)
    {
        playerTargetRotation = player.localRotation;
        cameraTargetRotation = camera.localRotation;
    }

    public void LookRotation(Transform player, Transform camera)
    {
        float xRot = Input.GetAxis("Mouse Y") * mouseSensitivity.y;
        float yRot = Input.GetAxis("Mouse X") * mouseSensitivity.x;

        playerTargetRotation *= Quaternion.Euler(0f, yRot, 0f);
        cameraTargetRotation *= Quaternion.Euler(-xRot, 0f, 0f);

        player.localRotation = playerTargetRotation;
        camera.localRotation = cameraTargetRotation;
    }

    public void SetCursorLock(bool value)
    {
        lockCursor = value;

        if(!lockCursor)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void LockUpdate()
    {
        if(Input.GetKeyUp(KeyCode.Escape))
            lockCursor = false;
        else if(Input.GetMouseButtonUp(0))
            lockCursor = true;

        if(lockCursor)
            Cursor.lockState = CursorLockMode.Locked;
        else
            Cursor.lockState = CursorLockMode.None;

        Cursor.visible = !lockCursor;
    }
}
