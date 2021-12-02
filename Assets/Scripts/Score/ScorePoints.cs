using System;
using System.Collections.Generic;
using System.Linq;
using MLAPI.Serialization;

public struct ScorePoints : INetworkSerializable
{
    public ulong PlayerId;
    public int Points;
    public int LostCounter;
    public List<DescriptivePoints> PlayerPoints;
    public List<DescriptivePoints> LastMatchPoints;


    public ScorePoints(ulong playerId)
    {
        PlayerId = playerId;
        Points = 0;
        LostCounter = 0;
        PlayerPoints = new List<DescriptivePoints>();
        LastMatchPoints = new List<DescriptivePoints>();
    }

    public void MoveToScoresToMainList()
    {
        PlayerPoints.AddRange(LastMatchPoints);
        LastMatchPoints.Clear();
    }

    public void NetworkSerialize(NetworkSerializer serializer)
    {
        serializer.Serialize(ref PlayerId);
        serializer.Serialize(ref Points);
        serializer.Serialize(ref LostCounter);
        
        // Length
        int length = 0;
        DescriptivePoints[] Array = new DescriptivePoints[1];
        if (!serializer.IsReading)
        {
            Array = LastMatchPoints.ToArray();
            length = Array.Length;
        }

        serializer.Serialize(ref length);

        // Array
        if (serializer.IsReading)
        {
            Array = new DescriptivePoints[length];
        }

        for (int n = 0; n < length; ++n)
        {
            Array[n].NetworkSerialize(serializer);
        }
        
        if (serializer.IsReading)
        {
            LastMatchPoints = Array.ToList();
        }
        
    }
}