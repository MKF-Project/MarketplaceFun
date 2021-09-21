using System.Collections;
using System.Collections.Generic;
using MLAPI;
using MLAPI.Messaging;
using UnityEngine;

public class ScoreNetwork : NetworkBehaviour
{
    public static ScoreNetwork Instance;


    private void Awake()
    {
        Instance = this;
    }

    public  void CallScoreServer()
    {
        ChangeSceneScore_ServerRpc();
    }

    [ServerRpc]
    public void ChangeSceneScore_ServerRpc()
    {
        SceneManager.LoadScore();
    }
}