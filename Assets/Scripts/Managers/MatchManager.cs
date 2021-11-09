using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using UnityEngine;
using UnityEngine.UI;

public class MatchManager : NetworkBehaviour
{
    //Player Complete Lists
    [HideInInspector]
    public List<ulong> ListCompletedPlayers;
    
    //Time variables
    public float MatchTime;
    private Text _clockText;
    private float _startTime;
    private bool _timeStarted;
    private bool _matchEnded;
    
    private NetworkVariableFloat networkTimeSpent = new NetworkVariableFloat(
        new NetworkVariableSettings
        {
            ReadPermission = NetworkVariablePermission.Everyone,
            WritePermission = NetworkVariablePermission.ServerOnly
        }
    );

    private ScoreController _scoreController;
    private const string SCORE_CONTROLLER_TAG = "ScoreController";
    private const string SCORE_SCENE_NAME = "ScoreScene";


    
    public delegate void OnMatchExitDelegate();
    public static event OnMatchExitDelegate OnMatchExit;



    public void Start()
    {
        _timeStarted = false;
        MatchTime = MatchTime * 60; 
        _clockText = GameObject.FindGameObjectWithTag("MatchCanvas").GetComponentInChildren<Text>();
        _clockText.text = "";

        SpawnController.OnSpawnOpened += InitiateStartTime;
        if (IsServer)
        {
            ListCompletedPlayers = new List<ulong>(4);
            _scoreController = GameObject.FindGameObjectWithTag(SCORE_CONTROLLER_TAG).GetComponent<ScoreController>();
        }

        networkTimeSpent.OnValueChanged = DisplayTime;

    }
    private void OnDestroy()
    {
        SpawnController.OnSpawnOpened -= InitiateStartTime;
    }
    

    public void InitiateStartTime()
    {
        SpawnController.OnSpawnOpened -= InitiateStartTime;
        _startTime = Time.time;
        _timeStarted = true;
    }

    private void Update()
    {
        if (IsServer)
        {
            if (_timeStarted)
            {
                float timeSpent = Time.time - _startTime;
                int secondsLeft = (int) (MatchTime - timeSpent) % 60;
                int networkSeconds = (int) (MatchTime - networkTimeSpent.Value) % 60;
                if (!secondsLeft.Equals(networkSeconds))
                {
                    networkTimeSpent.Value = timeSpent;
                }
            }
        }
    }

    public void DisplayTime(float pre, float timeSpent)
    {
        int minutesLeft = (int) (MatchTime - timeSpent) / 60;
        int secondsLeft = (int) (MatchTime - timeSpent) % 60;
        String minutesLeftText = minutesLeft > 9 ? "" + minutesLeft : "0" + minutesLeft;
        String secondsLeftText = secondsLeft > 9 ? "" + secondsLeft : "0" + secondsLeft;

        _clockText.text = "" + minutesLeftText + ":" + secondsLeftText;
        if (minutesLeft <= 0 && secondsLeft <= 0)
        {
            EndMatch();
        }
    }

    

    private void EndMatch()
    {
        if (!_matchEnded)
        {
            _timeStarted = false;
            _matchEnded = true;
            OnMatchExit?.Invoke();
            _clockText.text = "";

            if (IsServer)
            {
                EndMatch_ClientRpc();
                _scoreController.EndMatch();
                NetworkController.switchNetworkScene(SCORE_SCENE_NAME);
                //Use ScoreAuditor
                //Change to ScoreScene
            }

            Debug.Log("Match Ended");
            
        }
    }

    [ClientRpc]
    public void EndMatch_ClientRpc()
    {
        EndMatch();
    }

    [ServerRpc(RequireOwnership = false)]
    public void CheckOutPlayer_ServerRpc(ulong playerId)
    {
        if (!_matchEnded)
        {
            if (!VerifyPlayerAlreadyComplete(playerId))
            {
                ListCompletedPlayers.Add(playerId);
                WarnPlayerCheckOut_ClientRpc(playerId);
            }
        }
    }

    private bool VerifyPlayerAlreadyComplete(ulong playerId)
    {
        foreach (ulong completedId in ListCompletedPlayers)
        {
            if (completedId == playerId)
            {
                return true;
            }
        }

        return false;
    }

    [ClientRpc]
    public void WarnPlayerCheckOut_ClientRpc(ulong playerId)
    {
        GameObject playerGameObject = NetworkManager.ConnectedClients[playerId].PlayerObject.gameObject;
        String playerNickname = playerGameObject.GetComponent<PlayerInfo>().PlayerData.Nickname;
        MatchMessages.Instance.EditMessage("Player " + playerNickname + " Checked Out");
        MatchMessages.Instance.ShowMessage();
        if (NetworkManager.LocalClientId == playerId)
        {
            playerGameObject.GetComponent<CheckOut>().ConfirmCheckOut();
        }
    }
    
    

}
