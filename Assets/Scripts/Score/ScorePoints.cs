using MLAPI.Serialization;

public struct ScorePoints : INetworkSerializable
{
    public ulong PlayerId;
    public int Points;
    public int LostCounter;

    public ScorePoints(ulong playerId)
    {
        PlayerId = playerId;
        Points = 0;
        LostCounter = 0;
    }
    
    public void NetworkSerialize(NetworkSerializer serializer)
    {
        serializer.Serialize(ref PlayerId);
        serializer.Serialize(ref Points);
        serializer.Serialize(ref LostCounter);

    }

}
  
