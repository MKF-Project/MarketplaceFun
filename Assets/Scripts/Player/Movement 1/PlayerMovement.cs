using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;

public class PlayerMovement : NetworkBehaviour
{
    private CharacterController _controller;

    public float MoveSpeed;
    public float WalkSpeed;

    private float _currentSpeed = 0;
    private Vector2 _currentDirection = Vector2.zero;

    private float _verticalSpeed;
    private bool _isWalking = false;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();

        if(IsOwner)
        {
            InputController.OnMove += updateDirection;
            InputController.OnWalk += updateSpeed;
        }
    }

    private void OnDestroy()
    {
        if(IsOwner)
        {
            InputController.OnMove -= updateDirection;
            InputController.OnWalk -= updateSpeed;
        }
    }

    private void Start()
    {
        _currentSpeed = MoveSpeed;
    }

    private void Update()
    {
        if(IsOwner)
        {
            Move();
        }

    }

    private void updateSpeed()
    {
        _isWalking = !_isWalking;
        _currentSpeed = _isWalking? WalkSpeed : MoveSpeed;
    }

    private void updateDirection(Vector2 direction)
    {
        _currentDirection = direction;
    }

    private void Move()
    {
        var planeMovement = _currentSpeed * _currentDirection;
        var currentVelocity = new Vector3(planeMovement.x, 0, planeMovement.y);

        if(!_controller.isGrounded)
        {
            currentVelocity += Physics.gravity;
        }

        _controller.Move(transform.TransformDirection(currentVelocity) * Time.deltaTime);
    }
}
