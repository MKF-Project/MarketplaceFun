using System;
using System.Collections;
using System.Collections.Generic;
using MLAPI.Serialization;
using UnityEngine;

public struct PlayerData : INetworkSerializable
{
    
    public String Nickname;

    public int Color;
    

    public PlayerData(String nickname, int color)
    {
        Nickname = nickname;
        Color = color;
    }

    public void NetworkSerialize(NetworkSerializer serializer)
    {
        serializer.Serialize(ref Nickname);
        serializer.Serialize(ref Color);
    }
}
