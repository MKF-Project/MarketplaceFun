using System;
using System.Collections;
using System.Collections.Generic;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using UnityEngine;
using UnityEngine.UI;

public class LobbyPosition : NetworkBehaviour
{
    private bool _isOnLobby;
    private Vector3 _lobbyPosition;

    
    void Start()
    {
        _isOnLobby = true;
        SceneManager.OnMatchLoaded += ExitLobby;
    }

    private void OnDestroy()
    {
        SceneManager.OnMatchLoaded -= ExitLobby;

    }

    private void Update()
    {
        if (_isOnLobby && IsOwner)
        {
            transform.position = _lobbyPosition;
        }
    }
    
    
    public void PositionPlayer(Vector3 position, int index)
    {
        _lobbyPosition = position;
    }


    public void ExitLobby(String scene)
    {
        _isOnLobby = false;
        Destroy(this);
    }
   
}
