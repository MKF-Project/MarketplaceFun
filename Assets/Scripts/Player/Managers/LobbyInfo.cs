using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct LobbyInfo
{
    public LobbyInfo(int index, Vector3 position, String nickname)
    {
        Index = index;
        Position = position;
        Nickname = nickname;
    }

    public int Index;
    public Vector3 Position;
    public String Nickname;

}
