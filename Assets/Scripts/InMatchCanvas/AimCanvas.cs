using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimCanvas : MonoBehaviour
{
    public static AimCanvas Instance;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        Instance = this;
        gameObject.SetActive(false);
    }

    public void ActivateAim()
    {
        gameObject.SetActive(true);
    }
    
    public void DisableAim()
    {
        gameObject.SetActive(false);
    }
}
