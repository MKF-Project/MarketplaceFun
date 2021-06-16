using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitMenu : MonoBehaviour
{
    // Events
    public delegate void OnLeaveMatchDelegate();
    public static event OnLeaveMatchDelegate OnLeaveMatch;

    public delegate void OnStayOnMatchDelegate();
    public static event OnStayOnMatchDelegate OnStayOnMatch;

    private void Awake()
    {
        InputManager.OnEscapeKeyPress += handleMenuState;
    }

    private void OnDestroy()
    {
        InputManager.OnEscapeKeyPress -= handleMenuState;
    }

    private void handleMenuState()
    {
        // Toggles menu active state
        gameObject.SetActive(!gameObject.activeSelf);

        // If pressing esc caused the menu to now be Inactive,
        // Then trigger OnStayOnMatch
        if(!gameObject.activeSelf)
        {
            stayOnMatch();
        }
    }

    // Button Events
    public void leaveMatch() => OnLeaveMatch?.Invoke();
    public void stayOnMatch()
    {
        // Make sure menu is closed when the player decides to stay in the match
        gameObject.SetActive(false);
        OnStayOnMatch?.Invoke();
    }

}
