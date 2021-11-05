using System;
using System.Text.RegularExpressions;
using MLAPI;
using UnityEngine;

public class CheckOut : ScorableAction
{

    private const string MATCH_MANAGER_TAG = "MatchManager";
    private MatchManager _matchManager;
    private PlayerScore _playerScore;
    
    private ScoreType _scoreType;
    

    private void Awake()
    {
        _matchManager = GameObject.FindGameObjectWithTag(MATCH_MANAGER_TAG).GetComponent<MatchManager>();
        _playerScore = GetComponent<PlayerScore>();
            
    }

    public override void SetScore(ScoreType scoreType)
    {
        _scoreType = scoreType;
    }

    public void PlayerCheckOut()
    {
        if (IsOwner)
        {
            _matchManager.CheckOutPlayer_ServerRpc(NetworkManager.LocalClientId);
            _playerScore.ScoreAction(_scoreType);
        }
    }
}
