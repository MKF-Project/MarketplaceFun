using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreController : MonoBehaviour
{
    public static ScoreController Instance;

    public bool IAmWinner { get; private set; }

    // Start is called before the first frame update
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        Instance = this;
        IAmWinner = false;
    }

 


    // Update is called once per frame
    public void IWin()
    {
        IAmWinner = true;
        ScoreNetwork.Instance.CallScoreServer();
    }

    


}