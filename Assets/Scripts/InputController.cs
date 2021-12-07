using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using MLAPI;

public class InputController : MonoBehaviour
{
    /**
     * Aggregates events and their return values from multiple sources
     *
     * Behaviour is such that a function that returns false should block the associated event from being fired.
     */
    public class EventBlocker
    {
        private List<Func<bool>> _eventListeners;
        private Action _resultingEvent;

        public EventBlocker(Action associatedEvent)
        {
            _resultingEvent = associatedEvent;
            _eventListeners = new List<Func<bool>>();
        }

        public bool CanDispatchEvent()
        {
            if(_eventListeners.Count == 0)
            {
                return true;
            }

            return _eventListeners.TrueForAll(fn => fn.Invoke());
        }

        public bool DispatchEvent()
        {
            if(CanDispatchEvent())
            {
                _resultingEvent?.Invoke();
                return true;
            }
            else
            {
                return false;
            }
        }

        public void ClearDispatcher()
        {
            _eventListeners.Clear();
        }

        public static EventBlocker operator +(EventBlocker dispatcher, Func<bool> listener)
        {
            if(!dispatcher._eventListeners.Contains(listener))
            {
                dispatcher._eventListeners.Add(listener);
            }

            return dispatcher;
        }

        public static EventBlocker operator -(EventBlocker dispatcher, Func<bool> listener)
        {
            dispatcher._eventListeners.Remove(listener);
            return dispatcher;
        }
    }

    private static InputController _instance = null;

    private PlayerInput _playerInput;
    private InputActionMap _playerControls;
    private InputActionMap _menuControls;

    private delegate void OnDestroyControllerDelegate();
    private event OnDestroyControllerDelegate OnDestroyController;

    private bool _playerInputEnabled;
    public static bool playerInputEnabled
    {
        get => (bool) _instance?._playerInputEnabled;
        set
        {
            _instance._playerInputEnabled = value;

            Cursor.visible = !value;
            Cursor.lockState = value? CursorLockMode.Locked : CursorLockMode.None;
        }
    }

    private void Awake()
    {
        if(_instance == null)
        {
            _instance = this;
            GameObject.DontDestroyOnLoad(gameObject);
        }
        else
        {
            GameObject.DestroyImmediate(gameObject);
            return;
        }

        _playerInput = GetComponent<PlayerInput>();

        // In-Game Actions
        _playerControls = _playerInput.actions.FindActionMap("PlayerControls");

        watchInputAction(_playerControls, "Move",     OnMoveAction);
        watchInputAction(_playerControls, "Look",     OnLookAction);
        watchInputAction(_playerControls, "Jump",     OnJumpAction);
        watchInputAction(_playerControls, "Interact", OnInteractAction);
        watchInputAction(_playerControls, "Throw",    OnThrowAction);
        watchInputAction(_playerControls, "Drop",     OnDropAction);
        watchInputAction(_playerControls, "Walk",     OnWalkAction);
        watchInputAction(_playerControls, "Pause",    OnPauseAction);

        // Menu Actions
        // Menu navigation is mostly handled by the default Unity AcionMap
        // we only need to specify Custom behaviout here, such as Unpausing the game
        _menuControls = _playerInput.actions.FindActionMap("MenuControls");
        watchInputAction(_menuControls, "Unpause", OnUnpauseAction);

        if(_playerInput.defaultActionMap == _menuControls.name)
        {
            SwitchToMenuControls();
        }
        else
        {
            SwitchToPlayerControls();
        }

    }

    private void OnDestroy()
    {
        OnDestroyController?.Invoke();

        if(_instance == this)
        {
            _instance = null;
        }
    }

    // Subscribe and create unsubscribe callbacks for an InputAction
    private void watchInputAction(InputActionMap actionMap, string actionName, Action<InputAction.CallbackContext> actionCallback)
    {
        var action = actionMap.FindAction(actionName);

        if(action == null)
        {
            #if UNITY_EDITOR
                Debug.LogError($"[InputController]: Action \"{actionMap.name}/{actionName}\" not found!");
            #endif

            return;
        }

        action.started   += actionCallback;
        action.performed += actionCallback;
        action.canceled  += actionCallback;

        void autoUnsubscribeOnDestroy()
        {
            OnDestroyController -= autoUnsubscribeOnDestroy;

            action.started   -= actionCallback;
            action.performed -= actionCallback;
            action.canceled  -= actionCallback;
        }

        OnDestroyController += autoUnsubscribeOnDestroy;
    }

    // Allow/Deny input propagation to event subscribers
    public static void FreezePlayerControls() => PlayerControlsFrozen = true;
    public static void UnfreezePlayerControls() => PlayerControlsFrozen = false;
    private static bool _playerControlsFrozen = false;
    public static bool PlayerControlsFrozen
    {
        get => _playerControlsFrozen;
        private set
        {
            if(value)
            {
                // Stop Movement prior to freezing
                OnMove?.Invoke(Vector2.zero);
                OnMoveReleased?.Invoke();

                // Stop Look prior to freezing
                OnLook?.Invoke(Vector2.zero);
                OnLookStop?.Invoke();

                // Stop walk action
                OnWalkReleased?.Invoke();
            }
            _playerControlsFrozen = value;
        }
    }

    // Mode Switch Events
    public delegate void OnMenuControlsDelegate();
    public static event OnMenuControlsDelegate OnMenuControls;

    public static EventBlocker OnAllowMenuControlsSwitch = new EventBlocker(SwitchToMenuControls);

    // Can only switch to menu controls if MenuControls aren't frozen
    public static bool RequestMenuControlsSwitch() => OnAllowMenuControlsSwitch.DispatchEvent();
    private static void SwitchToMenuControls() => _instance?.instanceSwitchToMenuControls();
    private void instanceSwitchToMenuControls()
    {
        _playerInput.SwitchCurrentActionMap(_menuControls.name);
        playerInputEnabled = false;
        OnMenuControls?.Invoke();
    }


    public delegate void OnPlayerControlsDelegate();
    public static event OnPlayerControlsDelegate OnPlayerControls;

    public static EventBlocker OnAllowPlayerControlsSwitch = new EventBlocker(SwitchToPlayerControls);

    // Can only switch to player controls if PlayerControls aren't frozen
    public static bool RequestPlayerControlsSwitch() => OnAllowPlayerControlsSwitch.DispatchEvent();
    private static void SwitchToPlayerControls() => _instance?.instanceSwitchToPlayerControls();
    private void instanceSwitchToPlayerControls()
    {
        _playerInput.SwitchCurrentActionMap(_playerControls.name);
        playerInputEnabled = true;
        OnPlayerControls?.Invoke();
    }

    /** A quick overview of InputAction callback states:
     *
     * Disabled:  The action is disabled and will not detect any input.
     * Waiting:   The action is enabled and waiting for input. This is the sate the button is in when not pressed.
     * Started:   The action was started. This is the state the button immediately goes, and stays on, while it's being pressed.
     * Performed: The action was performed. This triggers exactly once, right after Started callback when the button is pressed. The action does not stay in this state.
     * Canceled:  The action was canceled or stopped. This triggers exactly once when the button is released, then the action immediately goes to the Waiting state.
     */

    // In Game Controls
    public delegate void OnMoveDelegate(Vector2 moveDirection);
    public delegate void OnMoveReleasedDelegate();

    public static event OnMoveDelegate OnMove;
    public static event OnMoveReleasedDelegate OnMoveReleased;

    private void OnMoveAction(InputAction.CallbackContext context)
    {
        if(PlayerControlsFrozen)
        {
            return;
        }

        switch(context.phase)
        {
            case InputActionPhase.Performed:
                OnMove?.Invoke(context.ReadValue<Vector2>());
                break;

            case InputActionPhase.Canceled:
                OnMove?.Invoke(Vector2.zero);
                OnMoveReleased?.Invoke();
                break;
        }
    }


    public delegate void OnLookDelegate(Vector2 lookDirection);
    public delegate void OnLookStopDelegate();

    public static event OnLookDelegate OnLook;
    public static event OnLookStopDelegate OnLookStop;

    private void OnLookAction(InputAction.CallbackContext context)
    {
        if(PlayerControlsFrozen)
        {
            return;
        }

        switch(context.phase)
        {
            case InputActionPhase.Performed:
                OnLook?.Invoke(context.ReadValue<Vector2>());
                break;

            case InputActionPhase.Canceled:
                OnLook?.Invoke(Vector2.zero);
                OnLookStop?.Invoke();
                break;
        }
    }


    public delegate void OnJumpDelegate();
    public static event OnJumpDelegate OnJump;

    private void OnJumpAction(InputAction.CallbackContext context)
    {
        if(PlayerControlsFrozen)
        {
            return;
        }

        if(context.performed) {
            OnJump?.Invoke();
        }
    }


    public delegate void OnInteractDelegate();
    public static event OnInteractDelegate OnInteract;

    private void OnInteractAction(InputAction.CallbackContext context)
    {
        if(PlayerControlsFrozen)
        {
            return;
        }

        if(context.performed) {
            OnInteract?.Invoke();
        }
    }


    public delegate void OnThrowDelegate();
    public static event OnThrowDelegate OnThrow;

    private void OnThrowAction(InputAction.CallbackContext context)
    {
        if(PlayerControlsFrozen)
        {
            return;
        }

        if(context.performed) {
            OnThrow?.Invoke();
        }
    }


    public delegate void OnDropDelegate();
    public static event OnDropDelegate OnDrop;

    private void OnDropAction(InputAction.CallbackContext context)
    {
        if(PlayerControlsFrozen)
        {
            return;
        }

        if(context.performed) {
            OnDrop?.Invoke();
        }
    }


    public delegate void OnWalkDelegate();
    public delegate void OnWalkPressedDelegate();
    public delegate void OnWalkReleasedDelegate();

    public static event OnWalkDelegate OnWalk;
    public static event OnWalkPressedDelegate OnWalkPressed;
    public static event OnWalkReleasedDelegate OnWalkReleased;

    private void OnWalkAction(InputAction.CallbackContext context)
    {
        if(PlayerControlsFrozen)
        {
            return;
        }

        switch(context.phase)
        {
            case InputActionPhase.Performed:
                OnWalk?.Invoke();
                break;

            case InputActionPhase.Started:
                OnWalkPressed?.Invoke();
                break;

            case InputActionPhase.Canceled:
                OnWalkReleased?.Invoke();
                break;
        }
    }


    public delegate void OnPauseDelegate();
    public static event OnPauseDelegate OnPause;

    private void OnPauseAction(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            OnPause?.Invoke();
            OnAllowMenuControlsSwitch.DispatchEvent();
        }
    }


    // Menu Controls
    public delegate void OnUnpauseDelegate();
    public static event OnUnpauseDelegate OnUnpause;

    private void OnUnpauseAction(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            OnUnpause?.Invoke();
            OnAllowPlayerControlsSwitch.DispatchEvent();
        }
    }
}
