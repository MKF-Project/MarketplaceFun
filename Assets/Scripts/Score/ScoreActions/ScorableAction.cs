
using System;
using MLAPI;

[Serializable]
public abstract class ScorableAction : NetworkBehaviour
{
    public abstract void SetScore(ScoreType scoreType);
}
