using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchManager : MonoBehaviour
{
    public static MatchManager Instance { get; private set; }

    public Player MainPlayer;

    public String Nickname;
    
    private void Awake()
    {
        Instance = this;
    }
    
    public void SetNickname(String newNickname)
    {
        Nickname = newNickname;
    }

    public bool IsMainPlayer(Player player)
    {
        return MainPlayer.Equals(player);
    }
}
