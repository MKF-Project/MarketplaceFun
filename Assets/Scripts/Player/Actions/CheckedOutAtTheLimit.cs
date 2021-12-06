using System.Collections;
using System.Collections.Generic;
using MLAPI.Messaging;
using UnityEngine;

public class CheckedOutAtTheLimit : ScorableAction
{
    
    private PlayerScore _playerScore;
    
    protected override void Awake()
    {
        base.Awake();
        _playerScore = GetComponent<PlayerScore>();
    }

    public void ScoreAtTheLimit_OnlyServer()
    {
        if (IsServer)
        {
            _playerScore.ScoreAction(_scoreType);
        }
    }
    
}
