using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSwitch : MonoBehaviour
{
    private Camera _scoreCamera; 
    // Start is called before the first frame update
    void Start()
    {
        _scoreCamera = GetComponent<Camera>();
        Camera.current.enabled = false;
        _scoreCamera.enabled = true;
    }

 
}
