using System.Collections.Generic;
using MLAPI;
using UnityEngine;

public class ScoreAuditor : MonoBehaviour
{
    private const string MATCH_MANAGER_TAG = "MatchManager";

    private ScoreController _scoreController;

    private void Awake()
    {
        _scoreController = GetComponent<ScoreController>();
    }

    public void Audit()
    {
        MatchManager matchManager = GameObject.FindGameObjectWithTag(MATCH_MANAGER_TAG).GetComponent<MatchManager>();
        List<ulong> listCompletedPlayers = matchManager.ListCompletedPlayers;
        if (listCompletedPlayers.Count == 0)
        {
            return;
        }

        if (listCompletedPlayers.Count == 1)
        {
            ScorePlayerPoints(listCompletedPlayers[0], 3);
            return;
        }
        
        ScorePlayerPoints(listCompletedPlayers[0], 1);
        
        for (int index = 1; index < listCompletedPlayers.Count; index++)
        {
            ScorePlayerPoints(listCompletedPlayers[index], 0);
        }

    }

    private void ScorePlayerPoints(ulong playerId, int addMorePoints)
    {
        int totalPoints = CalculatePlayersPointsFromMatch(playerId);
        totalPoints += addMorePoints;

        _scoreController.AddPointsToPlayer(playerId, totalPoints);
    }

    private int CalculatePlayersPointsFromMatch(ulong playerId)
    {
        int totalPoints = 0;
        PlayerScore playerScore = NetworkController.GetPlayerByID(playerId).GetComponent<PlayerScore>();
        Dictionary<int, int> playerScoreDictionary = playerScore.PlayerScoreDictionary;
        foreach (int scoreId in playerScoreDictionary.Keys)
        {
            if (scoreId == 0 & playerScoreDictionary[scoreId] > 5)
            {
                totalPoints += 5;
            }
            else
            {
                totalPoints += playerScoreDictionary[scoreId];
            }
        }

        return totalPoints;
    }
    
    
}
