using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;

public class LobbyMenu : NetworkBehaviour
{
    // Events
    public delegate void OnStartMatchDelegate();
    public static event OnStartMatchDelegate OnStartMatch;

    public delegate void OnCancelMatchDelegate();
    public static event OnCancelMatchDelegate OnCancelMatch;

    [SerializeField] private Text _playerList = null;
    [SerializeField] private Button _startGame = null;

    private void Awake()
    {
        // Opening Event
        NetworkController.OnConnected += openLobbyMenu;

        //////TEMP
        OnStartMatch += () => print("Start Match");
    }

    private void OnDestroy()
    {
        // Opening Event
        NetworkController.OnConnected -= openLobbyMenu;
    }

    private void OnEnable()
    {
        _startGame.interactable = IsHost;
    }

    private void openLobbyMenu(bool isHost)
    {
        this.toggleMenu();
    }

    // Button Actions
    public void startMatch()
    {
        if(IsHost)
        {
            OnStartMatch?.Invoke();
        }
    }

    public void cancelMatch() => OnCancelMatch?.Invoke();

}
