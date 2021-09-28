﻿using System.Collections;
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

    protected override void Update()
    {
        base.Update();
        if(IsOwner)
        {
            updateCamera();
        }
    }

    private void FixedUpdate()
    {
        if(IsOwner)
        {
            updateMovement();
        }
    }

    private void OnEnable()
    {
        ControlScheme = PlayerControlSchemes.CartControls;
    }

    public override void Jump()
    {

    }

    private void updateMovement()
    {
        // Forward/Backward Movement
        var targetMove = _currentDirection.y;
        var isMoving = Mathf.Abs(targetMove) > Deadzone;

        var targetVelocity = Vector3.zero;
        if(isMoving)
        {
            targetVelocity.Set(0, 0, targetMove);
            targetVelocity = transform.TransformDirection(targetVelocity);
            targetVelocity *= _currentSpeed;
        }

        targetVelocity = (targetVelocity - _rigidBody.velocity);

        // Dampen movement
        // ...

        _rigidBody.AddForce(targetVelocity, ForceMode.VelocityChange);

        // Rotation
        var targetRotation = _currentDirection.x * Time.fixedDeltaTime;
        var targetHorizontalRotation = Vector3.up * targetRotation * Sensitivity;

        if(_currentDirection.sqrMagnitude > 0)
        {
            targetHorizontalRotation *= isMoving? MovingTurnSpeed : InPlaceTurnSpeed;
        }

        targetHorizontalRotation = (targetHorizontalRotation - _rigidBody.angularVelocity);

        // Dampen rotation
        // ...

        _rigidBody.AddTorque(targetHorizontalRotation, ForceMode.VelocityChange);
    }

    private void updateCamera()
    {
        // Unlike on freeMovementControls, in this script the camera can rotate
        // side to side without applying Physics to the Player
        // so this method can be run on Update.
        _currentLookAngle += _nextRotation;
        _currentLookAngle.Set(Mathf.Clamp(_currentLookAngle.x, -MaximumViewAngle, MaximumViewAngle), Mathf.Clamp(_currentLookAngle.y, -MaximumViewAngle, MaximumViewAngle));

        _cameraPosition.transform.localEulerAngles = new Vector3(-_currentLookAngle.y, _currentLookAngle.x, 0);
    }
}
