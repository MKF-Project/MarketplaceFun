using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;

public class CameraMove : NetworkBehaviour
{
    public Transform Camera;

    public bool MouseLocked = true;

    public float Sensitivity;

    private float _mouseX;

    private float _mouseY;

    private void Update()
    {
        updateMouseLock(MouseLocked);

        if (MouseLocked)
        {
            _mouseX = Input.GetAxis("Mouse X") * Sensitivity;
            _mouseY = Input.GetAxis("Mouse Y") * Sensitivity;

            transform.Rotate(Vector3.up, _mouseX);

            Camera.Rotate(Vector3.left, _mouseY);
        }
    }

    private void updateMouseLock(bool shouldLock)
    {
        MouseLocked = shouldLock;

        // Update Mouse lock State
        Cursor.visible = !MouseLocked;
        Cursor.lockState = MouseLocked? CursorLockMode.Locked : CursorLockMode.None;
    }
}
