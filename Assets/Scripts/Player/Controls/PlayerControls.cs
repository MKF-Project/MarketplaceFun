using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;

public enum PlayerControlSchemes {
    None,
    FreeMovementControls,
    CartControls
}

public abstract class PlayerControls : NetworkBehaviour
{
    protected const string CAMERA_POSITION_NAME = "CameraPosition";
    public const float MINIMUM_INTERACTION_COOLDOWN = 0.1f;

    // Animator consts
    protected static readonly int ANIM_RUN_STATE          = Animator.StringToHash("Correndo_Sem_Item");
    protected static readonly int ANIM_HOLD_ITEM_STATE    = Animator.StringToHash("Correndo_Com_Item");

    protected static readonly int ANIM_PARAMETER_X        = Animator.StringToHash("Velocidade_X");
    protected static readonly int ANIM_PARAMETER_Z        = Animator.StringToHash("Velocidade_Z");

    protected static readonly int ANIM_JUMP_STATE         = Animator.StringToHash("Pulo");
    protected static readonly int ANIM_ITEM_JUMP_STATE    = Animator.StringToHash("Pulo_Com_Item");
    protected static readonly int ANIM_JUMP               = Animator.StringToHash("P_Pular");
    protected static readonly int ANIM_FALLING_STATE      = Animator.StringToHash("Caindo");
    protected static readonly int ANIM_ITEM_FALLING_STATE = Animator.StringToHash("Caindo_Com_Item");
    protected static readonly int ANIM_FALLING            = Animator.StringToHash("P_Caindo");
    protected static readonly int ANIM_LAND               = Animator.StringToHash("P_Pisa_No_Chao");

    protected static readonly int ANIM_INTERACT           = Animator.StringToHash("P_Interagiu");
    protected static readonly int ANIM_HAS_CART           = Animator.StringToHash("P_Pegou_carrinho");
    protected static readonly int ANIM_ITEM_IN_HAND       = Animator.StringToHash("P_Item_na_mao");
    protected static readonly int ANIM_THROW              = Animator.StringToHash("P_Atirar_Item");

    // This is the maximum speed the player is allowed to turn,
    // regardless of other factors. Keep this at a high value to allow fast mouse movement
    private const float MAX_ANGULAR_VELOCITY = 25;

    private PlayerControlSchemes _currentControlScheme;
    private FreeMovementControls _freeMovementControls = null;
    private CartControls _cartControls = null;

    public PlayerControlSchemes ControlScheme
    {
        get => _currentControlScheme;
        set
        {
            if(_currentControlScheme == value)
            {
                return;
            }

            _currentControlScheme = value;
            switch(value)
            {
                case PlayerControlSchemes.None:
                    _freeMovementControls.enabled = false;
                    _cartControls.enabled = false;
                    break;

                case PlayerControlSchemes.FreeMovementControls:
                    _freeMovementControls.enabled = true;
                    _cartControls.enabled = false;
                    break;

                case PlayerControlSchemes.CartControls:
                    _freeMovementControls.enabled = false;
                    _cartControls.enabled = true;
                    break;
            }
        }
    }

    // Components
    protected Rigidbody _rigidBody;
    protected NetworkAnimator _playerNetAnimator;
    protected PlayerAudio _playerAudioScript;

    protected Collider _currentLookingCollider = null;
    protected Collider _onInteractLookingCollider = null;

    private Interactable _interactableBuffer = null;
    private RaycastHit _raycastHitBuffer;

    protected Player _playerScript = null;

    protected GameObject _cameraPosition;
    protected Quaternion _initialCameraLocalRotation;

    protected Vector2 _currentDirection = Vector2.zero;
    protected Vector2 _nextRotation = Vector2.zero;

    protected Vector3 _wallCollisionNormal = Vector3.zero;

    protected bool _isWalking = false;
    protected float _currentSpeed = 0;

    // Collision related
    public bool isCollidingWithWall { get; protected set; } = false;
    public bool isGrounded { get; protected set; } = false;

    // Gravity related
    private Vector3 _recentGlobalGravity;
    private Vector3 _playerGravityVelocity;

    private float _gravityMagnitude = Physics.gravity.magnitude;
    public float PlayerGravity {
        get => _gravityMagnitude;
        set
        {
            _gravityMagnitude = value;
            _recentGlobalGravity = Physics.gravity;
            _playerGravityVelocity = Physics.gravity.normalized * value;

            #if UNITY_EDITOR
                _gravity = value;
            #endif
        }
    }

    // Interaction related
    public bool HasInteractedThisFrame { get; protected set; } = false;
    private bool _updateLastInteraction = false;
    protected float _lastInteractionTime;

    /** Inspector Variables **/
    [Header("Ground Detection")]
    [Range(0, 90)]
    public float MaximumGroundSlope;
    public LayerMask groundMask;

    [Header("Gravity")]
    [SerializeField]
    private float _gravity = Physics.gravity.magnitude;

    [Header("Movement")]
    public float MoveSpeed;
    public float WalkSpeed;

    [Header("Camera")]
    public float Sensitivity;

    [Header("Interaction")]
    public float InteractionDistance;

    [Min(MINIMUM_INTERACTION_COOLDOWN)]
    public float InteractCooldown = MINIMUM_INTERACTION_COOLDOWN;

    protected virtual void Awake()
    {
        _lastInteractionTime = -InteractCooldown;

        TryGetComponent(out _rigidBody);
        TryGetComponent(out _playerNetAnimator);
        TryGetComponent(out _playerScript);
        TryGetComponent(out _playerAudioScript);

        _cameraPosition = transform.Find(CAMERA_POSITION_NAME).gameObject;
        _initialCameraLocalRotation = _cameraPosition.transform.localRotation;

        #if UNITY_EDITOR
            if(_rigidBody == null)
            {
                Debug.LogError($"[{gameObject.name}::PlayerControls]: Player Rigidbody not Found!");
            }

            if(_playerNetAnimator == null)
            {
                Debug.LogError($"[{gameObject.name}::PlayerControls]: Player NetworkAnimator not Found!");
            }

            if(_cameraPosition == null)
            {
                Debug.LogError($"[{gameObject.name}::PlayerControls]: Player Camera Position not Found!");
            }

            if(_playerScript == null)
            {
                Debug.LogError($"[{gameObject.name}::PlayerControls]: Player Script not Found!");
            }

            if(_playerAudioScript == null)
            {
                Debug.LogError($"[{gameObject.name}::PlayerControls]: PlayerAudio Script not Found!");
            }
        #endif

        initializeControlScheme();

        _rigidBody.maxAngularVelocity = MAX_ANGULAR_VELOCITY;
        _rigidBody.sleepThreshold = 0; // Since this is the player object, we never sleep it's rigidbody
        _rigidBody.useGravity = false;

        // Set the player center of mass manually,
        // so that it doesn't change when adding/removing cart colliders
        var collider = GetComponent<CapsuleCollider>();
        _rigidBody.centerOfMass = collider.center;

        PlayerGravity = _gravity;

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

    public void switchControlScheme()
    {
        // Delay this by one frame to prevent input events from
        // one control scheme to bleed over to the other in the same frame
        StartCoroutine(switchControlSchemeCoroutine());
    }

    private IEnumerator switchControlSchemeCoroutine()
    {
        _interactableBuffer?.TriggerLookExit(_playerScript, _currentLookingCollider);

        yield return Utils.EndOfFrameWait;

        // Store currentDirection from the current active control script
        // so we can keep our momentum when we switch scripts
        var transferDirection = _currentDirection;

        switch(ControlScheme)
        {
            case PlayerControlSchemes.FreeMovementControls:
                ControlScheme = PlayerControlSchemes.CartControls;
                _cartControls.Move(transferDirection);
                break;

            case PlayerControlSchemes.CartControls:
                ControlScheme = PlayerControlSchemes.FreeMovementControls;
                _freeMovementControls.Move(transferDirection);
                break;
        }
    }

    private void initializeControlScheme()
    {
        _freeMovementControls = gameObject.GetComponent<FreeMovementControls>();
        _cartControls = gameObject.GetComponent<CartControls>();

        #if UNITY_EDITOR
            if(_freeMovementControls == null)
            {
                Debug.LogError($"[{gameObject.name}::PlayerControls]: Free Movement Controls not Found!");
            }

            if(_cartControls == null)
            {
                Debug.LogError($"[{gameObject.name}::PlayerControls]: Cart Controls not Found!");
            }
        #endif

        if(_freeMovementControls.enabled && !_cartControls.enabled)
        {
            ControlScheme = PlayerControlSchemes.FreeMovementControls;
        }
        else if(!_freeMovementControls.enabled && _cartControls.enabled)
        {
            ControlScheme = PlayerControlSchemes.CartControls;
        }
        else
        {
            ControlScheme = PlayerControlSchemes.None;
        }
    }

    protected virtual void OnDestroy()
    {
        ControlScheme = PlayerControlSchemes.None;

        // Control Events
        InputController.OnLook     -= Look;
        InputController.OnMove     -= Move;
        InputController.OnJump     -= Jump;
        InputController.OnWalk     -= Walk;
        InputController.OnInteract -= Interact;
        InputController.OnThrow    -= Throw;
        InputController.OnDrop     -= Drop;
    }

    protected virtual void OnDisable()
    {
        // Send final rotation and direction deltas when disabling
        _currentDirection = Vector3.zero;
        _nextRotation = Vector3.zero;
    }

    protected virtual void Update()
    {
        if(!IsOwner)
        {
            return;
        }

        if(!_playerScript.CanInteract)
        {
            if(_currentLookingCollider != null)
            {
                if(_currentLookingCollider.TryGetComponent(out _interactableBuffer))
                {
                    _interactableBuffer.TriggerLookExit(_playerScript, _currentLookingCollider);
                }

                _interactableBuffer = null;
                _currentLookingCollider = null;
            }
            return;
        }


        if(!_playerScript.IsHoldingItem && _playerNetAnimator.GetCurrentAnimatorStateInfo(0).shortNameHash == ANIM_HOLD_ITEM_STATE)
        {
            _playerNetAnimator.SetBool(ANIM_ITEM_IN_HAND, false);
        }

        // Wait before interaction
        if(Time.time - _lastInteractionTime < InteractCooldown)
        {
            if(_updateLastInteraction)
            {
                // Ensure we hide the interaction prompt when first stop interacting
                _updateLastInteraction = false;
                if(_currentLookingCollider != null && _currentLookingCollider.TryGetComponent(out _interactableBuffer))
                {
                    _interactableBuffer.TriggerLookExit(_playerScript, _currentLookingCollider);

                    _interactableBuffer = null;
                    _currentLookingCollider = null;
                }

            }
            return;
        }
        _updateLastInteraction = true;

        #if UNITY_EDITOR
            Debug.DrawRay(_cameraPosition.transform.position, _cameraPosition.transform.forward * InteractionDistance, Color.red);
        #endif

        if(Physics.Raycast(_cameraPosition.transform.position, _cameraPosition.transform.forward, out _raycastHitBuffer, InteractionDistance, Interactable.LAYER_MASK))
        {
            // Was looking at something different than current collider
            if(_currentLookingCollider != _raycastHitBuffer.collider)
            {
                // Was looking at one object, now looking at another
                if(_currentLookingCollider != null && _currentLookingCollider.TryGetComponent(out _interactableBuffer))
                {
                    _interactableBuffer.TriggerLookExit(_playerScript, _currentLookingCollider);
                }

                // Update current looking object
                _currentLookingCollider = _raycastHitBuffer.collider;
                if(_currentLookingCollider.TryGetComponent(out _interactableBuffer))
                {
                    _interactableBuffer.TriggerLookEnter(_playerScript, _currentLookingCollider);
                }

                _interactableBuffer = null;
            }
        }

        // Is no longer looking at a previous object. Call the object's OnLookExit
        else if(_currentLookingCollider != null && _currentLookingCollider.TryGetComponent(out _interactableBuffer))
        {
            _interactableBuffer.TriggerLookExit(_playerScript, _currentLookingCollider);

            _interactableBuffer = null;
            _currentLookingCollider = null;
        }
    }

    protected virtual void FixedUpdate()
    {
        // Update Gravity if it was changed in inspector
        #if UNITY_EDITOR
            if(_gravity != PlayerGravity)
            {
                PlayerGravity = _gravity;
            }
        #endif

        if(Physics.gravity != _recentGlobalGravity)
        {
            _recentGlobalGravity = Physics.gravity;

            // use vector normal from global gravity, but change strength magnitude
            _playerGravityVelocity = Physics.gravity.normalized * PlayerGravity;
        }

        // Apply custom player gravity manually
        _rigidBody.AddForce(_playerGravityVelocity * _rigidBody.mass, ForceMode.Force);
    }

    protected virtual void LateUpdate()
    {
        if(isGrounded)
        {
            // Start the clearGrounded coroutine,
            // which attempts to reset the grounded state for the next frame
            StartCoroutine(nameof(clearGrounded));
        }

        if(isCollidingWithWall)
        {
            StartCoroutine(nameof(clearWallCollision));
        }

        HasInteractedThisFrame = false;
    }

    // Detect ground
    protected virtual void OnCollisionStay(Collision other)
    {
        // Only check ground collisions with things
        // in a layer considered to be ground
        if((groundMask & (1 << other.gameObject.layer)) == 0)
        {
            return;
        }

        isCollidingWithWall = false;
        for(int i = 0; i < other.contactCount; i++)
        {
            var contact = other.GetContact(i);
            var angle = 0f;
            if(isFloorCollision(contact, out angle))
            {
                // Stop coroutine from previous frame from taking effect
                StopCoroutine(nameof(clearGrounded));

                if(isGrounded == false)
                {
                    _playerAudioScript.PlayStep();
                }

                isGrounded = true;
                var currentAnimatorState = _playerNetAnimator.GetCurrentAnimatorStateInfo(0);
                if(currentAnimatorState.shortNameHash == ANIM_FALLING_STATE || currentAnimatorState.shortNameHash == ANIM_ITEM_FALLING_STATE)
                {
                    _playerNetAnimator.SetTrigger(ANIM_LAND);
                }

                // Jump away from current surface (NYI)
                // jumpNormal = contact.normal;
            }
            else if(angle > MaximumGroundSlope && angle <= 90)
            {

                StopCoroutine(nameof(clearWallCollision));

                isCollidingWithWall = true;
                _wallCollisionNormal = contact.normal;
            }
        }
    }

    public bool isFloorCollision(ContactPoint contactPoint, out float angle)
    {
        angle = Vector3.Angle(transform.up, contactPoint.normal);

        return contactPoint.normal != Vector3.zero && angle <= MaximumGroundSlope;

    }

    private IEnumerator clearGrounded()
    {
        yield return Utils.FixedUpdateWait;

        isGrounded = false;
        var nextState = _playerNetAnimator.GetNextAnimatorStateInfo(0);
        if(!(nextState.shortNameHash == ANIM_JUMP_STATE || nextState.shortNameHash == ANIM_ITEM_JUMP_STATE))
        {
            _playerNetAnimator.SetTrigger(ANIM_FALLING);
        }
    }

    private IEnumerator clearWallCollision()
    {
        yield return Utils.FixedUpdateWait;
        isCollidingWithWall = false;
    }

    /** Input Actions **/
    public virtual void Move(Vector2 direction)
    {
        if(!(isActiveAndEnabled && IsOwner))
        {
            return;
        }

        _currentDirection = direction;

        _playerNetAnimator.SetFloat(ANIM_PARAMETER_X, direction.x * (_currentSpeed / MoveSpeed));
        _playerNetAnimator.SetFloat(ANIM_PARAMETER_Z, direction.y * (_currentSpeed / MoveSpeed));
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

        _playerNetAnimator.SetFloat(ANIM_PARAMETER_X, _currentDirection.x * (_currentSpeed / MoveSpeed));
        _playerNetAnimator.SetFloat(ANIM_PARAMETER_Z, _currentDirection.y * (_currentSpeed / MoveSpeed));
    }

    public virtual void Interact()
    {
        // Must wait for cooldown before interacting, and only if
        // not holding anything or driving a shopping cart
        if(HasInteractedThisFrame || Time.time - _lastInteractionTime < InteractCooldown || !(isActiveAndEnabled && IsOwner && _playerScript.CanInteract))
        {
            return;
        }

        _onInteractLookingCollider = _currentLookingCollider;
        if(_onInteractLookingCollider != null)
        {
            _playerNetAnimator.SetTrigger(ANIM_INTERACT);
        }
    }

    private void ExecuteGrab()
    {
        _lastInteractionTime = Time.time;

        if(_onInteractLookingCollider != null && _onInteractLookingCollider.TryGetComponent(out _interactableBuffer))
        {
            _interactableBuffer.TriggerInteract(_playerScript, _onInteractLookingCollider);
        }
        _onInteractLookingCollider = null;

        if(_playerScript.IsHoldingItem)
        {
            _playerNetAnimator.SetBool(ANIM_ITEM_IN_HAND, true);
        }

        HasInteractedThisFrame = true;
    }

    public virtual void Throw()
    {
        // Can only Throw if holding something
        if(HasInteractedThisFrame || !(isActiveAndEnabled && IsOwner && _playerScript.IsHoldingItem))
        {
            return;
        }

        _playerNetAnimator.SetTrigger(ANIM_THROW);
    }

    private void ExecuteThrow()
    {
        _playerNetAnimator.SetBool(ANIM_ITEM_IN_HAND, false);

        // On callback, unset currentLookingObject so that we
        // update it again in the frame after
        _playerScript.ThrowItem((item) => _currentLookingCollider = null);
    }

    public virtual void Drop()
    {
        // Can only Drop if holding something
        if(HasInteractedThisFrame || !(isActiveAndEnabled && IsOwner && _playerScript.IsHoldingItem))
        {
            return;
        }

        _playerNetAnimator.SetBool(ANIM_ITEM_IN_HAND, false);

        // On callback, unset currentLookingObject so that we
        // update it again in the frame after
        _playerScript.DropItem((item) => _currentLookingCollider = null);
    }
}
