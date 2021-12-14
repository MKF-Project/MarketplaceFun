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
    private const string MATCH_CANVAS_TAG = "MatchCanvas";
    private static readonly int MATCH_CANVAS_ANIMATION_PARAM = Animator.StringToHash("P_Hurry");

    // Player Complete Lists
    [HideInInspector]
    public List<ulong> ListCompletedPlayers;

    public Transform CompleteSpot;

    public bool MatchHurry;

    public Color TimeColor;
    public Color HurryColor;

    public float HurryTimeSeconds;


    // Time variables
    public float MatchTimeMinutes;

    private GameObject _clockCanvas;
    private Text _clockText;
    private Animator _clockAnimator;
    private AudioSource _clockAudioSource;
    private bool _hasSetHurry = false;
    private bool _hasSetThreshold = false;

    private float _startTime;
    private bool _timeStarted;
    private bool _matchEnded;

    private int _clientsFished;

    public int ThresholdSeconds;

    public NetworkVariableFloat NetworkTimeSpent = new NetworkVariableFloat
    (
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

    public delegate void OnMatchStartDelegate();
    public static event OnMatchStartDelegate OnMatchStart;

    public Camera SceneCamera;

    [Header("SFX")]
    public float TickingTempo = 0.5f;
    public float ThresholdTickingTempo = 0.25f;
    public List<AudioClip> ClockTickUpSounds;
    public List<AudioClip> ClockTickDownSounds;

    private float _lastClockTick = 0;
    private bool _isOnUpTick = true;

    private void Start()
    {
        OnMatchStart?.Invoke();
        _clientsFished = 0;
        _matchEnded = false;
        _timeStarted = false;

        MatchTimeMinutes = MatchTimeMinutes * 60;

        _clockCanvas = GameObject.FindGameObjectWithTag(MATCH_CANVAS_TAG);
        _clockAnimator = _clockCanvas.GetComponent<Animator>();
        _clockText = _clockCanvas.GetComponentInChildren<Text>();
        _clockText.text = "";

        _clockText.color = TimeColor;

        _clockAudioSource = GetComponent<AudioSource>();

        SpawnController.OnSpawnOpened += InitiateStartTime;
        if(IsServer)
        {
            ListCompletedPlayers = new List<ulong>(4);
            _scoreController = GameObject.FindGameObjectWithTag(SCORE_CONTROLLER_TAG).GetComponent<ScoreController>();
        }

        NetworkTimeSpent.OnValueChanged = DisplayTime;

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
        if(IsServer)
        {
            if(_timeStarted)
            {
                float timeSpent = Time.time - _startTime;
                int secondsLeft = (int) (MatchTimeMinutes - timeSpent) % 60;
                int networkSeconds = (int) (MatchTimeMinutes - NetworkTimeSpent.Value) % 60;
                if(!secondsLeft.Equals(networkSeconds))
                {
                    NetworkTimeSpent.Value = timeSpent;
                }
            }

            if(_matchEnded)
            {
                if(_clientsFished == NetworkManager.ConnectedClientsList.Count)
                {
                    NetworkController.switchNetworkScene(SCORE_SCENE_NAME);
                }
            }

            else if(_hasSetHurry)
            {
                var timeDiff = Time.time - _lastClockTick;
                if((_hasSetThreshold && timeDiff > ThresholdTickingTempo) || timeDiff > TickingTempo)
                {
                    _lastClockTick = Time.time;

                    var list = _isOnUpTick? ClockTickUpSounds : ClockTickDownSounds;
                    _clockAudioSource.PlayOneShot(list[UnityEngine.Random.Range(0, list.Count)]);

                    _isOnUpTick = !_isOnUpTick;
                }
            }
        }
    }

    public void DisplayTime(float pre, float timeSpent)
    {
        int minutesLeft = (int) (MatchTimeMinutes - timeSpent) / 60;
        int secondsLeft = (int) (MatchTimeMinutes - timeSpent) % 60;
        String minutesLeftText = minutesLeft > 9 ? "" + minutesLeft : "0" + minutesLeft;
        String secondsLeftText = secondsLeft > 9 ? "" + secondsLeft : "0" + secondsLeft;

        _clockText.text = "" + minutesLeftText + ":" + secondsLeftText;

        if(!_hasSetHurry && minutesLeft <= 0 && secondsLeft <= HurryTimeSeconds)
        {
            _hasSetHurry = true;

            _clockText.color = HurryColor;
            _clockAnimator.SetBool(MATCH_CANVAS_ANIMATION_PARAM, true);
        }

        if(!_hasSetThreshold && minutesLeft <= 0 && secondsLeft <= ThresholdSeconds)
        {
            _hasSetThreshold = true;
        }

        if(minutesLeft <= 0 && secondsLeft <= 0)
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
            InputController.RequestMenuControlsSwitch();
            if (IsClient)
            {
                WarnServerMatchEnd_ServerRpc();
            }

            if (IsServer)
            {
                EndMatch_ClientRpc();
                _scoreController.EndMatch();
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
    public void WarnServerMatchEnd_ServerRpc()
    {
        _clientsFished += 1;
    }

    [ServerRpc(RequireOwnership = false)]
    public void CheckOutPlayer_ServerRpc(ulong playerId)
    {
        if(!_matchEnded)
        {
            if(!VerifyPlayerAlreadyComplete(playerId))
            {
                ListCompletedPlayers.Add(playerId);
                WarnPlayerCheckOut_ClientRpc(playerId);
                GameObject playerGameObject = NetworkManager.ConnectedClients[playerId].PlayerObject.gameObject;
                playerGameObject.GetComponent<CheckOut>().ScoreCheckOut_OnlyServer();
                if(ThresholdSeconds > NetworkTimeSpent.Value)
                {
                    playerGameObject.GetComponent<CheckedOutAtTheLimit>().ScoreAtTheLimit_OnlyServer();
                }

                if(MatchHurry)
                {
                    float timeLeft = MatchTimeMinutes - NetworkTimeSpent.Value;
                    if(timeLeft > HurryTimeSeconds)
                    {
                        MatchTimeMinutes -= timeLeft - HurryTimeSeconds;
                        UpdateMatchTime_ClientRpc(MatchTimeMinutes);
                    }
                }

                if (ListCompletedPlayers.Count == NetworkController.GetLocalPlayers().Count)
                {
                    EndMatch();
                }
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
        GameObject playerGameObject = NetworkController.GetPlayerByID(playerId).gameObject;
        PlayerInfo playerInfo = playerGameObject.GetComponent<PlayerInfo>();
        String playerNickname = playerInfo.PlayerData.Nickname;
        Color color = ColorManager.Instance.GetColor(playerInfo.PlayerData.Color).color;
        MatchMessages.Instance.EditMessage(playerNickname + "\n" + "passou no caixa");
        MatchMessages.Instance.EditColorMessage(playerInfo.PlayerData.Color);

        MatchMessages.Instance.ShowMessage();
        if(NetworkManager.LocalClientId == playerId)
        {
            playerGameObject.GetComponent<CheckOut>().ConfirmCheckOut();
            PlayerStandBy(playerGameObject);
        }
    }

    private void PlayerStandBy(GameObject playerGameObject)
    {
        SceneCamera.GetComponent<CameraScript>().SetCameraOnScene();
        InputController.RequestMenuControlsSwitch();
        playerGameObject.transform.position = CompleteSpot.position;
        playerGameObject.GetComponent<Player>().ShoppingCart.transform.position = CompleteSpot.position;
    }

    [ClientRpc]
    private void UpdateMatchTime_ClientRpc(float newTime)
    {
        MatchTimeMinutes = newTime;
    }
}
