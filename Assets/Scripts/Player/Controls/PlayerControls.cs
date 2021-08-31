using UnityEngine;
using MLAPI;

public enum PlayerControlSchemes {
    None,
    FreeMovementControls,
    CartControls
}

public abstract class PlayerControls : NetworkBehaviour
{
    protected CharacterController _controller;
    protected Transform _camera;
    protected GameObject _currentLookingObject = null;

    protected Vector2 _currentDirection = Vector2.zero;
    protected Vector2 _nextRotation = Vector2.zero;

    protected bool _isWalking = false;
    protected float _currentSpeed = 0;

    public float MoveSpeed;
    public float WalkSpeed;
    public float Sensitivity;
    public float InteractionDistance;

    protected virtual void Awake()
    {
        _controller = gameObject.GetComponent<CharacterController>();

        // Get the GameObject that contains the camera
        _camera = gameObject.GetComponentInChildren<Camera>()?.transform;
        #if UNITY_EDITOR
            if(_camera == null)
            {
                Debug.LogError($"[{gameObject}::PlayerControls]: Player Camera not Found!");
            }
        #endif

        _currentSpeed = MoveSpeed;

        // Control Events
        InputController.OnLook += Look;
        InputController.OnMove += Move;
        InputController.OnJump += Jump;
        InputController.OnWalk += Walk;
        InputController.OnInteractOrThrow += Interact;
    }

    protected virtual void OnDestroy()
    {
        // Control Events
        InputController.OnLook -= Look;
        InputController.OnMove -= Move;
        InputController.OnJump -= Jump;
        InputController.OnWalk -= Walk;
        InputController.OnInteractOrThrow -= Interact;
    }

    protected virtual void FixedUpdate()
    {

    }

    public virtual void Move(Vector2 direction)
    {
        if(IsOwner)
        {
            _currentDirection = direction;
        }
    }

    public virtual void Look(Vector2 direction)
    {
        if(IsOwner)
        {
            _nextRotation = direction * Sensitivity;
        }
    }

    public virtual void Jump()
    {

    }

    public virtual void Walk()
    {
        if(IsOwner)
        {
            _isWalking = !_isWalking;
            _currentSpeed = _isWalking? WalkSpeed : MoveSpeed;
        }
    }

    public virtual void Interact()
    {

    }

}
