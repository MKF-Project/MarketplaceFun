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

    private void Start()
    {
        // Start Enabled
        MenuManager.toggleMenu(gameObject);

        OnQuitGame += () => Application.Quit();

        ConnectionMenu.OnBack += () => MenuManager.toggleMenu(gameObject);
    }

    // Button Actions
    public void joinGame() => OnJoinGame?.Invoke();
    public void hostGame() => OnHostGame?.Invoke();
    public void quitGame() => OnQuitGame?.Invoke();
}
