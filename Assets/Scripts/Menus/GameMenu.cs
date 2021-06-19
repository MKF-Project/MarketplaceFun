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

    private void Awake()
    {
        ConnectionMenu.OnBack += this.toggleMenu;
        DisconnectMenu.OnPressOK += this.toggleMenu;
        NetworkController.OnDisconnected += returnFromLobby;
    }

    private void OnDestroy()
    {
        ConnectionMenu.OnBack -= this.toggleMenu;
        DisconnectMenu.OnPressOK -= this.toggleMenu;
        NetworkController.OnDisconnected -= returnFromLobby;
    }

    private void returnFromLobby(bool wasHost, bool wasIntended) => this.toggleMenu();

    // Button Actions
    public void joinGame() => OnJoinGame?.Invoke();
    public void hostGame() => OnHostGame?.Invoke();
    public void quitGame() => Application.Quit();
}
