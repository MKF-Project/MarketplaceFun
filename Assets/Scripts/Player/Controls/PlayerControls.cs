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
        if(IsOwner)
        {
            #if UNITY_EDITOR
                Debug.DrawRay(_camera.transform.position, _camera.transform.forward * InteractionDistance, Color.red);
            #endif

            if(Physics.Raycast(_camera.transform.position, _camera.transform.forward, out var hitInfo, InteractionDistance, Interactable.LAYER_MASK))
            {
                // Was looking at something different than current object
                if(_currentLookingObject != hitInfo.transform.gameObject)
                {
                    // Was looking at one object, now looking at another
                    if(_currentLookingObject != null)
                    {
                        _currentLookingObject.GetComponent<Interactable>()?.TriggerLookExit(gameObject);
                    }

                    // Update current looking object
                    _currentLookingObject = hitInfo.transform.gameObject;
                    _currentLookingObject.GetComponent<Interactable>()?.TriggerLookEnter(gameObject);
                }
            }
            // Is no longer looking at a previous object. Call the object's OnLookExit
            else if(_currentLookingObject != null)
            {
                _currentLookingObject.GetComponent<Interactable>()?.TriggerLookExit(gameObject);
                _currentLookingObject = null;
            }
        }
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
        _currentLookingObject.GetComponent<Interactable>()?.TriggerInteract(gameObject);
    }

}
