using System;
using System.Collections;
using System.Collections.Generic;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using UnityEngine;

public class WinSceneManager : NetworkBehaviour
{
    private WinCanvas _winCanvas;
    
    private const string WIN_CANVAS_TAG = "WinCanvas";



    public NetworkVariable<SerializedWinnersList> winnersList = new NetworkVariable<SerializedWinnersList>(
        new NetworkVariableSettings
        {
            ReadPermission = NetworkVariablePermission.Everyone,
            WritePermission = NetworkVariablePermission.ServerOnly
        }
    );
    public void Awake()
    {
        _winCanvas = GameObject.FindGameObjectWithTag(WIN_CANVAS_TAG).GetComponent<WinCanvas>();

        if (IsServer)
        {
            ScoreController scoreController = GameObject.FindGameObjectWithTag("ScoreController").GetComponent<ScoreController>();
            //winnersList.Value = scoreController.GetSerializedWinnersList();
            PopulateTextOnCanvas();
        }

        if (IsClient)
        {
            winnersList.OnValueChanged += PopulateTextOnCanvas;
        }

    }
    

    public void PopulateTextOnCanvas(SerializedWinnersList prev, SerializedWinnersList pos)
    {
        PopulateTextOnCanvas();
    }
    
    public void PopulateTextOnCanvas()
    {
        ulong[] winnerList = winnersList.Value.Array;
        int numberOfWinners = winnerList.Length;
        if (numberOfWinners == 1)
        {
            GameObject playerGameObject = NetworkManager.ConnectedClients[winnerList[0]].PlayerObject.gameObject;
            String playerNickname = playerGameObject.GetComponent<PlayerInfo>().PlayerData.Nickname;
            _winCanvas.ShowWinText("The Winner is " + playerNickname);
        }
        else
        {
            GameObject playerGameObject = NetworkManager.ConnectedClients[winnerList[0]].PlayerObject.gameObject;
            String playerNickname = playerGameObject.GetComponent<PlayerInfo>().PlayerData.Nickname;
            String winners = playerNickname;
            for (int index = 1; index < winnerList.Length; index++)
            {
                playerGameObject = NetworkManager.ConnectedClients[winnerList[index]].PlayerObject.gameObject;
                playerNickname = playerGameObject.GetComponent<PlayerInfo>().PlayerData.Nickname;
                winners += " and " + playerNickname;
            }
            
            _winCanvas.ShowWinText("The Winners are " + winners);
        }



    }

    
}
