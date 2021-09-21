using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;

public class FreeMovementControls : PlayerControls
{
    private Vector3 _targetVelocity = Vector3.zero;
    private Vector3 _targetHorizontalRotation = Vector3.zero;
    private float _targetVerticalRotation = 0;
    private bool _shouldJump = false;
    private bool _isJumping = false;

    public float FallSpeed;
    public float JumpHeight;

    public float MaximumViewAngle = 90f;

    protected void Update()
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

    protected override void OnDisable()
    {
        // Stop propagation of previous forces when disabling the script
        base.OnDisable();

        updateMovement();

        updatePlayerRotation();
        updateCameraRotation();

        _shouldJump = false;
    }

    public override void Jump()
    {
        if(IsOwner)
        {
            _shouldJump = true;
        }
    }

    private void updateMovement()
    {
        if(!isGrounded)
        {
            _isJumping = false;
        }
        else if(_shouldJump)
        {
            _isJumping = true;
        }

        // Calculate desired velocity
        _targetVelocity.Set(_currentDirection.x, 0, _currentDirection.y);
        _targetVelocity = transform.TransformDirection(_targetVelocity);
        _targetVelocity *= _currentSpeed;

        // Apply force
        _targetVelocity = (_targetVelocity - _rigidBody.velocity);
        if(!isGrounded || _isJumping)
        {
            _targetVelocity.y = 0;
        }

        // Detect if we're colliding against a steep slope
        if(isCollidingWithWall && _wallCollisionNormal.y > 0)
        {
            var angle = Vector3.Angle(_targetVelocity, _wallCollisionNormal);
            if(angle > 90 && angle < 180)
            {
                _targetVelocity = Vector3.zero;
            }
        }

        _rigidBody.AddForce(_targetVelocity, ForceMode.VelocityChange);

        if(_shouldJump && isGrounded)
        {
            _rigidBody.AddForce(transform.up * Mathf.Sqrt(2 * Physics.gravity.magnitude * JumpHeight), ForceMode.VelocityChange);
        }
        _shouldJump = false;
    }

    private void updateCameraRotation()
    {
        // Since rotating the view sideways requires moving the player body,
        // this method ONLY affects vertical camera rotation.
        // This should be run on Update instead of FixedUpdate,
        // since it only affects the camera, not the player Physics.
        _targetVerticalRotation += _nextRotation.y;
        if(_targetVerticalRotation <= MaximumViewAngle && _targetVerticalRotation >= -MaximumViewAngle)
        {
            _cameraPosition.transform.Rotate(Vector3.left, _nextRotation.y);
        }

        _targetVerticalRotation = Mathf.Clamp(_targetVerticalRotation, -MaximumViewAngle, MaximumViewAngle);
    }

    private void updatePlayerRotation()
    {
        // This method handles the player's sideways rotation,
        // and since rotation to the sides requires Physics movement,
        // this method MUST run on FixedUpdate.
        // The rotation for the in-between frames is interpolated.
        _targetHorizontalRotation = Vector3.up * _nextRotation.x * Sensitivity;
        _targetHorizontalRotation = (_targetHorizontalRotation - _rigidBody.angularVelocity);
        _rigidBody.AddTorque(_targetHorizontalRotation, ForceMode.VelocityChange);
    }
}
