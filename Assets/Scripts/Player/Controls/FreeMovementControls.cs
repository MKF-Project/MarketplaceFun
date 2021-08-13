using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;

public class FreeMovementControls : NetworkBehaviour, PlayerControls
{
    private CharacterController _controller;
    private Transform _camera;

    private float _currentSpeed = 0;
    private bool _isWalking = false;
    private float _currentRotation = 0;
    private Vector2 _currentDirection = Vector2.zero;
    private Vector2 _nextRotation = Vector2.zero;

    public float MoveSpeed;
    public float WalkSpeed;
    public float Sensitivity = 1;

    public float MaximumViewAngle = 90f;

    private void Awake()
    {

        _controller = gameObject.GetComponent<CharacterController>();

        // Get the GameObject that contains the camera
        _camera = gameObject.GetComponentInChildren<Camera>().transform;

        _currentSpeed = MoveSpeed;

        // Control Events
        InputController.OnLook += Look;
        InputController.OnMove += Move;
        InputController.OnJump += Jump;
        InputController.OnWalk += Walk;
        InputController.OnInteractOrThrow += Interact;
    }

    private void OnDestroy()
    {
        // Control Events
        InputController.OnLook -= Look;
        InputController.OnMove -= Move;
        InputController.OnJump -= Jump;
        InputController.OnWalk -= Walk;
        InputController.OnInteractOrThrow -= Interact;
    }

    private void Update()
    {
        if(IsOwner)
        {
            updateCamera();
            updateMovement();
        }
    }

    public void Move(Vector2 direction)
    {
        if(IsOwner)
        {
            _currentDirection = direction;
        }
    }

    public void Look(Vector2 direction)
    {
        if(IsOwner)
        {
            _nextRotation = direction * Sensitivity;
        }
    }

    public void Jump()
    {
        if(IsOwner)
        {

        }
    }

    public void Walk()
    {
        if(IsOwner)
        {
            _isWalking = !_isWalking;
            _currentSpeed = _isWalking? WalkSpeed : MoveSpeed;
        }
    }

    public void Interact()
    {
        if(IsOwner)
        {

        }
    }

    private void updateMovement()
    {
        var planeMovement = _currentSpeed * _currentDirection;
        var currentVelocity = new Vector3(planeMovement.x, 0, planeMovement.y);

        if(!_controller.isGrounded)
        {
            currentVelocity += Physics.gravity;
        }

        _controller.Move(transform.TransformDirection(currentVelocity) * Time.deltaTime);
    }

    private void updateCamera()
    {
        transform.Rotate(Vector3.up, _nextRotation.x);

        _currentRotation += _nextRotation.y;
        if(_currentRotation <= MaximumViewAngle && _currentRotation >= -MaximumViewAngle)
        {
            _camera.Rotate(Vector3.left, _nextRotation.y);
        }

        _currentRotation = Mathf.Clamp(_currentRotation, -MaximumViewAngle, MaximumViewAngle);
    }
}
