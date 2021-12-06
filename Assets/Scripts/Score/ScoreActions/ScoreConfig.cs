using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreConfig : MonoBehaviour
{
    public List<ScoreType> StartingScoreTypeList;

    public static Dictionary<int, ScoreType> ScoreTypeDictionary;

    
    public void Awake()
    {
        //DontDestroyOnLoad(gameObject);
        
        ScoreTypeDictionary = new Dictionary<int, ScoreType>();
        PopulateDictionary();
        
    }

    private void PopulateDictionary()
    {
        for (int index = 0; index < StartingScoreTypeList.Count; index++)
        {
            ScoreType scoreType = StartingScoreTypeList[index];
            scoreType.Id = index;
            ScoreTypeDictionary.Add(index, scoreType);
        }
    }

    public static bool FindByScorableAction(ScorableAction scorableAction, out ScoreType scoreType)
    {
        Type scoreClassType = scorableAction.GetType();
        foreach (ScoreType scoreTypeInList in ScoreTypeDictionary.Values)
        {
            if (scoreTypeInList.ScorableAction.GetType() == scoreClassType)
            {
                scoreType = scoreTypeInList;
                return true;
            }
        }

        scoreType = new ScoreType();
        return false;
    }


}
