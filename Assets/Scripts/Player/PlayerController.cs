using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;

public class PlayerController : NetworkBehaviour
{
    // By default, players start with no movement or camera behaviour,
    // because they are first instantiated on the menu scene

    // This event allows a static class method call to inform all
    // Player instances that they should update their behaviour state
    private delegate void OnBehaviourChangeDelegate(bool behaviourEnabled);
    private static event OnBehaviourChangeDelegate OnPlayerBehaviourChanged;

    private static bool _playerBehaviourEnabled = false;
    public static bool playerBehaviourEnabled
    {
        get => _playerBehaviourEnabled;
        set
        {
            if(value != _playerBehaviourEnabled)
            {
                _playerBehaviourEnabled = value;
                OnPlayerBehaviourChanged?.Invoke(value);
            }
        }
    }

    private Camera _playerCamera;

    public PlayerControlSchemes ControlScheme
    {
        get => _currentControlScheme;
        set
        {
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

    private PlayerControlSchemes _currentControlScheme;
    private FreeMovementControls _freeMovementControls = null;
    private CartControls _cartControls = null;

    public bool isFrozen = false;

    private void Awake()
    {
        _playerCamera = gameObject.GetComponentInChildren<Camera>();

        _freeMovementControls = gameObject.GetComponent<FreeMovementControls>();
        _cartControls = gameObject.GetComponent<CartControls>();

        ControlScheme = PlayerControlSchemes.FreeMovementControls;

        // Listen on OnPlayerBehaviourChanged event
        OnPlayerBehaviourChanged += updateBehaviourState;
    }

    private void Start()
    {
        // Set initial behaviour state for this player
        updateBehaviourState(IsOwner && playerBehaviourEnabled);
    }

    private void OnDestroy()
    {
        ControlScheme = PlayerControlSchemes.None;

        OnPlayerBehaviourChanged -= updateBehaviourState;

    }

    private void updateBehaviourState(bool behaviourEnabled)
    {
    }
}
