using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using MLAPI;

public class PlayerInput : NetworkBehaviour
{
    // Events
    public delegate void OnMoveDelegate(Vector2 moveDirection);
    public event OnMoveDelegate OnMove;

    public delegate void OnMoveReleasedDelegate();
    public event OnMoveReleasedDelegate OnMoveReleased;

    public delegate void OnLookDelegate(Vector2 lookDirection);
    public event OnLookDelegate OnLook;

    public delegate void OnLookStopDelegate();
    public event OnLookStopDelegate OnLookStop;

    public delegate void OnJumpDelegate();
    public event OnJumpDelegate OnJump;

    public delegate void OnInteractOrThrowDelegate();
    public event OnInteractOrThrowDelegate OnInteractOrThrow;

    public delegate void OnWalkDelegate();
    public event OnWalkDelegate OnWalk;

    public delegate void OnWalkPressedDelegate();
    public event OnWalkPressedDelegate OnWalkPressed;

    public delegate void OnWalkReleasedDelegate();
    public event OnWalkReleasedDelegate OnWalkReleased;

    public delegate void OnPauseDelegate();
    public event OnPauseDelegate OnPause;

    private bool _playerInputEnabled;
    public bool playerInputEnabled {
        get => _playerInputEnabled;
        set
        {
            _playerInputEnabled = value && IsOwner;
            if(!IsOwner)
            {
                return;
            }

            Cursor.visible = !value;
            Cursor.lockState = value? CursorLockMode.Locked : CursorLockMode.None;
        }
    }

    private void Awake()
    {
        playerInputEnabled = false;
    }

    /** A quick overview of InputAction callback states:
     *
     * Disabled:  The action is disabled and willnot detect any input.
     * Waiting:   The action is enabled and waiting for input. This is the sate the button is in when not pressed.
     * Started:   The action was started. This is the state the button immediately goes, and stays on, while it's being pressed.
     * Performed: The action was performed. This triggers exactly once, right after Started callback when the button is pressed. The action does not stay in this state.
     * Canceled:  The action was canceled or stopped. This triggers exactly once when the button is released, then the action immediately goes to the Waiting state.
     */

    public void OnMoveAction(InputAction.CallbackContext context)
    {
        if(!_playerInputEnabled)
        {
            return;
        }

        switch(context.phase)
        {
            case InputActionPhase.Performed:
                OnMove?.Invoke(context.ReadValue<Vector2>());
                break;

            case InputActionPhase.Canceled:
                OnMoveReleased?.Invoke();
                break;
        }
    }

    public void OnLookAction(InputAction.CallbackContext context)
    {
        if(!_playerInputEnabled)
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

    public void OnJumpAction(InputAction.CallbackContext context)
    {
        if(!_playerInputEnabled)
        {
            return;
        }

        if(context.performed) {
            OnJump?.Invoke();
        }
    }

    public void OnInteractOrThrowAction(InputAction.CallbackContext context)
    {
        if(!_playerInputEnabled)
        {
            return;
        }

        if(context.performed) {
            OnInteractOrThrow?.Invoke();
        }
    }

    public void OnWalkAction(InputAction.CallbackContext context)
    {
        if(!_playerInputEnabled)
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

    public void OnPauseAction(InputAction.CallbackContext context)
    {
        // TODO create Menu InputMap instead of toggle in here
        if(!IsOwner)
        {
            return;
        }

        if(context.performed) {
            OnPause?.Invoke();
        }
    }
}
