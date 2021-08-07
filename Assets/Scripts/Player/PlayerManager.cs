using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public bool enablePlayerBehaviour = true;
    public bool allowControlSwitching = true;

    private void Awake()
    {
        InputController.OnAllowMenuControlsSwitch += canSwitchControls;
        InputController.OnAllowPlayerControlsSwitch += canSwitchControls;
    }

    private void OnDestroy()
    {
        InputController.OnAllowMenuControlsSwitch -= canSwitchControls;
        InputController.OnAllowPlayerControlsSwitch -= canSwitchControls;
    }

    private bool canSwitchControls() => allowControlSwitching;

    private void Start()
    {
        PlayerController.playerBehaviourEnabled = enablePlayerBehaviour;
        if(enablePlayerBehaviour)
        {
            InputController.RequestPlayerControlsSwitch();
        }
        else
        {
            InputController.RequestMenuControlsSwitch();
        }
    }
}
