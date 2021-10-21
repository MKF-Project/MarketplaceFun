using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimCanvas : MonoBehaviour
{
    public static AimCanvas Instance;

    private void Start()
    {
        if (AimCanvas.Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            gameObject.SetActive(false);
        }
        else
        {
            DestroyImmediate(gameObject);
        }
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
