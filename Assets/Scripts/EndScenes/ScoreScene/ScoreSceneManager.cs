
using System;
using System.Collections;
using System.Collections.Generic;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using UnityEngine;

public class ScoreSceneManager : NetworkBehaviour
{
    private const float markerScale = 0.3f;

    public int _playersReady;

    private ScoreController _scoreController;
    
    private int _playerIndex;
    
    //SceneObjects
    public Transform Camera;
    public float FirstPositionY;
    
    //Scene Controllers
    public ScoreCanvas ScoreCanvas;
    public PointMarkerController PointMarkerController;
    public ScoreSpotController ScoreSpotController;

    //Mutex
    private bool _haveWinner;
    private bool _canActivateReady;
    private bool _scoreFinished;

    //public WinCamera WinCamera;
    
    public delegate void OnWinDelegate(int winnerIndex);
    public static event OnWinDelegate OnWin;

    private List<Tuple<ulong, DescriptivePoints>> _listToAddInMain;


    private bool _startProcess;

    public NetworkVariable<SerializedScorePointList> scoreList = new NetworkVariable<SerializedScorePointList>(
        new NetworkVariableSettings
        {
            ReadPermission = NetworkVariablePermission.Everyone,
            WritePermission = NetworkVariablePermission.ServerOnly
        }
    );
    public void Awake()
    {
        _startProcess = false;
        _haveWinner = false;
        _canActivateReady = false;
        _playerIndex = -1;
        NetworkController.SelfPlayer.GetComponent<LobbyPosition>().EnterOnScore(Camera.position);

        if (IsServer)
        { 
            _listToAddInMain = new List<Tuple<ulong, DescriptivePoints>>();
            _scoreFinished = false;
            _playersReady = 1;
            _scoreController = GameObject.FindGameObjectWithTag("ScoreController").GetComponent<ScoreController>();
            scoreList.Value = _scoreController.GetSerializedScore();
            StartShowPoints();
            ScoreCanvas.ShowButtonStart();
            
        }

        if (IsClient & !IsHost)
        {
            ScoreCanvas.ShowButtonReady();
            scoreList.OnValueChanged += StartShowPoints;
        }
        TurnOnPlayerSpots();
    }

    public void Update()
    {
        if (IsServer )
        {
            if (_playersReady == NetworkManager.ConnectedClientsList.Count & _scoreFinished)
            {
                ScoreCanvas.ActivateButtonStart();
            }
        }

        if (_haveWinner)
        {
            ScoreCanvas.HideUI();
            PlayWinLoseAnimation();
            OnWin?.Invoke(_playerIndex);
            _haveWinner = false;
        }

        if (IsClient & !IsHost)
        {
            if (_canActivateReady)
            {
                ScoreCanvas.ActivateButtonReady();
                _canActivateReady = false;
            }
        }

        if (InputController.playerInputEnabled)
        {
            InputController.RequestMenuControlsSwitch();
        }

    }

    
    private void StartShowPoints()
    {
        if (!_startProcess)
        {
            _startProcess = true;
            GenerateBeginMarkers();
            StartCoroutine(nameof(GenerateMarkersForPointType));
        }

    }

    private void StartShowPoints(SerializedScorePointList prev, SerializedScorePointList  pos)
    {
        if (!_startProcess)
        {
            _startProcess = true;
            GenerateBeginMarkers();
            StartCoroutine(nameof(GenerateMarkersForPointType));
        }
    }

    private IEnumerator GenerateMarkersForPointType()
    {
        ScorePoints[] scorePointsList = scoreList.Value.Array;
        Dictionary<ulong, Player> localPlayers = NetworkController.GetLocalPlayers();

        bool haveScoreType;
        foreach (int scoreTypeId in ScoreConfig.ScoreTypeDictionary.Keys)
        {
            haveScoreType = false;
            //
            foreach (ScorePoints scorePoint in scorePointsList)
            {
                foreach (DescriptivePoints descriptivePoints in scorePoint.LastMatchDescriptivePoints)
                {
                    if (descriptivePoints.ScoreTypeId == scoreTypeId)
                    {
                        if (!haveScoreType)
                        {
                            ScoreType scoreType = ScoreConfig.ScoreTypeDictionary[scoreTypeId];
                            ScoreCanvas.ShowScoreText(scoreType.Type, scoreType.ScoreColor.color);
                        
                            yield return new WaitForSeconds(2f);
                            haveScoreType = true;
                        }

                        int playerIndex = localPlayers[scorePoint.PlayerId].GetComponent<PlayerInfo>().PlayerData.Color;
                        PointMarkerController.SpawnMarker(playerIndex-1, descriptivePoints.ScoreTypeId, descriptivePoints.Points);
                        
                        
                        #if UNITY_EDITOR
                            Debug.Log("Generated " + descriptivePoints.ScoreTypeId + " Points:  " + descriptivePoints.Points + " --- at " + playerIndex);
                        #endif
                        
                        if (IsServer)
                        {
                            Tuple<ulong, DescriptivePoints> tuple = new Tuple<ulong, DescriptivePoints>(scorePoint.PlayerId, descriptivePoints);
                            _listToAddInMain.Add(tuple);
                        }
                        
                        yield return new WaitForSeconds(3f);
                    }
                    
                }
            }
        }


        //populate ScoreSign
        foreach (ScorePoints scorePoint in scorePointsList)
        {
            int playerIndex = localPlayers[scorePoint.PlayerId].GetComponent<PlayerInfo>().PlayerData.Color - 1;
            ScoreSpotController.AddPointsAt(playerIndex, scorePoint.LastMatchPoints);
        }

        if (IsServer)
        {
            _scoreController.MoveToScoresToMainList();
            _scoreFinished = true;
            //-----------------------------------------
            if(_scoreController.VerifyWinner())
            {
                ulong winnerId = _scoreController.GetWinner();
                _playerIndex = NetworkController.GetPlayerByID(winnerId).GetComponent<PlayerInfo>().PlayerData.Color;
                HaveWinner_ClientRpc(_playerIndex);
                _haveWinner = true;
            }
            else
            {
                CanActivateReadyButton_ClientRpc();
            }
            //-----------------------------------------
        }
    }

    private void GenerateBeginMarkers()
    {
        ScorePoints[] scorePointsList = scoreList.Value.Array;

        float nextPositionY = 0;
        
        foreach (ScorePoints scorePoint in scorePointsList)
        {
            //Get Player Index
            int playerIndex = NetworkController.GetPlayerByID(scorePoint.PlayerId).GetComponent<PlayerInfo>().PlayerData.Color - 1;

            //Populate Sign
            ScoreSpotController.StartPointsAt(playerIndex, scorePoint.TotalPoints);
            
            //Generate Markers
            foreach (DescriptivePoints descriptivePoints in scorePoint.PlayerPoints)
            {
                Debug.Log("Begin Generated " + descriptivePoints.ScoreTypeId + " Points:  " + descriptivePoints.Points + " --- at " + playerIndex);
                float halfMarkerScale = (markerScale + 0.1f * descriptivePoints.Points) / 2;
                nextPositionY += halfMarkerScale;
                PointMarkerController.SpawnMarkerAt(playerIndex, descriptivePoints.ScoreTypeId, descriptivePoints.Points, FirstPositionY + nextPositionY);
                nextPositionY += halfMarkerScale;
            }
        }
    }



    private void PutMatchListIntoMainList()
    {

        _scoreController.AddToTotalPoints();
        foreach (Tuple<ulong,DescriptivePoints> tuple in _listToAddInMain)
        {
            _scoreController.AddToMainList(tuple.Item1, tuple.Item2);
        }
    }



    public void IAmReady()
    {
        PlayerIsReady_ServerRpc();
        ScoreCanvas.InactivateButtonReady();
    }

    [ClientRpc]
    public void HaveWinner_ClientRpc(int winnerIndex)
    {
        _playerIndex = winnerIndex;
        _haveWinner = true;
    }
    
    [ClientRpc]
    public void CanActivateReadyButton_ClientRpc()
    {
        _canActivateReady = true;
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayerIsReady_ServerRpc()
    {
        _playersReady += 1;
    }
    

    public void StartNewMatch()
    {
        PutMatchListIntoMainList();
        SceneManager.LoadMatch();
        //NetworkController.switchNetworkScene(SceneManager.MatchSceneTag);
    }

    public void PlayWinLoseAnimation()
    {
        NetworkAnimator playerAnimator =  NetworkController.SelfPlayer.GetComponent<NetworkAnimator>();
        if (NetworkController.SelfPlayer.GetComponent<PlayerInfo>().PlayerData.Color == _playerIndex)
        {
            playerAnimator.SetTrigger("P_Vitoria");
            return;
        }
       
        playerAnimator.SetTrigger("P_Derrota");
    }

    private void TurnOnPlayerSpots()
    {
        foreach (Player player in NetworkController.GetLocalPlayers().Values)
        {
            int spotIndex = player.GetComponent<PlayerInfo>().PlayerData.Color-1;
            ScoreSpotController.TurnSpotOn(spotIndex);
        }
    }

}