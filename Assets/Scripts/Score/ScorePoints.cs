

public struct ScorePoints
{
    public ulong PlayerId { get; private set; }
    public int Points;
    public int LostCounter;

    public ScorePoints(ulong playerId)
    {
        PlayerId = playerId;
        Points = 0;
        LostCounter = 0;
    }

}
  
