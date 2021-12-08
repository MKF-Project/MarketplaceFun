using System;
using MLAPI;
using UnityEngine;


public class LobbyPosition : NetworkBehaviour
{
    private bool _isOnLobby;
    private Vector3 _lobbyPosition;
    private bool _isOnScore;
    private Vector3 _lookAt;

    
    void Start()
    {
        _isOnLobby = true;
        _isOnScore = false;
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
            if (_isOnScore)
            {
                transform.position = _lobbyPosition + new Vector3(0,23,0);
            }
            else
            {
                transform.position = _lobbyPosition;
            }
        }
    }
    
    
    public void PositionPlayer(Vector3 position, int index)
    {
        _lobbyPosition = position;
    }


    public void ExitLobby(String scene)
    {
        _isOnLobby = false;
        //Destroy(this);
    }

    public void EnterOnScore(Vector3 cameraPosition)
    {
        _isOnScore = true;
        _isOnLobby = true;
        Quaternion rotation = transform.rotation;
        rotation.y = 180;
        transform.rotation = rotation;

    }
}
