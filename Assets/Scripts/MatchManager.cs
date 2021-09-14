using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchManager : MonoBehaviour
{
    public static MatchManager Instance { get; private set; }

    public GameObject MainPlayer;

    private void Awake()
    {
        Instance = this;
    }

    public GameObject Player1;
    public GameObject Player2;
    public GameObject Player3;
    public GameObject Player4;

    public void SpawnPlayers(Vector3 position1, Vector3 position2, Vector3 position3, Vector3 position4)
    {
        Player1.SetActive(true);
        Player1.transform.position = position1;

        Player2.transform.position = position2;
        Player2.SetActive(true);

        Player3.transform.position = position3;
        Player3.SetActive(true);

        Player4.transform.position = position4;
        Player4.SetActive(true);
    }

    public bool IsMainPlayer(GameObject player)
    {
        return MainPlayer.Equals(player);
    }
}
