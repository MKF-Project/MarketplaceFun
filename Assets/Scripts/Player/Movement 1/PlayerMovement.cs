using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController _controller;
    

    public float MoveSpeed;

    public float WalkSpeed;

    private float _realSpeed;
    
    private float _verticalSpeed;


    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateSpeed();
        Move();
    }

    private void UpdateSpeed()
    {
        _realSpeed = MoveSpeed;
        if (InputManager.Instance.WalkButton)
        {
            _realSpeed = WalkSpeed;
        }
    }

    private void Move()
    {
        float _forwardInput = InputManager.Instance.Frontal;
        float _lateralInput = InputManager.Instance.Lateral;
        Vector3 moveDirection = _forwardInput * transform.forward + _lateralInput * transform.right;

        if (moveDirection.sqrMagnitude > 1)
        {
            moveDirection.Normalize();
        }

        Vector3 frameMovement = _realSpeed * Time.deltaTime * moveDirection;

        if (_controller.isGrounded)
        {
            _verticalSpeed = 0;
        }
        else
        {
            float gravity = Physics.gravity.y;
            _verticalSpeed += gravity * Time.deltaTime;
            frameMovement += _verticalSpeed * Time.deltaTime * Vector3.up;
        }

        _controller.Move(frameMovement);
    }
}
