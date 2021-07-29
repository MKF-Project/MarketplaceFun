using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;

public class CameraMove : NetworkBehaviour
{
    public Transform Camera;

    public float sensitivity;

    private PlayerInput _playerInputScript;

    private Vector2 _nextRotation;

    private void Awake()
    {
        _playerInputScript = GetComponent<PlayerInput>();

        _playerInputScript.OnLook += onLook;
    }

    private void OnDestroy()
    {
        _playerInputScript.OnLook  -= onLook;
    }

    private void Update()
    {
        transform.Rotate(Vector3.up, _nextRotation.x);
        Camera.Rotate(Vector3.left, _nextRotation.y);
    }

    private void onLook(Vector2 lookDelta)
    {
        if(!_playerInputScript.playerInputEnabled)
        {
            return;
        }

        _nextRotation = lookDelta * sensitivity;
    }
}
