using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    public float Frontal;

    public float Lateral;

    public bool WalkButton;
    

    public void Awake()
    {
        Instance = this;
        WalkButton = false;
    }

    public void Update()
    {
        Frontal = Input.GetAxis("Vertical");
        Lateral = Input.GetAxis("Horizontal");
        WalkButtonUpdate();
    }

    private void WalkButtonUpdate()
    {
        WalkButton = Input.GetButton("Walk");
    }

}
