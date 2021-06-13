using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMenu : MonoBehaviour
{
    // Events
    public delegate void OnJoinGameDelegate();
    public static event OnJoinGameDelegate OnJoinGame;

    public delegate void OnHostGameDelegate();
    public static event OnHostGameDelegate OnHostGame;

    public delegate void OnQuitGameDelegate();
    public static event OnQuitGameDelegate OnQuitGame;

    private void Awake()
    {
        OnQuitGame += () => Application.Quit();

        ConnectionMenu.OnBack += this.toggleMenu;

        NetworkController.OnDisconnected += (wasHost) => {
            this.toggleMenu();
            print($"{(wasHost? "Host" : "Client")} disconnected");
        };
    }

    private void Start()
    {
        // Start Enabled
        // This method *MUST* be run on Start, so that the other menus have the chance
        // to subscribe their events and run their setups before they get deactivated here.
        this.toggleMenu();
    }

    // Button Actions
    public void joinGame() => OnJoinGame?.Invoke();
    public void hostGame() => OnHostGame?.Invoke();
    public void quitGame() => OnQuitGame?.Invoke();
}
