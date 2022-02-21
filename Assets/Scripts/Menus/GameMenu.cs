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

    public delegate void OnPressConfigurationsDelegate();
    public static event OnPressConfigurationsDelegate OnPressConfigurations;

    public delegate void OnViewTutorialDelegate();
    public static event OnViewTutorialDelegate OnViewTutorial;

    private void Awake()
    {
        ConnectionMenu.OnBack += this.toggleMenu;
        DisconnectMenu.OnPressOK += this.toggleMenu;
        ConfigMenu.OnPressOK += this.toggleMenu;
        TutorialMenu.OnExitTutorial += this.toggleMenu;
        NetworkController.OnDisconnected += returnFromLobby;
    }

    private void OnDestroy()
    {
        ConnectionMenu.OnBack -= this.toggleMenu;
        DisconnectMenu.OnPressOK -= this.toggleMenu;
        ConfigMenu.OnPressOK -= this.toggleMenu;
        TutorialMenu.OnExitTutorial -= this.toggleMenu;
        NetworkController.OnDisconnected -= returnFromLobby;
    }

    private void returnFromLobby(bool wasHost, bool wasIntended) => this.toggleMenu();

    // Button Actions
    public void joinGame() => OnJoinGame?.Invoke();
    public void hostGame() => OnHostGame?.Invoke();
    public void pressConfigurations() => OnPressConfigurations?.Invoke();
    public void viewTutorial() => OnViewTutorial?.Invoke();
    public void quitGame() => Application.Quit();
}
