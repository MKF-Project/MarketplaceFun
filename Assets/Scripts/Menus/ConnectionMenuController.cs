using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectionMenuController : MonoBehaviour
{
    // Events
    public delegate void OnGoToLobbyDelegate(bool isHost, NetworkTransportTypes transport, string address);
    public static event OnGoToLobbyDelegate OnGoToLobby;

    public delegate void OnBackDelegate();
    public static event OnBackDelegate OnBack;

    private void Start()
    {
        GameMenuController.OnJoinGame += () => initializeConnectionMenu(false);
        GameMenuController.OnHostGame += () => initializeConnectionMenu(true);
    }

    private void initializeConnectionMenu(bool isHost) {
        MenusController.toggleMenu(gameObject);
        print(isHost);
    }

    // Button Actions
    // public void goToLobby()
    public void back() => OnBack?.Invoke();
}
