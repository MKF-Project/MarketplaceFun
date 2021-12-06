using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MLAPI;
using UnityEngine;

public class ScoreController : MonoBehaviour
{

    //This class is used to guard the score information from each player
    //for the hole tournament, there for it is only instantiated on the Server
    private Dictionary<ulong, ScorePoints> _playerPoints;
    
    private ScoreAuditor _scoreAuditor;

    public int PointsToWin;
    
    void Awake()
    {
        if (!NetworkController.IsServer)
        {
            Destroy(gameObject);
        }

        _playerPoints = new Dictionary<ulong, ScorePoints>();
        DontDestroyOnLoad(gameObject);
        InitiatePlayerPoints(NetworkController.SelfID);

        _scoreAuditor = GetComponent<ScoreAuditor>();
        
        NetworkController.OnOtherClientConnected += InitiatePlayerPoints;
        NetworkController.OnOtherClientDisconnected += RemovePlayer;

    }

    private void OnDestroy()
    {
        NetworkController.OnOtherClientConnected += InitiatePlayerPoints;
        NetworkController.OnOtherClientDisconnected += RemovePlayer;
    }

    public void InitiatePlayerPoints(ulong playerId)
    {
        ScorePoints scorePoints = new ScorePoints(playerId);
        _playerPoints.Add(playerId, scorePoints);
        
        
        // REMOVER DEPOIS ---------------------------------------------------------------------------------------------------------------------------
        List<DescriptivePoints> descriptivePointsList = new List<DescriptivePoints>();
        DescriptivePoints descriptivePoints = new DescriptivePoints(6, 1);
        descriptivePointsList.Add(descriptivePoints);
        descriptivePointsList.Add(descriptivePoints);

        AddPointsToPlayer(playerId, 2, descriptivePointsList);
        // REMOVER DEPOIS ---------------------------------------------------------------------------------------------------------------------------

    }

    public void RemovePlayer(ulong playerId)
    {
        if (_playerPoints.ContainsKey(playerId))
        {
            _playerPoints.Remove(playerId);
        }
    }

    public void AddPointsToPlayer(ulong playerId, int points, List<DescriptivePoints> playerDescriptivePoints)
    {
        if (_playerPoints.TryGetValue(playerId, out ScorePoints scorePoints ))
        {
            scorePoints.Points += points;
            // MUDAR DEPOIS ---------------------------------------------------------------------------------------------------------------------------

            scorePoints.LastMatchPoints.AddRange(playerDescriptivePoints);
            //scorePoints.LastMatchPoints = playerDescriptivePoints;

            // MUDAR DEPOIS ---------------------------------------------------------------------------------------------------------------------------

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

    public SerializedScorePointList GetSerializedScore()
    {
        SerializedScorePointList serializedScorePointList = new SerializedScorePointList(_playerPoints.Values.ToArray());
        return serializedScorePointList;
    }


    public void EndMatch()
    {
        _scoreAuditor.Audit();
        AdicionaParaTeste();
    }

    public bool VerifyWinner()
    {
        foreach (ScorePoints scorePoints in _playerPoints.Values)
        {
            if (scorePoints.Points > PointsToWin)
            {
                return true;
            }
        }

        return false;
    }

    public List<ulong> GetWinnerList()
    {
        ScorePoints winnerScorePoints = new ScorePoints(0);
        List<ulong> winnersIdList = new List<ulong>();
        
        foreach (ScorePoints scorePoints in _playerPoints.Values)
        {
            if (scorePoints.Points > PointsToWin)
            {
                if (scorePoints.Points == winnerScorePoints.Points)
                {
                    winnersIdList.Add(scorePoints.PlayerId);
                }

                if (scorePoints.Points > winnerScorePoints.Points)
                {
                    winnersIdList = new List<ulong> {scorePoints.PlayerId};
                    winnerScorePoints = scorePoints;
                }
            }
        }

        return winnersIdList;
    }

    public SerializedWinnersList GetSerializedWinnersList()
    {
        SerializedWinnersList serializedWinnersList = new SerializedWinnersList(GetWinnerList().ToArray());
        return serializedWinnersList;
    }

    public void MoveToScoresToMainList()
    {
        foreach (ulong playerId in _playerPoints.Keys)
        {
            _playerPoints[playerId].ClearLastMatchPoints();
        }
    }

    public void AddToMainList(ulong playerId, DescriptivePoints descriptivePoints)
    {
        _playerPoints[playerId].PlayerPoints.Add(descriptivePoints);
    }

    // REMOVER DEPOIS ---------------------------------------------------------------------------------------------------------------------------

    private void AdicionaParaTeste()
    {
        List<DescriptivePoints> descriptivePointsList = new List<DescriptivePoints>();
        DescriptivePoints descriptivePoints = new DescriptivePoints(1, 5);
        descriptivePointsList.Add(descriptivePoints);

        AddPointsToPlayer(0, 5, descriptivePointsList);

    }
    // REMOVER DEPOIS ---------------------------------------------------------------------------------------------------------------------------


}