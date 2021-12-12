using System;
using System.Collections.Generic;
using System.Linq;
using MLAPI.Serialization;

public struct ScorePoints : INetworkSerializable
{
    public ulong PlayerId;
    public int TotalPoints;
    public int LastMatchPoints;
    public int LostCounter;
    public List<DescriptivePoints> PlayerPoints;
    public List<DescriptivePoints> LastMatchDescriptivePoints;


    public ScorePoints(ulong playerId)
    {
        PlayerId = playerId;
        TotalPoints = 0;
        LostCounter = 0;
        LastMatchPoints = 0;
        PlayerPoints = new List<DescriptivePoints>();
        LastMatchDescriptivePoints = new List<DescriptivePoints>();
    }

    public void ClearLastMatchPoints()
    {
        //PlayerPoints.AddRange(LastMatchPoints);
        LastMatchDescriptivePoints.Clear();
    }

    public void NetworkSerialize(NetworkSerializer serializer)
    {
        serializer.Serialize(ref PlayerId);
        serializer.Serialize(ref TotalPoints);
        serializer.Serialize(ref LastMatchPoints);
        serializer.Serialize(ref LostCounter);

        SerializeLastMatchPoints(serializer);
        SerializePlayerPoints(serializer);

    }

    private void SerializeLastMatchPoints(NetworkSerializer serializer)
    {
        // Length
        int length = 0;
        DescriptivePoints[] Array = new DescriptivePoints[1];
        if (!serializer.IsReading)
        {
            Array = LastMatchDescriptivePoints.ToArray();
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
            LastMatchDescriptivePoints = Array.ToList();
        }
    }
    
    private void SerializePlayerPoints(NetworkSerializer serializer)
    {
        // Length
        int length = 0;
        DescriptivePoints[] Array = new DescriptivePoints[1];
        if (!serializer.IsReading)
        {
            Array = PlayerPoints.ToArray();
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
            PlayerPoints = Array.ToList();
        }
    }
    
}