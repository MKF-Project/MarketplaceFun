using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MLAPI;

public class ExitMenu : NetworkBehaviour
{
    // Events
    public delegate void OnLeaveMatchDelegate();
    public static event OnLeaveMatchDelegate OnLeaveMatch;

    public delegate void OnStayOnMatchDelegate();
    public static event OnStayOnMatchDelegate OnStayOnMatch;

    public bool ShowPrompt;
    
    private const string hostPrompt = "Stop";
    private const string clientPrompt = "Leave";
    [SerializeField] private Text _exitPrompt = null;

    private void Awake()
    {
        if (ShowPrompt)
        {
            _exitPrompt.text = $"{(IsHost ? hostPrompt : clientPrompt)} Match?";
        }

        InputController.OnPause += handleMenuState;
        InputController.OnUnpause += handleMenuState;
    }

    private void OnDestroy()
    {
        InputController.OnPause -= handleMenuState;
        InputController.OnUnpause -= handleMenuState;
    }

    private void handleMenuState()
    {
        if(gameObject.activeSelf)
        {
            if(InputController.RequestPlayerControlsSwitch())
            {
                gameObject.SetActive(false);
                OnStayOnMatch?.Invoke();
            }
        }
        else if(InputController.RequestMenuControlsSwitch())
        {
            gameObject.SetActive(true);
        }
    }

    // Button Events
    public void leaveMatch() => OnLeaveMatch?.Invoke();
    public void stayOnMatch() => handleMenuState();

}
