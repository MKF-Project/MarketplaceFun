using System;
using System.Text.RegularExpressions;
using MLAPI;
using MLAPI.Messaging;
using UnityEditor;
using UnityEngine;

public class CheckOut : ScorableAction
{
    private bool _alreadyCheckOut;
    
    private const string MATCH_MANAGER_TAG = "MatchManager";
    private PlayerScore _playerScore;
    
    protected override void Awake()
    {
        base.Awake();
        _playerScore = GetComponent<PlayerScore>();
        _alreadyCheckOut = false;
        MatchManager.OnMatchExit += ResetPlayer;
    }

    private void OnDestroy()
    {
        MatchManager.OnMatchExit -= ResetPlayer;
    }

    public void PlayerCheckOut()
    {
        if (IsOwner & !_alreadyCheckOut)
        {
            MatchManager matchManager = GameObject.FindGameObjectWithTag(MATCH_MANAGER_TAG).GetComponent<MatchManager>();
            matchManager.CheckOutPlayer_ServerRpc(NetworkManager.LocalClientId);
        }
    }

    //Method to confirm if checkout was success in server
    public void ConfirmCheckOut()
    {
        _alreadyCheckOut = true;
        //ScoreCheckOut_ServerRpc();
    }

    [ServerRpc]
    public void ScoreCheckOut_ServerRpc()
    {
        _playerScore.ScoreAction(_scoreType);
    }
    
    public void ScoreCheckOut_OnlyServer()
    {
        if (IsServer)
        {
            _playerScore.ScoreAction(_scoreType);
        }
    }

    public void ResetPlayer()
    {
        _alreadyCheckOut = false;
    }
}
