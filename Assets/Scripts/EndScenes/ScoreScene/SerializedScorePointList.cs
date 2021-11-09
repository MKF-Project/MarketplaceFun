using System.Collections.Generic;
using MLAPI.Serialization;

public struct SerializedScorePointList : INetworkSerializable
{
    public ScorePoints[] Array;


    public SerializedScorePointList(ScorePoints[] list)
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
            Array = new ScorePoints[length];
        }

        for (int n = 0; n < length; ++n)
        {
            Array[n].NetworkSerialize(serializer);
        }
    }
}