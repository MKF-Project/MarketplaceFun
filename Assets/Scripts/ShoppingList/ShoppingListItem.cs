using System;
using System.Collections;
using System.Collections.Generic;
using MLAPI.Serialization;
using UnityEngine;

public struct ShoppingListItem : INetworkSerializable
{
    public bool Caught;

    public int ItemCode;

    public ShoppingListItem(int itemCode)
    {
        Caught = false;
        ItemCode = itemCode;
    }

    public void NetworkSerialize(NetworkSerializer serializer)
    {
        serializer.Serialize(ref Caught);
        serializer.Serialize(ref ItemCode);
    }
}
