using System.Collections.Generic;
using MLAPI.Serialization;

public struct SerializedWinnersList : INetworkSerializable
{
    public ulong[] Array;


    public SerializedWinnersList(ulong[] list)
    {
        Array = list;
    }

    public void NetworkSerialize(NetworkSerializer serializer)
    {
        // Length
        int length = 0;
        if (!serializer.IsReading)
        {
            length = Array.Length;
        }

        serializer.Serialize(ref length);

        // Array
        if (serializer.IsReading)
        {
            Array = new ulong[length];
        }

        for (int n = 0; n < length; ++n)
        {
            serializer.Serialize(ref Array[n]);
        }
    }
}