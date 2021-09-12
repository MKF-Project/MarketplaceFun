using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;

public class CartControls : PlayerControls
{
    private Vector2 _currentLookAngle = Vector2.zero;

    public float MovingTurnSpeed;
    public float InPlaceTurnSpeed;
    public float Deadzone;

    public float MaximumViewAngle = 60f;

    private void Update()
    {
        if(IsOwner)
        {
            updateCamera();
            updateMovement();
        }
    }

    public override void Jump()
    {

    }

    private void updateMovement()
    {
        var currentVelocity = Vector3.zero;
        var rotationAngle = 0f;

        if(Mathf.Abs(_currentDirection.y) > Deadzone)
        {
            currentVelocity = Vector3.forward * (_currentDirection.y > 0? 1 : -1) * _currentDirection.magnitude * _currentSpeed;
            rotationAngle = _currentDirection.x * MovingTurnSpeed;
        }
        else if(_currentDirection.sqrMagnitude > 0)
        {
            rotationAngle = _currentDirection.x * InPlaceTurnSpeed;
        }

        if(!_controller.isGrounded)
        {
            currentVelocity += Physics.gravity;
        }

        _controller.Move(transform.TransformDirection(currentVelocity) * Time.deltaTime);
        transform.Rotate(Vector3.up, rotationAngle * Time.deltaTime);
    }

    private void updateCamera()
    {
        _currentLookAngle += _nextRotation;
        _currentLookAngle.Set(Mathf.Clamp(_currentLookAngle.x, -MaximumViewAngle, MaximumViewAngle), Mathf.Clamp(_currentLookAngle.y, -MaximumViewAngle, MaximumViewAngle));

        _camera.transform.localEulerAngles = new Vector3(-_currentLookAngle.y, _currentLookAngle.x, 0);
    }
}
