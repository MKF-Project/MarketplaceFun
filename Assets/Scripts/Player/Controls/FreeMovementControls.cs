using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;

public class FreeMovementControls : PlayerControls
{
    private Vector3 _currentVelocity = Vector3.zero;
    private float _currentRotation = 0;
    private bool _isJumping = false;

    public float FallSpeed;
    public float JumpHeight;

    public float MaximumViewAngle = 90f;

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
        if(IsOwner)
        {
            _isJumping = true;
        }
    }

    private void updateMovement()
    {
        var planeMovement = (_controller.isGrounded? _currentSpeed : FallSpeed) * _currentDirection;
        _currentVelocity.Set(planeMovement.x, _currentVelocity.y, planeMovement.y);

        // Reset vertical acceleration if some external force causes it to be affected
        // Like hitting the floor or ceiling
        if(_controller.velocity.y == 0)
        {
            _currentVelocity.y = 0;
        }

        // Always apply gravity, even when the player is possibly already Grounded
        _currentVelocity += Physics.gravity * Time.deltaTime;

        if(_controller.isGrounded && _isJumping)
        {
            // if Jumping Apply instant vertical velocity change, overriding gravity
            _currentVelocity.y = Mathf.Sqrt(2 * Physics.gravity.magnitude * JumpHeight);
        }

        _controller.Move(transform.TransformDirection(_currentVelocity) * Time.deltaTime);

        // Reset jumpstate after every frame
        _isJumping = false;
    }

    private void updateCamera()
    {
        transform.Rotate(Vector3.up, _nextRotation.x);

        _currentRotation += _nextRotation.y;
        if(_currentRotation <= MaximumViewAngle && _currentRotation >= -MaximumViewAngle)
        {
            _cameraPosition.transform.Rotate(Vector3.left, _nextRotation.y);
        }

        _currentRotation = Mathf.Clamp(_currentRotation, -MaximumViewAngle, MaximumViewAngle);
    }
}
