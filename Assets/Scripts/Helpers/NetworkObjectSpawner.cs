using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkObjectSpawner : MonoBehaviour
{

    public static NetworkObjectSpawner Instance;
    
    public GameObject ScoreController;


    private void Awake()
    {
        Instance = this;
        LobbyMenu.OnEnterLobby += SpawnScoreController;
    }

    private void OnDestroy()
    {
        LobbyMenu.OnEnterLobby -= SpawnScoreController;
    }

    public void SpawnScoreController()
    {
        Instantiate(ScoreController);
    }
}
