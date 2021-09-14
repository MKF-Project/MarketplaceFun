using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;

public class PlayerController : NetworkBehaviour
{
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

    private void Awake()
    {
        _freeMovementControls = gameObject.GetComponent<FreeMovementControls>();
        _cartControls = gameObject.GetComponent<CartControls>();

        ControlScheme = PlayerControlSchemes.FreeMovementControls;
    }

    private void OnDestroy()
    {
        ControlScheme = PlayerControlSchemes.None;
    }
}
