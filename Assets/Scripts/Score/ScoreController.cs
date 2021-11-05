using System;
using System.Collections;
using System.Collections.Generic;
using MLAPI;
using UnityEngine;

public class ScoreController : NetworkBehaviour
{

    //This class is used to guard the score information from each player
    //for the hole tournament, there for it is only instantiated on the Server
    private Dictionary<ulong, ScorePoints> _playerPoints;
    void Awake()
    {
        if (!IsServer)
        {
            Destroy(gameObject);
        }

        _playerPoints = new Dictionary<ulong, ScorePoints>();
        DontDestroyOnLoad(gameObject);
        InitiatePlayerPoints(NetworkManager.LocalClientId);

        NetworkManager.OnClientConnectedCallback += InitiatePlayerPoints;
        NetworkManager.OnClientDisconnectCallback += RemovePlayer;
    }

    private void OnDestroy()
    {
        NetworkManager.OnClientConnectedCallback -= InitiatePlayerPoints;
        NetworkManager.OnClientDisconnectCallback -= RemovePlayer;

    }

    public void InitiatePlayerPoints(ulong playerId)
    {
        ScorePoints scorePoints = new ScorePoints(playerId);
        _playerPoints.Add(playerId, scorePoints);
    }

    public void RemovePlayer(ulong playerId)
    {
        if (_playerPoints.ContainsKey(playerId))
        {
            _playerPoints.Remove(playerId);
        }
    }

    public void AddPointsToPlayer(ulong playerId, int points)
    {
        if (_playerPoints.TryGetValue(playerId, out ScorePoints scorePoints ))
        {
            scorePoints.Points += points;
            _playerPoints.Remove(playerId);
            _playerPoints.Add(playerId, scorePoints);
        }
    }

    public void AddLostCounterToPlayer(ulong playerId)
    {
        if (_playerPoints.TryGetValue(playerId, out ScorePoints scorePoints ))
        {
            scorePoints.LostCounter += 1;
            _playerPoints.Remove(playerId);
            _playerPoints.Add(playerId, scorePoints);
        }
    }

    public void ResetLostCounterToPlayer(ulong playerId)
    {
        if (_playerPoints.TryGetValue(playerId, out ScorePoints scorePoints ))
        {
            scorePoints.LostCounter = 0;
            _playerPoints.Remove(playerId);
            _playerPoints.Add(playerId, scorePoints);
        }
    }




}