using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;

public class FreeMovementControls : NetworkBehaviour, PlayerControls
{
    private CharacterController _controller;
    private Transform _camera;

    private Vector3 _currentVelocity = Vector3.zero;
    private Vector2 _currentDirection = Vector2.zero;
    private Vector2 _nextRotation = Vector2.zero;
    private float _currentSpeed = 0;
    private float _currentRotation = 0;
    private bool _isWalking = false;
    private bool _isJumping = false;

    public float MoveSpeed;
    public float WalkSpeed;
    public float JumpSpeed;
    public float Sensitivity = 1;
    public float JumpHeight;
    public float JumpDampening;

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
            _isJumping = true;
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
        var planeMovement = (_controller.isGrounded? _currentSpeed : JumpSpeed) * _currentDirection;
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
            _camera.Rotate(Vector3.left, _nextRotation.y);
        }

        _currentRotation = Mathf.Clamp(_currentRotation, -MaximumViewAngle, MaximumViewAngle);
    }
}
