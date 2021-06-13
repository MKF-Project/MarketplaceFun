using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyMenu : MonoBehaviour
{
    // Events
    public delegate void OnStartMatchDelegate();
    public static event OnStartMatchDelegate OnStartMatch;

    public delegate void OnCancelMatchDelegate();
    public static event OnCancelMatchDelegate OnCancelMatch;

    private void Awake()
    {
        NetworkController.OnConnected += (isHost) => {
            print("On Lobby Menu");
            MenuManager.toggleMenu(gameObject);
        };

        OnStartMatch += () => print("Start Match");
    }

    // Button Actions
    public void startMatch() => OnStartMatch?.Invoke();
    public void cancelMatch() => OnCancelMatch?.Invoke();

}
