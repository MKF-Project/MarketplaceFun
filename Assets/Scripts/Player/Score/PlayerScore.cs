using System;
using System.Collections.Generic;
using MLAPI;
using MLAPI.Messaging;
using UnityEngine;

public class PlayerScore: NetworkBehaviour
{
    public Dictionary<int, int> PlayerScoreDictionary;

    private void Start()
    {
        if (!IsServer)
        {
            Destroy(this);
        }

        PlayerScoreDictionary = new Dictionary<int, int>();
        //waitTime = Time.time;
        MatchManager.OnMatchStart += ResetPlayerScore;
    }

    private void OnDestroy()
    {
        MatchManager.OnMatchStart -= ResetPlayerScore;
    }

    public void ScoreAction(ScoreType scoreType)
    {
        InsertScore(scoreType.Id, scoreType.Points);
    }


    private void InsertScore(int scoreCode, int scorePoints)
    {
        if (PlayerScoreDictionary.ContainsKey(scoreCode))
        {
            int points = PlayerScoreDictionary[scoreCode] + scorePoints;
            PlayerScoreDictionary.Remove(scoreCode);
            PlayerScoreDictionary.Add(scoreCode, points);
        }
        else
        {
            PlayerScoreDictionary.Add(scoreCode, scorePoints);
        }
    }

    [ServerRpc]
    public void ScoreAction_ServerRpc(int scoreCode)
    {
        ScoreAction(ScoreConfig.ScoreTypeDictionary[scoreCode]);
    }


    public void ResetPlayerScore()
    {
        PlayerScoreDictionary = new Dictionary<int, int>();
    }


    /*
    private float waitTime;

    private void Update()
    {
        if (Time.time > waitTime + 10)
        {
            waitTime = Time.time;
            Print();
        }
    }

    public void Print()
    {
        foreach (var key in PlayerScoreDictionary.Keys)
        {
            ScoreType scoreType = ScoreConfig.ScoreTypeDictionary[key];
            Debug.Log("Cliente: " + OwnerClientId + " - " + scoreType.Type + " " + scoreType.Id + ": " +  PlayerScoreDictionary[key]);
        }
    }
    */
}