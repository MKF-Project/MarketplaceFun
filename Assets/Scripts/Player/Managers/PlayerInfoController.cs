using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInfoController : MonoBehaviour
{
    public static PlayerInfoController Instance;
    public Dictionary<ulong, PlayerData> PlayerInfos;

    public Stack<int> colors;
    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
        PlayerInfos = new Dictionary<ulong, PlayerData>();
        
        InitializeColors();
    }

    private void InitializeColors()
    {
        colors = new Stack<int>();
        colors.Push(4);
        colors.Push(3);
        colors.Push(2);
        colors.Push(1);
        
    }

    public PlayerData Add(ulong playerId, String nickname)
    {
        PlayerData playerData = new PlayerData(nickname, colors.Pop());
        PlayerInfos.Add(playerId, playerData);

        Debug.Log("Player " + playerId + " add  to PlayerInfoController as " + playerData.Nickname + " with color " + playerData.Color);

        return playerData;
    }

    public void Remove(ulong playerId)
    {
        colors.Push(PlayerInfos[playerId].Color);
        PlayerInfos.Remove(playerId);
    }
}