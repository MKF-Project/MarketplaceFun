using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;

public class CartControls : NetworkBehaviour, PlayerControls
{
    private CharacterController _controller;
    private Transform _camera;

    private float _currentSpeed = 0;
    private bool _isWalking = false;
    private Vector2 _currentDirection = Vector2.zero;
    private Vector2 _nextRotation = Vector2.zero;
    private Vector2 _currentLookAngle = Vector2.zero;

    public float MoveSpeed;
    public float WalkSpeed;
    public float MovingTurnSpeed;
    public float InPlaceTurnSpeed;
    public float Sensitivity;
    public float Deadzone;

    public float MaximumViewAngle = 60f;


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
