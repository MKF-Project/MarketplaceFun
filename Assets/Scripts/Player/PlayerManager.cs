using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static bool AllowPlayerControls = true;
    public static bool AllowControlSwitching = true;
    

    [SerializeField] private bool _allowPlayerControls = true;
    [SerializeField] private bool _allowControlSwitching = true;

    private void Awake()
    {
        AllowPlayerControls = _allowPlayerControls;
        AllowControlSwitching = _allowControlSwitching;

        InputController.OnAllowMenuControlsSwitch += canSwitchControls;
        InputController.OnAllowPlayerControlsSwitch += canSwitchControls;
    }

    private void OnDestroy()
    {
        InputController.OnAllowMenuControlsSwitch -= canSwitchControls;
        InputController.OnAllowPlayerControlsSwitch -= canSwitchControls;
    }

    private bool canSwitchControls() => AllowControlSwitching;

    private void Start()
    {
        if(AllowPlayerControls)
        {
            InputController.RequestPlayerControlsSwitch();
        }
        else
        {
            InputController.RequestMenuControlsSwitch();
        }
    }
}
