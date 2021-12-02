using MLAPI.Serialization;

public struct DescriptivePoints :INetworkSerializable
{
    public int ScoreTypeId;
    public int Points;
    
    public DescriptivePoints(int scoreTypeId, int points)
    {
        ScoreTypeId = scoreTypeId;
        Points = points;
    }
    
    public void NetworkSerialize(NetworkSerializer serializer)
    {
        serializer.Serialize(ref ScoreTypeId);
        serializer.Serialize(ref Points);

    }
    
    
}