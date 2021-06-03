using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{

    //public Transform Player;
    
    public bool MouseLocked;

    public float Sensitivity;

    private float _mouseX;
    
    private float _mouseY;

    void Update()
    {
        if (MouseLocked)
        {
            Cursor.visible = false; 
            Cursor.lockState = CursorLockMode.Locked;

            _mouseX += Input.GetAxis("Mouse X") * Sensitivity;
            _mouseY -= Input.GetAxis("Mouse Y") * Sensitivity;

            transform.eulerAngles = new Vector3(_mouseY, _mouseX, 0); 
        }
    }
}
