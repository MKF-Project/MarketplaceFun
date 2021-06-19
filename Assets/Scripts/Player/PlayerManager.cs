using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public bool enablePlayerBehaviour = true;

    private void Start()
    {
        PlayerController.playerBehaviourEnabled = enablePlayerBehaviour;
    }
}
