
using System;
using MLAPI;

[Serializable]
public class ScorableAction : NetworkBehaviour
{
    protected ScoreType _scoreType;

    protected virtual void Awake()
    {
        ScoreConfig.FindByScorableAction(this, out _scoreType);
    }


    
}
