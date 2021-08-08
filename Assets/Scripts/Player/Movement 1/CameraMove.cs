using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;

public class CameraMove : NetworkBehaviour
{
    public Transform Camera;

    public float sensitivity;

    private Vector2 _nextRotation = Vector2.zero;

    private void Awake()
    {
        InputController.OnLook += onLook;
    }

    private void OnDestroy()
    {
        InputController.OnLook -= onLook;
    }

    private void Update()
    {
        transform.Rotate(Vector3.up, _nextRotation.x);
        Camera.Rotate(Vector3.left, _nextRotation.y);
    }

    private void onLook(Vector2 lookDelta)
    {
        if(InputController.playerInputEnabled && IsOwner)
        {
            _nextRotation = lookDelta * sensitivity;
        }

    }
}
