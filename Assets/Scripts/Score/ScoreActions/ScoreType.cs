using System;

[Serializable]
public struct ScoreType
{
    public String Type;
    public int Points;
    public int Code;
    public ScorableAction ScorableAction;
    
    
    public ScoreType(String type, int points, int code, ScorableAction scorableAction)
    {
        Points = points;
        Type = type;
        Code = code;
        ScorableAction = scorableAction;
    }

}
