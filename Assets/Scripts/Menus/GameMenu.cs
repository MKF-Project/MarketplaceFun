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
        NetworkController.OnDisconnected += returnAfterDisconnect;
    }

    private void OnDestroy()
    {
        ConnectionMenu.OnBack -= this.toggleMenu;
        NetworkController.OnDisconnected -= returnAfterDisconnect;
    }

    private void returnAfterDisconnect(bool wasHost, bool wasIntended)
    {
        this.toggleMenu();

        #if UNITY_EDITOR
            print($"{(wasHost? "Host" : "Client")} {(wasIntended? "disconnected" : "lost connection")}.");
        #endif
    }

    // Button Actions
    public void joinGame() => OnJoinGame?.Invoke();
    public void hostGame() => OnHostGame?.Invoke();
    public void quitGame() => OnQuitGame?.Invoke();
}
