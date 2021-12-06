
using System;
using System.Collections;
using System.Collections.Generic;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using UnityEngine;

public class ScoreSceneManager_old : NetworkBehaviour
{
    /*
    private ScoreCanvas _scoreCanvas;

    private const string SCORE_CANVAS_TAG = "ScoreCanvas";
    
    private const string WIN_SCENE_NAME = "WinScene";

    public int _playersReady;

    private ScoreController _scoreController;

    public NetworkVariable<SerializedScorePointList> scoreList = new NetworkVariable<SerializedScorePointList>(
        new NetworkVariableSettings
        {
            ReadPermission = NetworkVariablePermission.Everyone,
            WritePermission = NetworkVariablePermission.ServerOnly
        }
    );
    public void Awake()
    {
        _scoreCanvas = GameObject.FindGameObjectWithTag(SCORE_CANVAS_TAG).GetComponent<ScoreCanvas>();

        if (IsServer)
        {
            _playersReady = 1;
            _scoreController = GameObject.FindGameObjectWithTag("ScoreController").GetComponent<ScoreController>();
            scoreList.Value = _scoreController.GetSerializedScore();
            PopulateTextOnCanvas();
            _scoreCanvas.ShowButtonStart();
            
        }

        if (IsClient)
        {
            scoreList.OnValueChanged += PopulateTextOnCanvas;
            if (!IsHost)
            {
                _scoreCanvas.ShowButtonReady();
            }
        }

    }

    private void Start()
    {
        if (IsServer)
        {
            if(_scoreController.VerifyWinner())
            {
                //Start Courotine to Win Scene
                //StartCoroutine(nameof(WinSceneCoroutine));
                GoToWinScene();
            }
        }
    }
    

    public void Update()
    {
        if (IsServer )
        {
            if (_playersReady == NetworkManager.ConnectedClientsList.Count)
            {
                _scoreCanvas.ActivateButtonStart();
            }
        }
    }

    public void PopulateTextOnCanvas(SerializedScorePointList prev, SerializedScorePointList pos)
    {
        PopulateTextOnCanvas();
        
    }
    
    
    public void PopulateTextOnCanvas()
    {
        ScorePoints[] scorePointsList = scoreList.Value.Array;
        foreach (ScorePoints scorePoints in scorePointsList)
        {
            int points = scorePoints.Points;
            
            int playerNumber = NetworkController.GetPlayerByID(scorePoints.PlayerId).GetComponent<PlayerInfo>().PlayerData.Color;
            Debug.Log("Finalizou o player " + scorePoints.PlayerId + " com a cor " + playerNumber + " e pontuação " +  points);
            _scoreCanvas.SetScorePoints(playerNumber -1 , points);
            String description = "";
            foreach (DescriptivePoints lastMatchPoint in scorePoints.LastMatchPoints)
            {
                String scoreTypeName = ScoreConfig.ScoreTypeDictionary[lastMatchPoint.ScoreTypeId].Type;
                description += "+" + lastMatchPoint.Points + " " + scoreTypeName + "    ";
            }
            _scoreCanvas.SetScoreDescription(playerNumber -1, description);
        }
        
        if (IsServer)
        {
            _scoreController.MoveToScoresToMainList();
        }
    }

    public void IAmReady()
    {
        PlayerIsReady_ServerRpc();
        _scoreCanvas.InactivateButtonReady();
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayerIsReady_ServerRpc()
    {
        _playersReady += 1;
    }

    public void StartNewMatch()
    {
        NetworkController.switchNetworkScene(SceneManager.MatchSceneTag);
    }

    IEnumerator WinSceneCoroutine()
    {
        yield return new WaitForSeconds(1f);
        GoToWinScene();
    }

    public void GoToWinScene()
    {
        NetworkController.switchNetworkScene(WIN_SCENE_NAME);
    }

*/
}