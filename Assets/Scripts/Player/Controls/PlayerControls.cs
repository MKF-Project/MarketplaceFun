using UnityEngine;
using MLAPI;

public enum PlayerControlSchemes {
    None,
    FreeMovementControls,
    CartControls
}

public abstract class PlayerControls : NetworkBehaviour
{
    protected const string CAMERA_TAG = "MainCamera";

    protected CharacterController _controller;
    protected GameObject _cameraPosition;
    protected GameObject _currentLookingObject = null;
    protected Player _playerScript = null;

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

        _cameraPosition = gameObject.FindChildWithTag(CAMERA_TAG);
        _playerScript = gameObject.GetComponent<Player>();
        #if UNITY_EDITOR
            if(_cameraPosition == null)
            {
                Debug.LogError($"[{gameObject.name}::PlayerControls]: Player Camera Position not Found!");
            }
            if(_playerScript == null)
            {
                Debug.LogError($"[{gameObject.name}::PlayerControls]: Player Script not Found!");
            }
        #endif

        _currentSpeed = MoveSpeed;

        // Control Events
        InputController.OnLook     += Look;
        InputController.OnMove     += Move;
        InputController.OnJump     += Jump;
        InputController.OnWalk     += Walk;
        InputController.OnInteract += Interact;
        InputController.OnThrow    += Throw;
        InputController.OnDrop     += Drop;
    }

    protected virtual void OnDestroy()
    {
        // Control Events
        InputController.OnLook     -= Look;
        InputController.OnMove     -= Move;
        InputController.OnJump     -= Jump;
        InputController.OnWalk     -= Walk;
        InputController.OnInteract -= Interact;
        InputController.OnThrow    -= Throw;
        InputController.OnDrop     -= Drop;
    }

    protected virtual void FixedUpdate()
    {
        if(!IsOwner)
        {
            return;
        }

        #if UNITY_EDITOR
            Debug.DrawRay(_cameraPosition.transform.position, _cameraPosition.transform.forward * InteractionDistance, Color.red);
        #endif

        if(Physics.Raycast(_cameraPosition.transform.position, _cameraPosition.transform.forward, out var hitInfo, InteractionDistance, Interactable.LAYER_MASK))
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

    public virtual void Move(Vector2 direction)
    {
        if(!(isActiveAndEnabled && IsOwner))
        {
            return;
        }

        _currentDirection = direction;
    }

    public virtual void Look(Vector2 direction)
    {
        if(!(isActiveAndEnabled && IsOwner))
        {
            return;
        }

        _nextRotation = direction * Sensitivity;
    }

    public virtual void Jump()
    {
        if(!(isActiveAndEnabled && IsOwner))
        {
            return;
        }

    }

    public virtual void Walk()
    {
        if(!(isActiveAndEnabled && IsOwner))
        {
            return;
        }


        _isWalking = !_isWalking;
        _currentSpeed = _isWalking? WalkSpeed : MoveSpeed;
    }

    public virtual void Interact()
    {
        // Can only Interact if not holding anything
        if(!(isActiveAndEnabled && IsOwner) || _playerScript.IsHoldingItem)
        {
            return;
        }

        _currentLookingObject?.GetComponent<Interactable>()?.TriggerInteract(gameObject);

    }

    public virtual void Throw()
    {
        // Can only Throw if holding something
        if(!(isActiveAndEnabled && IsOwner) || !_playerScript.IsHoldingItem)
        {
            return;
        }

        _playerScript.ThrowItem();
    }

    public virtual void Drop()
    {
        // Can only Drop if holding something
        if(!(isActiveAndEnabled && IsOwner) || !_playerScript.IsHoldingItem)
        {
            return;
        }

        _playerScript.DropItem();
    }

}
