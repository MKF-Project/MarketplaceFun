using System.Collections.Generic;
using MLAPI;
using UnityEngine;

public class ScoreAuditor : MonoBehaviour
{
    private const string MATCH_MANAGER_TAG = "MatchManager";

    private ScoreController _scoreController;
    
    private const int ENEMY_HIT_ID = 0;

    private const int SOLO_WINNER_ID = 3;

    private const int FIRST_TO_CHECK_OUT_ID = 5;


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
            ScorePlayerPoints(listCompletedPlayers[0], SOLO_WINNER_ID);
            return;
        }
        
        ScorePlayerPoints(listCompletedPlayers[0], FIRST_TO_CHECK_OUT_ID);
        
        for (int index = 1; index < listCompletedPlayers.Count; index++)
        {
            ScorePlayerPoints(listCompletedPlayers[index]);
        }

    }

    private void ScorePlayerPoints(ulong playerId)
    {
        int totalPoints = CalculatePlayersPointsFromMatch(playerId, out var playerDescriptivePoints);

        _scoreController.AddPointsToPlayer(playerId, totalPoints, playerDescriptivePoints);
    }

    
    private void ScorePlayerPoints(ulong playerId, int ScoreThisPoints)
    {

        int totalPoints = CalculatePlayersPointsFromMatch(playerId, out var playerDescriptivePoints);
        
        ScoreType scoreToAdd = ScoreConfig.ScoreTypeDictionary[ScoreThisPoints];
        DescriptivePoints descriptivePoints = new DescriptivePoints(scoreToAdd.Id, scoreToAdd.Points);
        playerDescriptivePoints.Add(descriptivePoints);
        totalPoints += scoreToAdd.Points;
        
        _scoreController.AddPointsToPlayer(playerId, totalPoints, playerDescriptivePoints);
    }

 

    private int CalculatePlayersPointsFromMatch(ulong playerId, out List<DescriptivePoints> playerDescriptivePoints)
    {
        int totalPoints = 0;
        playerDescriptivePoints = new List<DescriptivePoints>();
        PlayerScore playerScore = NetworkController.GetPlayerByID(playerId).GetComponent<PlayerScore>();
        Dictionary<int, int> playerScoreDictionary = playerScore.PlayerScoreDictionary;
        foreach (int scoreId in playerScoreDictionary.Keys)
        {
            if (scoreId == ENEMY_HIT_ID & playerScoreDictionary[scoreId] > 5)
            {
                DescriptivePoints descriptivePoints = new DescriptivePoints(scoreId, 5);
                playerDescriptivePoints.Add(descriptivePoints);
                totalPoints += 5;
            }
            else
            {
                int scorePoints = playerScoreDictionary[scoreId];
                DescriptivePoints descriptivePoints = new DescriptivePoints(scoreId, scorePoints);
                playerDescriptivePoints.Add(descriptivePoints);
                totalPoints += scorePoints;
            }
        }

        return totalPoints;
    }
    
    
}
