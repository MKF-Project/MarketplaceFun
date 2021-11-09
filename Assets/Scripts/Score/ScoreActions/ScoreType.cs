using System;
using UnityEngine;

[Serializable]
public struct ScoreType
{
    public String Type;
    public int Points;
    [HideInInspector]
    public int Id;
    public ScorableAction ScorableAction;
    
    
    public ScoreType(String type, int points, int id, ScorableAction scorableAction)
    {
        Points = points;
        Type = type;
        Id = id;
        ScorableAction = scorableAction;
    }

}
