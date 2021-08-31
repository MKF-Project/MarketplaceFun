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

        watchInputAction(_playerControls, "Move",           OnMoveAction);
        watchInputAction(_playerControls, "Look",           OnLookAction);
        watchInputAction(_playerControls, "Jump",           OnJumpAction);
        watchInputAction(_playerControls, "Interact/Throw", OnInteractOrThrowAction);
        watchInputAction(_playerControls, "Walk",           OnWalkAction);
        watchInputAction(_playerControls, "Pause",          OnPauseAction);
        watchInputAction(_playerControls, "Put",            OnPutAction);

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

    // Mode Switch Events
    public delegate void OnMenuControlsDelegate();
    public static event OnMenuControlsDelegate OnMenuControls;

    public static EventBlocker OnAllowMenuControlsSwitch = new EventBlocker(SwitchToMenuControls);

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
     * Disabled:  The action is disabled and willnot detect any input.
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
        if(context.performed) {
            OnJump?.Invoke();
        }
    }


    public delegate void OnInteractOrThrowDelegate();
    public static event OnInteractOrThrowDelegate OnInteractOrThrow;

    private void OnInteractOrThrowAction(InputAction.CallbackContext context)
    {
        if(context.performed) {
            OnInteractOrThrow?.Invoke();
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
            print("Pause");
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
            print("Unpause");
            OnUnpause?.Invoke();
            OnAllowPlayerControlsSwitch.DispatchEvent();
        }
    }
    
    
    
    public delegate void OnPutDelegate();
    public static event OnPutDelegate OnPut;

    private void OnPutAction(InputAction.CallbackContext context)
    {
        if(context.performed) {
            OnPut?.Invoke();
        }
    }
}
