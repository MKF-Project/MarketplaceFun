using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnChangeScene : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        SceneManager.OnSceneLoaded += DestroySelf;
    }

    private void DestroySelf(string sceneName)
    {
        DestroyImmediate(gameObject);
    }

    private void OnDestroy()
    {
        SceneManager.OnSceneLoaded -= DestroySelf;
    }
}
