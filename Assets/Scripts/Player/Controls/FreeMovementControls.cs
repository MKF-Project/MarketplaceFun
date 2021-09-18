using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;

public class FreeMovementControls : PlayerControls
{
    private Vector3 _targetVelocity = Vector3.zero;
    private Vector3 _targetHorizontalRotation = Vector3.zero;
    private float _targetVerticalRotation = 0;
    private bool _isJumping = false;

    public float FallSpeed;
    public float JumpHeight;

    public float MaximumViewAngle = 90f;

    private void Update()
    {
        if(IsOwner)
        {
            updateCameraRotation();
        }
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if(IsOwner)
        {
            updatePlayerRotation();
            updateMovement();
        }
    }

    private void OnEnable()
    {
        ControlScheme = PlayerControlSchemes.FreeMovementControls;
    }

    public override void Jump()
    {
        if(IsOwner)
        {
            _isJumping = true;
        }
    }

    private void updateMovement()
    {
        // Calculate desired velocity
        _targetVelocity.Set(_currentDirection.x, 0, _currentDirection.y);
        _targetVelocity = transform.TransformDirection(_targetVelocity);
        _targetVelocity *= _currentSpeed;


        // Apply force
        _targetVelocity = (_targetVelocity - _rigidBody.velocity);
        _targetVelocity.y = 0;
        _rigidBody.AddForce(_targetVelocity, ForceMode.VelocityChange);

        // if(_isJumping)
        // {
        //     print("Jump!");
        //     _rigidBody.AddForce(transform.up * JumpHeight, ForceMode.VelocityChange);
        // }
        // _isJumping = false;
    }

    private void updateCameraRotation()
    {
        _targetVerticalRotation += _nextRotation.y;
        if(_targetVerticalRotation <= MaximumViewAngle && _targetVerticalRotation >= -MaximumViewAngle)
        {
            _cameraPosition.transform.Rotate(Vector3.left, _nextRotation.y);
        }

        _targetVerticalRotation = Mathf.Clamp(_targetVerticalRotation, -MaximumViewAngle, MaximumViewAngle);
    }

    private void updatePlayerRotation()
    {
        _targetHorizontalRotation = Vector3.up * _nextRotation.x * Sensitivity;
        _targetHorizontalRotation = (_targetHorizontalRotation - _rigidBody.angularVelocity);
        _rigidBody.AddTorque(_targetHorizontalRotation, ForceMode.VelocityChange);
    }
}
