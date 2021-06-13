using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{

    public Transform Camera;

    public bool MouseLocked;

    public float Sensitivity;

    private float _mouseX;

    private float _mouseY;

    private void Update()
    {
        if (MouseLocked)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            _mouseX = Input.GetAxis("Mouse X") * Sensitivity;
            _mouseY = Input.GetAxis("Mouse Y") * Sensitivity;

            transform.Rotate(Vector3.up, _mouseX);

            Camera.Rotate(Vector3.left, _mouseY);
        }
    }
}
