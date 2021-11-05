using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreList : MonoBehaviour
{
    public List<ScoreType> StartingScoreTypeList;

    public static Dictionary<int, ScoreType> ScoreTypeList;

    
    public void Awake()
    {
        DontDestroyOnLoad(gameObject);
        
        ScoreTypeList = new Dictionary<int, ScoreType>();
        PopulateDictionary();
        
    }

    private void PopulateDictionary()
    {
        foreach (ScoreType scoreType in StartingScoreTypeList)
        {
            scoreType.ScorableAction.SetScore(scoreType);
            ScoreTypeList.Add(scoreType.Code, scoreType);
        }
    }
    
    
}
