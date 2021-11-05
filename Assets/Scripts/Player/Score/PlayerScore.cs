using System;
using System.Collections.Generic;
using MLAPI;
using MLAPI.Messaging;
using UnityEngine;

public class PlayerScore: NetworkBehaviour
{
    public Dictionary<int, int> playerScoreList;

    private void Start()
    {
        if (!IsServer)
        {
            Destroy(this);
        }

        playerScoreList = new Dictionary<int, int>();
        waitTime = Time.time;
    }

    public void ScoreAction(ScoreType scoreType)
    {
        InsertScore(scoreType.Code, scoreType.Points);
    }


    private void InsertScore(int scoreCode, int scorePoints)
    {
        if (playerScoreList.ContainsKey(scoreCode))
        {
            int points = playerScoreList[scoreCode] + scorePoints;
            playerScoreList.Remove(scoreCode);
            playerScoreList.Add(scoreCode, points);
        }
        else
        {
            playerScoreList.Add(scoreCode, scorePoints);
        }
    }

    [ServerRpc]
    public void ScoreAction_ServerRpc(int scoreCode)
    {
        ScoreAction(ScoreList.ScoreTypeList[scoreCode]);
    }

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
        foreach (int key in playerScoreList.Keys)
        {
            ScoreType scoreType = ScoreList.ScoreTypeList[key];
            Debug.Log("Cliente: " + OwnerClientId + " - " + scoreType.Type + " " + scoreType.Code + ": " +  playerScoreList[key]);
        }
    }
}