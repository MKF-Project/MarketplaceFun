using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using MLAPI;
using MLAPI.Messaging;
using UnityEngine;
using UnityEngine.UI;

public class MatchManager : NetworkBehaviour
{
    //Player Complete Lists
    private ulong[] ListCompletedPlayers;
    private int _freeIndex;
    
    //Time variables
    public float MatchTime;
    private Text _clockText;
    private float _startTime;
    private bool _timeStarted;
    private bool _matchEnded;
    
    // Spawn Events
    public delegate void OnMatchStartDelegate();
    public static event OnMatchStartDelegate OnMatchStart;
    
    public delegate void OnMatchEndDelegate();
    public static event OnMatchEndDelegate OnMatchEnd;


    public void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void Start()
    {
        _timeStarted = false;
        MatchTime = MatchTime * 60; 
        _clockText = GameObject.FindGameObjectWithTag("MatchCanvas").GetComponentInChildren<Text>();
        _clockText.text = "";

        SpawnController.OnSpawnOpened += InitiateStartTime;
        if (IsServer)
        {
            ListCompletedPlayers = new ulong[4];
            _freeIndex = 0;
        }
        
    }
    

    public void InitiateStartTime()
    {
        SpawnController.OnSpawnOpened -= InitiateStartTime;
        _startTime = Time.time;
        _timeStarted = true;
        OnMatchStart?.Invoke();
    }

    private void Update()
    {
        if (_timeStarted)
        {
            float timeSpent = Time.time - _startTime;
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
    }

    private void OnDestroy()
    {
        SpawnController.OnSpawnOpened -= InitiateStartTime;
    }

    private void EndMatch()
    {
        if (!_matchEnded)
        {
            _timeStarted = false;
            _matchEnded = true;
            OnMatchEnd?.Invoke();
            _clockText.text = "";

            if (IsServer)
            {
                EndMatch_ClientRpc();
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

    [ServerRpc]
    public void CheckOutPlayer_ServerRpc(ulong playerId)
    {
        if (!_matchEnded)
        {
            ListCompletedPlayers[_freeIndex] = playerId;
            _freeIndex++;
            WarnPlayerCheckOut_ClientRpc(playerId);
        }
    }

    [ClientRpc]
    public void WarnPlayerCheckOut_ClientRpc(ulong playerId)
    {
        String playerNickname = NetworkManager.ConnectedClients[playerId].PlayerObject.GetComponent<PlayerInfo>().PlayerData.Nickname;
        MatchMessages.Instance.EditMessage("Player " + playerNickname + " Checked Out");
        MatchMessages.Instance.ShowMessage();
    }


}
